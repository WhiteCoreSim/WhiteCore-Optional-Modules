using System;
using System.Collections.Specialized;
using System.Globalization;
using MetaBuilders.Irc.Dcc;


namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// This message is a request to resume sending a file previously, but not completely sent to the requestor.
    /// </summary>
    [Serializable]
    public class DccResumeRequestMessage : CtcpRequestMessage
    {
        string fileName = "";
        int port = -1;
        int position = -1;

        /// <summary>
        /// Creates a new instance of the <see cref="DccResumeRequestMessage"/> class.
        /// </summary>
        public DccResumeRequestMessage ()
        {
            InternalCommand = "DCC";
        }

        /// <summary>
        /// Gets the data payload of the Ctcp request.
        /// </summary>
        protected override string ExtendedData {
            get {
                return MessageUtil.ParametersToString (
                    false,
                    DccCommand,
                    FileName,
                    Port.ToString (CultureInfo.InvariantCulture),
                    Position.ToString (CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Gets the dcc sub-command.
        /// </summary>
        protected virtual string DccCommand {
            get {
                return "RESUME";
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
        /// Gets or sets the port the connection should be on.
        /// </summary>
        public virtual int Port {
            get {
                return port;
            }
            set {
                port = value;
            }
        }

        /// <summary>
        /// Gets or sets the position in the file at which to resume sending.
        /// </summary>
        public virtual int Position {
            get {
                return position;
            }
            set {
                position = value;
            }
        }

        /// <summary>
        /// Determines if the message can be parsed by this type.
        /// </summary>
        public override bool CanParse (string unparsedMessage)
        {
            if (!base.CanParse (unparsedMessage)) {
                return false;
            }

            return CanParseDccCommand (DccUtil.GetCommand (unparsedMessage));
        }

        /// <summary>
        /// Determines if the message's DCC command is compatible with this message.
        /// </summary>
        public virtual bool CanParseDccCommand (string command)
        {
            if (command == null) {
                return false;
            }
            return DccCommand.EndsWith (command, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Parses the given string to populate this <see cref="IrcMessage"/>.
        /// </summary>
        public override void Parse (string unparsedMessage)
        {
            base.Parse (unparsedMessage);
            FileName = DccUtil.GetArgument (unparsedMessage);
            StringCollection p = DccUtil.GetParameters (unparsedMessage);
            Port = Convert.ToInt32 (p [2], CultureInfo.InvariantCulture);
            Position = Convert.ToInt32 (p [3], CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnDccResumeRequest (new IrcMessageEventArgs<DccResumeRequestMessage> (this));
        }

    }
}
