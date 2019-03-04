using System;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// A single reply to the <see cref="ListMessage"/> query.
    /// </summary>
    [Serializable]
    public class ListReplyMessage : NumericMessage, IChannelTargetedMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="ListReplyMessage"/> class.
        /// </summary>
        public ListReplyMessage ()
        {
            InternalNumeric = 322;
        }

        /// <summary>
        /// Gets or sets the channel for this reply.
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
        /// Gets or sets the number of people in the channel.
        /// </summary>
        public virtual int MemberCount {
            get {
                return memberCount;
            }
            set {
                memberCount = value;
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

        string topic = "";
        int memberCount = -1;
        string channel = "";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Channel);
            writer.AddParameter (MemberCount.ToString (CultureInfo.InvariantCulture));
            writer.AddParameter (Topic);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count == 4) {
                Channel = parameters [1];
                MemberCount = Convert.ToInt32 (parameters [2], CultureInfo.InvariantCulture);
                Topic = parameters [3];
            } else {
                Channel = "";
                MemberCount = -1;
                Topic = "";
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnListReply (new IrcMessageEventArgs<ListReplyMessage> (this));
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
