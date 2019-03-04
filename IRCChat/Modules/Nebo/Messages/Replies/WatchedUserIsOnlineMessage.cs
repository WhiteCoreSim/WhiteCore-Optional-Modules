using System;


namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// A Watch system notification that a user is online.
    /// </summary>
    [Serializable]
    public class WatchedUserIsOnlineMessage : WatchedUserOnlineMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="WatchedUserIsOnlineMessage"/>.
        /// </summary>
        public WatchedUserIsOnlineMessage ()
        {
            InternalNumeric = 604;
        }

        /// <exclude />
        protected override string ChangeMessage {
            get {
                return "is online";
            }
        }

    }
}
