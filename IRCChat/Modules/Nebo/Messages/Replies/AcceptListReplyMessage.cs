using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// An Accept/CallerId system message received in response to an AcceptListRequestMessage.
    /// </summary>
    /// <remarks>
    /// You may receive more than 1 of these in response to the request.
    /// </remarks>
    [Serializable]
    public class AcceptListReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="AcceptListReplyMessage"/>.
        /// </summary>
        public AcceptListReplyMessage ()
        {
            InternalNumeric = 281;
        }

        /// <summary>
        /// Gets the collection of nicks of the users on the watch list.
        /// </summary>
        public StringCollection Nicks {
            get {
                if (nicks == null) {
                    nicks = new StringCollection ();
                }
                return nicks;
            }
        }
        StringCollection nicks;

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            // :irc.pte.hu 281 artificer azure bbs

            base.AddParametersToFormat (writer);
            foreach (string nick in Nicks) {
                writer.AddParameter (nick);
            }
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            // :irc.pte.hu 281 artificer azure bbs

            base.ParseParameters (parameters);

            Nicks.Clear ();
            for (int i = 1; i < parameters.Count; i++) {
                Nicks.Add (parameters [i]);
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnAcceptListReply (new IrcMessageEventArgs<AcceptListReplyMessage> (this));
        }

    }
}
