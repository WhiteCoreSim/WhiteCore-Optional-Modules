using System;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// A reply to a <see cref="IrcxMessage"/> or a <see cref="IsIrcxMessage"/>.
    /// </summary>
    [Serializable]
    public class IrcxReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="IrcxReplyMessage"/>.
        /// </summary>
        public IrcxReplyMessage ()
        {
            InternalNumeric = 800;
        }

        /// <summary>
        /// Gets or sets if the server has set the client into ircx mode.
        /// </summary>
        public virtual bool IsIrcxClientMode {
            get {
                return isIrcxClientMode;
            }
            set {
                isIrcxClientMode = value;
            }
        }
        bool isIrcxClientMode = false;

        /// <summary>
        /// Gets or sets the version of Ircx the server implements.
        /// </summary>
        public virtual string Version {
            get {
                return ircxVersion;
            }
            set {
                ircxVersion = value;
            }
        }
        string ircxVersion = "";

        /// <summary>
        /// Gets the collection of authentication packages
        /// </summary>
        public virtual StringCollection AuthenticationPackages {
            get {
                return authenticationPackages;
            }
        }
        StringCollection authenticationPackages = new StringCollection ();

        /// <summary>
        /// Gets or sets the maximum message length, in bytes.
        /// </summary>
        public virtual int MaximumMessageLength {
            get {
                return maximumMessageLength;
            }
            set {
                maximumMessageLength = value;
            }
        }
        int maximumMessageLength = -1;

        /// <summary>
        /// Gets or sets the tokens
        /// </summary>
        /// <remarks>
        /// There are no known servers that implement this property.
        /// It is almost always just *.
        /// </remarks>
        public virtual string Tokens {
            get {
                return tokens;
            }
            set {
                tokens = value;
            }
        }
        string tokens = "*";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            if (IsIrcxClientMode) {
                writer.AddParameter ("1");
            } else {
                writer.AddParameter ("0");
            }
            writer.AddParameter (Version);
            writer.AddList (AuthenticationPackages, ",", false);
            writer.AddParameter (MaximumMessageLength.ToString (CultureInfo.InvariantCulture));
            writer.AddParameter (Tokens);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count >= 5) {
                IsIrcxClientMode = (parameters [1] == "1");
                Version = parameters [2];
                AuthenticationPackages.Clear ();
                foreach (string package in parameters [3].Split (',')) {
                    AuthenticationPackages.Add (package);
                }
                MaximumMessageLength = int.Parse (parameters [4], CultureInfo.InvariantCulture);
                if (parameters.Count == 6) {
                    Tokens = parameters [5];
                } else {
                    Tokens = "";
                }
            } else {
                IsIrcxClientMode = false;
                Version = "";
                AuthenticationPackages.Clear ();
                MaximumMessageLength = -1;
                Tokens = "";
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnIrcxReply (new IrcMessageEventArgs<IrcxReplyMessage> (this));
        }

    }
}
