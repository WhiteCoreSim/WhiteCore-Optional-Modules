using System;
using System.Collections.Specialized;
using System.Text;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{
	/// <summary>
	/// The UserNotificationServerSideMessage is passed between servers to notify of a new user on the network.
	/// </summary>
	[Serializable]
	public class UserNotificationServerSideMessage : CommandMessage
	{

		/// <summary>
		/// Gets the Irc command associated with this message.
		/// </summary>
		protected override String Command
		{
			get
			{
				return "USER";
			}
		}

		/// <summary>
		/// Gets or sets the UserName of client.
		/// </summary>
		public virtual String UserName
		{
			get
			{
				return this.userName;
			}
			set
			{
				this.userName = value;
			}
		}
		private String userName = "";

		/// <summary>
		/// Gets or sets the name of the user's host.
		/// </summary>
		public String HostName
		{
			get
			{
				return hostName;
			}
			set
			{
				hostName = value;
			}
		}
		private String hostName;

		/// <summary>
		/// Gets or sets the name of the server which the user is on.
		/// </summary>
		public String ServerName
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
		private String serverName;
		
		/// <summary>
		/// Gets or sets the real name of the client.
		/// </summary>
		public virtual String RealName
		{
			get
			{
				return this.realName;
			}
			set
			{
				this.realName = value;
			}
		}
		private String realName = "";

		/// <exclude />
		public override bool CanParse( string unparsedMessage )
		{
			if ( !base.CanParse( unparsedMessage ) ) {
				return false;
			}			
			StringCollection p = MessageUtil.GetParameters( unparsedMessage );
			if ( p.Count != 4 || p[ 2 ] == "*" )
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
		/// </summary>
		protected override void AddParametersToFormat( IrcMessageWriter writer )
		{
			base.AddParametersToFormat( writer );
			writer.AddParameter( this.UserName );
			writer.AddParameter( this.HostName );
			writer.AddParameter( this.ServerName );
			writer.AddParameter( this.RealName );
		}

		/// <summary>
		/// Parses the parameters portion of the message.
		/// </summary>
		protected override void ParseParameters( StringCollection parameters )
		{
			base.ParseParameters( parameters );
			if ( parameters.Count >= 4 )
			{
				this.UserName = parameters[ 0 ];
				this.HostName = parameters[ 1 ];
				this.ServerName = parameters[ 2 ];
				this.RealName = parameters[ 3 ];
			}
			else
			{
				this.UserName = "";
				this.HostName = "";
				this.ServerName = "";
				this.RealName = "";
			}
		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MetaBuilders.Irc.Messages.MessageConduit conduit )
		{
			conduit.OnUserNotificationServerSide( new IrcMessageEventArgs<UserNotificationServerSideMessage>( this ) );
		}

	}
}
