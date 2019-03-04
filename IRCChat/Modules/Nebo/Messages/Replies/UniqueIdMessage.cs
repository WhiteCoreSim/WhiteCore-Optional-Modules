using System;
using System.Collections.Specialized;
using System.Diagnostics;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// This message is sent by the server early during connection, and tells the user the alpha-numeric id the server uses to identify the user.
    /// </summary>
    [Serializable]
    public class UniqueIdMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="UniqueIdMessage"/> class.
        /// </summary>
        public UniqueIdMessage ()
        {
            InternalNumeric = 042;
        }

        /// <summary>
        /// Gets or sets the alpha-numeric id the server uses to identify the client.
        /// </summary>
        public string UniqueId {
            get;
            set;
        }

        string yourUniqueID = "your unique ID";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            // :irc.inter.net.il 042 NeboBot NeboBot 5ILABFZUY :your unique ID

            base.AddParametersToFormat (writer);
            writer.AddParameter (Target);
            writer.AddParameter (UniqueId);
            writer.AddParameter (yourUniqueID);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            // :irc.inter.net.il 042 NeboBot NeboBot 5ILABFZUY :your unique ID

            base.ParseParameters (parameters);
            if (parameters.Count == 4) {
                UniqueId = parameters [2];
            } else {
                UniqueId = "";
                Trace.WriteLine ("Unknown format of UniqueIDMessage parameters: '" + MessageUtil.ParametersToString (parameters) + "'");
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnUniqueId (new IrcMessageEventArgs<UniqueIdMessage> (this));
        }

    }
}
