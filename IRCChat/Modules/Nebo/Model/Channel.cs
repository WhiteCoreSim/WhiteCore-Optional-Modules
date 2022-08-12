using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using MetaBuilders.Irc.Network;

namespace MetaBuilders.Irc
{

    /// <summary>
    /// Represents a single irc channel, with it's users.
    /// </summary>
    public class Channel : INotifyPropertyChanged
    {

        #region ctor

        /// <summary>
        /// Creates a new instance of the <see cref="Channel"/> class on the given client.
        /// </summary>
        public Channel(Client client)
        {
            _client = client;
            users.CollectionChanged += new NotifyCollectionChangedEventHandler(Users_CollectionChanged);
            Modes.CollectionChanged += new NotifyCollectionChangedEventHandler(Modes_CollectionChanged);
            journal.CollectionChanged += new NotifyCollectionChangedEventHandler(Journal_CollectionChanged);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Channel"/> class on the given client with the given name.
        /// </summary>
        public Channel(Client client, string name)
            : this(client)
        {
            Name = name;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of general properties assigned to this channel
        /// </summary>
        public virtual NameValueCollection Properties {
            get {
                return properties;
            }
        }

        /// <summary>
        /// Gets the client which the channel is on.
        /// </summary>
        public virtual Client Client {
            get {
                return _client;
            }
        }


        /// <summary>
        /// Gets or sets whether the channel is currently open
        /// </summary>
        public bool Open {
            get {
                return _open && (Client != null && Client.Connection.Status == ConnectionStatus.Connected);
            }
            internal set {
                _open = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Open"));
            }
        }
        bool _open = false;

        /// <summary>
        /// Gets or sets the name of the channel.
        /// </summary>
        public virtual string Name {
            get {
                return Properties["NAME"] ?? "";
            }
            set {
                string currentValue = Name;
                if (currentValue != value) {
                    Properties["NAME"] = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Name"));
                }
            }
        }

        /// <summary>
        /// Gets or sets the topic of the channel
        /// </summary>
        public virtual string Topic {
            get {
                return Properties["TOPIC"] ?? "";
            }
            set {
                string originalValue = Topic;
                if (originalValue != value) {
                    Properties["TOPIC"] = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Topic"));
                }
            }
        }

        /// <summary>
        /// Gets or sets the user which set the current topic
        /// </summary>
        public virtual User TopicSetter {
            get {
                return topicSetter;
            }
            set {
                if (topicSetter != value) {
                    topicSetter = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("TopicSetter"));
                }
            }
        }

        /// <summary>
        /// Gets or sets the time which topic was set
        /// </summary>
        public virtual DateTime TopicSetTime {
            get {
                return topicSetTime;
            }
            set {
                if (topicSetTime != value) {
                    topicSetTime = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("TopicSetTime"));
                }
            }
        }

        /// <summary>
        /// Gets the users in the channel.
        /// </summary>
        public virtual UserCollection Users {
            get {
                return users;
            }
        }

        /// <summary>
        /// Gets the modes in the channel.
        /// </summary>
        public virtual Messages.Modes.ChannelModeCollection Modes {
            get {
                return modes;
            }
        }

        /// <summary>
        /// Gets the journal of messages on the channel
        /// </summary>
        public virtual Journal Journal {
            get {
                return journal;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the status for the given <see cref="T:User"/> in the channel.
        /// </summary>
        public virtual ChannelStatus GetStatusForUser(User channelUser)
        {
            VerifyUserInChannel(channelUser);
            if (userModes.ContainsKey(channelUser)) {
                return userModes[channelUser];
            }
            return ChannelStatus.None;
        }

        /// <summary>
        /// Applies the given <see cref="T:ChannelStatus"/> to the given <see cref="T:User"/> in the channel.
        /// </summary>
        public virtual void SetStatusForUser(User channelUser, ChannelStatus status)
        {
            if (status == ChannelStatus.None && userModes.ContainsKey(channelUser)) {
                userModes.Remove(channelUser);
            } else {
                VerifyUserInChannel(channelUser);
                userModes[channelUser] = status;
            }
        }

        void VerifyUserInChannel(User channelUser)
        {
            if (channelUser == null) {
                throw new ArgumentNullException("channelUser");
            }
            if (!Users.Contains(channelUser)) {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, NeboResources.UserIsNotInChannel, channelUser.Nick, this.Name), "channelUser");
            }
        }

        #endregion

        #region Private

        void Users_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UserCollection __users = Users;

            if (e.Action == NotifyCollectionChangedAction.Remove) {
                userModes.RemoveAll(delegate (KeyValuePair<User, ChannelStatus> keyValue) {
                    return !__users.Contains(keyValue.Key);
                }
                );
            }
            OnPropertyChanged(new PropertyChangedEventArgs("Users"));
        }

        void Modes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(new PropertyChangedEventArgs("Modes"));
        }

        void Journal_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(new PropertyChangedEventArgs("Journal"));
        }


        Client _client;
        User topicSetter;
        DateTime topicSetTime;
        UserCollection users = new UserCollection();
        Messages.Modes.ChannelModeCollection modes = new Messages.Modes.ChannelModeCollection();
        Journal journal = new Journal();
        UserStatusMap userModes = new UserStatusMap();
        NameValueCollection properties = new NameValueCollection();

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// The event raised when a property on the object changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null) {
                PropertyChanged(this, e);
            }
        }

        #endregion

        #region UserStatusMap

        class UserStatusMap : Dictionary<User, ChannelStatus>
        {

            /// <summary>
            /// Removes all of the items from the dictionary which the given predictate matches.
            /// </summary>
            /// <returns>The number of items removed from the dictionary.</returns>
            public int RemoveAll(Predicate<System.Collections.Generic.KeyValuePair<User, ChannelStatus>> match)
            {
                if (match == null) {
                    throw new ArgumentNullException("match");
                }
                int countOfItemsRemoved = 0;

                User[] __users = new User[Keys.Count];
                Keys.CopyTo(__users, 0);
                foreach (User u in __users) {
                    if (ContainsKey(u) && match(new KeyValuePair<User, ChannelStatus>(u, this[u]))) {
                        Remove(u);
                        countOfItemsRemoved++;
                    }
                }

                return countOfItemsRemoved;

            }
        }

        #endregion

    }
}
