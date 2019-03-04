using System;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// An Accept/CallerId system message marking the end of the responses to an AcceptListRequestMessage.
    /// </summary>
    [Serializable]
    public class AcceptListEndReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="AcceptListEndReplyMessage"/>.
        /// </summary>
        public AcceptListEndReplyMessage ()
        {
            InternalNumeric = 282;
        }

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            // :irc.pte.hu 282 artificer :End of /ACCEPT list.

            base.AddParametersToFormat (writer);
            writer.AddParameter ("End of /ACCEPT list.");
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnAcceptListEndReply (new IrcMessageEventArgs<AcceptListEndReplyMessage> (this));
        }

    }
}
