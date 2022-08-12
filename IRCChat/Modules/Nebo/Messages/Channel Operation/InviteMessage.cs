using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{
    /// <summary>
    /// The InviteMessage is used to invite users to a channel.
    /// </summary>
    /// <remarks>
    /// This message wraps the INVITE command.
    /// </remarks>
    [Serializable]
    public class InviteMessage : CommandMessage, IChannelTargetedMessage
    {

        string channel = "";
        string nick = "";

        /// <summary>
        /// Creates a new instance of the <see cref="InviteMessage"/> class.
        /// </summary>
        public InviteMessage()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InviteMessage"/> class with the given channel and nick.
        /// </summary>
        /// <param name="channel">The channel the person is being invited into.</param>
        /// <param name="nick">The nick of the user invited</param>
        public InviteMessage(string channel, string nick)
        {
            this.channel = channel;
            this.nick = nick;
        }

        /// <summary>
        /// Gets or sets the channel the person is being invited into.
        /// </summary>
        public virtual string Channel {
            get {
                return channel;
            }
            set {
                channel = value;
            }
        }

        /// <summary>
        /// Gets or sets the nick of the user invited
        /// </summary>
        public virtual string Nick {
            get {
                return nick;
            }
            set {
                nick = value;
            }
        }

        /// <summary>
        /// Gets the Irc command associated with this message.
        /// </summary>
        protected override string Command {
            get {
                return "INVITE";
            }
        }

        /// <summary>
        /// Validates this message against the given server support
        /// </summary>
        public override void Validate(ServerSupport serverSupport)
        {
            base.Validate(serverSupport);
            Channel = MessageUtil.EnsureValidChannelName(Channel, serverSupport);
        }

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
        /// </summary>
        protected override void AddParametersToFormat(IrcMessageWriter writer)
        {
            base.AddParametersToFormat(writer);
            writer.AddParameter(Channel);
            writer.AddParameter(Nick);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters(StringCollection parameters)
        {
            base.ParseParameters(parameters);
            if (parameters.Count >= 2) {
                Channel = parameters[0];
                Nick = parameters[1];
            } else {
                Channel = "";
                Nick = "";
            }
        }


        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify(MessageConduit conduit)
        {
            conduit.OnInvite(new IrcMessageEventArgs<InviteMessage>(this));
        }


        #region IChannelTargetedMessage Members

        bool IChannelTargetedMessage.IsTargetedAtChannel(string channelName)
        {
            return IsTargetedAtChannel(channelName);
        }

        /// <summary>
        /// Determines if the the current message is targeted at the given channel.
        /// </summary>
        protected virtual bool IsTargetedAtChannel(string channelName)
        {
            return MessageUtil.IsIgnoreCaseMatch(Channel, channelName);
        }

        #endregion
    }

}
