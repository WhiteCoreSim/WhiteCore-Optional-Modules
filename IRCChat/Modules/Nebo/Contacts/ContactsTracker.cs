using System.Collections.Specialized;

namespace MetaBuilders.Irc.Contacts
{
    abstract class ContactsTracker
    {
        protected ContactsTracker (ContactList contacts)
        {
            this.contacts = contacts;
            this.contacts.Users.CollectionChanged += Users_CollectionChanged;
        }

        void Users_CollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add) {
                foreach (User newUser in e.NewItems) {
                    AddNick (newUser.Nick);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Remove) {
                foreach (User oldUser in e.OldItems) {
                    RemoveNick (oldUser.Nick);
                }
            }
        }

        ContactList contacts;

        protected ContactList Contacts {
            get {
                return contacts;
            }
        }

        public virtual void Initialize ()
        {
            StringCollection nicks = new StringCollection ();
            foreach (User u in Contacts.Users) {
                nicks.Add (u.Nick);
            }
            AddNicks (nicks);
        }

        protected abstract void AddNicks (StringCollection nicks);

        protected abstract void AddNick (string nick);

        protected abstract void RemoveNick (string nick);

    }
}
