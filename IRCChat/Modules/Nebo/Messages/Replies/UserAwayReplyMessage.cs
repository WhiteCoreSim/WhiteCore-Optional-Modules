using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The message is received by a client from a server 
    /// when they attempt to send a message to a user who
    /// is marked as away using the <see cref="AwayMessage"/>.
    /// </summary>
    [Serializable]
    public class UserAwayMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="UserAwayMessage"/>.
        /// </summary>
        public UserAwayMessage ()
        {
            InternalNumeric = 301;
        }

        /// <summary>
        /// Gets or sets the user's away message.
        /// </summary>
        public virtual string Text {
            get { return text; }
            set { text = value; }
        }

        /// <summary>
        /// Gets or sets the nick of the user who is away.
        /// </summary>
        public virtual string Nick {
            get { return nick; }
            set { nick = value; }
        }

        string text = "";
        string nick = "";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Nick);
            writer.AddParameter (Text);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count > 2) {
                Nick = parameters [1];
                Text = parameters [2];
            } else {
                Nick = string.Empty;
                Text = string.Empty;
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnUserAway (new IrcMessageEventArgs<UserAwayMessage> (this));
        }

    }
}
