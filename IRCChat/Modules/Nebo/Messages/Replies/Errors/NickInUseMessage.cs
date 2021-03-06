using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Returned when a <see cref="NickChangeMessage"/> is processed that results in an attempt to change to a currently existing nickname. 
    /// </summary>
    [Serializable]
    public class NickInUseMessage : ErrorMessage
    {

        /// <summary>
        /// Creates a new instances of the <see cref="NickInUseMessage"/> class.
        /// </summary>
        public NickInUseMessage ()
        {
            InternalNumeric = 433;
        }

        /// <summary>
        /// Gets or sets the nick which was already taken.
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
            writer.AddParameter ("Nickname is already in use.");
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
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
            conduit.OnNickInUse (new IrcMessageEventArgs<NickInUseMessage> (this));
        }

    }
}
