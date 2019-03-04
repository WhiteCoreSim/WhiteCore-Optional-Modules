using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// This is the reply to the <see cref="TimeMessage"/> server query.
    /// </summary>
    [Serializable]
    public class ServerTimeReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="ServerTimeReplyMessage"/> class
        /// </summary>
        public ServerTimeReplyMessage ()
        {
            InternalNumeric = 391;
        }

        /// <summary>
        /// Gets or sets the server replying to the time request.
        /// </summary>
        public virtual string Server {
            get {
                return server;
            }
            set {
                server = value;
            }
        }

        /// <summary>
        /// Gets or sets the time value requested.
        /// </summary>
        public virtual string Time {
            get {
                return time;
            }
            set {
                time = value;
            }
        }

        string server = "";
        string time = "";


        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Server);
            writer.AddParameter (Time);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count == 3) {
                Server = parameters [1];
                Time = parameters [2];
            } else {
                Server = "";
                Time = "";
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnServerTimeReply (new IrcMessageEventArgs<ServerTimeReplyMessage> (this));
        }

    }
}
