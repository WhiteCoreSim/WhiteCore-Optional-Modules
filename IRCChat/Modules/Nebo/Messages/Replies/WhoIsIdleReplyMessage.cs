using System;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// As a reply to the <see cref="WhoIsMessage"/> message,
    /// carries information about idle time and such.
    /// </summary>
    [Serializable]
    public class WhoIsIdleReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="WhoIsIdleReplyMessage"/> class.
        /// </summary>
        public WhoIsIdleReplyMessage ()
        {
            InternalNumeric = 317;
        }

        /// <summary>
        /// Gets or sets the nick of the user who is being examined.
        /// </summary>
        public virtual string Nick {
            get {
                return nick;
            }
            set {
                nick = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of seconds the user has been idle.
        /// </summary>
        public virtual int IdleLength {
            get {
                return idleTime;
            }
            set {
                idleTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the time the user signed on to their current server.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "SignOn")]
        public virtual DateTime SignOnTime {
            get {
                return signOnTime;
            }
            set {
                signOnTime = value;
            }
        }

        /// <summary>
        /// Gets or sets some additional info about the user being examined.
        /// </summary>
        public virtual string Info {
            get {
                return info;
            }
            set {
                info = value;
            }
        }

        string nick = "";
        int idleTime = 0;
        DateTime signOnTime = DateTime.Now;
        string info = "";


        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Nick);
            writer.AddParameter (IdleLength.ToString (CultureInfo.InvariantCulture));
            writer.AddParameter (MessageUtil.ConvertToUnixTime (SignOnTime).ToString (CultureInfo.InvariantCulture));
            writer.AddParameter (Info);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);

            Nick = "";
            IdleLength = 0;
            SignOnTime = DateTime.Now;
            Info = "";

            if (parameters.Count > 2) {
                Nick = parameters [1];
                IdleLength = Convert.ToInt32 (parameters [2], CultureInfo.InvariantCulture);

                if (parameters.Count == 5) {
                    SignOnTime = MessageUtil.ConvertFromUnixTime (Convert.ToInt32 (parameters [3], CultureInfo.InvariantCulture));
                    Info = parameters [4];
                }
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnWhoIsIdleReply (new IrcMessageEventArgs<WhoIsIdleReplyMessage> (this));
        }

    }
}
