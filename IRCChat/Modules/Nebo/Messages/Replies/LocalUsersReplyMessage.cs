using System;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// This message indicates the number of local-server users.
    /// </summary>
    [Serializable]
    public class LocalUsersReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="LocalUsersReplyMessage"/> class.
        /// </summary>
        public LocalUsersReplyMessage ()
        {
            InternalNumeric = 265;
        }

        /// <summary>
        /// Gets or sets the number of local users.
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
        /// Gets or sets the maximum number of users for the server.
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

        string currentLocalUsers = "Current local users: ";
        string max = " Max: ";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            // official version:
            // :irc.server.com 265 artificer :Current local users: 606 Max: 610";

            // seen in the wild:
            // :irc.ptptech.com 265 artificer 606 610 :Current local users 606, max 610

            base.AddParametersToFormat (writer);
            writer.AddParameter (currentLocalUsers + UserCount + max + UserLimit);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            // official version:
            // :irc.server.com 265 artificer :Current local users: 606 Max: 610";

            // seen in the wild:
            // :irc.ptptech.com 265 artificer 606 610 :Current local users 606, max 610

            base.ParseParameters (parameters);
            if (parameters.Count == 2) {
                string payload = parameters [1];
                UserCount = Convert.ToInt32 (MessageUtil.StringBetweenStrings (payload, currentLocalUsers, max), CultureInfo.InvariantCulture);
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
            conduit.OnLocalUsersReply (new IrcMessageEventArgs<LocalUsersReplyMessage> (this));
        }

    }
}
