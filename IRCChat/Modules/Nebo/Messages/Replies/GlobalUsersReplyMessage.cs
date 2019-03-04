using System;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// This message indicates the number of network-wide users.
    /// </summary>
    [Serializable]
    public class GlobalUsersReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="GlobalUsersReplyMessage"/> class.
        /// </summary>
        public GlobalUsersReplyMessage ()
        {
            InternalNumeric = 266;
        }

        /// <summary>
        /// Gets or sets the number of global users.
        /// </summary>
        public virtual int UserCount {
            get {
                return userCount;
            }
            set {
                userCount = value;
            }
        }
        int userCount = -1;

        /// <summary>
        /// Gets or sets the maximum number of users for the network.
        /// </summary>
        public virtual int UserLimit {
            get {
                return userLimit;
            }
            set {
                userLimit = value;
            }
        }
        int userLimit = -1;

        string currentGlobalUsers = "Current global users: ";
        string max = " Max: ";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            // we only write the official version of this message, although other versions exist,
            // thus the message may not be the same raw as parsed.
            base.AddParametersToFormat (writer);
            writer.AddParameter (currentGlobalUsers + userCount + max + UserLimit);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            // official version:
            // :irc.ptptech.com 266 artificer :Current global users: 84236 Max: 87756

            // seen in the wild:
            // :irc.ptptech.com 266 artificer 84236 87756 :Current global users 84236, max 87756

            base.ParseParameters (parameters);
            if (parameters.Count == 2) {
                string payload = parameters [1];
                UserCount = Convert.ToInt32 (MessageUtil.StringBetweenStrings (payload, currentGlobalUsers, max), CultureInfo.InvariantCulture);
                UserLimit = Convert.ToInt32 (payload.Substring (payload.IndexOf (max, StringComparison.Ordinal) + max.Length), CultureInfo.InvariantCulture);
            } else if (parameters.Count == 4) {
                UserCount = Convert.ToInt32 (parameters [1], CultureInfo.InvariantCulture);
                UserLimit = Convert.ToInt32 (parameters [2], CultureInfo.InvariantCulture);
            } else {
                UserCount = -1;
                UserLimit = -1;
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnGlobalUsersReply (new IrcMessageEventArgs<GlobalUsersReplyMessage> (this));
        }

    }
}
