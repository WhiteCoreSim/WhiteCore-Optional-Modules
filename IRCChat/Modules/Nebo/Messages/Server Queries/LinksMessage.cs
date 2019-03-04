using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The <see cref="LinksMessage" /> asks the server to send a list all servers which are known by the server answering the message.
    /// </summary>
    [Serializable]
    public class LinksMessage : ServerQueryBase
    {

        /// <summary>
        /// Gets the Irc command associated with this message.
        /// </summary>
        protected override string Command {
            get {
                return "LINKS";
            }
        }

        /// <summary>
        /// Gets or sets the mask for server info to limit the list or replies.
        /// </summary>
        public virtual string Mask {
            get {
                return mask;
            }
            set {
                mask = value;
            }
        }
        string mask = "";


        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            if (Mask != null && Mask.Length != 0) {
                writer.AddParameter (Target);
                writer.AddParameter (Mask);
            }
        }


        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count >= 2) {
                Mask = parameters [1];
            } else {
                Mask = "";
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnLinks (new IrcMessageEventArgs<LinksMessage> (this));
        }

    }
}
