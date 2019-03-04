using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// This message is a chat message which is scoped to the current channel.
    /// </summary>
    /// <remarks>
    /// This is a non-standard message.
    /// This command exists because many servers limit the number of standard chat messages
    /// you can send in a time frame. However, they will let channel operators send this chat message
    /// as often as they want to people who are in that channel.
    /// </remarks>
    [Serializable]
    public class ChannelScopedChatMessage : CommandMessage, IChannelTargetedMessage
    {

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="ChannelScopedChatMessage"/> class.
        /// </summary>
        public ChannelScopedChatMessage ()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ChannelScopedChatMessage"/> class with the given text string.
        /// </summary>
        public ChannelScopedChatMessage (string text)
        {
            Text = text;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ChannelScopedChatMessage"/> class with the given text string and target channel or user.
        /// </summary>
        public ChannelScopedChatMessage (string text, string target)
            : this (text)
        {
            Target = target;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Irc command associated with this message.
        /// </summary>
        protected override string Command {
            get {
                return "CPRIVMSG";
            }
        }

        /// <summary>
        /// Gets or sets the actual text of this message.
        /// </summary>
        public virtual string Text {
            get {
                return text;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException ("value");
                }
                text = value;
            }
        }
        string text = "";

        /// <summary>
        /// Gets or sets the target of this message.
        /// </summary>
        public virtual string Target {
            get {
                return target;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException ("value");
                }
                target = value;
            }
        }
        string target = "";

        /// <summary>
        /// Gets or sets the channel which this message is scoped to.
        /// </summary>
        public virtual string Channel {
            get {
                return channel;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException ("value");
                }
                channel = value;
            }
        }
        string channel = "";

        #endregion

        #region Methods

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Target);
            writer.AddParameter (Channel);
            writer.AddParameter (Text);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);

            if (parameters.Count == 3) {
                Target = parameters [0];
                Channel = parameters [1];
                Text = parameters [2];
            } else {
                Target = string.Empty;
                Channel = string.Empty;
                Text = string.Empty;
            }
        }

        /// <summary>
        /// Validates this message against the given server support
        /// </summary>
        public override void Validate (ServerSupport serverSupport)
        {
            base.Validate (serverSupport);
            MessageUtil.EnsureValidChannelName (Channel, serverSupport);
            if (string.IsNullOrEmpty (Target)) {
                throw new InvalidMessageException (NeboResources.TargetcannotBeEmpty);
            }
            if (string.IsNullOrEmpty (Channel)) {
                throw new InvalidMessageException (NeboResources.ChannelCannotBeEmpty);
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnChannelScopedChat (new IrcMessageEventArgs<ChannelScopedChatMessage> (this));
        }

        #endregion


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
