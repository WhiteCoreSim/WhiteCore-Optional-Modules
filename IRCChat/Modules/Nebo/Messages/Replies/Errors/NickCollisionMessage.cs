using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Returned by a server to a client when it detects a nickname collision.
    /// </summary>
    [Serializable]
    public class NickCollisionMessage : ErrorMessage
    {

        /// <summary>
        /// Creates a new instances of the <see cref="NickCollisionMessage"/> class.
        /// </summary>
        public NickCollisionMessage ()
        {
            InternalNumeric = 436;
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
            writer.AddParameter ("Nickname collision KILL");
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
            conduit.OnNickCollision (new IrcMessageEventArgs<NickCollisionMessage> (this));
        }

    }
}
