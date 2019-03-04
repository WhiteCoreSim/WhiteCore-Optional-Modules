using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The error recieved when a message containing target parameters has too many targets specified.
    /// </summary>
    [Serializable]
    public class TooManyTargetsMessage : ErrorMessage
    {

        /// <summary>
        /// Creates a new instances of the <see cref="TooManyTargetsMessage"/> class.
        /// </summary>
        public TooManyTargetsMessage ()
        {
            InternalNumeric = 407;
        }

        /// <summary>
        /// Gets or sets the target which was invalid.
        /// </summary>
        public virtual string InvalidTarget {
            get {
                return invalidTarget;
            }
            set {
                invalidTarget = value;
            }
        }
        string invalidTarget = "";

        /// <summary>
        /// Gets or sets the errorcode
        /// </summary>
        /// <remarks>An example error code might be, "Duplicate"</remarks>
        public virtual string ErrorCode {
            get {
                return errorCode;
            }
            set {
                errorCode = value;
            }
        }
        string errorCode = "";

        /// <summary>
        /// Gets or sets the message explaining what was done about the error.
        /// </summary>
        public virtual string AbortMessage {
            get {
                return abortMessage;
            }
            set {
                abortMessage = value;
            }
        }
        string abortMessage = "";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (InvalidTarget);
            writer.AddParameter (ErrorCode + " recipients. " + AbortMessage);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);

            InvalidTarget = "";
            ErrorCode = "";
            AbortMessage = "";

            if (parameters.Count > 1) {
                InvalidTarget = parameters [1];
                if (parameters.Count > 2) {
                    string [] messagePieces = System.Text.RegularExpressions.Regex.Split (parameters [2], " recipients.");
                    if (messagePieces.Length == 2) {
                        ErrorCode = messagePieces [0];
                        AbortMessage = messagePieces [1];
                    }
                }
            }
        }

        // "<target> :<error code> recipients. <abort message>"

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnTooManyTargets (new IrcMessageEventArgs<TooManyTargetsMessage> (this));
        }

    }
}
