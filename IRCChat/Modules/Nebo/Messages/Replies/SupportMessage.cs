using System;
using System.Collections.Specialized;


namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// This is a message sent from a server to a client upon connection 
    /// to tell the client what irc features the server supports.
    /// </summary>
    [Serializable]
    public class SupportMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="SupportMessage"/> class.
        /// </summary>
        public SupportMessage ()
        {
            InternalNumeric = 005;
        }

        /// <summary>
        /// Gets the list of items supported by the server.
        /// </summary>
        public virtual NameValueCollection SupportedItems {
            get {
                return supportedItems;
            }
        }
        NameValueCollection supportedItems = new NameValueCollection ();

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);

            StringCollection paramsToString = new StringCollection ();
            foreach (string name in SupportedItems.Keys) {
                string value = SupportedItems [name];
                if (value.Length != 0) {
                    paramsToString.Add (name + "=" + SupportedItems [name]);
                } else {
                    paramsToString.Add (name);
                }
            }
            writer.AddList (paramsToString, " ", false);
            writer.AddParameter (areSupported);
        }

        string areSupported = "are supported by this server";

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            for (int i = 1; i < parameters.Count - 1; i++) {
                string nameValue = parameters [i];
                string name;
                string value;
                int indexOfEquals = nameValue.IndexOf ("=", StringComparison.Ordinal);
                if (indexOfEquals > 0) {
                    name = nameValue.Substring (0, indexOfEquals);
                    value = nameValue.Substring (indexOfEquals + 1);
                } else {
                    name = nameValue;
                    value = "";
                }
                SupportedItems [name] = value;
            }
        }

        /// <summary>
        /// Determines if the message can be parsed by this type.
        /// </summary>
        public override bool CanParse (string unparsedMessage)
        {
            if (unparsedMessage == null) {
                return false;
            }
            return (base.CanParse (unparsedMessage) && unparsedMessage.IndexOf (areSupported, StringComparison.Ordinal) > 0);
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnSupport (new IrcMessageEventArgs<SupportMessage> (this));
        }

    }
}
