using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Contains a Channel and BanId as one of possible many replies to a ban list request.
    /// </summary>
    [Serializable]
    public class BansReplyMessage : NumericMessage, IChannelTargetedMessage
    {

        /// <summary>
        /// Creates a new instances of the <see cref="BansReplyMessage"/> class.
        /// </summary>
        public BansReplyMessage ()
        {
            InternalNumeric = 367;
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

        /// <summary>
        /// Gets or sets the ban referenced.
        /// </summary>
        public virtual string BanId {
            get {
                return banId;
            }
            set {
                banId = value;
            }
        }

        string channel = "";
        string banId = "";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Channel);
            writer.AddParameter (BanId);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count > 2) {
                Channel = parameters [1];
                BanId = parameters [2];
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnBansReply (new IrcMessageEventArgs<BansReplyMessage> (this));
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
