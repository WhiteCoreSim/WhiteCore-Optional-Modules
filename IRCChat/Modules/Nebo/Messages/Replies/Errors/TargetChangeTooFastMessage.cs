using System;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The ErrorMessage sent when a user tries to send commands to too many targets in a short amount of time.
    /// </summary>
    /// <remarks>
    /// The purpose of this error condition is to help stop spammers.
    /// </remarks>
    [Serializable]
    public class TargetChangeTooFastMessage : ErrorMessage, IChannelTargetedMessage
    {

        /// <summary>
        /// Creates a new instances of the <see cref="TargetChangeTooFastMessage"/> class.
        /// </summary>
        public TargetChangeTooFastMessage ()
        {
            InternalNumeric = 439;
        }

        /// <summary>
        /// Gets or sets the nick or channel which was attempted
        /// </summary>
        public string TargetChanged {
            get {
                return target;
            }
            set {
                target = value;
            }
        }
        string target;

        /// <summary>
        /// Gets or sets the number of seconds which must be waited before attempting again.
        /// </summary>
        public int Seconds {
            get {
                return seconds;
            }
            set {
                seconds = value;
            }
        }
        int seconds;


        /// <exclude />
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (TargetChanged);
            writer.AddParameter (string.Format (CultureInfo.InvariantCulture, "Target change too fast. Please wait {0} seconds.", Seconds));
        }

        /// <exclude />
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            TargetChanged = "";
            Seconds = -1;
            if (parameters.Count > 1) {
                TargetChanged = parameters [1];
                if (parameters.Count > 2) {
                    Seconds = Convert.ToInt32 (MessageUtil.StringBetweenStrings (parameters [2], "Please wait ", " seconds"), CultureInfo.InvariantCulture);
                }
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnTargetChangeTooFast (new IrcMessageEventArgs<TargetChangeTooFastMessage> (this));
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
            return MessageUtil.IsIgnoreCaseMatch (TargetChanged, channelName);
        }

        #endregion
    }
}
