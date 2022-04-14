using System;
using Microsoft.Extensions.Configuration;
using SolaceSystems.Solclient.Messaging;
using System.Collections.Generic;

namespace PublisherNonBlocking {

    internal class Controller : IDisposable {
        
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(nameof(Controller));
        private readonly Settings settings;
        private Publisher publisher;
        private IList<IMessage> messageList;
        private int lastSent = -1;
        private int blockedCount;

        public Controller(IConfiguration config) {
            settings = config.GetRequiredSection("Settings").Get<Settings>();
            settings.ApplyOverride(config);

            if (log.IsDebugEnabled) {
                log.Debug($"Settings:\n\n{settings}");
            }
        }

        #region IDisposable

        private bool disposed;
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (!disposed) {
                    if (publisher != null) {
                        publisher.OnSessionEvent -= OnSessionEventHandler;
                        publisher.Dispose();
                        publisher = null;
                    }
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

        internal void Init() {
            // initialize context for Solace -- the context is a singleton
            var cfp = new ContextFactoryProperties() {
                SolClientLogLevel = SolLogLevel.Warning  // Set log level.
            };
            cfp.LogToConsoleError();  // Log errors to console.
            ContextFactory.Instance.Init(cfp);  // Must init the API before using any of its artifacts.

            publisher = new Publisher(settings);
            publisher.OnSessionEvent += OnSessionEventHandler;
            publisher.Connect();

            // create a list of messages to send
            messageList = CreateMessageList();
        }

        /// <summary>
        /// Send messages stored in the message list until <c><seealso href="https://docs.solace.com/API-Developer-Online-Ref-Documentation/net/html/4eaedb16-3901-03fb-e300-7a327dc5495f.htm">ReturnCode</seealso>.SOLCLIENT_WOULD_BLOCK</c> 
        /// is returned by the <seealso href="https://docs.solace.com/API-Developer-Online-Ref-Documentation/net/html/e30d50c1-b50c-afac-4dbc-9d4ff5e7e031.htm">session</seealso>.
        /// </summary>
        internal void SendMessageList() {
            for (int i = lastSent+1; i < messageList.Count; i++) {
                var message = messageList[i];
                message.SequenceNumber = i;
                var returnCode = publisher.SendMessage(message);
                if (returnCode == ReturnCode.SOLCLIENT_WOULD_BLOCK) {
                    if (log.IsDebugEnabled) {
                        log.Debug($"{returnCode} - last sent = {lastSent}");
                    }
                    blockedCount++;
                    break;
                }
                lastSent = i;
            }

            if (log.IsInfoEnabled) {
                if (lastSent+1 == messageList.Count) {
                    log.Info($"All messages sent.  Blocked {blockedCount} time(s).");
                }
            }
        }

        /// <summary>
        /// Handler for session event.
        /// </summary>
        /// <remarks>This is where </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSessionEventHandler(object sender, SessionEventArgs e) {
            if (e.Event == SessionEvent.CanSend) {
                if (log.IsDebugEnabled) {
                    log.Debug("Resume sending...");
                }
                SendMessageList();
            }
        }

        /// <summary>
        /// Initialize a message list
        /// </summary>
        /// <returns>A list of messages to send.</returns>
        private IList<IMessage> CreateMessageList() {
            var topic = ContextFactory.Instance.CreateTopic(settings.Topic);

            if (log.IsDebugEnabled) {
                if (settings.Count <= 1) {
                    log.Debug($"Creating {settings.Count} message...");
                } else {
                    log.Debug($"Creating {settings.Count:#,0} messages...");
                }
            }

            var retval = new List<IMessage>(settings.Count);
            for (int i = 0; i < settings.Count; i++) {                    
                var message = ContextFactory.Instance.CreateMessage();
                message.DeliveryMode     = settings.DeliveryMode;
                message.Destination      = topic;
                message.SequenceNumber   = i;
                message.BinaryAttachment = TestMessage.CreateInstance().GetBytes();

                retval.Add(message);
            }
            return retval;
        }

    }

}