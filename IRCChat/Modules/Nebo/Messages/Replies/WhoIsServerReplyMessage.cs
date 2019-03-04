using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// A reply to a <see cref="WhoIsMessage"/> that specifies what server they are on.
    /// </summary>
    [Serializable]
    public class WhoIsServerReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="WhoIsServerReplyMessage"/> class.
        /// </summary>
        public WhoIsServerReplyMessage ()
        {
            InternalNumeric = 312;
        }

        /// <summary>
        /// Gets or sets the nick of the user being examined.
        /// </summary>
        public virtual string Nick {
            get {
                return nick;
            }
            set {
                nick = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the server the user is connected to.
        /// </summary>
        public virtual string ServerName {
            get {
                return serverName;
            }
            set {
                serverName = value;
            }
        }

        /// <summary>
        /// Gets or sets additional information about the user's server connection.
        /// </summary>
        public virtual string Info {
            get {
                return info;
            }
            set {
                info = value;
            }
        }

        string nick = "";
        string serverName = "";
        string info = "";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Nick);
            writer.AddParameter (ServerName);
            writer.AddParameter (Info);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count > 3) {
                Nick = parameters [1];
                ServerName = parameters [2];
                Info = parameters [3];
            } else {
                Nick = "";
                ServerName = "";
                Info = "";
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnWhoIsServerReply (new IrcMessageEventArgs<WhoIsServerReplyMessage> (this));
        }

    }
}
