using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{
    /// <summary>
    /// The PasswordMessage is used to set a 'connection password'.
    /// </summary>
    /// <remarks>
    /// The password can and must be set before any attempt to register the connection is made. 
    /// Currently this requires that clients send a PASS command before sending the NICK/USER combination
    /// and servers *must* send a PASS command before any SERVER command. 
    /// The password supplied must match the one contained in the C/N lines (for servers) or I lines (for clients). 
    /// It is possible to send multiple PASS commands before registering 
    /// but only the last one sent is used for verification and it may not be changed once registered.
    /// </remarks>
    [Serializable]
    public class PasswordMessage : CommandMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="PasswordMessage"/> class.
        /// </summary>
        public PasswordMessage()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PasswordMessage"/> class with the given password.
        /// </summary>
        public PasswordMessage(string password)
        {
            msg_password = password;
        }

        /// <summary>
        /// Gets the Irc command associated with this message.
        /// </summary>
        protected override string Command {
            get {
                return "PASS";
            }
        }

        /// <summary>
        /// Gets or sets the password for the sender.
        /// </summary>
        public virtual string Password {
            get {
                return msg_password;
            }
            set {
                msg_password = value;
            }
        }
        string msg_password = "";


        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
        /// </summary>
        protected override void AddParametersToFormat(IrcMessageWriter writer)
        {
            base.AddParametersToFormat(writer);
            writer.AddParameter(Password);
        }


        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters(StringCollection parameters)
        {
            base.ParseParameters(parameters);
            if (parameters.Count >= 1) {
                Password = parameters[0];
            } else {
                Password = "";
            }
        }


        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify(MessageConduit conduit)
        {
            conduit.OnPassword(new IrcMessageEventArgs<PasswordMessage>(this));
        }

    }

}
