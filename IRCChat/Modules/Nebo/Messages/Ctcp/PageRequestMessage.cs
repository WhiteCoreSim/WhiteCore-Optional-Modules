using System;


namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Sends a page request to the target.
    /// </summary>
    [Serializable]
    public class PageRequestMessage : CtcpRequestMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="PageRequestMessage"/> class.
        /// </summary>
        public PageRequestMessage()
        {
            InternalCommand = "PAGE";
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify(MessageConduit conduit)
        {
            conduit.OnPageRequest(new IrcMessageEventArgs<PageRequestMessage>(this));
        }

    }
}
