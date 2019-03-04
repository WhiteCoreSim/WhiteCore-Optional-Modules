using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Marks the end of the replies to the <see cref="LinksMessage"/> query.
    /// </summary>
    [Serializable]
    public class LinksEndReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="LinksEndReplyMessage"/> class.
        /// </summary>
        public LinksEndReplyMessage ()
        {
            InternalNumeric = 365;
        }

        /// <summary>
        /// Gets or sets the server mask that the links list used.
        /// </summary>
        public virtual string Mask {
            get {
                return mask;
            }
            set {
                mask = value;
            }
        }

        string mask = "";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Mask);
            writer.AddParameter ("End of /LINKS list");
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count == 3) {
                Mask = parameters [1];
            } else {
                Mask = "";
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnLinksEndReply (new IrcMessageEventArgs<LinksEndReplyMessage> (this));
        }

    }
}
