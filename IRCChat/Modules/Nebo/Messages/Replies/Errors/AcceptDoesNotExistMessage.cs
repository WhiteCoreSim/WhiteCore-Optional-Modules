using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The ErrorMessage received when the client attempts to remove a nick from his accept list
    /// when that nick does not exist on the list.
    /// </summary>
    [Serializable]
    public class AcceptDoesNotExistMessage : ErrorMessage
    {

        /// <summary>
        /// Creates a new instances of the <see cref="AcceptDoesNotExistMessage"/> class.
        /// </summary>
        public AcceptDoesNotExistMessage ()
        {
            InternalNumeric = 458;
        }

        /// <summary>
        /// Gets or sets the nick which wasn't added
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
            // :irc.pte.hu 458 artificer BBS :is not on your accept list

            base.AddParametersToFormat (writer);
            writer.AddParameter (Nick);
            writer.AddParameter ("is not on your accept list");
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            // :irc.pte.hu 458 artificer BBS :is not on your accept list

            base.ParseParameters (parameters);
            if (parameters.Count > 1) {
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
            conduit.OnAcceptDoesNotExist (new IrcMessageEventArgs<AcceptDoesNotExistMessage> (this));
        }

    }
}
