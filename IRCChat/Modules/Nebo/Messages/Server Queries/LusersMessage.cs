using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Requests that the server send information about the size of the IRC network.
    /// </summary>
    [Serializable]
    public class LusersMessage : ServerQueryBase
    {

        /// <summary>
        /// Gets the Irc command associated with this message.
        /// </summary>
        protected override string Command {
            get {
                return "LUSERS";
            }
        }

        /// <summary>
        /// Gets or sets the mask that limits the servers which information will be returned.
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
                writer.AddParameter (Mask);
                writer.AddParameter (Target);
            }
        }

        /// <summary>
        /// Gets the index of the parameter which holds the server which should respond to the query.
        /// </summary>
        protected override int TargetParsingPosition {
            get {
                return 1;
            }
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count >= 1) {
                Mask = parameters [0];
            } else {
                Mask = "";
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnLusers (new IrcMessageEventArgs<LusersMessage> (this));
        }

    }
}
