using System;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// A Watch system notification that a watched user's status has changed
    /// </summary>
    [Serializable]
    public abstract class WatchedUserChangedMessage : NumericMessage
    {

        /// <summary>
        /// Gets or sets the watched User who's status has changed.
        /// </summary>
        public User WatchedUser {
            get {
                if (watchedUser == null) {
                    watchedUser = new User ();
                }
                return watchedUser;
            }
            set {
                watchedUser = value;
            }
        }
        User watchedUser;

        /// <summary>
        /// Gets or sets the time at which the change occured.
        /// </summary>
        public DateTime TimeOfChange {
            get {
                return changeTime;
            }
            set {
                changeTime = value;
            }
        }
        DateTime changeTime;

        /// <exclude />
        protected abstract string ChangeMessage {
            get;
        }


        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (WatchedUser.Nick);
            writer.AddParameter (WatchedUser.UserName);
            writer.AddParameter (WatchedUser.HostName);
            writer.AddParameter (MessageUtil.ConvertToUnixTime (TimeOfChange).ToString (CultureInfo.InvariantCulture));
            writer.AddParameter (ChangeMessage);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);

            WatchedUser = new User ();
            TimeOfChange = DateTime.MinValue;

            if (parameters.Count == 6) {
                WatchedUser.Nick = parameters [1];
                WatchedUser.UserName = parameters [2];
                WatchedUser.HostName = parameters [3];
                TimeOfChange = MessageUtil.ConvertFromUnixTime (Convert.ToInt32 (parameters [4], CultureInfo.InvariantCulture));
            }
        }

    }
}
