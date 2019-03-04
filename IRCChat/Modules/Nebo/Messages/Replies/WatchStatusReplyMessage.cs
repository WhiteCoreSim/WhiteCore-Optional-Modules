using System;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// A reply for the <see cref="WatchStatusRequestMessage"/> query.
    /// </summary>
    [Serializable]
    public class WatchStatusReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="WatchStatusRequestMessage"/>.
        /// </summary>
        public WatchStatusReplyMessage ()
        {
            InternalNumeric = 603;
        }

        /// <summary>
        /// Gets or sets the number of watched users that you have on your watch list.
        /// </summary>
        public int WatchListCount {
            get {
                return watchesYouHave;
            }
            set {
                watchesYouHave = value;
            }
        }
        int watchesYouHave = 0;

        /// <summary>
        /// Gets or sets the number of users which you on their watch list.
        /// </summary>
        public int UsersWatchingYou {
            get {
                return watchesThatHaveYou;
            }
            set {
                watchesThatHaveYou = value;
            }
        }
        int watchesThatHaveYou = 0;

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (string.Format (CultureInfo.InvariantCulture, "You have {0} and are on {1} WATCH entries", WatchListCount, UsersWatchingYou));
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);

            string unparsedSentence = parameters [parameters.Count - 1];

            string watchesHave = MessageUtil.StringBetweenStrings (unparsedSentence, "You have ", " and are on ");
            string watchesOn = MessageUtil.StringBetweenStrings (unparsedSentence, " and are on ", " WATCH entries");

            WatchListCount = int.Parse (watchesHave, CultureInfo.InvariantCulture);
            UsersWatchingYou = int.Parse (watchesOn, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnWatchStatusReply (new IrcMessageEventArgs<WatchStatusReplyMessage> (this));
        }

    }
}
