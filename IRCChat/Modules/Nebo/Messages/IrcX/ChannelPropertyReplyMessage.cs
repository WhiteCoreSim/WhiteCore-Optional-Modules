using System;
using System.Collections.Specialized;
using System.Text;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// A reply to a <see cref="ChannelPropertyMessage"/> designed to read one or all channel properties.
    /// </summary>
    [Serializable]
    public class ChannelPropertyReplyMessage : NumericMessage, IChannelTargetedMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="ChannelPropertyReplyMessage"/>.
        /// </summary>
        public ChannelPropertyReplyMessage ()
        {
            InternalNumeric = 818;
        }

        /// <summary>
        /// Gets or sets channel being referenced.
        /// </summary>
        public virtual String Channel {
            get {
                return channel;
            }
            set {
                channel = value;
            }
        }
        string channel = "";

        /// <summary>
        /// Gets or sets the name of the channel property being referenced.
        /// </summary>
        public virtual string Prop {
            get {
                return property;
            }
            set {
                property = value;
            }
        }
        string property = "";

        /// <summary>
        /// Gets or sets the value of the channel property.
        /// </summary>
        public virtual string Value {
            get {
                return propValue;
            }
            set {
                propValue = value;
            }
        }
        string propValue = "";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Channel);
            writer.AddParameter (Prop);
            writer.AddParameter (Value);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);

            Channel = "";
            Prop = "";
            Value = "";

            if (parameters.Count > 1) {
                Channel = parameters [1];
                if (parameters.Count > 2) {
                    Prop = parameters [2];
                    if (parameters.Count > 3) {
                        Value = parameters [3];
                    }
                }
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnChannelPropertyReply (new IrcMessageEventArgs<ChannelPropertyReplyMessage> (this));
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
