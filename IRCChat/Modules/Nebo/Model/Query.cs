using System.ComponentModel;
using System.Collections.Specialized;

namespace MetaBuilders.Irc
{

    /// <summary>
    /// Represents a query window for private chat with one User
    /// </summary>
    public class Query : INotifyPropertyChanged
    {

        #region CTor

        /// <summary>
        /// Creates a new instance of the <see cref="Query"/> class on the given client with the given User.
        /// </summary>
        public Query (Client client, User user)
        {
            _client = client;
            journal.CollectionChanged += new NotifyCollectionChangedEventHandler (journal_CollectionChanged);
            User = user;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the User in the private chat.
        /// </summary>
        public User User {
            get {
                return _user;
            }
            private set {
                _user = value;
                NotifyPropertyChanged ("User");
            }
        }
        User _user;

        /// <summary>
        /// Gets the journal of messages on the query
        /// </summary>
        public virtual Journal Journal {
            get {
                return journal;
            }
        }
        Journal journal = new Journal ();

        /// <summary>
        /// Gets the client which the query is on.
        /// </summary>
        public virtual Client Client {
            get {
                return _client;
            }
        }
        Client _client;

        #endregion

        #region Event Handlers

        void journal_CollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged (new PropertyChangedEventArgs ("Journal"));
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised when a property value has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged (PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null) {
                PropertyChanged (this, e);
            }
        }

        void NotifyPropertyChanged (string propertyName)
        {
            OnPropertyChanged (new PropertyChangedEventArgs (propertyName));
        }

        #endregion
    }
}
