using System;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// A Monitor system message giving you the list of users on your monitor list.
    /// </summary>
    /// <remarks>
    /// You may receive more than 1 of these replies in response to a <see cref="T:MonitorListRequestMessage" />.
    /// </remarks>
    [Serializable]
    public class MonitorListReplyMessage : MonitoredNicksListMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="MonitorListReplyMessage"/>.
        /// </summary>
        public MonitorListReplyMessage ()
        {
            InternalNumeric = 732;
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnMonitorListReply (new IrcMessageEventArgs<MonitorListReplyMessage> (this));
        }

    }
}
