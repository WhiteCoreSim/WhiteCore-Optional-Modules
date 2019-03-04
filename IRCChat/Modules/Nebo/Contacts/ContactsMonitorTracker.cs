using MetaBuilders.Irc.Messages;

namespace MetaBuilders.Irc.Contacts
{
    class ContactsMonitorTracker : ContactsTracker
    {
        public ContactsMonitorTracker (ContactList contacts)
            : base (contacts)
        {
        }

        public override void Initialize ()
        {
            Contacts.Client.Messages.MonitoredUserOffline += client_MonitoredUserOffline;
            Contacts.Client.Messages.MonitoredUserOnline += client_MonitoredUserOnline;
            base.Initialize ();
        }

        protected override void AddNicks (System.Collections.Specialized.StringCollection nicks)
        {
            MonitorAddUsersMessage add = new MonitorAddUsersMessage ();
            foreach (string nick in nicks) {
                add.Nicks.Add (nick);
            }
            Contacts.Client.Send (add);
        }

        protected override void AddNick (string nick)
        {
            MonitorAddUsersMessage add = new MonitorAddUsersMessage ();
            add.Nicks.Add (nick);
            Contacts.Client.Send (add);
        }

        protected override void RemoveNick (string nick)
        {
            MonitorRemoveUsersMessage remove = new MonitorRemoveUsersMessage ();
            remove.Nicks.Add (nick);
            Contacts.Client.Send (remove);
        }

        #region Reply Handlers

        void client_MonitoredUserOnline (object sender, IrcMessageEventArgs<MonitoredUserOnlineMessage> e)
        {
            foreach (User onlineUser in e.Message.Users) {
                User knownUser = Contacts.Users.Find (onlineUser.Nick);
                if (knownUser != null) {
                    knownUser.MergeWith (onlineUser);
                    if (knownUser.OnlineStatus == UserOnlineStatus.Offline) {
                        knownUser.OnlineStatus = UserOnlineStatus.Online;
                    }
                }
            }
        }

        void client_MonitoredUserOffline (object sender, IrcMessageEventArgs<MonitoredUserOfflineMessage> e)
        {
            foreach (string offlineNick in e.Message.Nicks) {
                User knownUser = Contacts.Users.Find (offlineNick);
                if (knownUser != null) {
                    knownUser.OnlineStatus = UserOnlineStatus.Offline;
                }
            }
        }

        #endregion

    }
}
