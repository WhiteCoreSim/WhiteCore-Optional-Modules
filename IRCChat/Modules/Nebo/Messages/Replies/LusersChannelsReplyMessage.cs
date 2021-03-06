using System;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// One of the responses to the <see cref="LusersMessage"/> query.
    /// </summary>
    [Serializable]
    public class LusersChannelsReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="LusersChannelsReplyMessage"/> class.
        /// </summary>
        public LusersChannelsReplyMessage ()
        {
            InternalNumeric = 254;
        }

        /// <summary>
        /// Gets or sets the number of channels available.
        /// </summary>
        public virtual int ChannelCount {
            get {
                return channelCount;
            }
            set {
                channelCount = value;
            }
        }
        int channelCount = -1;

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (ChannelCount.ToString (CultureInfo.InvariantCulture));
            writer.AddParameter (channelsFormed);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            ChannelCount = Convert.ToInt32 (parameters [1], CultureInfo.InvariantCulture);
        }

        string channelsFormed = "channels formed";

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnLusersChannelsReply (new IrcMessageEventArgs<LusersChannelsReplyMessage> (this));
        }
    }
}
