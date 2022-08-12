using System;


namespace MetaBuilders.Irc.Messages
{
    /// <summary>
    /// The response to a <see cref="PingRequestMessage"/>.
    /// </summary>
    [Serializable]
    public class PingReplyMessage : CtcpReplyMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="PingReplyMessage"/> class.
        /// </summary>
        public PingReplyMessage()
        {
            InternalCommand = "PING";
        }

        /// <summary>
        /// The timestamp of the ping reply.
        /// </summary>
        /// <remarks>
        /// When receiving a <see cref="PingRequestMessage"/>, this reply should send the same exact timestamp that the request had.
        /// This allows the requestor to substract the two to calculate the timespan to whatever degree
        /// of exactness that they want.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TimeStamp")]
        public virtual string TimeStamp {
            get {
                return timeStamp;
            }
            set {
                timeStamp = value;
            }
        }
        string timeStamp = "";

        /// <summary>
        /// Gets the data payload of the Ctcp request.
        /// </summary>
        protected override string ExtendedData {
            get {
                return timeStamp;
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify(MessageConduit conduit)
        {
            conduit.OnPingReply(new IrcMessageEventArgs<PingReplyMessage>(this));
        }

        /// <summary>
        /// Parses the given string to populate this <see cref="IrcMessage"/>.
        /// </summary>
        public override void Parse(string unparsedMessage)
        {
            base.Parse(unparsedMessage);
            TimeStamp = CtcpUtil.GetExtendedData(unparsedMessage);
        }

    }
}
