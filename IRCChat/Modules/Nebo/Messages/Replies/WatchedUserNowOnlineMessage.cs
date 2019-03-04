using System;


namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// A Watch system notification that a user is online.
    /// </summary>
    [Serializable]
    public class WatchedUserNowOnlineMessage : WatchedUserOnlineMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="WatchedUserNowOnlineMessage"/>.
        /// </summary>
        public WatchedUserNowOnlineMessage ()
        {
            InternalNumeric = 601;
        }

        /// <exclude />
        protected override string ChangeMessage {
            get {
                return "logged online";
            }
        }

    }
}
