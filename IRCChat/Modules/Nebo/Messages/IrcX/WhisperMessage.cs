using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// With the WhisperMessage, clients can send messages to people within the context of a channel.
    /// </summary>
    [Serializable]
    public class WhisperMessage : CommandMessage, IChannelTargetedMessage
    {

        /// <summary>
        /// Creates a new instance of the WhisperMessage class.
        /// </summary>
        public WhisperMessage()
        {
        }

        /// <summary>
        /// Gets the Irc command associated with this message.
        /// </summary>
        protected override string Command {
            get {
                return "WHISPER";
            }
        }

        /// <summary>
        /// Gets or sets the channel being targeted.
        /// </summary>
        public virtual string Channel {
            get {
                return channel;
            }
            set {
                channel = value;
            }
        }
        string channel = "";

        /// <summary>
        /// Gets the target of this <see cref="TextMessage"/>.
        /// </summary>
        public virtual StringCollection Targets {
            get {
                return targets;
            }
        }
        StringCollection targets = new StringCollection();

        /// <summary>
        /// Gets or sets the actual text of this <see cref="TextMessage"/>.
        /// </summary>
        /// <remarks>
        /// This property holds the core purpose of irc itself... sending text communication to others.
        /// </remarks>
        public virtual string Text {
            get {
                return text;
            }
            set {
                text = value;
            }
        }
        string text = "";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
        /// </summary>
        protected override void AddParametersToFormat(IrcMessageWriter writer)
        {
            base.AddParametersToFormat(writer);
            writer.AddParameter(Channel);
            writer.AddList(Targets, ",", false);
            writer.AddParameter(Text, true);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters(StringCollection parameters)
        {
            base.ParseParameters(parameters);
            Targets.Clear();
            if (parameters.Count > 2) {
                Channel = parameters[0];
                Targets.AddRange(parameters[1].Split(','));
                Text = parameters[2];
            } else {
                Channel = string.Empty;
                Text = string.Empty;
            }
        }

        /// <summary>
        /// Validates this message's properties according to the given <see cref="ServerSupport"/>.
        /// </summary>
        public override void Validate(ServerSupport serverSupport)
        {
            base.Validate(serverSupport);
            Channel = MessageUtil.EnsureValidChannelName(Channel, serverSupport);
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify(MessageConduit conduit)
        {
            conduit.OnWhisper(new IrcMessageEventArgs<WhisperMessage>(this));
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
