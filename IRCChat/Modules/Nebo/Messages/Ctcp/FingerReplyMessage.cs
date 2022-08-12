using System;
using System.Text;
using System.Globalization;


namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The reply to the <see cref="FingerRequestMessage"/>, containing the user's name and idle time.
    /// </summary>
    [Serializable]
    public class FingerReplyMessage : CtcpReplyMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="FingerReplyMessage"/> class.
        /// </summary>
        public FingerReplyMessage()
        {
            InternalCommand = "FINGER";
        }


        /// <summary>
        /// Gets or sets the real name of the user.
        /// </summary>
        public virtual string RealName {
            get {
                return realName;
            }
            set {
                realName = value;
            }
        }

        /// <summary>
        /// Gets or sets the login name of the user.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login")]
        public virtual string LoginName {
            get {
                return loginName;
            }
            set {
                loginName = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of seconds that the user has been idle.
        /// </summary>
        public virtual double IdleSeconds {
            get {
                return idleSeconds;
            }
            set {
                idleSeconds = value;
            }
        }


        /// <summary>
        /// Gets the data payload of the Ctcp request.
        /// </summary>
        protected override string ExtendedData {
            get {
                StringBuilder result = new StringBuilder();
                result.Append(":");
                result.Append(RealName);
                result.Append(" (");
                result.Append(LoginName);
                result.Append(") - Idle ");
                result.Append(IdleSeconds.ToString(CultureInfo.InvariantCulture));
                result.Append(" seconds");
                return result.ToString();
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify(MessageConduit conduit)
        {
            conduit.OnFingerReply(new IrcMessageEventArgs<FingerReplyMessage>(this));
        }

        string realName = "";
        string loginName = "";
        double idleSeconds;

        /// <summary>
        /// Parses the given string to populate this <see cref="IrcMessage"/>.
        /// </summary>
        public override void Parse(string unparsedMessage)
        {
            base.Parse(unparsedMessage);
            string payload = CtcpUtil.GetExtendedData(unparsedMessage);
            if (payload.StartsWith(":", StringComparison.Ordinal)) {
                payload = payload.Substring(1);
            }
            RealName = payload.Substring(0, payload.IndexOf(" ", StringComparison.Ordinal));

            int startOfLoginName = payload.IndexOf(" (", StringComparison.Ordinal);
            int endOfLoginName = payload.IndexOf(")", StringComparison.Ordinal);
            if (startOfLoginName > 0) {
                startOfLoginName += 2;
                LoginName = payload.Substring(startOfLoginName, endOfLoginName - startOfLoginName);

                int startOfIdle = payload.IndexOf("- Idle ", StringComparison.Ordinal);
                if (startOfIdle > 0) {
                    startOfIdle += 6;
                    string idleSecs = payload.Substring(startOfIdle, payload.Length - startOfIdle - 8);
                    double foo;
                    if (double.TryParse(idleSecs, NumberStyles.Any, null, out foo)) {
                        IdleSeconds = foo;
                    } else {
                        IdleSeconds = -1;
                    }

                }
            }
        }

    }
}
