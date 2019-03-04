using System;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The ErrorMessage received when a UserModeMessage was sent with a UserMode which the server didn't recognize.
    /// </summary>
    [Serializable]
    public class UnknownUserModeMessage : ErrorMessage
    {
        //:irc.dkom.at 501 artificer :Unknown MODE flag

        /// <summary>
        /// Creates a new instances of the <see cref="UnknownUserModeMessage"/> class.
        /// </summary>
        public UnknownUserModeMessage ()
        {
            InternalNumeric = 501;
        }

        /// <exclude />
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter ("Unknown MODE flag");
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnUnknownUserMode (new IrcMessageEventArgs<UnknownUserModeMessage> (this));
        }

    }
}
