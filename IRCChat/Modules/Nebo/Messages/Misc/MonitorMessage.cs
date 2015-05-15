using System;

namespace MetaBuilders.Irc.Messages {

	/// <summary>
	/// A Message that participates in the Monitor system.
	/// </summary>
	[Serializable]
	public abstract class MonitorMessage : CommandMessage
	{
		
		/// <summary>
		/// Gets the Irc command associated with this message.
		/// </summary>
		protected override String Command {
			get {
				return "MONITOR";
			}
		}

		/// <summary>
		/// Validates this message against the given server support
		/// </summary>
		public override void Validate(ServerSupport serverSupport) {
			base.Validate( serverSupport );
			if ( serverSupport != null && serverSupport.MaxMonitors == 0 ) {
				throw new InvalidMessageException( NeboResources.ServerDoesNotSupportMonitor );
			}
		}

	}
}
