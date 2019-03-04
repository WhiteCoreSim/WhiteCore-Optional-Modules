using System;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Requests that the server send its Message Of The Day to the client.
    /// </summary>
    [Serializable]
    public class MotdMessage : ServerQueryBase
    {

        /// <summary>
        /// Gets the Irc command associated with this message.
        /// </summary>
        protected override string Command {
            get {
                return "MOTD";
            }
        }

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Target);
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnMotd (new IrcMessageEventArgs<MotdMessage> (this));
        }

    }
}
