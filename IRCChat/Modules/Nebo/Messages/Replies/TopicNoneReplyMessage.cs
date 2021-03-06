using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The reply to a <see cref="TopicMessage"/> which requests the topic, 
    /// and there is none set.
    /// </summary>
    [Serializable]
    public class TopicNoneReplyMessage : NumericMessage, IChannelTargetedMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="TopicNoneReplyMessage"/> class.
        /// </summary>
        public TopicNoneReplyMessage ()
        {
            InternalNumeric = 331;
        }

        /// <summary>
        /// The name of the channel which has no topic set.
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
            writer.AddParameter ("No topic is set");
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count >= 2) {
                Channel = parameters [1];
            } else {
                Channel = "";
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnTopicNoneReply (new IrcMessageEventArgs<TopicNoneReplyMessage> (this));
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
