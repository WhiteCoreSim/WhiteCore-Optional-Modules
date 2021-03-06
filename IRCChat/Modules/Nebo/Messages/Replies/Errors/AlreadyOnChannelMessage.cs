using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The ErrorMessage sent when a user tries to invite a person onto a channel which they
    /// are already on
    /// </summary>
    [Serializable]
    public class AlreadyOnChannelMessage : ErrorMessage, IChannelTargetedMessage
    {
        //:irc.dkom.at 443 artificer bob #NeboBot :is already on channel

        /// <summary>
        /// Creates a new instances of the <see cref="AlreadyOnChannelMessage"/> class.
        /// </summary>
        public AlreadyOnChannelMessage ()
        {
            InternalNumeric = 443;
        }

        /// <summary>
        /// Gets or sets the nick of the user invited
        /// </summary>
        public string Nick {
            get {
                return nick;
            }
            set {
                nick = value;
            }
        }
        string nick;

        /// <summary>
        /// Gets or sets the channel being invited to
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
            writer.AddParameter (Nick);
            writer.AddParameter (Channel);
            writer.AddParameter ("is already on channel");
        }

        /// <exclude />
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            Nick = "";
            Channel = "";
            if (parameters.Count > 2) {
                Nick = parameters [1];
                Channel = parameters [2];
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnAlreadyOnChannel (new IrcMessageEventArgs<AlreadyOnChannelMessage> (this));
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
