using System.ComponentModel;
using System.Collections.Specialized;

namespace MetaBuilders.Irc
{

    /// <summary>
    /// Represents a status window for communication between the user and the server
    /// </summary>
    public class ServerQuery : INotifyPropertyChanged
    {

        #region CTor

        /// <summary>
        /// Creates a new instance of the <see cref="Query"/> class on the given client with the given User.
        /// </summary>
        public ServerQuery (Client client)
        {
            _client = client;
            journal.CollectionChanged += new NotifyCollectionChangedEventHandler (journal_CollectionChanged);
        }

        #endregion

        #region Properties

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
        /// Gets the client which the status is on.
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
        /// Raised when a property on the instance has changed.
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
