using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Signals the end of replies to a Watch system query.
    /// </summary>
    [Serializable]
    public class WatchListEndReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="WatchListEndReplyMessage"/>.
        /// </summary>
        public WatchListEndReplyMessage ()
        {
            InternalNumeric = 607;
        }

        /// <summary>
        /// Gets or sets the type of the list which was sent.
        /// </summary>
        public string ListType {
            get {
                return listType;
            }
            set {
                listType = value;
            }
        }
        string listType = "";


        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter ("End of WATCH " + ListType);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);

            string lastParam = parameters [parameters.Count - 1];
            ListType = lastParam.Substring (lastParam.Length - 1);
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnWatchListEndReply (new IrcMessageEventArgs<WatchListEndReplyMessage> (this));
        }

    }
}
