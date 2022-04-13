using Microsoft.Extensions.Configuration;
using SolaceSystems.Solclient.Messaging;
using System.Text;

namespace PublisherNonBlocking {

    /// <summary>
    /// Provide support for conflating settings between defaults set here, 
    /// values expressed in appsettings.json, and paraemters supplied via the CLI.
    /// </summary>
    internal class Settings {

        /// <summary>
        /// Applying overrides from either appsettings or the CLI.
        /// </summary>
        /// <param name="config">An instance of values expressed in appsettings.json.</param>
        internal void ApplyOverride(IConfiguration config) {
            Host                 = config.GetString(nameof(Host), Host);
            VPN                  = config.GetString(nameof(VPN), VPN);
            UserName             = config.GetString(nameof(UserName), UserName);
            Password             = config.GetString(nameof(Password), Password);
            Topic                = config.GetString(nameof(Topic), Topic);
            DeliveryMode         = config.GetEnum(nameof(DeliveryMode), DeliveryMode);
            Count                = config.GetInteger(nameof(Count), Count);
            SdkBufferSize        = config.GetInteger(nameof(SdkBufferSize), SdkBufferSize);
            SocketSendBufferSize = config.GetInteger(nameof(SocketSendBufferSize), SocketSendBufferSize);
        }

        public override string ToString() {
            return new StringBuilder()
                .AppendLine($"Host     = {Host}")
                .AppendLine($"VPN      = {VPN}")
                .AppendLine($"UserName = {UserName}")
                .AppendLine($"Password = {Password}")
                .AppendLine($"Topic    = {Topic}")
                .AppendLine($"Count    = {Count:#,0}")
                .AppendLine()
                .AppendLine($"DeliveryMode = {DeliveryMode}")
                .AppendLine()
                .AppendLine($"SdkBufferSize        = {SdkBufferSize:#,0}")
                .AppendLine($"SocketSendBufferSize = {SocketSendBufferSize:#,0}")
                .ToString();
        }

        // [Option('c', "count", HelpText="How many messages to send.  Default is 1.")]
        public int Count { get; set; } = 1;

        /// <summary>
        /// Controls the buffering of transmit messsages.  <seealso href="https://docs.solace.com/API-Developer-Online-Ref-Documentation/net/html/9c65421d-738e-81b6-0876-f6d6c69ffbbf.htm"/>
        /// </summary>
        public int SdkBufferSize { get; set; } = 90000;

        /// <summary>
        /// The socket send buffer size.  <seealso href="https://docs.solace.com/API-Developer-Online-Ref-Documentation/net/html/fe07a2d5-317b-213d-abff-39196c69894c.htm"/>
        /// </summary>
        public int SocketSendBufferSize { get; set; } = 90000;

        // [Option('h', "host", HelpText="Host name or IP.  Default is 'tcp://localhost:51555'.")]
        public string Host { get; set; } = "tcp://localhost:51555";

        // [Option('v', "vpn", HelpText="Message VPN.  Default is 'default'.")]
        public string VPN { get; set; } = "default";

        // [Option('u', "user", HelpText="Client user name.  Default is 'default'.")]
        public string UserName { get; set; } = "default";

        // [Option('w', "password", Required=false, HelpText="Password.")]
        public string Password { get; set; } = null;

        // [Option('t', "topic", Required=true, HelpText="Topic.")]
        public string Topic { get; set; }

        public MessageDeliveryMode DeliveryMode { get; set; } = MessageDeliveryMode.Direct;

    }
    
}