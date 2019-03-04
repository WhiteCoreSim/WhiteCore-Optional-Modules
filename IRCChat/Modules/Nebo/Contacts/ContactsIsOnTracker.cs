using System;
using MetaBuilders.Irc.Messages;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Contacts
{
    class ContactsIsOnTracker : ContactsTracker, IDisposable
    {
        public ContactsIsOnTracker (ContactList contacts)
            : base (contacts)
        {
        }

        public override void Initialize ()
        {
            Contacts.Client.Messages.IsOnReply += client_IsOnReply;
            base.Initialize ();
            if (timer != null) {
                timer.Dispose ();
            }
            timer = new System.Timers.Timer ();
            timer.Elapsed += timer_Elapsed;
            timer.Start ();
        }

        void timer_Elapsed (object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Contacts.Client.Connection.Status == Network.ConnectionStatus.Connected) {
                IsOnMessage ison = new IsOnMessage ();
                foreach (string nick in trackedNicks) {
                    ison.Nicks.Add (nick);
                    if (!waitingOnNicks.Contains (nick)) {
                        waitingOnNicks.Add (nick);
                    }
                }
                Contacts.Client.Send (ison);
            }
        }

        protected override void AddNicks (StringCollection nicks)
        {
            foreach (string nick in nicks) {
                AddNick (nick);
            }
        }

        protected override void AddNick (string nick)
        {
            if (!trackedNicks.Contains (nick)) {
                trackedNicks.Add (nick);
            }
        }

        protected override void RemoveNick (string nick)
        {
            if (trackedNicks.Contains (nick)) {
                trackedNicks.Remove (nick);
            }
        }

        StringCollection trackedNicks = new StringCollection ();
        StringCollection waitingOnNicks = new StringCollection ();
        System.Timers.Timer timer;

        #region Reply Handlers

        void client_IsOnReply (object sender, IrcMessageEventArgs<IsOnReplyMessage> e)
        {
            foreach (string onlineNick in e.Message.Nicks) {
                if (waitingOnNicks.Contains (onlineNick)) {
                    waitingOnNicks.Remove (onlineNick);
                }
                User knownUser = Contacts.Users.Find (onlineNick);
                if (knownUser != null && knownUser.OnlineStatus == UserOnlineStatus.Offline) {
                    knownUser.OnlineStatus = UserOnlineStatus.Online;
                }
                if (knownUser == null && trackedNicks.Contains (onlineNick)) {
                    trackedNicks.Remove (onlineNick);
                }
            }
            foreach (string nick in waitingOnNicks) {
                User offlineUser = Contacts.Users.Find (nick);
                if (offlineUser != null)
                    offlineUser.OnlineStatus = UserOnlineStatus.Offline;
                waitingOnNicks.Remove (nick);
            }
        }

        #endregion


        #region IDisposable Members

        bool disposed;

        public void Dispose ()
        {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        void Dispose (bool disposing)
        {
            if (!disposed) {
                if (disposing) {
                    timer.Dispose ();

                }
                disposed = true;
            }
        }

        ~ContactsIsOnTracker ()
        {
            Dispose (false);
        }

        #endregion
    }
}
