using System;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The base class for all numeric messages sent from the server to the client.
    /// </summary>
    [Serializable]
    public abstract class NumericMessage : IrcMessage
    {

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (internalNumeric.ToString ("000", CultureInfo.InvariantCulture));
            if (Target.Length != 0) {
                writer.AddParameter (Target);
            }
        }

        /// <summary>
        /// Gets or sets the target of the message.
        /// </summary>
        public virtual string Target {
            get {
                return target;
            }
            set {
                target = value;
            }
        }
        string target = "";

        /// <summary>
        /// Determines if the given numeric is an error message.
        /// </summary>
        public static bool IsError (int numeric)
        {
            int normalStart = 400;
            int normalEnd = 599;
            int ircxStart = 900;
            int ircxEnd = 998;
            return ((normalStart <= numeric && numeric <= normalEnd) || (ircxStart <= numeric && numeric <= ircxEnd));
        }

        /// <summary>
        /// Determines if the given numeric is a command reply message.
        /// </summary>
        public static bool IsCommandReply (int numeric)
        {
            return (!IsError (numeric) && !IsDirect (numeric));
        }

        /// <summary>
        /// Determines if the given numeric is a direct message.
        /// </summary>
        public static bool IsDirect (int numeric)
        {
            return (0 < numeric && numeric < 100);
        }

        /// <summary>
        /// Gets the Numeric command of the Message
        /// </summary>
        public virtual int InternalNumeric {
            get {
                return internalNumeric;
            }
            protected set {
                internalNumeric = value;
            }
        }
        int internalNumeric = -1;

        /// <summary>
        /// Determines if the message can be parsed by this type.
        /// </summary>
        public override bool CanParse (string unparsedMessage)
        {
            double foo;
            if (double.TryParse (MessageUtil.GetCommand (unparsedMessage), NumberStyles.Integer, null, out foo)) {
                if (0 <= foo && foo < 1000) {
                    if (InternalNumeric == -1) {
                        return true;
                    }
                    int parsedNumeric = Convert.ToInt32 (foo);
                    return (InternalNumeric == parsedNumeric);
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count > 0) {
                Target = parameters [0];
            } else {
                Target = "";
            }
        }

    }

}
