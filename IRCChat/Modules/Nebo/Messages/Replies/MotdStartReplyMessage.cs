using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{


    /// <summary>
    /// Signifies the start of the motd sent by the server.
    /// </summary>
    [Serializable]
    public class MotdStartReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="MotdStartReplyMessage"/> class.
        /// </summary>
        public MotdStartReplyMessage ()
        {
            InternalNumeric = 375;
        }

        string info;

        /// <summary>
        /// Gets or sets the info included in the message.
        /// </summary>
        public string Info {
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
            writer.AddParameter (Info);
        }

        /// <summary>
        /// Overrides <see cref="IrcMessage.ParseParameters" />
        /// </summary>
        /// <param name="parameters"></param>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            info = parameters [parameters.Count - 1];
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnMotdStartReply (new IrcMessageEventArgs<MotdStartReplyMessage> (this));
        }

    }
}
