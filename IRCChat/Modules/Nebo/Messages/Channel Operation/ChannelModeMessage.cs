using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// The ChannelModeMessage allows channels to have their mode changed. 
	/// </summary>
	/// <remarks>
	/// Modes include such things as channel user limits and passwords, as well as the bans list and settings ops.
	/// This message wraps the MODE command.
	/// </remarks>
	[Serializable]
	public class ChannelModeMessage : CommandMessage, IChannelTargetedMessage
	{

		/// <summary>
		/// Gets the Irc command associated with this message.
		/// </summary>
		protected override String Command
		{
			get
			{
				return "MODE";
			}
		}

		/// <summary>
		/// Creates a new instance of the ChannelModeMessage class.
		/// </summary>
		public ChannelModeMessage()
		{
		}

		/// <summary>
		/// Creates a new instance of the ChannelModeMessage class and applies the given parameters.
		/// </summary>
		/// <param name="channel">The name of the channel being affected.</param>
		/// <param name="modeChanges">The mode changes being applied.</param>
		/// <param name="modeArguments">The arguments ( parameters ) for the <see cref="ChannelModeMessage.ModeChanges"/> property.</param>
		public ChannelModeMessage( String channel, String modeChanges, params String[] modeArguments )
		{
			this.channel = channel;
			this.modeChanges = modeChanges;
			this.modeArguments.AddRange( modeArguments );
		}

		/// <summary>
		/// Gets or sets the name of the channel being affected.
		/// </summary>
		public virtual String Channel
		{
			get
			{
				return channel;
			}
			set
			{
				channel = value;
			}
		}
		private String channel = "";

		/// <summary>
		/// Gets or sets the mode changes being applied.
		/// </summary>
		/// <remarks>
		/// An example ModeChanges might look like "+ool".
		/// This means adding the cannel op mode for two users, and setting a limit on the user count.
		/// </remarks>
		public virtual String ModeChanges
		{
			get
			{
				return modeChanges;
			}
			set
			{
				modeChanges = value;
			}
		}
		String modeChanges = "";

		/// <summary>
		/// Gets the collection of arguments ( parameters ) for the <see cref="ChannelModeMessage.ModeChanges"/> property.
		/// </summary>
		/// <remarks>
		/// Some modes require a parameter, such as +o requires the mask of the person to be given ops.
		/// </remarks>
		public virtual StringCollection ModeArguments
		{
			get
			{
				return modeArguments;
			}
		}
		StringCollection modeArguments = new StringCollection();

		/// <summary>
		/// Determines if the message can be parsed by this type.
		/// </summary>
		public override Boolean CanParse( String unparsedMessage )
        {
            if (!base.CanParse (unparsedMessage))
            {
                return false;
            }
            StringCollection p = MessageUtil.GetParameters (unparsedMessage);
            if (p.Count >= 1)
            {
                return MessageUtil.HasValidChannelPrefix (p [0]);
            }
            return false;
        }

		/// <summary>
		/// Parses the parameters portion of the message.
		/// </summary>
		protected override void ParseParameters( StringCollection parameters )
		{
			base.ParseParameters( parameters );
			Channel = parameters[ 0 ];

			if ( parameters.Count > 1 )
			{
				ModeChanges = parameters[ 1 ];
			}
			else
			{
				ModeChanges = "";
			}

			ModeArguments.Clear();
			if ( parameters.Count > 2 )
			{
				for ( Int32 i = 2; i < parameters.Count; i++ )
				{
					ModeArguments.Add( parameters[ i ] );
				}
			}
		}

		/// <summary>
		/// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
		/// </summary>
		protected override void AddParametersToFormat( IrcMessageWriter writer )
		{
			base.AddParametersToFormat( writer );
			writer.AddParameter( Channel );
			if ( !String.IsNullOrEmpty( ModeChanges ) )
			{
				writer.AddParameter( ModeChanges );
				foreach ( String modeArg in ModeArguments )
				{
					writer.AddParameter( modeArg );
				}
			}
		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MessageConduit conduit )
		{
			conduit.OnChannelMode( new IrcMessageEventArgs<ChannelModeMessage>( this ) );
		}


		#region IChannelTargetedMessage Members

		bool IChannelTargetedMessage.IsTargetedAtChannel( string channelName )
		{
			return IsTargetedAtChannel( channelName );
		}

		/// <summary>
		/// Determines if the the current message is targeted at the given channel.
		/// </summary>
		protected virtual bool IsTargetedAtChannel( string channelName )
		{
			return MessageUtil.IsIgnoreCaseMatch( Channel, channelName );
		}

		#endregion
	}

}
