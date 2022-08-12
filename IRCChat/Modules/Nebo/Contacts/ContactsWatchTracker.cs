using MetaBuilders.Irc.Messages;

namespace MetaBuilders.Irc.Contacts
{
    class ContactsWatchTracker : ContactsTracker
    {
        public ContactsWatchTracker(ContactList contacts)
            : base(contacts)
        {
        }

        public override void Initialize()
        {
            Contacts.Client.Messages.WatchedUserOffline += Client_WatchedUserOffline;
            Contacts.Client.Messages.WatchedUserOnline += Client_WatchedUserOnline;
            base.Initialize();
        }

        protected override void AddNicks(System.Collections.Specialized.StringCollection nicks)
        {
            WatchListEditorMessage addMsg = new WatchListEditorMessage();
            foreach (string nick in nicks) {
                addMsg.AddedNicks.Add(nick);
            }
            Contacts.Client.Send(addMsg);
        }

        protected override void AddNick(string nick)
        {
            WatchListEditorMessage addMsg = new WatchListEditorMessage();
            addMsg.AddedNicks.Add(nick);
            Contacts.Client.Send(addMsg);
        }

        protected override void RemoveNick(string nick)
        {
            WatchListEditorMessage remMsg = new WatchListEditorMessage();
            remMsg.RemovedNicks.Add(nick);
            Contacts.Client.Send(remMsg);
        }

        #region Reply Handlers

        void Client_WatchedUserOnline(object sender, IrcMessageEventArgs<WatchedUserOnlineMessage> e)
        {
            User knownUser = Contacts.Users.Find(e.Message.WatchedUser.Nick);
            if (knownUser != null && knownUser.OnlineStatus == UserOnlineStatus.Offline) {
                knownUser.OnlineStatus = UserOnlineStatus.Online;
            }
        }

        void Client_WatchedUserOffline(object sender, IrcMessageEventArgs<WatchedUserOfflineMessage> e)
        {
            User knownUser = Contacts.Users.Find(e.Message.WatchedUser.Nick);
            if (knownUser != null) {
                knownUser.OnlineStatus = UserOnlineStatus.Offline;
            }
        }

        #endregion



    }
}
