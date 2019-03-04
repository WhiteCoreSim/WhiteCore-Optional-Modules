using System;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// This is much like the <see cref="ChatMessage"/> message, 
    /// except the intent is to describe an "action" that the sender is doing.
    /// </summary>
    [Serializable]
    public class ActionRequestMessage : CtcpRequestMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="ActionRequestMessage"/> class.
        /// </summary>
        public ActionRequestMessage ()
        {
            InternalCommand = "ACTION";
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ActionRequestMessage"/> class with the given text.
        /// </summary>
        /// <param name="text">The text of the action.</param>
        public ActionRequestMessage (string text)
            : this ()
        {
            req_text = text;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ActionRequestMessage"/> class with the given text and target.
        /// </summary>
        /// <param name="text">The text of the action.</param>
        /// <param name="target">The target of the action.</param>
        public ActionRequestMessage (string text, string target)
            : this (text)
        {
            Target = target;
        }

        /// <summary>
        /// Gets or sets the communicated text of this <see cref="ActionRequestMessage"/>.
        /// </summary>
        public virtual string Text {
            get {
                return req_text;
            }
            set {
                req_text = value;
            }
        }
        string req_text = "";

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnActionRequest (new IrcMessageEventArgs<ActionRequestMessage> (this));
        }

        /// <summary>
        /// Gets the data payload of the Ctcp request.
        /// </summary>
        protected override string ExtendedData {
            get {
                return Text;
            }
        }

        /// <summary>
        /// Parses the given string to populate this <see cref="IrcMessage"/>.
        /// </summary>
        public override void Parse (string unparsedMessage)
        {
            base.Parse (unparsedMessage);
            Text = CtcpUtil.GetExtendedData (unparsedMessage);
        }

    }

}
