using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The reply for the <see cref="TopicMessage"/>.
    /// </summary>
    [Serializable]
    public class TopicReplyMessage : NumericMessage, IChannelTargetedMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="TopicReplyMessage"/> class.
        /// </summary>
        public TopicReplyMessage ()
        {
            InternalNumeric = 332;
        }

        /// <summary>
        /// Gets or sets the channel referenced.
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
        /// Gets or sets the topic of the channel.
        /// </summary>
        public virtual string Topic {
            get {
                return topic;
            }
            set {
                topic = value;
            }
        }

        string channel = "";
        string topic = "";


        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Channel);
            writer.AddParameter (Topic);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count >= 2) {
                Channel = parameters [1];
                Topic = parameters [2];
            } else {
                Channel = "";
                Topic = "";
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnTopicReply (new IrcMessageEventArgs<TopicReplyMessage> (this));
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
