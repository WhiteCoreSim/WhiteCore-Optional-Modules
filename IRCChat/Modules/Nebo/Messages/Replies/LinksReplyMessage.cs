using System;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The reply to the <see cref="LinksMessage"/> query.
    /// </summary>
    [Serializable]
    public class LinksReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="LinksReplyMessage"/>.
        /// </summary>
        public LinksReplyMessage ()
        {
            InternalNumeric = 364;
        }

        /// <summary>
        /// Gets or sets the mask which will limit the list of returned servers.
        /// </summary>
        public virtual string Mask {
            get {
                return mask;
            }
            set {
                mask = value;
            }
        }

        /// <summary>
        /// Gets or sets the server which should respond.
        /// </summary>
        /// <remarks>
        /// If empty, the current server is used.
        /// </remarks>
        public virtual string Server {
            get {
                return server;
            }
            set {
                server = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of hops from the answering server to the listed server.
        /// </summary>
        public virtual int HopCount {
            get {
                return hopCount;
            }
            set {
                hopCount = value;
            }
        }

        /// <summary>
        /// Gets or sets any additional server information.
        /// </summary>
        public virtual string ServerInfo {
            get {
                return serverInfo;
            }
            set {
                serverInfo = value;
            }
        }

        string mask = "";
        string server = "";
        int hopCount = -1;
        string serverInfo = "";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Mask);
            writer.AddParameter (Server);
            writer.AddParameter (HopCount.ToString (CultureInfo.InvariantCulture) + " " + ServerInfo);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count == 4) {
                Mask = parameters [1];
                Server = parameters [2];
                string trailing = parameters [3];
                string first = trailing.Substring (0, trailing.IndexOf (" ", StringComparison.Ordinal));
                HopCount = Convert.ToInt32 (first, CultureInfo.InvariantCulture);
                ServerInfo = trailing.Substring (first.Length);
            } else {
                Mask = "";
                Server = "";
                HopCount = -1;
                ServerInfo = "";
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnLinksReply (new IrcMessageEventArgs<LinksReplyMessage> (this));
        }

    }
}
