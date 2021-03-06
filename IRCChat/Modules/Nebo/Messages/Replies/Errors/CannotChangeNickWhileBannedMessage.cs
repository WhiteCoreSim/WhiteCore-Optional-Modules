using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The ErrorMessage sent when a user tries to change his nick while on a channel in which he is banned.
    /// </summary>
    /// <remarks>
    /// This is error code is also defined as "Resource Unavailable", but this message variant is more common.
    /// </remarks>
    [Serializable]
    public class CannotChangeNickWhileBannedMessage : ErrorMessage, IChannelTargetedMessage
    {

        /// <summary>
        /// Creates a new instances of the <see cref="TooManyLinesMessage"/> class.
        /// </summary>
        public CannotChangeNickWhileBannedMessage ()
        {
            InternalNumeric = 437;
        }

        /// <summary>
        /// Gets or sets the channel in which the user is banned.
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

        /// <exclude />
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Channel);
            writer.AddParameter ("Cannot change nickname while banned on channel");
        }

        /// <exclude />
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            Channel = "";
            if (parameters.Count > 1) {
                Channel = parameters [1];
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnCannotChangeNickWhileBanned (new IrcMessageEventArgs<CannotChangeNickWhileBannedMessage> (this));
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
