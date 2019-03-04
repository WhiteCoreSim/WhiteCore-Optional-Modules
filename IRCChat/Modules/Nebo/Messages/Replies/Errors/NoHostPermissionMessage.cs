using System;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Returned to a client which attempts to register with a server which has not been setup to allow connections from which the host attempted connection.
    /// </summary>
    [Serializable]
    public class NoHostPermissionMessage : ErrorMessage
    {

        /// <summary>
        /// Creates a new instances of the <see cref="NoHostPermissionMessage"/> class.
        /// </summary>
        public NoHostPermissionMessage ()
        {
            InternalNumeric = 463;
        }

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter ("Your host isn't among the privileged");
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnNoHostPermission (new IrcMessageEventArgs<NoHostPermissionMessage> (this));
        }

    }
}
