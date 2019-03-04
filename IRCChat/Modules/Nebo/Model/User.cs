using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace MetaBuilders.Irc
{

    /// <summary>
    /// Represents a User on an irc server.
    /// </summary>
    [Serializable]
    public sealed class User : INotifyPropertyChanged
    {

        #region CTor

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        public User ()
        {
            modes.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) {
                PropChanged ("Modes");
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class with the given mask string
        /// </summary>
        /// <param name="mask">The mask string to parse.</param>
        public User (string mask) : this ()
        {
            Parse (mask);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the nickname of the User
        /// </summary>
        public string Nick {
            get {
                return nick;
            }
            set {
                if (nick != value) {
                    nick = value;
                    PropChanged ("Nick");
                }
            }
        }
        string nick = "";

        /// <summary>
        /// Gets or sets the supposed real name of the User
        /// </summary>
        public string RealName {
            get {
                return realName;
            }
            set {
                if (realName != value) {
                    realName = value;
                    PropChanged ("RealName");
                }
            }
        }
        string realName = "";

        /// <summary>
        /// Gets or sets the Password the User will use on the server
        /// </summary>
        public string Password {
            get {
                return password;
            }
            set {
                if (password != value) {
                    password = value;
                    PropChanged ("Password");
                }
            }
        }
        string password = "";

        /// <summary>
        /// Gets or sets the username of the User on her local server.
        /// </summary>
        public string UserName {
            get {
                return username;
            }
            set {
                if (username != value) {
                    username = value;
                    PropChanged ("UserName");
                }
            }
        }
        string username = "";

        /// <summary>
        /// Gets or sets the hostname of the local machine of this User
        /// </summary>
        public string HostName {
            get {
                return hostname;
            }
            set {
                if (hostname != value) {
                    hostname = value;
                    PropChanged ("HostName");
                }
            }
        }
        string hostname = "";

        /// <summary>
        /// Gets or sets the online status of this User
        /// </summary>
        public UserOnlineStatus OnlineStatus {
            get {
                return onlineStatus;
            }
            set {
                if (onlineStatus != value) {
                    onlineStatus = value;
                    PropChanged ("OnlineStatus");
                }
            }
        }
        UserOnlineStatus onlineStatus;

        /// <summary>
        /// Gets or sets the away message of this User
        /// </summary>
        public string AwayMessage {
            get {
                return awayMessage;
            }
            set {
                if (awayMessage != value) {
                    awayMessage = value;
                    PropChanged ("AwayMessage");
                }
            }
        }
        string awayMessage;

        /// <summary>
        /// Gets or sets the name of the server which the User is connected to.
        /// </summary>
        public string ServerName {
            get {
                return serverName;
            }
            set {
                if (serverName != value) {
                    serverName = value;
                    PropChanged ("ServerName");
                }
            }
        }
        string serverName;

        /// <summary>
        /// Gets or sets if the User is an IRC Operator
        /// </summary>
        public bool IrcOperator {
            get {
                return ircOperator;
            }
            set {
                if (ircOperator != value) {
                    ircOperator = value;
                    PropChanged ("IrcOperator");
                }
            }
        }
        bool ircOperator;

        /// <summary>
        /// Gets the modes which apply to the user.
        /// </summary>
        public Messages.Modes.UserModeCollection Modes {
            get {
                return modes;
            }
        }
        Messages.Modes.UserModeCollection modes = new Messages.Modes.UserModeCollection ();

        #endregion

        #region Methods

        /// <summary>
        /// Represents this User's information as an irc mask
        /// </summary>
        /// <returns></returns>
        public override string ToString ()
        {
            StringBuilder result = new StringBuilder ();
            result.Append (Nick);
            if (!string.IsNullOrEmpty (UserName)) {
                result.Append ("!");
                result.Append (UserName);
            }
            if (!string.IsNullOrEmpty (HostName)) {
                result.Append ("@");
                result.Append (HostName);
            }

            return result.ToString ();
        }

        /// <summary>
        /// Represents this User's information with a guarenteed nick!user@host format.
        /// </summary>
        public string ToNickUserHostString ()
        {
            string finalNick = (string.IsNullOrEmpty (Nick)) ? "*" : Nick;
            string user = (string.IsNullOrEmpty (UserName)) ? "*" : UserName;
            string host = (string.IsNullOrEmpty (HostName)) ? "*" : HostName;

            return finalNick + "!" + user + "@" + host;
        }

        /// <summary>
        /// Determines wether the current user mask matches the given user mask.
        /// </summary>
        /// <param name="wildcardMask">The wild-card filled mask to compare with the current.</param>
        /// <returns>True if this mask is described by the given wildcard Mask. False if not.</returns>
        public bool IsMatch (User wildcardMask)
        {
            if (wildcardMask == null) {
                return false;
            }

            //Fist we'll return quickly if they are exact matches
            if (Nick == wildcardMask.Nick && UserName == wildcardMask.UserName && HostName == wildcardMask.HostName) {
                return true;
            }

            return (true
                && Regex.IsMatch (Nick, makeRegexPattern (wildcardMask.Nick), RegexOptions.IgnoreCase)
                && Regex.IsMatch (UserName, makeRegexPattern (wildcardMask.UserName), RegexOptions.IgnoreCase)
                && Regex.IsMatch (HostName, makeRegexPattern (wildcardMask.HostName), RegexOptions.IgnoreCase)
                );
        }

        /// <summary>
        /// Decides if the given user address matches the given address mask.
        /// </summary>
        /// <param name="actualMask">The user address mask to compare match.</param>
        /// <param name="wildcardMask">The address mask containing wildcards to match with.</param>
        /// <returns>True if <parmref>actualMask</parmref> is contained within ( or described with ) the <paramref>wildcardMask</paramref>. False if not.</returns>
        public static bool IsMatch (string actualMask, string wildcardMask)
        {
            return new User (actualMask).IsMatch (new User (wildcardMask));
        }

        /// <summary>
        /// Parses the given string as a mask to populate this user object.
        /// </summary>
        /// <param name="rawMask">The mask to parse.</param>
        public void Parse (string rawMask)
        {
            Reset ();

            string mask = rawMask;
            int indexOfBang = mask.IndexOf ("!", StringComparison.Ordinal);
            int indexOfAt = mask.LastIndexOf ("@", StringComparison.Ordinal);

            if (indexOfAt > 1) {
                HostName = mask.Substring (indexOfAt + 1);
                mask = mask.Substring (0, indexOfAt);
            }

            if (indexOfBang != -1) {
                UserName = mask.Substring (indexOfBang + 1);
                mask = mask.Substring (0, indexOfBang);
            }

            if (!string.IsNullOrEmpty (mask)) {
                string newNick = mask;
                string firstLetter = newNick.Substring (0, 1);
                if (ChannelStatus.Exists (firstLetter)) {
                    newNick = newNick.Substring (1);
                }
                Nick = newNick;
            }
        }

        /// <summary>
        /// Resets the User properties to the default values
        /// </summary>
        public void Reset ()
        {
            Nick = "";
            UserName = "";
            HostName = "";
            OnlineStatus = UserOnlineStatus.Online;
            AwayMessage = "";
            IrcOperator = false;
            Modes.Clear ();
            Password = "";
            RealName = "";
            ServerName = "";
            UserName = "";

            dirtyProperties.Clear ();
        }

        /// <summary>
        /// Merges the properties of the given User onto this User.
        /// </summary>
        public void MergeWith (User user)
        {
            if (user == null) {
                return;
            }
            if (user.IsDirty ("OnlineStatus") && !IsDirty ("OnlineStatus")) {
                OnlineStatus = user.OnlineStatus;
            }
            if (user.IsDirty ("AwayMessage") && !IsDirty ("AwayMessage")) {
                AwayMessage = user.AwayMessage;
            }
            if (user.IsDirty ("HostName") && !IsDirty ("HostName")) {
                HostName = user.HostName;
            }
            if (user.IsDirty ("Nick") && !IsDirty ("Nick")) {
                Nick = user.Nick;
            }
            if (user.IsDirty ("Password") && !IsDirty ("Password")) {
                Password = user.Password;
            }
            if (user.IsDirty ("RealName") && !IsDirty ("RealName")) {
                RealName = user.RealName;
            }
            if (user.IsDirty ("UserName") && !IsDirty ("UserName")) {
                UserName = user.UserName;
            }
            if (user.IsDirty ("ServerName") && !IsDirty ("ServerName")) {
                ServerName = user.ServerName;
            }
            if (user.IsDirty ("IrcOperator") && !IsDirty ("IrcOperator")) {
                IrcOperator = user.IrcOperator;
            }
        }

        /// <summary>
        /// Copies the properties of the given User onto this User.
        /// </summary>
        public void CopyFrom (User user)
        {
            if (user.IsDirty ("OnlineStatus")) {
                OnlineStatus = user.OnlineStatus;
            }
            if (user.IsDirty ("AwayMessage")) {
                AwayMessage = user.AwayMessage;
            }
            if (user.IsDirty ("HostName")) {
                HostName = user.HostName;
            }
            if (user.IsDirty ("Nick")) {
                Nick = user.Nick;
            }
            if (user.IsDirty ("Password")) {
                Password = user.Password;
            }
            if (user.IsDirty ("RealName")) {
                RealName = user.RealName;
            }
            if (user.IsDirty ("UserName")) {
                UserName = user.UserName;
            }
            if (user.IsDirty ("ServerName")) {
                ServerName = user.ServerName;
            }
            if (user.IsDirty ("IrcOperator")) {
                IrcOperator = user.IrcOperator;
            }
        }

        static string makeRegexPattern (string wildcardString)
        {
            return Regex.Escape (wildcardString).Replace (@"\*", @".*").Replace (@"\?", @".");
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

        #endregion

        void PropChanged (string propertyName)
        {
            if (!dirtyProperties.Contains (propertyName)) {
                dirtyProperties.Add (propertyName);
            }
            OnPropertyChanged (new PropertyChangedEventArgs (propertyName));
        }

        bool IsDirty (string propertyName)
        {
            return dirtyProperties.Contains (propertyName);
        }

        List<string> dirtyProperties = new List<string> ();

    }

}