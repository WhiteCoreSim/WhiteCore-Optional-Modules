using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    // :irc2.secsup.org 004 artificer irc2.secsup.org 2.8/hybrid-6.3.1 oOiwszcrkfydnxb biklmnopstve

    /// <summary>
    /// Contains basic information about a server.
    /// </summary>
    [Serializable]
    public class ServerInfoMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="ServerInfoMessage"/> class.
        /// </summary>
        public ServerInfoMessage ()
        {
            InternalNumeric = 004;
        }

        /// <summary>
        /// Gets or sets the name of the server being referenced.
        /// </summary>
        public virtual string ServerName {
            get {
                return serverName;
            }
            set {
                serverName = value;
            }
        }
        string serverName = "";

        /// <summary>
        /// Gets or sets the version of the server.
        /// </summary>
        public virtual string Version {
            get {
                return version;
            }
            set {
                version = value;
            }
        }
        string version = "";

        /// <summary>
        /// Gets or sets the user modes supported by this server.
        /// </summary>
        public virtual string UserModes {
            get {
                return userModes;
            }
            set {
                userModes = value;
            }
        }
        private string userModes = "";

        /// <summary>
        /// Gets or sets the channel modes supported by this server.
        /// </summary>
        public virtual string ChannelModes {
            get {
                return channelModes;
            }
            set {
                channelModes = value;
            }
        }
        private string channelModes = "";

        /// <summary>
        /// Gets or sets the channel modes that require a parameter.
        /// </summary>
        public virtual string ChannelModesWithParams {
            get {
                return channelModesWithParams;
            }
            set {
                channelModesWithParams = value;
            }
        }
        string channelModesWithParams = "";

        /// <summary>
        /// Gets or sets the user modes that require a parameter.
        /// </summary>
        public virtual string UserModesWithParams {
            get {
                return userModesWithParams;
            }
            set {
                userModesWithParams = value;
            }
        }
        string userModesWithParams = "";

        /// <summary>
        /// Gets or sets the server modes supported by this server.
        /// </summary>
        public virtual string ServerModes {
            get {
                return serverModes;
            }
            set {
                serverModes = value;
            }
        }
        string serverModes = "";

        /// <summary>
        /// Gets or sets the server modes which require parameters.
        /// </summary>
        public virtual string ServerModesWithParams {
            get {
                return serverModesWithParams;
            }
            set {
                serverModesWithParams = value;
            }
        }
        string serverModesWithParams = "";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);

            writer.AddParameter (ServerName);
            writer.AddParameter (Version);
            writer.AddParameter (UserModes);
            writer.AddParameter (ChannelModes);
            if (ChannelModesWithParams.Length != 0) {
                writer.AddParameter (ChannelModesWithParams);
                if (UserModesWithParams.Length != 0) {
                    writer.AddParameter (UserModesWithParams);
                    if (ServerModes.Length != 0) {
                        writer.AddParameter (ServerModes);
                        if (ServerModesWithParams.Length != 0) {
                            writer.AddParameter (ServerModesWithParams);
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);

            ServerName = parameters [1];
            Version = parameters [2];
            UserModes = parameters [3];
            ChannelModes = parameters [4];

            int pCount = parameters.Count;

            if (pCount > 5) {
                ChannelModesWithParams = parameters [5];
                if (pCount > 6) {
                    UserModesWithParams = parameters [6];

                    if (pCount > 7) {
                        ServerModes = parameters [7];

                        if (pCount > 8) {
                            ServerModesWithParams = parameters [8];
                        }
                    }
                }


            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnServerInfo (new IrcMessageEventArgs<ServerInfoMessage> (this));
        }

    }

}
