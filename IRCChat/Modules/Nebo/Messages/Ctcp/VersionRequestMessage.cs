using System;


namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Sends a request for the version of the target's client code.
    /// </summary>
    [Serializable]
    public class VersionRequestMessage : CtcpRequestMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="VersionRequestMessage"/> class.
        /// </summary>
        public VersionRequestMessage ()
        {
            InternalCommand = "VERSION";
        }


        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnVersionRequest (new IrcMessageEventArgs<VersionRequestMessage> (this));
        }

    }
}
