using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// This message is sent to all users with <see cref="Modes.ReceiveWallopsMode"/>,
    /// <see cref="Modes.NetworkOperatorMode"/>, or <see cref="Modes.ServerOperatorMode"/> user modes.
    /// </summary>
    [Serializable]
    public class WallopsMessage : CommandMessage
    {

        /// <summary>
        /// Gets or sets the text of the <see cref="WallopsMessage"/>.
        /// </summary>
        public virtual string Text {
            get {
                return _text;
            }
            set {
                _text = value;
            }
        }
        string _text = "";

        /// <summary>
        /// Gets the Irc command associated with this message.
        /// </summary>
        protected override string Command {
            get {
                return "WALLOPS";
            }
        }

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Text, true);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count >= 1) {
                Text = parameters [0];
            } else {
                Text = string.Empty;
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnWallops (new IrcMessageEventArgs<WallopsMessage> (this));
        }

    }
}
