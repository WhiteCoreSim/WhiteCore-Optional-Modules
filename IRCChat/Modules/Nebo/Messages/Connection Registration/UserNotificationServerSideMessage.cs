using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{
    /// <summary>
    /// The UserNotificationServerSideMessage is passed between servers to notify of a new user on the network.
    /// </summary>
    [Serializable]
    public class UserNotificationServerSideMessage : CommandMessage
    {

        /// <summary>
        /// Gets the Irc command associated with this message.
        /// </summary>
        protected override string Command {
            get {
                return "USER";
            }
        }

        /// <summary>
        /// Gets or sets the UserName of client.
        /// </summary>
        public virtual string UserName {
            get {
                return userName;
            }
            set {
                userName = value;
            }
        }
        string userName = "";

        /// <summary>
        /// Gets or sets the name of the user's host.
        /// </summary>
        public string HostName {
            get {
                return hostName;
            }
            set {
                hostName = value;
            }
        }
        string hostName;

        /// <summary>
        /// Gets or sets the name of the server which the user is on.
        /// </summary>
        public string ServerName {
            get {
                return serverName;
            }
            set {
                serverName = value;
            }
        }
        string serverName;

        /// <summary>
        /// Gets or sets the real name of the client.
        /// </summary>
        public virtual string RealName {
            get {
                return realName;
            }
            set {
                realName = value;
            }
        }
        string realName = "";

        /// <exclude />
        public override bool CanParse(string unparsedMessage)
        {
            if (!base.CanParse(unparsedMessage)) {
                return false;
            }
            StringCollection p = MessageUtil.GetParameters(unparsedMessage);
            if (p.Count != 4 || p[2] == "*") {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
        /// </summary>
        protected override void AddParametersToFormat(IrcMessageWriter writer)
        {
            base.AddParametersToFormat(writer);
            writer.AddParameter(UserName);
            writer.AddParameter(HostName);
            writer.AddParameter(ServerName);
            writer.AddParameter(RealName);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters(StringCollection parameters)
        {
            base.ParseParameters(parameters);
            if (parameters.Count >= 4) {
                UserName = parameters[0];
                HostName = parameters[1];
                ServerName = parameters[2];
                RealName = parameters[3];
            } else {
                UserName = "";
                HostName = "";
                ServerName = "";
                RealName = "";
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify(MessageConduit conduit)
        {
            conduit.OnUserNotificationServerSide(new IrcMessageEventArgs<UserNotificationServerSideMessage>(this));
        }

    }
}
