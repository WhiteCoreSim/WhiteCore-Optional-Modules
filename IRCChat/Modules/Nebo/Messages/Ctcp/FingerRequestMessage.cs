using System;


namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Requests the target's name and idle time.
    /// </summary>
    [Serializable]
    public class FingerRequestMessage : CtcpRequestMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="FingerRequestMessage"/> class.
        /// </summary>
        public FingerRequestMessage()
        {
            InternalCommand = "FINGER";
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify(MessageConduit conduit)
        {
            conduit.OnFingerRequest(new IrcMessageEventArgs<FingerRequestMessage>(this));
        }

    }
}
