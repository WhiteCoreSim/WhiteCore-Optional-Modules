using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The reply to the <see cref="SilenceMessage"/> query.
    /// </summary>
    [Serializable]
    public class SilenceReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="SilenceReplyMessage"/>.
        /// </summary>
        public SilenceReplyMessage ()
        {
            InternalNumeric = 271;
        }

        /// <summary>
        /// Gets or sets the user being silenced.
        /// </summary>
        public virtual User SilencedUser {
            get {
                return silencedUser;
            }
            set {
                silencedUser = value;
            }
        }
        User silencedUser = new User ();

        /// <summary>
        /// Gets or sets the nick of the owner of the silence list
        /// </summary>
        public virtual string SilenceListOwner {
            get {
                return silenceListOwner;
            }
            set {
                silenceListOwner = value;
            }
        }
        string silenceListOwner = "";


        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (SilenceListOwner);
            writer.AddParameter (SilencedUser.ToString ());
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count > 2) {
                SilenceListOwner = parameters [1];
                SilencedUser = new User (parameters [2]);
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnSilenceReply (new IrcMessageEventArgs<SilenceReplyMessage> (this));
        }

    }
}
