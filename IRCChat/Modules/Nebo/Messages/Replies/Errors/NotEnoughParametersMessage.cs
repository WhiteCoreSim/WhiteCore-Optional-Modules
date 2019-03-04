using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The ErrorMessage sent when a command is sent which doesn't contain all the required parameters
    /// </summary>
    [Serializable]
    public class NotEnoughParametersMessage : ErrorMessage
    {
        //:irc.dkom.at 443 artificer KICK :not enough parameters

        /// <summary>
        /// Creates a new instances of the <see cref="NotEnoughParametersMessage"/> class.
        /// </summary>
        public NotEnoughParametersMessage ()
        {
            InternalNumeric = 461;
        }

        /// <summary>
        /// Gets or sets the command sent
        /// </summary>
        public string Command {
            get {
                return command;
            }
            set {
                command = value;
            }
        }
        string command;

        /// <exclude />
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Command);
            writer.AddParameter ("Not enough parameters");
        }

        /// <exclude />
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            Command = "";
            if (parameters.Count > 2) {
                Command = parameters [1];
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnNotEnoughParameters (new IrcMessageEventArgs<NotEnoughParametersMessage> (this));
        }

    }
}
