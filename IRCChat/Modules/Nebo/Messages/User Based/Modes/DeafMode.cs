using System;

namespace MetaBuilders.Irc.Messages.Modes {

	/// <summary>
	/// This mode signifies that the user does not receive channal chat messages
	/// </summary>
	public class DeafMode : UserMode {

		/// <summary>
		/// Creates a new instance of the <see cref="DeafMode"/> class.
		/// </summary>
		public DeafMode() {
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DeafMode"/> class with the given <see cref="ModeAction"/>.
		/// </summary>
		public DeafMode( ModeAction action )
		{
			this.Action = action;
		}

		/// <summary>
		/// Gets the irc string representation of the mode being changed or applied.
		/// </summary>
		protected override String Symbol {
			get {
				return "d";
			}
		}

	}
}
