using System;
using System.Collections.Specialized;
using System.Text;


namespace MetaBuilders.Irc.Messages
{

	// :irc2.secsup.org 004 artificer irc2.secsup.org 2.8/hybrid-6.3.1 oOiwszcrkfydnxb biklmnopstve

	/// <summary>
	/// Contains basic information about a server.
	/// </summary>
	[Serializable]
	public class ServerInfoMessage : NumericMessage
	{

		/// <summary>
		/// Creates a new instance of the <see cref="ServerInfoMessage"/> class.
		/// </summary>
		public ServerInfoMessage()
			: base()
		{
			this.InternalNumeric = 004;
		}

		/// <summary>
		/// Gets or sets the name of the server being referenced.
		/// </summary>
		public virtual String ServerName
		{
			get
			{
				return serverName;
			}
			set
			{
				serverName = value;
			}
		}
		private String serverName = "";

		/// <summary>
		/// Gets or sets the version of the server.
		/// </summary>
		public virtual String Version
		{
			get
			{
				return version;
			}
			set
			{
				version = value;
			}
		}
		private String version = "";

		/// <summary>
		/// Gets or sets the user modes supported by this server.
		/// </summary>
		public virtual String UserModes
		{
			get
			{
				return userModes;
			}
			set
			{
				userModes = value;
			}
		}
		private String userModes = "";

		/// <summary>
		/// Gets or sets the channel modes supported by this server.
		/// </summary>
		public virtual String ChannelModes
		{
			get
			{
				return channelModes;
			}
			set
			{
				channelModes = value;
			}
		}
		private String channelModes = "";

		/// <summary>
		/// Gets or sets the channel modes that require a parameter.
		/// </summary>
		public virtual String ChannelModesWithParams
		{
			get
			{
				return channelModesWithParams;
			}
			set
			{
				channelModesWithParams = value;
			}
		}
		private String channelModesWithParams = "";

		/// <summary>
		/// Gets or sets the user modes that require a parameter.
		/// </summary>
		public virtual String UserModesWithParams
		{
			get
			{
				return userModesWithParams;
			}
			set
			{
				userModesWithParams = value;
			}
		}
		private String userModesWithParams = "";

		/// <summary>
		/// Gets or sets the server modes supported by this server.
		/// </summary>
		public virtual String ServerModes
		{
			get
			{
				return serverModes;
			}
			set
			{
				serverModes = value;
			}
		}
		private String serverModes = "";

		/// <summary>
		/// Gets or sets the server modes which require parameters.
		/// </summary>
		public virtual String ServerModesWithParams
		{
			get
			{
				return serverModesWithParams;
			}
			set
			{
				serverModesWithParams = value;
			}
		}
		private String serverModesWithParams = "";

		/// <summary>
		/// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
		/// </summary>
		protected override void AddParametersToFormat( IrcMessageWriter writer )
		{
			base.AddParametersToFormat( writer );

			writer.AddParameter( this.ServerName );
			writer.AddParameter( this.Version );
			writer.AddParameter( this.UserModes );
			writer.AddParameter( this.ChannelModes );
			if ( this.ChannelModesWithParams.Length != 0 )
			{
				writer.AddParameter( this.ChannelModesWithParams );
				if ( this.UserModesWithParams.Length != 0 )
				{
					writer.AddParameter( this.UserModesWithParams );
					if ( this.ServerModes.Length != 0 )
					{
						writer.AddParameter( this.ServerModes );
						if ( this.ServerModesWithParams.Length != 0 )
						{
							writer.AddParameter( this.ServerModesWithParams );
						}
					}
				}
			}

		}

		/// <summary>
		/// Parses the parameters portion of the message.
		/// </summary>
		protected override void ParseParameters( StringCollection parameters )
		{
			base.ParseParameters( parameters );

			this.ServerName = parameters[ 1 ];
			this.Version = parameters[ 2 ];
			this.UserModes = parameters[ 3 ];
			this.ChannelModes = parameters[ 4 ];

			Int32 pCount = parameters.Count;

			if ( pCount > 5 )
			{
				this.ChannelModesWithParams = parameters[ 5 ];
				if ( pCount > 6 )
				{
					this.UserModesWithParams = parameters[ 6 ];

					if ( pCount > 7 )
					{
						this.ServerModes = parameters[ 7 ];

						if ( pCount > 8 )
						{
							this.ServerModesWithParams = parameters[ 8 ];
						}
					}
				}


			}
		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MetaBuilders.Irc.Messages.MessageConduit conduit )
		{
			conduit.OnServerInfo( new IrcMessageEventArgs<ServerInfoMessage>( this ) );
		}

	}

}
