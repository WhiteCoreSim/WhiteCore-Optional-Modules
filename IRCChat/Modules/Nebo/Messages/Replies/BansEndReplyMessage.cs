using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Signifies the end of the reply listing Bans on a channel.
    /// </summary>
    [Serializable]
    public class BansEndReplyMessage : NumericMessage, IChannelTargetedMessage
    {

        /// <summary>
        /// Creates a new instances of the <see cref="BansEndReplyMessage"/> class.
        /// </summary>
        public BansEndReplyMessage ()
        {
            InternalNumeric = 368;
        }

        /// <summary>
        /// Gets or sets the channel the ban list refers to.
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
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Channel);
            writer.AddParameter ("End of channel ban list");
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count > 1) {
                Channel = parameters [1];
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnBansEndReply (new IrcMessageEventArgs<BansEndReplyMessage> (this));
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
