using System;
using System.Collections.Specialized;


namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// This message is sent directly after connecting, 
    /// giving the client information about the server software in use.
    /// </summary>
    [Serializable]
    public class YourHostMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="YourHostMessage"/> class.
        /// </summary>
        public YourHostMessage ()
        {
            InternalNumeric = 002;
        }

        /// <summary>
        /// Gets or sets the name of the software the server is running.
        /// </summary>
        public virtual string ServerName {
            get {
                return serverName;
            }
            set {
                serverName = value;
            }
        }
        string serverName = "";

        /// <summary>
        /// Gets or sets the version of the software the server is running.
        /// </summary>
        public virtual string Version {
            get {
                return version;
            }
            set {
                version = value;
            }
        }
        string version = "";

        const string yourHostIs = "Your host is ";
        const string runningVersion = ", running version ";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (yourHostIs + ServerName + runningVersion + Version);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            string reply = parameters [1];
            if (reply.IndexOf (yourHostIs, StringComparison.Ordinal) != -1 && reply.IndexOf (runningVersion, StringComparison.Ordinal) != -1) {
                int startOfServerName = yourHostIs.Length;
                int startOfVersion = reply.IndexOf (runningVersion, StringComparison.Ordinal) + runningVersion.Length;
                int lengthOfServerName = reply.IndexOf (runningVersion, StringComparison.Ordinal) - startOfServerName;

                ServerName = reply.Substring (startOfServerName, lengthOfServerName);
                Version = reply.Substring (startOfVersion);
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnYourHost (new IrcMessageEventArgs<YourHostMessage> (this));
        }

    }
}
