using System;


namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Sends a request for the script version of the target's client.
    /// </summary>
    [Serializable]
    public class ScriptRequestMessage : CtcpRequestMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="ScriptRequestMessage"/> class.
        /// </summary>
        public ScriptRequestMessage ()
        {
            InternalCommand = "SCRIPT";
        }


        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnScriptRequest (new IrcMessageEventArgs<ScriptRequestMessage> (this));
        }

    }
}
