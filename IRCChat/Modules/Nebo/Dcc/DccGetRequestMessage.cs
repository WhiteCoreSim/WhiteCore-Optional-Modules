using System;
using MetaBuilders.Irc.Dcc;


namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// Sends a request for the transfer of the given file.
	/// </summary>
	
	public class DccGetRequestMessage : CtcpRequestMessage
	{
        String fileName = "";
        Boolean turboMode;
        Boolean secure;

        /// <summary>
		/// Creates a new instance of the <see cref="DccGetRequestMessage"/> class.
		/// </summary>
		public DccGetRequestMessage()
        {
			this.InternalCommand = "DCC";
		}


		/// <summary>
		/// Gets the data payload of the Ctcp request.
		/// </summary>
		protected override String ExtendedData
		{
			get
			{
				return MessageUtil.ParametersToString( false, DccCommand, FileName );
			}
		}

		/// <summary>
		/// Gets the dcc sub-command.
		/// </summary>
		
		protected virtual String DccCommand
		{
			get
			{
				String result = "GET";
				if ( Secure )
				{
					result = "S" + result;
				}
				if ( TurboMode )
				{
					result = "T" + result;
				}
				return result;
			}
		}

		/// <summary>
		/// Gets or sets the name of the file being requested.
		/// </summary>
		public virtual String FileName
		{
			get
			{
				return fileName;
			}
			set
			{
				fileName = value;
			}
		}

		/// <summary>
		/// Gets or sets if the dcc connection should use turbo mode.
		/// </summary>
		public virtual Boolean TurboMode
		{
			get
			{
				return turboMode;
			}
			set
			{
				turboMode = value;
			}
		}

		/// <summary>
		/// Gets or sets if the dcc connection should use SSL.
		/// </summary>
		public virtual Boolean Secure
		{
			get
			{
				return secure;
			}
			set
			{
				secure = value;
			}
		}

		/// <summary>
		/// Determines if the message can be parsed by this type.
		/// </summary>
		public override Boolean CanParse( String unparsedMessage )
		{
			if ( !base.CanParse( unparsedMessage ) )
			{
				return false;
			}

			return CanParseDccCommand( DccUtil.GetCommand( unparsedMessage ) );
		}

		/// <summary>
		/// Determines if the message's DCC command is compatible with this message.
		/// </summary>
		
		public virtual Boolean CanParseDccCommand( String command )
		{
			if ( String.IsNullOrEmpty( command ) )
			{
				return false;
			}
			return ( this.DccCommand.ToUpperInvariant().EndsWith( command.ToUpperInvariant(), StringComparison.Ordinal ) );
		}

		/// <summary>
		/// Parses the given string to populate this <see cref="IrcMessage"/>.
		/// </summary>
		public override void Parse( String unparsedMessage )
		{
			base.Parse( unparsedMessage );
			FileName = DccUtil.GetArgument( unparsedMessage );
			String unparsedCommand = DccUtil.GetCommand( unparsedMessage ).ToUpperInvariant();
			String commandExtenstion = unparsedCommand.Substring( 0, unparsedCommand.Length - 3 );
			TurboMode = commandExtenstion.IndexOf( "T", StringComparison.Ordinal ) >= 0;
			Secure = commandExtenstion.IndexOf( "S", StringComparison.Ordinal ) >= 0;
		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MessageConduit conduit )
		{
			conduit.OnDccGetRequest( new IrcMessageEventArgs<DccGetRequestMessage>( this ) );
		}

	}
}
