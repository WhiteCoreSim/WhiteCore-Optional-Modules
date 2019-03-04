using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{
    /// <summary>
    /// The base class for server query messages.
    /// </summary>
    [Serializable]
    public abstract class ServerQueryBase : CommandMessage
    {

        /// <summary>
        /// Gets or sets the target server of the query.
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
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count >= TargetParsingPosition + 1) {
                Target = parameters [TargetParsingPosition];
            } else {
                Target = "";
            }
        }

        /// <summary>
        /// Gets the index of the parameter which holds the server which should respond to the query.
        /// </summary>
        protected virtual int TargetParsingPosition {
            get {
                return 0;
            }
        }

    }
}
