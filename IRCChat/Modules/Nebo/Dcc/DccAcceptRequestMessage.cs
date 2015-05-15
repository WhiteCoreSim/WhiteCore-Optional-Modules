using System;
using System.Collections.Specialized;
using System.Globalization;
using MetaBuilders.Irc.Dcc;


namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// This message is an acknowledgement to resume sending a file previously, but not completely sent to the requestor.
	/// </summary>
	[Serializable]
	public class DccAcceptRequestMessage : CtcpRequestMessage
	{
        String fileName = "";
        Int32 port = -1;
        Int32 position = -1;

		/// <summary>
		/// Creates a new instance of the <see cref="DccAcceptRequestMessage"/> class.
		/// </summary>
		public DccAcceptRequestMessage()
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
				return MessageUtil.ParametersToString(
                    false, 
                    DccCommand,
                    FileName, 
                    Port.ToString(CultureInfo.InvariantCulture), 
                    Position.ToString(CultureInfo.InvariantCulture) );
			}
		}

		/// <summary>
		/// Gets the dcc sub-command.
		/// </summary>
		protected virtual String DccCommand
		{
			get
			{
				return "ACCEPT";
			}
		}

		/// <summary>
		/// Gets or sets the name of the file being sent.
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
		/// Gets or sets the port the connection should be on.
		/// </summary>
		public virtual Int32 Port
		{
			get
			{
				return port;
			}
			set
			{
				port = value;
			}
		}

		/// <summary>
		/// Gets or sets the position in the file at which to resume sending.
		/// </summary>
		public virtual Int32 Position
		{
			get
			{
				return position;
			}
			set
			{
				position = value;
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
		/// Determines if the message
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		
		public virtual Boolean CanParseDccCommand( String command )
		{
			if ( String.IsNullOrEmpty( command ) )
			{
				return false;
			}
			return ( DccCommand.ToUpperInvariant().EndsWith( command.ToUpperInvariant(), StringComparison.Ordinal ) );
		}

		/// <summary>
		/// Parses the given string to populate this <see cref="IrcMessage"/>.
		/// </summary>
		public override void Parse( String unparsedMessage )
		{
			base.Parse( unparsedMessage );
			FileName = DccUtil.GetArgument( unparsedMessage );
		}

		/// <summary>
		/// Parses the parameters portion of the message.
		/// </summary>
		protected override void ParseParameters( StringCollection parameters )
		{
			base.ParseParameters( parameters );
			if ( parameters.Count >= 4 )
			{
				Port = Convert.ToInt32( parameters[ 2 ], CultureInfo.InvariantCulture );
				Position = Convert.ToInt32( parameters[ 3 ], CultureInfo.InvariantCulture );
			}
			else
			{
				Port = -1;
				Position = -1;
			}
		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MessageConduit conduit )
		{
			conduit.OnDccAcceptRequest( new IrcMessageEventArgs<DccAcceptRequestMessage>( this ) );
		}


	}
}
