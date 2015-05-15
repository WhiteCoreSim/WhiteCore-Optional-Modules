using System;
using System.Globalization;

namespace MetaBuilders.Irc.Messages.Modes
{

	/// <summary>
	/// A user limit may be set on channels by using this mode.
	/// </summary>
	public class LimitMode : FlagMode
	{

		/// <summary>
		/// Creates a new instance of the <see cref="LimitMode"/> class.
		/// </summary>
		public LimitMode()
		{
		}

		/// <summary>
		/// Creates a new instance of the <see cref="LimitMode"/> class with the given <see cref="ModeAction"/>.
		/// </summary>
		public LimitMode( ModeAction action )
		{
			this.Action = action;
		}

		/// <summary>
		/// Creates a new instance of the <see cref="LimitMode"/> class 
		/// with the given <see cref="ModeAction"/> and user limit.
		/// </summary>
		public LimitMode( ModeAction action, Int32 userLimit )
		{
			this.Action = action;
			this.userLimit = userLimit;
		}

		/// <summary>
		/// Creates a new instance of the <see cref="LimitMode"/> class 
		/// with the given user limit.
		/// </summary>
		public LimitMode( Int32 userLimit )
		{
			this.userLimit = userLimit;
		}


		/// <summary>
		/// Gets the irc string representation of the mode being changed or applied.
		/// </summary>
		protected override String Symbol
		{
			get
			{
				return "l";
			}
		}

		/// <summary>
		/// Gets or sets the maximum number of users allowed to join the channel.
		/// </summary>
		public virtual Int32 UserLimit
		{
			get
			{
				return userLimit;
			}
			set
			{
				userLimit = value;
			}
		}
		Int32 userLimit = -1;

		/// <summary>
		/// Applies this mode to the ModeArguments property of the given <see cref="ChannelModeMessage" />.
		/// </summary>
		/// <param name="msg">The message which will be modified to include this mode.</param>
		protected override void AddParameter( MetaBuilders.Irc.Messages.ChannelModeMessage msg )
		{
			if ( UserLimit != -1 )
			{
				msg.ModeArguments.Add( this.userLimit.ToString( CultureInfo.InvariantCulture ) );
			}
		}
	}
}
