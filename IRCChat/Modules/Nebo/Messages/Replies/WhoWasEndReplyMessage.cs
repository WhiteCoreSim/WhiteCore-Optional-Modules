using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Signals the end of a reply to a <see cref="WhoWasMessage"/>.
    /// </summary>
    [Serializable]
    public class WhoWasEndReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="WhoWasEndReplyMessage"/> class.
        /// </summary>
        public WhoWasEndReplyMessage ()
        {
            InternalNumeric = 369;
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

        string nick = "";


        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Nick);
            writer.AddParameter ("End of WHOWAS");
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count == 3) {
                Nick = parameters [1];
            } else {
                Nick = "";
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnWhoWasEndReply (new IrcMessageEventArgs<WhoWasEndReplyMessage> (this));
        }

    }
}
