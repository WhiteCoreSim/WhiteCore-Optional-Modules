using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Requests information about the given user or users.
    /// </summary>
    [Serializable]
    public class WhoMessage : CommandMessage
    {

        /// <summary>
        /// Gets or sets the mask which is matched for users to return information about.
        /// </summary>
        public virtual User Mask {
            get {
                return mask;
            }
            set {
                mask = value;
            }
        }
        User mask = new User ();

        /// <summary>
        /// Gets or sets if the results should only contain irc operators.
        /// </summary>
        public virtual bool RestrictToOps {
            get {
                return restrictToOps;
            }
            set {
                restrictToOps = value;
            }
        }

        /// <summary>
        /// Gets the Irc command associated with this message.
        /// </summary>
        protected override string Command {
            get {
                return "WHO";
            }
        }

        bool restrictToOps = false;

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Mask.ToString ());
            if (restrictToOps) {
                writer.AddParameter ("o");
            }
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            Mask = new User ();
            if (parameters.Count >= 1) {
                Mask.Nick = parameters [0];
                RestrictToOps = (parameters.Count > 1 && parameters [1] == "o");
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnWho (new IrcMessageEventArgs<WhoMessage> (this));
        }

    }
}
