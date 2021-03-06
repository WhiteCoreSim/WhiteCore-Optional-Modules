using System;
using System.Collections.Specialized;


namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The WelcomeMessage is sent from a server to a client as the first message 
    /// once the client is registered.
    /// </summary>
    [Serializable]
    public class WelcomeMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="WelcomeMessage"/> class.
        /// </summary>
        public WelcomeMessage ()
        {
            InternalNumeric = 001;
        }

        /// <summary>
        /// The content of the welcome message.
        /// </summary>
        public virtual string Text {
            get {
                return text;
            }
            set {
                text = value;
            }
        }
        string text = "";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Text);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count == 2) {
                Text = parameters [1];
            } else {
                Text = string.Empty;
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnWelcome (new IrcMessageEventArgs<WelcomeMessage> (this));
        }

    }
}
