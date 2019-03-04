
namespace MetaBuilders.Irc.Messages.Modes
{

    /// <summary>
    /// This mode sets or unsets a password on a channel.
    /// </summary>
    public class KeyMode : FlagMode
    {

        /// <summary>
        /// Creates a new instance of the <see cref="KeyMode"/> class.
        /// </summary>
        public KeyMode ()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="KeyMode"/> class with the given <see cref="ModeAction"/>.
        /// </summary>
        public KeyMode (ModeAction action)
        {
            Action = action;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="KeyMode"/> class 
        /// with the given <see cref="ModeAction"/> and password.
        /// </summary>
        public KeyMode (ModeAction action, string password)
        {
            Action = action;
            key_password = password;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="KeyMode"/> class 
        /// with the given password.
        /// </summary>
        public KeyMode (string password)
        {
            key_password = password;
        }

        /// <summary>
        /// Gets the irc string representation of the mode being changed or applied.
        /// </summary>
        protected override string Symbol {
            get {
                return "k";
            }
        }

        /// <summary>
        /// Gets or sets the password needed to gain access to a channel.
        /// </summary>
        public virtual string Password {
            get {
                return key_password;
            }
            set {
                key_password = value;
            }
        }
        string key_password = "";

        /// <summary>
        /// Applies this mode to the ModeArguments property of the given <see cref="ChannelModeMessage" />.
        /// </summary>
        /// <param name="msg">The message which will be modified to include this mode.</param>
        protected override void AddParameter (ChannelModeMessage msg)
        {
            if (Password.Length != 0) {
                msg.ModeArguments.Add (Password);
            }
        }
    }
}
