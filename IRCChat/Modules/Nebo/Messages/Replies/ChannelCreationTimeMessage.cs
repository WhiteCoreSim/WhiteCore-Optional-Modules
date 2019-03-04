using System;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The message received informing the user of a channel's creation time.
    /// </summary>
    [Serializable]
    public class ChannelCreationTimeMessage : NumericMessage, IChannelTargetedMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="ChannelCreationTimeMessage"/> class.
        /// </summary>
        public ChannelCreationTimeMessage ()
        {
            InternalNumeric = 329;
        }

        /// <summary>
        /// Gets or sets the channel reffered to.
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
        /// Gets or sets the time which the channel was created.
        /// </summary>
        public virtual DateTime TimeCreated {
            get {
                return timeCreated;
            }
            set {
                timeCreated = value;
            }
        }

        string channel = "";
        DateTime timeCreated = DateTime.MinValue;


        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            //:mesra.kl.my.dal.net 329 artificer #c# 1043382332

            base.AddParametersToFormat (writer);
            writer.AddParameter (Channel);
            writer.AddParameter (MessageUtil.ConvertToUnixTime (TimeCreated).ToString (CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            //:mesra.kl.my.dal.net 329 artificer #c# 1043382332

            base.ParseParameters (parameters);
            Channel = "";
            TimeCreated = DateTime.MinValue;

            if (parameters.Count > 2) {
                Channel = parameters [1];
                DateTime? unixTime = MessageUtil.ConvertFromUnixTime (parameters [2]);
                if (unixTime.HasValue) {
                    TimeCreated = unixTime.Value;
                }
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnChannelCreationTime (new IrcMessageEventArgs<ChannelCreationTimeMessage> (this));
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
