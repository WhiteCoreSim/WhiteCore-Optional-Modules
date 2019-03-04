using System;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The ErrorMessage received when attempting to join a channel which invite-only.
    /// </summary>
    /// <remarks>
    /// A channel can be set invite-only with a ChannelModeMessage containing a InviteOnlyMode.
    /// </remarks>
    [Serializable]
    public class ChannelBlockedMessage : ErrorMessage, IChannelTargetedMessage
    {
        //:irc.dkom.at 485 artificer #chaos :Cannot join channel (Q-Line Blocked)

        /// <summary>
        /// Creates a new instances of the <see cref="ChannelBlockedMessage"/> class.
        /// </summary>
        public ChannelBlockedMessage ()
        {
            InternalNumeric = 485;
        }

        /// <summary>
        /// Gets or sets the channel which is blocked
        /// </summary>
        public string Channel {
            get {
                return channel;
            }
            set {
                channel = value;
            }
        }
        string channel;

        /// <summary>
        /// Gets or sets the reason the channel is blocked
        /// </summary>
        public string Reason {
            get {
                return reason;
            }
            set {
                reason = value;
            }
        }
        string reason;


        /// <exclude />
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Channel);
            writer.AddParameter (string.Format (CultureInfo.InvariantCulture, "Cannot join channel ({0})", Reason));
        }

        /// <exclude />
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);

            Channel = "";
            Reason = "";

            if (parameters.Count > 2) {
                Channel = parameters [1];
                Reason = MessageUtil.StringBetweenStrings (parameters [2], "Cannot join channel (", ")");
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnChannelBlocked (new IrcMessageEventArgs<ChannelBlockedMessage> (this));
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
            ;
        }

        #endregion
    }
}
