using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{
    /// <summary>
    /// A Message which carries a ctcp command.
    /// </summary>
    [Serializable]
    public abstract class CtcpMessage : IrcMessage, IChannelTargetedMessage, IQueryTargetedMessage
    {

        /// <summary>
        /// Gets the targets of this <see cref="CtcpMessage"/>.
        /// </summary>
        public string Target {
            get {
                return target;
            }
            set {
                target = value;
            }
        }
        string target = "";

        /// <summary>
        /// Gets the ctcp Command requested.
        /// </summary>
        protected string InternalCommand {
            get {
                return internalCommand;
            }
            set {
                internalCommand = value;
            }
        }
        string internalCommand = "";

        /// <summary>
        /// Gets the data payload of the Ctcp request.
        /// </summary>
        protected abstract string ExtendedData {
            get;
        }

        /// <summary>
        /// Gets the irc command used to send the ctcp command to another user.
        /// </summary>
        protected abstract string TransportCommand {
            get;
        }

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (TransportCommand);
            writer.AddParameter (Target);
            string extendedData = CtcpUtil.Escape (ExtendedData);
            if (extendedData.Length > 0) {
                extendedData = " " + extendedData;
            }
            string payLoad = CtcpUtil.ExtendedDataMarker + InternalCommand + extendedData + CtcpUtil.ExtendedDataMarker;
            writer.AddParameter (payLoad);
        }

        /// <summary>
        /// Determines if the message can be parsed by this type.
        /// </summary>
        public override bool CanParse (string unparsedMessage)
        {
            if (!CtcpUtil.IsCtcpMessage (unparsedMessage)) {
                return false;
            }

            if (TransportCommand != CtcpUtil.GetTransportCommand (unparsedMessage)) {
                return false;
            }

            if (InternalCommand.Length != 0 && InternalCommand != CtcpUtil.GetInternalCommand (unparsedMessage)) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count > 0) {
                Target = parameters [0];
            } else {
                Target = "";
            }
        }


        #region IChannelTargetedMessage Members

        bool IChannelTargetedMessage.IsTargetedAtChannel (string channelName)
        {
            return IsTargetedAtChannel (channelName);
        }

        /// <summary>
        /// Determines if the the current message is targeted at the given channel.
        /// </summary>
        protected virtual bool IsTargetedAtChannel (string channelName)
        {
            return MessageUtil.IsIgnoreCaseMatch (Target, channelName);
        }

        #endregion

        #region IQueryTargetedMessage Members

        bool IQueryTargetedMessage.IsQueryToUser (User user)
        {
            return IsQueryToUser (user);
        }

        /// <summary>
        /// Determines if the current message is targeted at a query to the given user.
        /// </summary>
        protected virtual bool IsQueryToUser (User user)
        {
            return (MessageUtil.IsIgnoreCaseMatch (user.Nick, target));
        }

        #endregion
    }
}
