using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{
    /// <summary>
    /// The PartMessage causes the client sending the message to be removed 
    /// from the list of active users for all given channels listed in the <see cref="Channels"/> property.
    /// </summary>
    [Serializable]
    public class PartMessage : CommandMessage, IChannelTargetedMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="PartMessage"/> class.
        /// </summary>
        public PartMessage ()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PartMessage"/> class with the given channel.
        /// </summary>
        public PartMessage (string channel)
        {
            channels.Add (channel);
        }

        /// <summary>
        /// Gets the channel name parted.
        /// </summary>
        public virtual StringCollection Channels {
            get {
                return channels;
            }
        }

        /// <summary>
        /// Gets or sets the reason for the part.
        /// </summary>
        public virtual string Reason {
            get {
                return reason;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException (nameof (value));
                }
                reason = value;
            }
        }

        /// <summary>
        /// Gets the Irc command associated with this message.
        /// </summary>
        protected override string Command {
            get {
                return "PART";
            }
        }

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddList (Channels, ",", true);
            if (Reason.Length != 0) {
                writer.AddParameter (Reason);
            }
        }

        /// <summary>
        /// Validates this message against the given server support
        /// </summary>
        public override void Validate (ServerSupport serverSupport)
        {
            base.Validate (serverSupport);
            for (int i = 0; i < Channels.Count; i++) {
                Channels [i] = MessageUtil.EnsureValidChannelName (Channels [i], serverSupport);
            }
        }

        /// <summary>
        /// Parse the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            Channels.Clear ();
            if (parameters.Count >= 1) {
                Channels.AddRange (parameters [0].Split (','));
                if (parameters.Count >= 2) {
                    Reason = parameters [1];
                }
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnPart (new IrcMessageEventArgs<PartMessage> (this));
        }

        StringCollection channels = new StringCollection ();
        string reason = "";



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
            return MessageUtil.ContainsIgnoreCaseMatch (Channels, channelName);
        }

        #endregion
    }
}
