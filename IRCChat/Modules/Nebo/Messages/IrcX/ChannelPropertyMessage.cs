using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// With the ChannelPropertyMessage, clients can read and write property values for IrcX enabled channels.
    /// </summary>
    /// <remarks>
    /// <p>
    /// To read all channel proprties for a channel, simply set the <see cref="Channel"/> property.
    /// To read a channel property, set  <see cref="Channel"/> and <see cref="Prop"/>.
    /// To write a channel property, set the <see cref="Channel"/>, <see cref="Prop"/>, and <see cref="NewValue"/> properties. When a server sets the property, the client will receive the same property message back.
    /// </p>
    /// <p>This command is only effective for an IrcX enabled server.</p>
    /// </remarks>
    [Serializable]
    public class ChannelPropertyMessage : CommandMessage
    {

        /// <summary>
        /// Gets the Irc command associated with this message.
        /// </summary>
        protected override string Command {
            get {
                return "PROP";
            }
        }

        /// <summary>
        /// Gets or sets the channel being targeted.
        /// </summary>
        /// <remarks>
        /// Some implementations allow for this to be the name of a server, but this is an extension.
        /// </remarks>
        public virtual string Channel {
            get {
                return channel;
            }
            set {
                channel = value;
            }
        }
        String channel = "";

        /// <summary>
        /// Gets or sets the channel property being targeted.
        /// </summary>
        /// <remarks>
        /// When this message is sent with an empty <see cref="Prop"/>, the values of all current channel properties are sent from the server.
        /// </remarks>
        public virtual string Prop {
            get {
                return property;
            }
            set {
                property = value;
            }
        }
        string property = "";

        /// <summary>
        /// Gets or sets the value being applied to the target channel property.
        /// </summary>
        /// <remarks>
        /// You can set the value of a channel property by specify its name in the <see cref="Prop"/> property, and the value in the <see cref="NewValue"/> property.
        /// </remarks>
        public virtual string NewValue {
            get {
                return newValue;
            }
            set {
                newValue = value;
            }
        }
        string newValue = "";

        /// <summary>
        /// Validates this message against the given server support
        /// </summary>
        public override void Validate (ServerSupport serverSupport)
        {
            base.Validate (serverSupport);
            Channel = MessageUtil.EnsureValidChannelName (Channel, serverSupport);
        }

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (this.Channel);
            if (Prop.Length == 0) {
                writer.AddParameter ("*");
            } else {
                writer.AddParameter (Prop);
                if (NewValue.Length != 0) {
                    writer.AddParameter (NewValue);
                }
            }
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);

            Channel = "";
            Prop = "";
            NewValue = "";

            if (parameters.Count > 0) {
                Channel = parameters [0];
                if (parameters.Count > 1) {
                    Prop = parameters [1];
                    if (Prop == "*") {
                        Prop = "";
                    }
                    if (parameters.Count > 2) {
                        NewValue = parameters [2];
                    }
                }
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnChannelProperty (new IrcMessageEventArgs<ChannelPropertyMessage> (this));
        }

    }
}
