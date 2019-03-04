using System;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// One of the responses to the <see cref="LusersMessage"/> query.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Op"), Serializable]
    public class LusersOpReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="LusersOpReplyMessage"/> class
        /// </summary>
        public LusersOpReplyMessage ()
        {
            InternalNumeric = 252;
        }

        /// <summary>
        /// Gets or sets the number of irc operators connected to the server.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Op")]
        public virtual int OpCount {
            get {
                return opCount;
            }
            set {
                opCount = value;
            }
        }

        /// <summary>
        /// Gets or sets any additionaly information about the operators connected.
        /// </summary>
        public virtual string Info {
            get {
                return info;
            }
            set {
                info = value;
            }
        }

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (OpCount.ToString (CultureInfo.InvariantCulture));
            writer.AddParameter (Info);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count > 2) {
                OpCount = Convert.ToInt32 (parameters [1], CultureInfo.InvariantCulture);
                Info = parameters [2];
            } else {
                OpCount = -1;
                Info = "";
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnLusersOpReply (new IrcMessageEventArgs<LusersOpReplyMessage> (this));
        }

        int opCount = -1;
        string info = "";
    }
}
