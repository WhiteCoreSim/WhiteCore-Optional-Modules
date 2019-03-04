using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// This is the reply to an empty <see cref="ChannelModeMessage"/>.
    /// </summary>
    [Serializable]
    public class ChannelModeIsReplyMessage : NumericMessage, IChannelTargetedMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="ChannelModeIsReplyMessage"/> class.
        /// </summary>
        public ChannelModeIsReplyMessage ()
        {
            InternalNumeric = 324;
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
        /// Gets or sets the modes in effect.
        /// </summary>
        /// <remarks>
        /// An example Modes might look like "+ml".
        /// </remarks>
        public virtual string Modes {
            get {
                return modes;
            }
            set {
                modes = value;
            }
        }

        /// <summary>
        /// Gets the collection of arguments ( parameters ) for the <see cref="Modes"/> property.
        /// </summary>
        /// <remarks>
        /// Some modes require a parameter, such as +l ( user limit ) requires the number being limited to.
        /// </remarks>
        public virtual StringCollection ModeArguments {
            get {
                return modeArguments;
            }
        }

        string channel = "";
        string modes = "";
        StringCollection modeArguments = new StringCollection ();

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Channel);
            writer.AddParameter (Modes);
            if (ModeArguments.Count != 0) {
                writer.AddList (ModeArguments, " ", false);
            }
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);

            ModeArguments.Clear ();
            if (parameters.Count > 2) {
                Channel = parameters [1];
                Modes = parameters [2];
                for (int i = 3; i < parameters.Count; i++) {
                    ModeArguments.Add (parameters [i]);
                }
            } else {
                Channel = "";
                Modes = "";
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnChannelModeIsReply (new IrcMessageEventArgs<ChannelModeIsReplyMessage> (this));
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
