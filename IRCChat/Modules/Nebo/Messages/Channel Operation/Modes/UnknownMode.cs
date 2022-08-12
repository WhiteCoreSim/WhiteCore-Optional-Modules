
namespace MetaBuilders.Irc.Messages.Modes
{

    /// <summary>
    /// A channel mode sent in a <see cref="ChannelModeMessage"/> which is not known.
    /// </summary>
    public class UnknownChannelMode : ChannelMode
    {

        /// <summary>
        /// Creates a new instance of the <see cref="UnknownChannelMode"/> class with the given <see cref="ModeAction"/> and value.
        /// </summary>
        public UnknownChannelMode(ModeAction action, string symbol)
        {
            Action = action;
            mode_symbol = symbol;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="UnknownChannelMode"/> class with the given <see cref="ModeAction"/>, value, and parameter.
        /// </summary>
        public UnknownChannelMode(ModeAction action, string symbol, string parameter)
        {
            Action = action;
            mode_symbol = symbol;
            mode_parameter = parameter;
        }


        /// <summary>
        /// Gets or sets the parameter passed with this mode.
        /// </summary>
        public virtual string Parameter {
            get {
                return mode_parameter;
            }
            set {
                mode_parameter = value;
            }
        }

        /// <summary>
        /// Gets the irc string representation of the mode being changed or applied.
        /// </summary>
        protected override string Symbol {
            get {
                return mode_symbol;
            }
        }


        string mode_symbol;
        string mode_parameter = "";

        /// <summary>
        /// Applies this mode to the ModeArguments property of the given <see cref="ChannelModeMessage" />.
        /// </summary>
        /// <param name="msg">The message which will be modified to include this mode.</param>
        protected override void AddParameter(ChannelModeMessage msg)
        {
            if (Parameter.Length != 0) {
                msg.ModeArguments.Add(Parameter);
            }
        }


    }
}
