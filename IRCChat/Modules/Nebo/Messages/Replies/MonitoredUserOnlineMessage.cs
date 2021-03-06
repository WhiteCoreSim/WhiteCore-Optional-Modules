using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// A Monitor system notification that a monitored user is online
    /// </summary>
    [Serializable]
    public class MonitoredUserOnlineMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="MonitoredUserOnlineMessage"/>.
        /// </summary>
        public MonitoredUserOnlineMessage ()
        {
            InternalNumeric = 730;
        }

        /// <summary>
        /// Gets the collection of users who are online.
        /// </summary>
        public UserCollection Users {
            get {
                if (users == null) {
                    users = new UserCollection ();
                }
                return users;
            }
        }
        UserCollection users;

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            string userList = MessageUtil.CreateList (Users, ",", delegate (User user) {
                return user.ToNickUserHostString ();
            });

            writer.AddParameter (userList);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);

            Users.Clear ();

            if (parameters.Count > 1) {
                string userListParam = parameters [1];
                string [] userList = userListParam.Split (new string [] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string userMask in userList) {
                    User newUser = new User ();
                    newUser.Parse (userMask);
                    Users.Add (newUser);
                }
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnMonitoredUserOnline (new IrcMessageEventArgs<MonitoredUserOnlineMessage> (this));
        }

    }
}
