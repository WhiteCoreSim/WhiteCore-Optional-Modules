using System;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// An unknown <see cref="CtcpRequestMessage"/>.
    /// </summary>
    [Serializable]
    public class GenericCtcpRequestMessage : CtcpRequestMessage
    {

        /// <summary>
        /// Gets or sets the information packaged with the ctcp command.
        /// </summary>
        public virtual string DataPackage {
            get {
                return dataPackage;
            }
            set {
                dataPackage = value;
            }
        }
        string dataPackage = "";


        /// <summary>
        /// Gets the data payload of the Ctcp request.
        /// </summary>
        protected override string ExtendedData {
            get {
                return dataPackage;
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify(MessageConduit conduit)
        {
            conduit.OnGenericCtcpRequest(new IrcMessageEventArgs<GenericCtcpRequestMessage>(this));
        }

        /// <summary>
        /// Gets or sets the Ctcp command.
        /// </summary>
        public virtual string Command {
            get {
                return InternalCommand;
            }
            set {
                InternalCommand = value;
            }
        }

        /// <summary>
        /// Parses the given string to populate this <see cref="IrcMessage"/>.
        /// </summary>
        public override void Parse(string unparsedMessage)
        {
            base.Parse(unparsedMessage);
            Command = CtcpUtil.GetInternalCommand(unparsedMessage);
            DataPackage = CtcpUtil.GetExtendedData(unparsedMessage);
        }

    }

}
