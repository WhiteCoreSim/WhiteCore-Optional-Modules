using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// This message is sent to all channel operators.
    /// </summary>
    [Serializable]
    public class WallchopsMessage : CommandMessage, IChannelTargetedMessage
    {

        /// <summary>
        /// Gets or sets the text of the <see cref="WallchopsMessage"/>.
        /// </summary>
        public virtual string Text {
            get {
                return _text;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException ("value");
                }
                _text = value;
            }
        }
        string _text = "";

        /// <summary>
        /// Gets or sets the channel being targeted by the message.
        /// </summary>
        public virtual string Channel {
            get {
                return _channel;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException ("value");
                }
                _channel = value;
            }
        }
        string _channel = "";

        /// <summary>
        /// Gets the Irc command associated with this message.
        /// </summary>
        protected override string Command {
            get {
                return "WALLCHOPS";
            }
        }

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Channel, false);
            writer.AddParameter (Text, true);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            Text = string.Empty;
            Channel = string.Empty;

            if (parameters.Count >= 1) {
                Channel = parameters [0];
                if (parameters.Count >= 2) {
                    Text = parameters [1];
                }
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnWallchops (new IrcMessageEventArgs<WallchopsMessage> (this));
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
