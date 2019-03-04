using System;
using System.Globalization;
using MetaBuilders.Irc.Dcc;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// This message is a request to send a file directly from the sender of the request to the receiver.
    /// </summary>
    [Serializable]
    public class DccSendRequestMessage : DccRequestMessage
    {
        string fileName = "";
        int size = -1;
        bool secure = false;
        bool turboMode = false;

        /// <summary>
        /// Creates a new instance of the <see cref="DccSendRequestMessage"/> class.
        /// </summary>
        public DccSendRequestMessage ()
        {
        }

        /// <summary>
        /// Gets the data payload of the Ctcp request.
        /// </summary>
        protected override string ExtendedData {
            get {
                return base.ExtendedData + " " + Size.ToString (CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets the dcc sub-command.
        /// </summary>
        protected override string DccCommand {
            get {
                string result = "SEND";
                if (Secure) {
                    result = "S" + result;
                }
                if (TurboMode) {
                    result = "T" + result;
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the dcc sub-command's argument.
        /// </summary>
        protected override string DccArgument {
            get {
                return FileName;
            }
        }

        /// <summary>
        /// Gets or sets the name of the file being sent.
        /// </summary>
        public virtual string FileName {
            get {
                return fileName;
            }
            set {
                fileName = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the file being sent.
        /// </summary>
        public virtual int Size {
            get {
                return size;
            }
            set {
                size = value;
            }
        }

        /// <summary>
        /// Gets or sets if the dcc connection should use turbo mode.
        /// </summary>
        public virtual bool TurboMode {
            get {
                return turboMode;
            }
            set {
                turboMode = value;
            }
        }

        /// <summary>
        /// Gets or sets if the dcc connection should use SSL.
        /// </summary>
        public virtual bool Secure {
            get {
                return secure;
            }
            set {
                secure = value;
            }
        }

        /// <summary>
        /// Determines if the message's DCC command is compatible with this message.
        /// </summary>
        public override bool CanParseDccCommand (string command)
        {
            if (command == null) {
                return false;
            }
            return command.EndsWith ("SEND", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Parses the given string to populate this <see cref="IrcMessage"/>.
        /// </summary>
        public override void Parse (string unparsedMessage)
        {
            base.Parse (unparsedMessage);
            FileName = DccUtil.GetArgument (unparsedMessage);
            Size = Convert.ToInt32 (DccUtil.GetParameters (unparsedMessage) [4], CultureInfo.InvariantCulture);
            string unparsedCommand = DccUtil.GetCommand (unparsedMessage).ToUpperInvariant ();
            string commandExtenstion = unparsedCommand.Substring (0, unparsedCommand.Length - 4);
            TurboMode = commandExtenstion.IndexOf ("T", StringComparison.Ordinal) >= 0;
            Secure = commandExtenstion.IndexOf ("S", StringComparison.Ordinal) >= 0;
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnDccSendRequest (new IrcMessageEventArgs<DccSendRequestMessage> (this));
        }

    }
}
