using System;
using SolaceSystems.Solclient.Messaging;

namespace PublisherNonBlocking {

    /// <summary>
    /// Provides support for publshing messages through Solace PubSub+.
    /// </summary>
    internal class Publisher : IDisposable {
        
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(nameof(Publisher));
        private readonly Settings settings;
        private ISession session;
        private IContext context;

        internal Publisher(Settings settings) {
            this.settings = settings;
        }

        #region IDisposable

        private bool disposed;
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (!disposed) {
                    if (session != null) {
                        session.Dispose();
                        session = null;
                    }
                    if (context != null) {
                        context.Dispose();
                        context = null;
                    }
                    ContextFactory.Instance.Cleanup();

                    disposed = true;
                }
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Controller() {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Initialze the context for connecting to PubSub+ then connect to it.
        /// </summary>
        internal void Connect() {
            context = ContextFactory.Instance.CreateContext(new ContextProperties(), null);

            var sessionProps = new SessionProperties() {
                Host     = settings.Host,
                VPNName  = settings.VPN,
                UserName = settings.UserName,
                Password = settings.Password,
                
                SendBlocking                = false,
                SdkBufferSize               = settings.SdkBufferSize,
                SocketSendBufferSizeInBytes = settings.SocketSendBufferSize
            };
            session = context.CreateSession(sessionProps, MessageEventHandler, SessionEventHandler);
            session.Connect();
        }

        internal ReturnCode SendMessage(IMessage message) {
            return session.Send(message);
        }

        private void MessageEventHandler(object source, MessageEventArgs e) {
            OnMessageEvent?.Invoke(source, e);
        }

        private void SessionEventHandler(object sender, SessionEventArgs e) {
            OnSessionEvent?.Invoke(sender, e);
        }

        public event EventHandler<MessageEventArgs> OnMessageEvent;
        public event EventHandler<SessionEventArgs> OnSessionEvent;

    }

}