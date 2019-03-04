using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Requests information about the nicks supplied in the Nick property.
    /// </summary>
    [Serializable]
    public class UserHostMessage : CommandMessage
    {

        /// <summary>
        /// Gets the Irc command associated with this message.
        /// </summary>
        protected override string Command {
            get {
                return "USERHOST";
            }
        }

        /// <summary>
        /// Gets the collection of nicks to request information for.
        /// </summary>
        public virtual StringCollection Nicks {
            get {
                return nicks;
            }
        }

        StringCollection nicks = new StringCollection ();

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddList (Nicks, " ");
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            Nicks.Clear ();
            foreach (string nick in parameters) {
                Nicks.Add (nick);
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnUserHost (new IrcMessageEventArgs<UserHostMessage> (this));
        }

    }
}
