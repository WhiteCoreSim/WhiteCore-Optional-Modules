using System;
using MetaBuilders.Irc.Messages;
using MetaBuilders.Irc.Messages.Modes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MetaBuilders.Irc.Contacts;

namespace MetaBuilders.Irc
{

    /// <summary>
    /// Helps manage routine tasks parsing the messages from a server.
    /// </summary>
    [System.ComponentModel.DesignerCategory ("Code")]
    public class ClientManager : System.ComponentModel.Component
    {

        /// <summary>
        /// Creates a new instance of the <see cref="ClientManager"/> class.
        /// </summary>
        public ClientManager ()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ClientManager"/> class with the given client.
        /// </summary>
        /// <param name="client"></param>
        public ClientManager (Client client)
            : this ()
        {
            AddClient (client);
        }

        /// <summary>
        /// Adds the given <see cref="T:Client"/> to the <see cref="P:Clients"/> collection.
        /// </summary>
        public void AddClient (Client client)
        {
            if (client == null) {
                throw new ArgumentNullException ("client");
            }

            _clients.Add (client);
            ServerQueries [client] = new ServerQuery (client);
            Channels [client] = new ChannelCollection ();
            Users [client] = new UserCollection ();
            Contacts [client] = new ContactList ();
            Queries [client] = new QueryCollection ();
            AttachHandlers (client);
        }

        /// <summary>
        /// Removes the given <see cref="T:Client"/> from the <see cref="P:Clients"/> collection.
        /// </summary>
        public void RemoveClient (Client client)
        {
            if (client == null) {
                throw new ArgumentNullException ("client");
            }
            ServerQueries.Remove (client);
            Channels.Remove (client);
            Users.Remove (client);
            Contacts.Remove (client);
            Queries.Remove (client);
            DetachHandlers (client);
            _clients.Remove (client);
        }

        #region Properties

        /// <summary>
        /// Gets a read-only version of the <see cref="T:ClientCollection"/> used by this manager.
        /// </summary>
        /// <remarks>
        /// Too add or remove clients on this manager, use the <see cref="M:AddClient"/> and <see cref="M:RemoveClient"/> methods.
        /// </remarks>
        public virtual ReadOnlyObservableCollection<Client> Clients {
            get {
                if (_readOnlyClients == null) {
                    _readOnlyClients = new ReadOnlyObservableCollection<Client> (_clients);
                }
                return _readOnlyClients;
            }
        }
        ClientCollection _clients = new ClientCollection ();
        ReadOnlyObservableCollection<Client> _readOnlyClients;

        /// <summary>
        /// Gets the status for the server which the Client is connected to
        /// </summary>
        public virtual Dictionary<Client, ServerQuery> ServerQueries {
            get {
                return serverQueries;
            }
        }
        Dictionary<Client, ServerQuery> serverQueries = new Dictionary<Client, ServerQuery> ();

        /// <summary>
        /// Gets the users, per <see cref="T:Client"/>, which the user has seen in channels, or has otherwise had contact with.
        /// </summary>
        public virtual Dictionary<Client, UserCollection> Users {
            get {
                return users;
            }
        }
        Dictionary<Client, UserCollection> users = new Dictionary<Client, UserCollection> ();

        /// <summary>
        /// Gets the channels, per <see cref="T:Client"/>, which the user has joined.
        /// </summary>
        public virtual Dictionary<Client, ChannelCollection> Channels {
            get {
                return channels;
            }
        }
        Dictionary<Client, ChannelCollection> channels = new Dictionary<Client, ChannelCollection> ();

        /// <summary>
        /// Gets the queries, per <see cref="T:Client"/>, which the user is engaged in.
        /// </summary>
        public virtual Dictionary<Client, QueryCollection> Queries {
            get {
                return queries;
            }
        }
        Dictionary<Client, QueryCollection> queries = new Dictionary<Client, QueryCollection> ();

        /// <summary>
        /// Gets the ContactLists, per <see cref="T:Client"/>, which the user has created.
        /// </summary>
        public virtual Dictionary<Client, ContactList> Contacts {
            get {
                return contacts;
            }
        }
        Dictionary<Client, ContactList> contacts = new Dictionary<Client, ContactList> ();

        #endregion

        void AttachHandlers (Client client)
        {
            client.MessageParsed += new EventHandler<IrcMessageEventArgs<IrcMessage>> (client_MessageParsed);

            client.Messages.Join += new EventHandler<IrcMessageEventArgs<JoinMessage>> (routeJoins);
            client.Messages.Kick += new EventHandler<IrcMessageEventArgs<KickMessage>> (routeKicks);
            client.Messages.Kill += new EventHandler<IrcMessageEventArgs<KillMessage>> (routeKills);
            client.Messages.Part += new EventHandler<IrcMessageEventArgs<PartMessage>> (routeParts);
            client.Messages.Quit += new EventHandler<IrcMessageEventArgs<QuitMessage>> (routeQuits);

            client.Messages.TopicNoneReply += new EventHandler<IrcMessageEventArgs<TopicNoneReplyMessage>> (routeTopicNones);
            client.Messages.TopicReply += new EventHandler<IrcMessageEventArgs<TopicReplyMessage>> (routeTopics);
            client.Messages.TopicSetReply += new EventHandler<IrcMessageEventArgs<TopicSetReplyMessage>> (routeTopicSets);
            client.Messages.ChannelModeIsReply += new EventHandler<IrcMessageEventArgs<ChannelModeIsReplyMessage>> (client_ChannelModeIsReply);
            client.Messages.ChannelProperty += new EventHandler<IrcMessageEventArgs<ChannelPropertyMessage>> (client_ChannelProperty);
            client.Messages.ChannelPropertyReply += new EventHandler<IrcMessageEventArgs<ChannelPropertyReplyMessage>> (client_ChannelPropertyReply);

            client.Messages.NamesReply += new EventHandler<IrcMessageEventArgs<NamesReplyMessage>> (routeNames);
            client.Messages.NickChange += new EventHandler<IrcMessageEventArgs<NickChangeMessage>> (routeNicks);
            client.Messages.WhoReply += new EventHandler<IrcMessageEventArgs<WhoReplyMessage>> (routeWhoReplies);
            client.Messages.WhoIsOperReply += new EventHandler<IrcMessageEventArgs<WhoIsOperReplyMessage>> (client_WhoIsOperReply);
            client.Messages.WhoIsServerReply += new EventHandler<IrcMessageEventArgs<WhoIsServerReplyMessage>> (client_WhoIsServerReply);
            client.Messages.WhoIsUserReply += new EventHandler<IrcMessageEventArgs<WhoIsUserReplyMessage>> (client_WhoIsUserReply);
            client.Messages.UserHostReply += new EventHandler<IrcMessageEventArgs<UserHostReplyMessage>> (client_UserHostReply);
            client.Messages.OperReply += new EventHandler<IrcMessageEventArgs<OperReplyMessage>> (client_OperReply);
            client.Messages.UserMode += new EventHandler<IrcMessageEventArgs<UserModeMessage>> (client_UserMode);
            client.Messages.UserModeIsReply += new EventHandler<IrcMessageEventArgs<UserModeIsReplyMessage>> (client_UserModeIsReply);

            client.Messages.Away += new EventHandler<IrcMessageEventArgs<AwayMessage>> (client_Away);
            client.Messages.Back += new EventHandler<IrcMessageEventArgs<BackMessage>> (client_Back);
            client.Messages.SelfAway += new EventHandler<IrcMessageEventArgs<SelfAwayMessage>> (client_SelfAway);
            client.Messages.SelfUnAway += new EventHandler<IrcMessageEventArgs<SelfUnAwayMessage>> (client_SelfUnAway);
            client.Messages.UserAway += new EventHandler<IrcMessageEventArgs<UserAwayMessage>> (client_UserAway);

            client.Messages.NoSuchChannel += new EventHandler<IrcMessageEventArgs<NoSuchChannelMessage>> (client_NoSuchChannel);
            client.Messages.NoSuchNick += new EventHandler<IrcMessageEventArgs<NoSuchNickMessage>> (client_NoSuchNick);
        }

        void DetachHandlers (Client client)
        {

            client.MessageParsed -= client_MessageParsed;

            client.Messages.Join -= new EventHandler<IrcMessageEventArgs<JoinMessage>> (routeJoins);
            client.Messages.Kick -= new EventHandler<IrcMessageEventArgs<KickMessage>> (routeKicks);
            client.Messages.Kill -= new EventHandler<IrcMessageEventArgs<KillMessage>> (routeKills);
            client.Messages.Part -= new EventHandler<IrcMessageEventArgs<PartMessage>> (routeParts);
            client.Messages.Quit -= new EventHandler<IrcMessageEventArgs<QuitMessage>> (routeQuits);

            client.Messages.TopicNoneReply -= new EventHandler<IrcMessageEventArgs<TopicNoneReplyMessage>> (routeTopicNones);
            client.Messages.TopicReply -= new EventHandler<IrcMessageEventArgs<TopicReplyMessage>> (routeTopics);
            client.Messages.TopicSetReply -= new EventHandler<IrcMessageEventArgs<TopicSetReplyMessage>> (routeTopicSets);
            client.Messages.ChannelModeIsReply -= client_ChannelModeIsReply;
            client.Messages.ChannelProperty -= client_ChannelProperty;
            client.Messages.ChannelPropertyReply -= client_ChannelPropertyReply;

            client.Messages.NamesReply -= new EventHandler<IrcMessageEventArgs<NamesReplyMessage>> (routeNames);
            client.Messages.NickChange -= new EventHandler<IrcMessageEventArgs<NickChangeMessage>> (routeNicks);
            client.Messages.WhoReply -= new EventHandler<IrcMessageEventArgs<WhoReplyMessage>> (routeWhoReplies);
            client.Messages.WhoIsOperReply -= client_WhoIsOperReply;
            client.Messages.WhoIsServerReply -= client_WhoIsServerReply;
            client.Messages.WhoIsUserReply -= client_WhoIsUserReply;
            client.Messages.UserHostReply -= client_UserHostReply;
            client.Messages.OperReply -= client_OperReply;
            client.Messages.UserMode -= client_UserMode;
            client.Messages.UserModeIsReply -= client_UserModeIsReply;

            client.Messages.Away -= new EventHandler<IrcMessageEventArgs<AwayMessage>> (client_Away);
            client.Messages.Back -= new EventHandler<IrcMessageEventArgs<BackMessage>> (client_Back);
            client.Messages.SelfAway -= new EventHandler<IrcMessageEventArgs<SelfAwayMessage>> (client_SelfAway);
            client.Messages.SelfUnAway -= new EventHandler<IrcMessageEventArgs<SelfUnAwayMessage>> (client_SelfUnAway);
            client.Messages.UserAway -= new EventHandler<IrcMessageEventArgs<UserAwayMessage>> (client_UserAway);

            client.Messages.NoSuchChannel -= client_NoSuchChannel;
            client.Messages.NoSuchNick -= client_NoSuchNick;
        }

        #region Message Handlers

        void client_MessageParsed (object sender, IrcMessageEventArgs<IrcMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            bool routed = false;

            IrcMessage ircMessage = e.Message;
            if (ircMessage is IChannelTargetedMessage || ircMessage is IQueryTargetedMessage) {

                IQueryTargetedMessage queryMessage = ircMessage as IQueryTargetedMessage;
                if (queryMessage != null) {
                    if (queryMessage.IsQueryToUser (source.User)) {
                        User msgSender = Users [source].EnsureUser (ircMessage.Sender);
                        Query qry = Queries [source].EnsureQuery (msgSender, source);
                        qry.Journal.Add (new JournalEntry (ircMessage));
                        routed = true;
                    }
                }

                IChannelTargetedMessage channelMessage = ircMessage as IChannelTargetedMessage;
                if (channelMessage != null) {
                    foreach (Channel channel in Channels [source]) {
                        if (channelMessage.IsTargetedAtChannel (channel.Name)) {
                            channel.Journal.Add (new JournalEntry (ircMessage));
                            routed = true;
                        }
                    }
                }

            }

            if (!routed) {
                ServerQueries [source].Journal.Add (new JournalEntry (ircMessage));
            }

        }

        void routeJoins (object sender, IrcMessageEventArgs<JoinMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            User msgUser = e.Message.Sender;
            User joinedUser = (IsMe (msgUser.Nick, source)) ? source.User : Users [source].EnsureUser (msgUser);

            foreach (string channelname in e.Message.Channels) {
                Channel joinedChannel = Channels [source].EnsureChannel (channelname, source);
                joinedChannel.Open = true;
                joinedChannel.Users.Add (joinedUser);
            }
        }

        void routeKicks (object sender, IrcMessageEventArgs<KickMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            for (int i = 0; i < e.Message.Channels.Count; i++) {
                string channelName = e.Message.Channels [i];
                string nick = e.Message.Nicks [i];
                Channel channel = Channels [source].FindChannel (channelName);
                if (channel != null) {
                    if (IsMe (nick, source)) {
                        // we don't want to actually remove the channel, but just close the channel
                        // this allows a consumer to easily keep their reference to channels between kicks and re-joins.
                        //Channels[source].Remove( channel );
                        channel.Open = false;
                    } else {
                        channel.Users.RemoveFirst (nick);
                    }
                }
            }
        }

        void routeKills (object sender, IrcMessageEventArgs<KillMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            string nick = e.Message.Nick;
            if (IsMe (nick, source)) {
                foreach (Channel c in Channels [source]) {
                    c.Open = false;
                }
            } else {
                foreach (Channel channel in Channels [source]) {
                    channel.Users.RemoveFirst (nick);
                }
            }
        }

        void routeNames (object sender, IrcMessageEventArgs<NamesReplyMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            Channel channel = Channels [source].EnsureChannel (e.Message.Channel, source);
            foreach (string nick in e.Message.Nicks.Keys) {
                User user = Users [source].EnsureUser (nick);
                if (!channel.Users.Contains (user)) {
                    channel.Users.Add (user);
                }
                ChannelStatus status = e.Message.Nicks [nick];
                channel.SetStatusForUser (user, status);
            }
        }

        void routeNicks (object sender, IrcMessageEventArgs<NickChangeMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            string oldNick = e.Message.Sender.Nick;
            string newNick = e.Message.NewNick;
            User u = Users [source].Find (oldNick);
            if (u != null) {
                u.Nick = newNick;
            }
        }

        void routeParts (object sender, IrcMessageEventArgs<PartMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            string nick = e.Message.Sender.Nick;
            foreach (string channelName in e.Message.Channels) {
                Channel channel = Channels [source].FindChannel (channelName);
                if (channel != null) {
                    if (IsMe (nick, source))
                        channel.Open = false;
                    else
                        channel.Users.RemoveFirst (nick);
                }
            }
        }

        void routeQuits (object sender, IrcMessageEventArgs<QuitMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            string nick = e.Message.Sender.Nick;
            if (IsMe (nick, source)) {
                foreach (Channel c in Channels [source]) {
                    c.Open = false;
                }
            } else {
                foreach (Channel channel in Channels [source]) {
                    channel.Users.RemoveFirst (nick);
                }
            }
        }

        void routeTopicNones (object sender, IrcMessageEventArgs<TopicNoneReplyMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            Channel channel = Channels [source].FindChannel (e.Message.Channel);
            if (channel != null) {
                channel.Topic = "";
            }
        }

        void routeTopics (object sender, IrcMessageEventArgs<TopicReplyMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            Channel channel = Channels [source].FindChannel (e.Message.Channel);
            if (channel != null) {
                channel.Topic = e.Message.Topic;
            }
        }

        void routeTopicSets (object sender, IrcMessageEventArgs<TopicSetReplyMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            Channel channel = Channels [source].FindChannel (e.Message.Channel);
            if (channel != null) {
                User topicSetter = Users [source].EnsureUser (e.Message.User);
                channel.TopicSetter = topicSetter;
                channel.TopicSetTime = e.Message.TimeSet;
            }
        }

        void routeWhoReplies (object sender, IrcMessageEventArgs<WhoReplyMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            User whoUser = Users [source].EnsureUser (e.Message.User);
            string channelName = e.Message.Channel;

            Channel channel = Channels [source].FindChannel (channelName);
            if (channel != null) {
                if (!channel.Users.Contains (whoUser)) {
                    channel.Users.Add (whoUser);
                }
                channel.SetStatusForUser (whoUser, e.Message.Status);
            }
        }

        void client_NoSuchChannel (object sender, IrcMessageEventArgs<NoSuchChannelMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            Channel channel = Channels [source].FindChannel (e.Message.Channel);
            if (channel != null) {
                Channels [source].Remove (channel);
            }
        }

        void client_NoSuchNick (object sender, IrcMessageEventArgs<NoSuchNickMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            string nick = e.Message.Nick;

            if (MessageUtil.HasValidChannelPrefix (nick)) { // NoSuchNickMessage is sent by some servers instead of a NoSuchChannelMessage
                Channel channel = Channels [source].FindChannel (e.Message.Nick);
                if (channel != null) {
                    Channels [source].Remove (channel);
                }
            } else {
                Users [source].RemoveFirst (nick);
                foreach (Channel channel in Channels [source]) {
                    channel.Users.RemoveFirst (nick);
                }
            }
        }

        void client_ChannelModeIsReply (object sender, IrcMessageEventArgs<ChannelModeIsReplyMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            Channel channel = Channels [source].FindChannel (e.Message.Channel);
            if (channel != null) {
                ChannelModesCreator modes = new ChannelModesCreator ();
                modes.ServerSupport = source.ServerSupports;
                modes.Parse (e.Message.Modes, e.Message.ModeArguments);
                channel.Modes.ResetWith (modes.Modes);
            }

        }

        void client_UserModeIsReply (object sender, IrcMessageEventArgs<UserModeIsReplyMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            if (IsMe (e.Message.Target, source)) {
                UserModesCreator modeCreator = new UserModesCreator ();
                modeCreator.Parse (e.Message.Modes);
                source.User.Modes.Clear ();
                foreach (UserMode mode in modeCreator.Modes) {
                    source.User.Modes.Add (mode);
                }
            }
        }

        void client_UserMode (object sender, IrcMessageEventArgs<UserModeMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            if (IsMe (e.Message.User, source)) {
                UserModesCreator modeCreator = new UserModesCreator ();
                modeCreator.Parse (e.Message.ModeChanges);
                source.User.Modes.Clear ();
                foreach (UserMode mode in modeCreator.Modes) {
                    source.User.Modes.Add (mode);
                }
            }
        }

        void client_UserHostReply (object sender, IrcMessageEventArgs<UserHostReplyMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            foreach (User sentUser in e.Message.Users) {
                User user = Users [source].EnsureUser (sentUser);
                if (user != sentUser) {
                    user.CopyFrom (sentUser);
                }
                if (user.OnlineStatus != UserOnlineStatus.Away) {
                    user.AwayMessage = "";
                }
            }
        }

        void client_UserAway (object sender, IrcMessageEventArgs<UserAwayMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            User user = Users [source].EnsureUser (e.Message.Nick);
            user.OnlineStatus = UserOnlineStatus.Away;
            user.AwayMessage = e.Message.Text;
        }

        void client_SelfUnAway (object sender, IrcMessageEventArgs<SelfUnAwayMessage> e)
        {
            User self = ((Client)sender).User;
            self.OnlineStatus = UserOnlineStatus.Online;
            self.AwayMessage = "";
        }

        void client_SelfAway (object sender, IrcMessageEventArgs<SelfAwayMessage> e)
        {
            User self = ((Client)sender).User;
            self.OnlineStatus = UserOnlineStatus.Away;
        }

        void client_OperReply (object sender, IrcMessageEventArgs<OperReplyMessage> e)
        {
            ((Client)sender).User.IrcOperator = true;
        }

        void client_ChannelProperty (object sender, IrcMessageEventArgs<ChannelPropertyMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            Channel channel = Channels [source].EnsureChannel (e.Message.Channel, source);
            channel.Properties [e.Message.Prop] = e.Message.NewValue;
        }

        void client_ChannelPropertyReply (object sender, IrcMessageEventArgs<ChannelPropertyReplyMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            Channel channel = Channels [source].EnsureChannel (e.Message.Channel, source);
            channel.Properties [e.Message.Prop] = e.Message.Value;
        }

        void client_Back (object sender, IrcMessageEventArgs<BackMessage> e)
        {
            User user = Users [(Client)sender].EnsureUser (e.Message.Sender);
            user.OnlineStatus = UserOnlineStatus.Online;
            user.AwayMessage = "";
        }

        void client_Away (object sender, IrcMessageEventArgs<AwayMessage> e)
        {
            User user = Users [(Client)sender].EnsureUser (e.Message.Sender);
            user.OnlineStatus = UserOnlineStatus.Away;
            user.AwayMessage = e.Message.Reason;
        }

        void client_WhoIsUserReply (object sender, IrcMessageEventArgs<WhoIsUserReplyMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            Users [source].EnsureUser (e.Message.User);
        }

        void client_WhoIsServerReply (object sender, IrcMessageEventArgs<WhoIsServerReplyMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }
            User user = Users [source].EnsureUser (e.Message.Nick);
            user.ServerName = e.Message.ServerName;
        }

        void client_WhoIsOperReply (object sender, IrcMessageEventArgs<WhoIsOperReplyMessage> e)
        {
            Client source = sender as Client;
            if (source == null) {
                return;
            }

            User user = Users [source].EnsureUser (e.Message.Nick);
            user.IrcOperator = true;
        }

        #endregion

        static bool IsMe (string nick, Client source)
        {
            return (MessageUtil.IsIgnoreCaseMatch (source.User.Nick, nick));
        }

    }
}
