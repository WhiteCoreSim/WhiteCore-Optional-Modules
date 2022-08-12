using System;


namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The reply to a <see cref="ScriptRequestMessage"/>.
    /// </summary>
    [Serializable]
    public class ScriptReplyMessage : CtcpReplyMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="ScriptReplyMessage"/> class.
        /// </summary>
        public ScriptReplyMessage()
        {
            InternalCommand = "SCRIPT";
        }

        /// <summary>
        /// Gets or sets the script name that the client is using
        /// </summary>
        public virtual string Response {
            get {
                return response;
            }
            set {
                response = value;
            }
        }
        string response = "";


        /// <summary>
        /// Gets the data payload of the Ctcp request.
        /// </summary>
        protected override string ExtendedData {
            get {
                return response;
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify(MessageConduit conduit)
        {
            conduit.OnScriptReply(new IrcMessageEventArgs<ScriptReplyMessage>(this));
        }

        /// <summary>
        /// Parses the given string to populate this <see cref="IrcMessage"/>.
        /// </summary>
        public override void Parse(string unparsedMessage)
        {
            base.Parse(unparsedMessage);
            Response = CtcpUtil.GetExtendedData(unparsedMessage);
        }

    }
}
