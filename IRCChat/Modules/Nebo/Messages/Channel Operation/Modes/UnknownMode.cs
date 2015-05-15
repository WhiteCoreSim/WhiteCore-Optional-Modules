using System;

namespace MetaBuilders.Irc.Messages.Modes {

	/// <summary>
	/// A channel mode sent in a <see cref="ChannelModeMessage"/> which is not known.
	/// </summary>
	public class UnknownChannelMode : ChannelMode {

		/// <summary>
		/// Creates a new instance of the <see cref="UnknownChannelMode"/> class with the given <see cref="ModeAction"/> and value.
		/// </summary>
		public UnknownChannelMode( ModeAction action, String symbol ) {
			this.Action = action;
			this.symbol = symbol;
		}

		/// <summary>
		/// Creates a new instance of the <see cref="UnknownChannelMode"/> class with the given <see cref="ModeAction"/>, value, and parameter.
		/// </summary>
		public UnknownChannelMode( ModeAction action, String symbol, String parameter ) {
			this.Action = action;
			this.symbol = symbol;
			this.parameter = parameter;
		}

		
		/// <summary>
		/// Gets or sets the parameter passed with this mode.
		/// </summary>
		public virtual String Parameter {
			get {
				return parameter;
			}
			set {
				parameter = value;
			}
		}

		/// <summary>
		/// Gets the irc string representation of the mode being changed or applied.
		/// </summary>
		protected override String Symbol {
			get {
				return symbol;
			}
		}


		String symbol;
		String parameter = "";

		/// <summary>
		/// Applies this mode to the ModeArguments property of the given <see cref="ChannelModeMessage" />.
		/// </summary>
		/// <param name="msg">The message which will be modified to include this mode.</param>
		protected override void AddParameter(ChannelModeMessage msg) {
			if ( Parameter.Length != 0 ) {
				msg.ModeArguments.Add(Parameter);
			}
		}


	}
}
