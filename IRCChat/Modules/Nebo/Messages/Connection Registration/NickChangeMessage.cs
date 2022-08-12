using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// NickChangeMessage is used to give a user a nickname or change the previous one. 
    /// </summary>
    [Serializable]
    public class NickChangeMessage : CommandMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="NickChangeMessage"/> class.
        /// </summary>
        public NickChangeMessage()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="NickChangeMessage"/> class with the given nick.
        /// </summary>
        /// <param name="newNick"></param>
        public NickChangeMessage(string newNick)
        {
            _newNick = newNick;
        }

        /// <summary>
        /// Gets the Irc command associated with this message.
        /// </summary>
        protected override string Command {
            get {
                return "NICK";
            }
        }

        /// <summary>
        /// Gets or sets the nick requested by the sender.
        /// </summary>
        /// <remarks>
        /// Some servers limit you to 9 characters in you nick, while others allow more.
        /// Some servers will send a <see cref="SupportMessage"/> telling you the maximum nick length allowed.
        /// </remarks>
        public virtual string NewNick {
            get {
                return _newNick;
            }
            set {
                _newNick = value;
            }
        }
        string _newNick = "";


        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
        /// </summary>
        protected override void AddParametersToFormat(IrcMessageWriter writer)
        {
            base.AddParametersToFormat(writer);
            writer.AddParameter(NewNick);
        }


        /// <summary>
        /// Parses the given string to populate this <see cref="IrcMessage"/>.
        /// </summary>
        public override void Parse(string unparsedMessage)
        {
            base.Parse(unparsedMessage);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters(StringCollection parameters)
        {
            base.ParseParameters(parameters);
            if (parameters.Count >= 1) {
                NewNick = parameters[0];
            } else {
                NewNick = "";
            }
        }


        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify(MessageConduit conduit)
        {
            conduit.OnNickChange(new IrcMessageEventArgs<NickChangeMessage>(this));
        }

    }
}
