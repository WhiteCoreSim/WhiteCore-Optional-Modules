using System;

namespace MetaBuilders.Irc.Contacts
{

    /// <summary>
    /// A contact list which tracks the online and offline status of the users within the Users collection property.
    /// </summary>
    /// <remarks>
    /// The ContactList will use Watch, Monitor, or IsOn, depending on server support. User status changes 
    /// will be updated via the User.OnlineStatus property.
    /// </remarks>
    public class ContactList : IDisposable
    {

        /// <summary>
        /// Gets the collection of users being tracked as a contact list.
        /// </summary>
        public UserCollection Users {
            get;
            private set;
        }

        /// <summary>
        /// The client on which the list is tracked.
        /// </summary>
        public Client Client {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the ContactList on the given client.
        /// </summary>
        /// <remarks>
        /// This method should not be called until the Client receives the ServerSupport is populated.
        /// An easy way to make sure is to wait until the Ready event of the Client.
        /// </remarks>
        public void Initialize(Client client)
        {
            if (client == null) {
                throw new ArgumentNullException(nameof(client));
            }
            ServerSupport support = client.ServerSupports;
            Client = client;
            Users = new UserCollection();

            if (support.MaxWatches > 0) {
                tracker = new ContactsWatchTracker(this);
            } else if (support.MaxMonitors > 0) {
                tracker = new ContactsMonitorTracker(this);
            } else {
                tracker = new ContactsIsOnTracker(this);
            }

            tracker.Initialize();
        }

        ContactsTracker tracker;


        #region IDisposable Members

        bool disposed;

        /// <summary>
        /// Implements IDisposable.Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implements IDisposable.Dispose
        /// </summary>
        void Dispose(bool disposing)
        {
            if (!disposed) {
                if (disposing) {
                    IDisposable disposableTracker = tracker as IDisposable;
                    if (disposableTracker != null) {
                        disposableTracker.Dispose();
                    }

                }
                disposed = true;
            }
        }

        /// <exclude />
        ~ContactList()
        {
            Dispose(false);
        }

        #endregion

    }
}
