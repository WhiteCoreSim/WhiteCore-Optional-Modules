using System;

namespace MetaBuilders.Irc.Messages.Modes {

	/// <summary>
	/// This mode is used to toggle the operator status of a channel member.
	/// </summary>
 	public class OperatorMode : MemberStatusMode {

		/// <summary>
		/// Creates a new instance of the <see cref="OperatorMode"/> class.
		/// </summary>
		public OperatorMode() {
		}

		/// <summary>
		/// Creates a new instance of the <see cref="OperatorMode"/> class with the given <see cref="ModeAction"/>.
		/// </summary>
		public OperatorMode(ModeAction action) {
			this.Action = action;
		}

		/// <summary>
		/// Creates a new instance of the <see cref="OperatorMode"/> class 
		/// with the given <see cref="ModeAction"/> and member's nick.
		/// </summary>
		public OperatorMode(ModeAction action, String nick) {
			this.Action = action;
			this.Nick = nick;
		}

		/// <summary>
		/// Gets the irc string representation of the mode being changed or applied.
		/// </summary>
		protected override String Symbol {
			get {
				return "o";
			}
		}

	}
}
