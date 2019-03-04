using System;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// A reply to a <see cref="WhoMessage"/> query.
    /// </summary>
    [Serializable]
    public class WhoReplyMessage : NumericMessage, IChannelTargetedMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="WhoReplyMessage"/> class.
        /// </summary>
        public WhoReplyMessage ()
        {
            InternalNumeric = 352;
        }

        /// <summary>
        /// Gets or sets the channel associated with the user.
        /// </summary>
        /// <remarks>
        /// In the case of a non-channel based WhoMessage, 
        /// Channel will contain the most recent channel which the user joined and is still on.
        /// </remarks>
        public virtual string Channel {
            get {
                return channel;
            }
            set {
                channel = value;
            }
        }

        /// <summary>
        /// Gets or sets the user being examined.
        /// </summary>
        public virtual User User {
            get {
                return user;
            }
            set {
                user = value;
            }
        }

        /// <summary>
        /// Gets or sets the status of the user on the associated channel.
        /// </summary>
        public virtual ChannelStatus Status {
            get {
                return status;
            }
            set {
                Status = value;
            }
        }

        /// <summary>
        /// Gets or sets the server the user is on.
        /// </summary>
        public virtual string Server {
            get {
                return server;
            }
            set {
                server = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of hops to the server the user is on.
        /// </summary>
        public virtual int HopCount {
            get {
                return hopCount;
            }
            set {
                hopCount = value;
            }
        }

        /// <summary>
        /// Gets or sets if the user is an irc operator.
        /// </summary>
        public virtual bool IsOper {
            get {
                return isOper;
            }
            set {
                isOper = value;
            }
        }

        string channel = "";
        User user = new User ();
        string server = "";
        int hopCount = -1;
        bool isOper = false;
        ChannelStatus status = ChannelStatus.None;


        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Channel);
            writer.AddParameter (User.UserName);
            writer.AddParameter (User.HostName);
            writer.AddParameter (Server);
            writer.AddParameter (User.Nick);
            //HACK it could also be a G, but I was unable to determine what it meant.
            if (IsOper) {
                writer.AddParameter ("H*");
            } else {
                writer.AddParameter ("H");
            }
            writer.AddParameter (Status.ToString ());
            writer.AddParameter (HopCount.ToString (CultureInfo.InvariantCulture) + " " + User.RealName);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            User = new User ();
            if (parameters.Count == 7) {

                Channel = parameters [1];
                User.UserName = parameters [2];
                User.HostName = parameters [3];
                Server = parameters [4];
                User.Nick = parameters [5];

                string levels = parameters [6];
                // TODO I'm going to ignore the H/G issue until i know what it means.
                IsOper = (levels.IndexOf ("*", StringComparison.Ordinal) != -1);
                string lastLetter = levels.Substring (levels.Length - 1);
                if (ChannelStatus.Exists (lastLetter)) {
                    Status = ChannelStatus.GetInstance (lastLetter);
                } else {
                    Status = ChannelStatus.None;
                }

                string trailing = parameters [7];
                HopCount = Convert.ToInt32 (trailing.Substring (0, trailing.IndexOf (" ", StringComparison.Ordinal)), CultureInfo.InvariantCulture);
                User.RealName = trailing.Substring (trailing.IndexOf (" ", StringComparison.Ordinal));

            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnWhoReply (new IrcMessageEventArgs<WhoReplyMessage> (this));
        }


        #region IChannelTargetedMessage Members

        bool IChannelTargetedMessage.IsTargetedAtChannel (string channelName)
        {
            return IsTargetedAtChannel (channelName);
        }

        /// <summary>
        /// Determines if the the current message is targeted at the given channel.
        /// </summary>
        protected virtual bool IsTargetedAtChannel (string channelName)
        {
            return MessageUtil.IsIgnoreCaseMatch (Channel, channelName);
        }

        #endregion
    }
}
