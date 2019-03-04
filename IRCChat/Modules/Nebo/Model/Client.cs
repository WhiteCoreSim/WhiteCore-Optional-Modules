using System;
using System.Globalization;
using System.IO;
using MetaBuilders.Irc.Messages;
using MetaBuilders.Irc.Network;
using MetaBuilders.Irc.Messages.Modes;

namespace MetaBuilders.Irc
{

    /// <summary>
    /// Represents an irc client. it has a connection, a user, etc
    /// </summary>
    /// <remarks>A gui frontend should use one instance of these per client/server <see cref="ClientConnection"/> it wants to make.</remarks>
    [System.ComponentModel.DesignerCategory ("Code")]
    public class Client : IDisposable
    {

        #region Constructors

        /// <summary>Initializes a new instance of the <see cref="Client"/> class.</summary>
        public Client ()
        {
            DefaultQuitMessage = "Quiting";
            EnableAutoIdent = true;
            ServerName = "";
            ServerSupports = new ServerSupport ();

            Messages = new MessageConduit ();
            User = new User ();
            Connection = new ClientConnection ();

            ServerQuery = new ServerQuery (this);
            Channels = new ChannelCollection ();
            Queries = new QueryCollection ();
            Peers = new UserCollection ();
            Contacts = new Contacts.ContactList ();

            Peers.Add (User);

            HookupEvents ();


        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> with the given address.
        /// </summary>
        /// <param name="address">The address that will be connected to.</param>
        public Client (string address)
            : this ()
        {
            Connection.Address = address;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> with the given address.
        /// </summary>
        /// <param name="address">The address that will be connected to.</param>
        /// <param name="nick">The nick of the <see cref="Client.User"/></param>
        public Client (string address, string nick)
            : this (address)
        {
            User.Nick = nick;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> with the given address.
        /// </summary>
        /// <param name="address">The address that will be connected to.</param>
        /// <param name="nick">The <see cref="MetaBuilders.Irc.User.Nick"/> of the <see cref="Client.User"/></param>
        /// <param name="realName">The <see cref="MetaBuilders.Irc.User.RealName"/> of the <see cref="Client.User"/></param>
        public Client (string address, string nick, string realName)
            : this (address, nick)
        {
            User.RealName = realName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the conduit thru which individual message received events can be attached.
        /// </summary>
        public MessageConduit Messages {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the default quit message if the client 
        /// has to close the connection itself.
        /// </summary>
        public string DefaultQuitMessage {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the <see cref="Client"/> will automaticly start and stop
        /// an <see cref="Ident"/> service as needed to connect to the irc server.
        /// </summary>
        public bool EnableAutoIdent {
            get;
            set;
        }

        /// <summary>
        /// Gets the <see cref="ClientConnection"/> of the current <see cref="Client"/>.
        /// </summary>
        public ClientConnection Connection {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the <see cref="User"/> of the current <see cref="Client"/>.
        /// </summary>
        public User User {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the server that you are connected to.
        /// </summary>
        /// <remarks>
        /// This is the name that server referes to itself by in messages, not neccesarily the name you use to connect.
        /// </remarks>
        public string ServerName {
            get;
            private set;
        }

        /// <summary>
        /// Gets a <see cref="ServerSupport"/> object containing knowledge about what the current server supports.
        /// </summary>
        public ServerSupport ServerSupports {
            get;
            private set;
        }

        /// <summary>
        /// Gets the query window to the server to which this client is connected
        /// </summary>
        public ServerQuery ServerQuery {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the collection of channels which the user has joined
        /// </summary>
        public ChannelCollection Channels {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the collection of queries the user is enganged in
        /// </summary>
        public QueryCollection Queries {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the collection users which the user has seen
        /// </summary>
        public UserCollection Peers {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the <see cref="MetaBuilders.Irc.Contacts.ContactList" /> for this client.
        /// </summary>
        public Contacts.ContactList Contacts {
            get;
            protected set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends a <see cref="IrcMessage"/> over a <see cref="ClientConnection"/> to an irc server.
        /// </summary>
        /// <param name="message">The <see cref="IrcMessage"/> to send.</param>
        public virtual void Send (IrcMessage message)
        {
            if (message == null) {
                return;
            }

            CancelIrcMessageEventArgs<IrcMessage> e = new CancelIrcMessageEventArgs<IrcMessage> (message);
            OnMessageSending (e);
            if (e.Cancel) {
                return;
            }

            TextWriter originalWriter = writer.InnerWriter;
            try {
                using (StringWriter myInnerWriter = new StringWriter (CultureInfo.InvariantCulture)) {
                    writer.InnerWriter = myInnerWriter;

                    message.Validate (ServerSupports);
                    message.Format (writer);
                    Connection.Write (myInnerWriter.ToString ());


                    writer.InnerWriter = originalWriter;
                }
            } catch {
                writer.InnerWriter = originalWriter;
            }

        }

        #region Send Helpers

        /// <summary>
        /// Sends a <see cref="ChatMessage"/> with the given text to the given channel or user.
        /// </summary>
        /// <param name="text">The text of the message.</param>
        /// <param name="target">The target of the message, either a channel or nick.</param>
        public virtual void SendChat (string text, string target)
        {
            Send (new ChatMessage (text, target));
        }

        /// <summary>
        /// Sends a <see cref="ActionRequestMessage"/> with the given text to the given  channel or user.
        /// </summary>
        /// <param name="text">The text of the action.</param>
        /// <param name="target">The target of the message, either a channel or nick.</param>
        public virtual void SendAction (string text, string target)
        {
            Send (new ActionRequestMessage (text, target));
        }

        /// <summary>
        /// Sends a <see cref="JoinMessage"/> for the given channel.
        /// </summary>
        /// <param name="channel">The channel to join.</param>
        public virtual void SendJoin (string channel)
        {
            Send (new JoinMessage (channel));
        }

        /// <summary>
        /// Sends a <see cref="PartMessage"/> for the given channel. 
        /// </summary>
        /// <param name="channel">The channel to part.</param>
        public virtual void SendPart (string channel)
        {
            Send (new PartMessage (channel));
        }

        /// <summary>
        /// Sends an <see cref="AwayMessage"/> with the given reason.
        /// </summary>
        /// <param name="reason">The reason for being away.</param>
        public virtual void SendAway (string reason)
        {
            Send (new AwayMessage (reason));
        }

        /// <summary>
        /// Sends a <see cref="BackMessage"/>.
        /// </summary>
        public virtual void SendBack ()
        {
            Send (new BackMessage ());
        }

        /// <summary>
        /// Sends a <see cref="QuitMessage"/>.
        /// </summary>
        public virtual void SendQuit ()
        {
            SendQuit (DefaultQuitMessage);
        }

        /// <summary>
        /// Sends a <see cref="QuitMessage"/> with the given reason.
        /// </summary>
        /// <param name="reason">The reason for quitting.</param>
        public virtual void SendQuit (string reason)
        {
            Send (new QuitMessage (reason));
        }

        #endregion

        /// <summary>
        /// Determines if the given message originated from the currently connected server.
        /// </summary>
        public virtual bool IsMessageFromServer (IrcMessage msg)
        {
            if (msg == null) {
                return false;
            }
            return (msg.Sender.Nick == ServerName);
        }

        bool IsMe (string nick)
        {
            return (MessageUtil.IsIgnoreCaseMatch (User.Nick, nick));
        }

        void RouteData (string messageData)
        {
            IrcMessage msg = null;
            try {
                msg = MessageParserService.Service.Parse (messageData);
            } catch (InvalidMessageException ex) {
                // Try one more time to load it as a generic message
                msg = new GenericMessage ();
                if (msg.CanParse (messageData)) {
                    msg.Parse (messageData);
                } else {
                    msg = null;
                    System.Diagnostics.Trace.WriteLine (ex.Message + " { " + ex.ReceivedMessage + " } ", "Invalid Message");
                }
            }

            if (msg != null) {
                OnMessageParsed (new IrcMessageEventArgs<IrcMessage> (msg));

                msg.Notify (Messages);
            }
        }

        void HookupEvents ()
        {

            Connection.Connected += new EventHandler (sendClientConnectionMessage);
            Connection.DataReceived += new EventHandler<ConnectionDataEventArgs> (dataReceived);
            Connection.DataSent += new EventHandler<ConnectionDataEventArgs> (dataSent);
            Connection.Disconnected += new EventHandler<ConnectionDataEventArgs> (ensureIdentEnds);


            Messages.Welcome += new EventHandler<IrcMessageEventArgs<WelcomeMessage>> (serverNameAquired);
            Messages.Ping += new EventHandler<IrcMessageEventArgs<PingMessage>> (autoPingPong);
            Messages.NickChange += new EventHandler<IrcMessageEventArgs<NickChangeMessage>> (keepOwnNickCorrect);
            Messages.Support += new EventHandler<IrcMessageEventArgs<SupportMessage>> (populateSupports);

            MessageParsed += new EventHandler<IrcMessageEventArgs<IrcMessage>> (messageParsed);

            Messages.Join += new EventHandler<IrcMessageEventArgs<JoinMessage>> (routeJoins);
            Messages.Kick += new EventHandler<IrcMessageEventArgs<KickMessage>> (routeKicks);
            Messages.Kill += new EventHandler<IrcMessageEventArgs<KillMessage>> (routeKills);
            Messages.Part += new EventHandler<IrcMessageEventArgs<PartMessage>> (routeParts);
            Messages.Quit += new EventHandler<IrcMessageEventArgs<QuitMessage>> (routeQuits);

            Messages.TopicNoneReply += new EventHandler<IrcMessageEventArgs<TopicNoneReplyMessage>> (routeTopicNones);
            Messages.TopicReply += new EventHandler<IrcMessageEventArgs<TopicReplyMessage>> (routeTopics);
            Messages.TopicSetReply += new EventHandler<IrcMessageEventArgs<TopicSetReplyMessage>> (routeTopicSets);
            Messages.ChannelModeIsReply += new EventHandler<IrcMessageEventArgs<ChannelModeIsReplyMessage>> (client_ChannelModeIsReply);
            Messages.ChannelProperty += new EventHandler<IrcMessageEventArgs<ChannelPropertyMessage>> (client_ChannelProperty);
            Messages.ChannelPropertyReply += new EventHandler<IrcMessageEventArgs<ChannelPropertyReplyMessage>> (client_ChannelPropertyReply);

            Messages.NamesReply += new EventHandler<IrcMessageEventArgs<NamesReplyMessage>> (routeNames);
            Messages.NickChange += new EventHandler<IrcMessageEventArgs<NickChangeMessage>> (routeNicks);
            Messages.WhoReply += new EventHandler<IrcMessageEventArgs<WhoReplyMessage>> (routeWhoReplies);
            Messages.WhoIsOperReply += new EventHandler<IrcMessageEventArgs<WhoIsOperReplyMessage>> (client_WhoIsOperReply);
            Messages.WhoIsServerReply += new EventHandler<IrcMessageEventArgs<WhoIsServerReplyMessage>> (client_WhoIsServerReply);
            Messages.WhoIsUserReply += new EventHandler<IrcMessageEventArgs<WhoIsUserReplyMessage>> (client_WhoIsUserReply);
            Messages.UserHostReply += new EventHandler<IrcMessageEventArgs<UserHostReplyMessage>> (client_UserHostReply);
            Messages.OperReply += new EventHandler<IrcMessageEventArgs<OperReplyMessage>> (client_OperReply);
            Messages.UserMode += new EventHandler<IrcMessageEventArgs<UserModeMessage>> (client_UserMode);
            Messages.UserModeIsReply += new EventHandler<IrcMessageEventArgs<UserModeIsReplyMessage>> (client_UserModeIsReply);

            Messages.Away += new EventHandler<IrcMessageEventArgs<AwayMessage>> (client_Away);
            Messages.Back += new EventHandler<IrcMessageEventArgs<BackMessage>> (client_Back);
            Messages.SelfAway += new EventHandler<IrcMessageEventArgs<SelfAwayMessage>> (client_SelfAway);
            Messages.SelfUnAway += new EventHandler<IrcMessageEventArgs<SelfUnAwayMessage>> (client_SelfUnAway);
            Messages.UserAway += new EventHandler<IrcMessageEventArgs<UserAwayMessage>> (client_UserAway);

            Messages.NoSuchChannel += new EventHandler<IrcMessageEventArgs<NoSuchChannelMessage>> (client_NoSuchChannel);
            Messages.NoSuchNick += new EventHandler<IrcMessageEventArgs<NoSuchNickMessage>> (client_NoSuchNick);




            Messages.GenericCtcpRequest += new EventHandler<IrcMessageEventArgs<GenericCtcpRequestMessage>> (logGenericCtcp);
            Messages.GenericMessage += new EventHandler<IrcMessageEventArgs<GenericMessage>> (logGenericMessage);
            Messages.GenericNumericMessage += new EventHandler<IrcMessageEventArgs<GenericNumericMessage>> (logGenericNumeric);
            Messages.GenericErrorMessage += new EventHandler<IrcMessageEventArgs<GenericErrorMessage>> (logGenericError);

        }

        #region Dispose

        /// <summary>
        /// Releases the disposable resources used
        /// </summary>
        protected virtual void Dispose (bool disposing)
        {
            if (disposing) {
                if (Connection != null) {
                    ((IDisposable)Connection).Dispose ();
                }
                if (writer != null) {
                    ((IDisposable)writer).Dispose ();
                }
            }
        }

        void IDisposable.Dispose ()
        {
            Dispose (true);
        }

        /// <summary>
        /// Client destructor
        /// </summary>
        ~Client ()
        {
            Dispose (false);
        }

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the <see cref="ClientConnection"/> recieves data.
        /// </summary>
        public event EventHandler<ConnectionDataEventArgs> DataReceived;

        /// <summary>
        /// Occurs when the <see cref="ClientConnection"/> sends data.
        /// </summary>
        public event EventHandler<ConnectionDataEventArgs> DataSent;

        /// <summary>
        /// Occurs when any message is received and parsed.
        /// </summary>
        public event EventHandler<IrcMessageEventArgs<IrcMessage>> MessageParsed;
        /// <summary>Raises the MessageParsed event.</summary>
        protected void OnMessageParsed (IrcMessageEventArgs<IrcMessage> e)
        {
            if (MessageParsed != null) {
                MessageParsed (this, e);
            }
        }

        /// <summary>
        /// Occurs when the connection is opened and the server has sent a welcome message.
        /// </summary>
        /// <remarks>
        /// This is the earliest the messages can be sent over the irc nextwork
        /// </remarks>
        public event EventHandler Ready;

        /// <summary>
        /// Occurs when any message is about to be sent.
        /// </summary>
        public event EventHandler<CancelIrcMessageEventArgs<IrcMessage>> MessageSending;
        /// <summary>
        /// Raises the MessageSending event.
        /// </summary>
        /// <param name="e"></param>
        protected void OnMessageSending (CancelIrcMessageEventArgs<IrcMessage> e)
        {
            if (MessageSending != null) {
                MessageSending (this, e);
            }
        }


        #endregion

        #region EventHandlers

        /// <summary>
        /// Transforms <see cref="ClientConnection"/> data into raised <see cref="IrcMessage"/> events
        /// </summary>
        void dataReceived (object sender, ConnectionDataEventArgs e)
        {
            if (DataReceived != null) {
                DataReceived (this, e);
            }

            RouteData (e.Data);

        }

        void dataSent (object sender, ConnectionDataEventArgs e)
        {
            if (DataSent != null) {
                DataSent (this, e);
            }
        }

        /// <summary>
        /// Keeps an irc connection alive.
        /// </summary>
        /// <remarks>
        /// An irc server will ping you every x seconds to make sure you are still alive.
        /// This method will auto-pong a return to keep the <see cref="ClientConnection"/> alive automagically.
        /// </remarks>
        /// <param name="sender">the connection object sending the ping</param>
        /// <param name="e">the message sent</param>
        void autoPingPong (object sender, IrcMessageEventArgs<PingMessage> e)
        {
            PongMessage pong = new PongMessage ();
            pong.Target = e.Message.Target;
            Send (pong);
        }

        void sendClientConnectionMessage (object sender, EventArgs e)
        {
            StartIdent ();
            readyRaised = false;

            //Send Password
            if (User.Password != null && User.Password.Length != 0) {
                PasswordMessage pass = new PasswordMessage ();
                pass.Password = User.Password;
                Send (pass);
            }

            //Send Nick
            NickChangeMessage nick = new NickChangeMessage ();
            nick.NewNick = User.Nick;
            Send (nick);

            //Send User
            UserNotificationMessage userNotification = new UserNotificationMessage ();
            if (User.RealName.Length == 0) {
                userNotification.RealName = User.Nick;
            } else {
                userNotification.RealName = User.RealName;
            }
            if (User.UserName.Length == 0) {
                userNotification.UserName = User.Nick;
            } else {
                userNotification.UserName = User.UserName;
            }
            userNotification.InitialInvisibility = true;
            Send (userNotification);

        }

        void ensureIdentEnds (object sender, ConnectionDataEventArgs e)
        {
            Ident.Service.Stop ();
        }

        void serverNameAquired (object sender, IrcMessageEventArgs<WelcomeMessage> e)
        {
            ServerName = e.Message.Sender.Nick;
            Ident.Service.Stop ();

            if (e.Message.Target != User.Nick) {
                User.Nick = e.Message.Target;
            }

        }

        void keepOwnNickCorrect (object sender, IrcMessageEventArgs<NickChangeMessage> e)
        {
            if (e.Message.Sender.Nick == User.Nick) {
                User.Nick = e.Message.NewNick;
            }
        }

        void populateSupports (object sender, IrcMessageEventArgs<SupportMessage> e)
        {
            ServerSupports.LoadInfo (e.Message);
        }

        void lookForReady (object sender, IrcMessageEventArgs<IrcMessage> e)
        {
            NumericMessage numeric = e.Message as NumericMessage;
            if (numeric == null) {
                return;
            }

            if (NumericMessage.IsDirect (numeric.InternalNumeric)) {
                return;
            }


            if (Ready != null) {
                readyRaised = true;
                Ready (this, EventArgs.Empty);
            }
        }

        void messageParsed (object sender, IrcMessageEventArgs<IrcMessage> e)
        {

            if (!readyRaised) {
                lookForReady (sender, e);
            }

            bool routed = false;

            IrcMessage ircMessage = e.Message;
            if (ircMessage is IChannelTargetedMessage || ircMessage is IQueryTargetedMessage) {

                IQueryTargetedMessage queryMessage = ircMessage as IQueryTargetedMessage;
                if (queryMessage != null) {
                    if (queryMessage.IsQueryToUser (User)) {
                        User msgSender = Peers.EnsureUser (ircMessage.Sender);
                        Query qry = Queries.EnsureQuery (msgSender, this);
                        qry.Journal.Add (new JournalEntry (ircMessage));
                        routed = true;
                    }
                }

                IChannelTargetedMessage channelMessage = ircMessage as IChannelTargetedMessage;
                if (channelMessage != null) {
                    foreach (Channel channel in Channels) {
                        if (channelMessage.IsTargetedAtChannel (channel.Name)) {
                            channel.Journal.Add (new JournalEntry (ircMessage));
                            routed = true;
                        }
                    }
                }

            }

            if (!routed) {
                ServerQuery.Journal.Add (new JournalEntry (ircMessage));
            }

        }

        void routeJoins (object sender, IrcMessageEventArgs<JoinMessage> e)
        {
            User msgUser = e.Message.Sender;
            User joinedUser = (IsMe (msgUser.Nick)) ? User : Peers.EnsureUser (msgUser);

            foreach (string channelname in e.Message.Channels) {
                Channel joinedChannel = Channels.EnsureChannel (channelname, this);
                joinedChannel.Open = true;
                joinedChannel.Users.Add (joinedUser);
            }
        }

        void routeKicks (object sender, IrcMessageEventArgs<KickMessage> e)
        {
            for (int i = 0; i < e.Message.Channels.Count; i++) {
                string channelName = e.Message.Channels [i];
                string nick = e.Message.Nicks [i];
                Channel channel = Channels.FindChannel (channelName);
                if (channel != null) {

                    if (IsMe (nick)) {
                        // we don't want to actually remove the channel, but just close the channel
                        // this allows a consumer to easily keep their reference to channels between kicks and re-joins.
                        channel.Open = false;
                    } else {
                        channel.Users.RemoveFirst (nick);
                    }
                }
            }
        }

        void routeKills (object sender, IrcMessageEventArgs<KillMessage> e)
        {
            string nick = e.Message.Nick;
            if (IsMe (nick)) {
                foreach (Channel c in Channels) {
                    c.Open = false;
                }
            } else {
                foreach (Channel channel in Channels) {
                    channel.Users.RemoveFirst (nick);
                }
            }
        }

        void routeNames (object sender, IrcMessageEventArgs<NamesReplyMessage> e)
        {
            Channel channel = Channels.EnsureChannel (e.Message.Channel, this);
            foreach (string nick in e.Message.Nicks.Keys) {
                User user = Peers.EnsureUser (nick);
                if (!channel.Users.Contains (user)) {
                    channel.Users.Add (user);
                }
                ChannelStatus status = e.Message.Nicks [nick];
                channel.SetStatusForUser (user, status);
            }
        }

        void routeNicks (object sender, IrcMessageEventArgs<NickChangeMessage> e)
        {
            string oldNick = e.Message.Sender.Nick;
            string newNick = e.Message.NewNick;
            if (IsMe (oldNick)) {
                User.Nick = newNick;
            } else {
                User u = Peers.Find (oldNick);
                if (u != null) {
                    u.Nick = newNick;
                }
            }
        }

        void routeParts (object sender, IrcMessageEventArgs<PartMessage> e)
        {
            string nick = e.Message.Sender.Nick;
            foreach (string channelName in e.Message.Channels) {
                Channel channel = Channels.FindChannel (channelName);
                if (channel != null) {

                    if (IsMe (nick)) {
                        channel.Open = false;
                    } else {
                        channel.Users.RemoveFirst (nick);
                    }
                }
            }
        }

        void routeQuits (object sender, IrcMessageEventArgs<QuitMessage> e)
        {
            string nick = e.Message.Sender.Nick;
            if (IsMe (nick)) {
                foreach (Channel c in Channels) {
                    c.Open = false;
                }
            } else {
                foreach (Channel channel in Channels) {
                    channel.Users.RemoveFirst (nick);
                }
            }
        }

        void routeTopicNones (object sender, IrcMessageEventArgs<TopicNoneReplyMessage> e)
        {
            Channel channel = Channels.FindChannel (e.Message.Channel);
            if (channel != null) {
                channel.Topic = "";
            }
        }

        void routeTopics (object sender, IrcMessageEventArgs<TopicReplyMessage> e)
        {
            Channel channel = Channels.FindChannel (e.Message.Channel);
            if (channel != null) {
                channel.Topic = e.Message.Topic;
            }
        }

        void routeTopicSets (object sender, IrcMessageEventArgs<TopicSetReplyMessage> e)
        {
            Channel channel = Channels.FindChannel (e.Message.Channel);
            if (channel != null) {
                User topicSetter = Peers.EnsureUser (e.Message.User);
                channel.TopicSetter = topicSetter;
                channel.TopicSetTime = e.Message.TimeSet;
            }
        }

        void routeWhoReplies (object sender, IrcMessageEventArgs<WhoReplyMessage> e)
        {
            User whoUser = Peers.EnsureUser (e.Message.User);
            string channelName = e.Message.Channel;

            Channel channel = Channels.FindChannel (channelName);
            if (channel != null) {
                if (!channel.Users.Contains (whoUser)) {
                    channel.Users.Add (whoUser);
                }
                channel.SetStatusForUser (whoUser, e.Message.Status);
            }
        }

        void client_NoSuchChannel (object sender, IrcMessageEventArgs<NoSuchChannelMessage> e)
        {
            Channel channel = Channels.FindChannel (e.Message.Channel);
            if (channel != null) {
                channel.Open = false;
            }
        }

        void client_NoSuchNick (object sender, IrcMessageEventArgs<NoSuchNickMessage> e)
        {
            string nick = e.Message.Nick;

            if (MessageUtil.HasValidChannelPrefix (nick)) { // NoSuchNickMessage is sent by some servers instead of a NoSuchChannelMessage
                Channel channel = Channels.FindChannel (e.Message.Nick);
                if (channel != null) {
                    channel.Open = false;
                }
            } else {
                Peers.RemoveFirst (nick);
                foreach (Channel channel in Channels) {
                    channel.Users.RemoveFirst (nick);
                }
            }
        }

        void client_ChannelModeIsReply (object sender, IrcMessageEventArgs<ChannelModeIsReplyMessage> e)
        {
            Channel channel = Channels.FindChannel (e.Message.Channel);
            if (channel != null) {
                ChannelModesCreator modes = new ChannelModesCreator ();
                modes.ServerSupport = ServerSupports;
                modes.Parse (e.Message.Modes, e.Message.ModeArguments);
                channel.Modes.ResetWith (modes.Modes);
            }

        }

        void client_UserModeIsReply (object sender, IrcMessageEventArgs<UserModeIsReplyMessage> e)
        {
            if (IsMe (e.Message.Target)) {
                UserModesCreator modeCreator = new UserModesCreator ();
                modeCreator.Parse (e.Message.Modes);
                User.Modes.Clear ();
                foreach (UserMode mode in modeCreator.Modes) {
                    User.Modes.Add (mode);
                }
            }
        }

        void client_UserMode (object sender, IrcMessageEventArgs<UserModeMessage> e)
        {
            if (IsMe (e.Message.User)) {
                UserModesCreator modeCreator = new UserModesCreator ();
                modeCreator.Parse (e.Message.ModeChanges);
                User.Modes.Clear ();
                foreach (UserMode mode in modeCreator.Modes) {
                    User.Modes.Add (mode);
                }
            }
        }

        void client_UserHostReply (object sender, IrcMessageEventArgs<UserHostReplyMessage> e)
        {
            foreach (User sentUser in e.Message.Users) {
                if (IsMe (sentUser.Nick)) {
                    User.CopyFrom (sentUser);
                } else {
                    User user = Peers.EnsureUser (sentUser);
                    if (user != sentUser) {
                        user.CopyFrom (sentUser);
                    }
                    if (user.OnlineStatus != UserOnlineStatus.Away) {
                        user.AwayMessage = "";
                    }
                }
            }
        }

        void client_UserAway (object sender, IrcMessageEventArgs<UserAwayMessage> e)
        {
            User user = Peers.EnsureUser (e.Message.Nick);
            user.OnlineStatus = UserOnlineStatus.Away;
            user.AwayMessage = e.Message.Text;
        }

        void client_SelfUnAway (object sender, IrcMessageEventArgs<SelfUnAwayMessage> e)
        {
            User.OnlineStatus = UserOnlineStatus.Online;
            User.AwayMessage = "";
        }

        void client_SelfAway (object sender, IrcMessageEventArgs<SelfAwayMessage> e)
        {
            User.OnlineStatus = UserOnlineStatus.Away;
        }

        void client_OperReply (object sender, IrcMessageEventArgs<OperReplyMessage> e)
        {
            User.IrcOperator = true;
        }

        void client_ChannelProperty (object sender, IrcMessageEventArgs<ChannelPropertyMessage> e)
        {
            Channel channel = Channels.EnsureChannel (e.Message.Channel, this);
            channel.Properties [e.Message.Prop] = e.Message.NewValue;
        }

        void client_ChannelPropertyReply (object sender, IrcMessageEventArgs<ChannelPropertyReplyMessage> e)
        {
            Channel channel = Channels.EnsureChannel (e.Message.Channel, this);
            channel.Properties [e.Message.Prop] = e.Message.Value;
        }

        void client_Back (object sender, IrcMessageEventArgs<BackMessage> e)
        {
            User user = Peers.EnsureUser (e.Message.Sender);
            user.OnlineStatus = UserOnlineStatus.Online;
            user.AwayMessage = "";
        }

        void client_Away (object sender, IrcMessageEventArgs<AwayMessage> e)
        {
            User user = Peers.EnsureUser (e.Message.Sender);
            user.OnlineStatus = UserOnlineStatus.Away;
            user.AwayMessage = e.Message.Reason;
        }

        void client_WhoIsUserReply (object sender, IrcMessageEventArgs<WhoIsUserReplyMessage> e)
        {
            User target = Peers.EnsureUser (e.Message.User);
            target.CopyFrom (e.Message.User);
        }

        void client_WhoIsServerReply (object sender, IrcMessageEventArgs<WhoIsServerReplyMessage> e)
        {
            User user = Peers.EnsureUser (e.Message.Nick);
            user.ServerName = e.Message.ServerName;
        }

        void client_WhoIsOperReply (object sender, IrcMessageEventArgs<WhoIsOperReplyMessage> e)
        {
            User user = Peers.EnsureUser (e.Message.Nick);
            user.IrcOperator = true;
        }






        void logGenericCtcp (object sender, IrcMessageEventArgs<GenericCtcpRequestMessage> e)
        {
            logUnimplementedMessage (e.Message);
        }
        void logGenericMessage (object sender, IrcMessageEventArgs<GenericMessage> e)
        {
            logUnimplementedMessage (e.Message);
        }
        void logGenericNumeric (object sender, IrcMessageEventArgs<GenericNumericMessage> e)
        {
            logUnimplementedMessage (e.Message);
        }
        void logGenericError (object sender, IrcMessageEventArgs<GenericErrorMessage> e)
        {
            logUnimplementedMessage (e.Message);
        }

        void logUnimplementedMessage (IrcMessage msg)
        {
            System.Diagnostics.Trace.WriteLine (msg.ToString (), "Unimplemented Message");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        void selfDie (object sender, EventArgs e)
        {
            if (Connection != null && Connection.Status != ConnectionStatus.Disconnected) {
                Connection.DataReceived -= new EventHandler<ConnectionDataEventArgs> (dataReceived);
                Connection.DataSent -= new EventHandler<ConnectionDataEventArgs> (dataSent);
                Die ();
            }
        }

        #endregion

        #region Private

        bool readyRaised = false;
        IrcMessageWriter writer = new IrcMessageWriter ();

        void StartIdent ()
        {
            if (EnableAutoIdent) {
                Ident.Service.User = User;
                Ident.Service.Start (true);
                DateTime started = DateTime.Now;
                TimeSpan tooMuchTime = new TimeSpan (0, 0, 5);
                while (Ident.Service.Status != ConnectionStatus.Connected && DateTime.Now.Subtract (started) < tooMuchTime) {
                    System.Threading.Thread.Sleep (25);
                }
                if (Ident.Service.Status != ConnectionStatus.Connected) {
                    System.Diagnostics.Trace.WriteLine ("Ident Failed To AutoStart", "Ident");
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        void StopIdent ()
        {
            Ident.Service.Stop ();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        void Die ()
        {
            SendQuit (DefaultQuitMessage);
            Connection.DisconnectForce ();
        }

        #endregion

    }
}
