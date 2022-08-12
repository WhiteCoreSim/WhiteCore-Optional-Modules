using System;


namespace MetaBuilders.Irc.Messages
{
    /// <summary>
    /// Sends a request for the current time on the target's machine.
    /// </summary>
    [Serializable]
    public class TimeRequestMessage : CtcpRequestMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="TimeRequestMessage"/> class.
        /// </summary>
        public TimeRequestMessage()
        {
            InternalCommand = "TIME";
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify(MessageConduit conduit)
        {
            conduit.OnTimeRequest(new IrcMessageEventArgs<TimeRequestMessage>(this));
        }

    }
}
