using System;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Queries the server to see if it supports the Ircx extension without changing the ircx mode.
    /// </summary>
    [Serializable]
    public class IsIrcxMessage : CommandMessage
    {

        /// <summary>
        /// Creates a new instance of the IsIrcxMessage class.
        /// </summary>
        public IsIrcxMessage()
        {
        }

        /// <summary>
        /// Gets the Irc command associated with this message.
        /// </summary>
        protected override string Command {
            get {
                return "ISIRCX";
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify(MessageConduit conduit)
        {
            conduit.OnIsIrcx(new IrcMessageEventArgs<IsIrcxMessage>(this));
        }

    }
}
