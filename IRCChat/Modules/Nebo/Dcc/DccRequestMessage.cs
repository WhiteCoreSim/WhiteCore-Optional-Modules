using System;
using System.Net;
using System.Globalization;
using MetaBuilders.Irc.Dcc;


namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// The base for dcc request messages.
	/// </summary>
	[Serializable]
	public abstract class DccRequestMessage : CtcpRequestMessage
	{
        IPAddress _address = IPAddress.None;
        Int32 port = -1;

		/// <summary>
		/// Creates a new instance of the <see cref="DccRequestMessage"/> class.
		/// </summary>
		protected DccRequestMessage()
			: base()
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
				return MessageUtil.ParametersToString( false, this.DccCommand, this.DccArgument, TransportAddressFromAddress( this.Address ), this.Port.ToString( CultureInfo.InvariantCulture ) );
			}
		}


		/// <summary>
		/// Gets the dcc sub-command.
		/// </summary>
		protected abstract String DccCommand
		{
			get;
		}

		/// <summary>
		/// Gets the dcc sub-command's argument.
		/// </summary>
		protected abstract String DccArgument
		{
			get;
		}

		/// <summary>
		/// Gets or sets the host address on which the initiator expects the connection.
		/// </summary>
		public virtual System.Net.IPAddress Address
		{
			get
			{
				return _address;
			}
			set
			{
				_address = value;
			}
		}

		/// <summary>
		/// Gets or sets the port on which the initiator expects the connection.
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
			return DccCommand.EndsWith( command, StringComparison.OrdinalIgnoreCase );
		}

		/// <summary>
		/// Parses the given string to populate this <see cref="IrcMessage"/>.
		/// </summary>
		public override void Parse( String unparsedMessage )
		{
			base.Parse( unparsedMessage );

			Address = AddressFromTransportAddress( DccUtil.GetAddress( unparsedMessage ) );
			Port = Convert.ToInt32( DccUtil.GetPort( unparsedMessage ), CultureInfo.InvariantCulture );
		}

		static IPAddress AddressFromTransportAddress( String transportAddress )
		{
			Double theAddress;
			if ( Double.TryParse( transportAddress, NumberStyles.Integer, null, out theAddress ) )
			{
				IPAddress backwards = new IPAddress( Convert.ToInt64( theAddress ) );
				if ( backwards.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork )
				{
					String[] addy = backwards.ToString().Split( '.' );
					Array.Reverse( addy );
					return IPAddress.Parse( String.Join( ".", addy ) );
				}
				return backwards;
				
			}
			return IPAddress.Parse( transportAddress );
			
		}


		static String TransportAddressFromAddress( IPAddress address )
		{
			if ( address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork )
			{
				String[] nums = address.ToString().Split( '.' );
				Array.Reverse( nums );
				IPAddress backwards = IPAddress.Parse( String.Join( ".", nums ) );

#pragma warning disable 0618
				return backwards.Address.ToString( CultureInfo.InvariantCulture );
#pragma warning restore 0618
			}
			return address.ToString();
			
		}


	}

}
