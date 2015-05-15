using System;
using System.Collections.Specialized;
using System.Text;

namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// A reply to a <see cref="WhoIsMessage"/> that specifies what server they are on.
	/// </summary>
	[Serializable]
	public class WhoIsServerReplyMessage : NumericMessage
	{

		/// <summary>
		/// Creates a new instance of the <see cref="WhoIsServerReplyMessage"/> class.
		/// </summary>
		public WhoIsServerReplyMessage()
			: base()
		{
			this.InternalNumeric = 312;
		}

		/// <summary>
		/// Gets or sets the nick of the user being examined.
		/// </summary>
		public virtual String Nick
		{
			get
			{
				return nick;
			}
			set
			{
				nick = value;
			}
		}

		/// <summary>
		/// Gets or sets the name of the server the user is connected to.
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

		/// <summary>
		/// Gets or sets additional information about the user's server connection.
		/// </summary>
		public virtual String Info
		{
			get
			{
				return info;
			}
			set
			{
				info = value;
			}
		}

		private String nick = "";
		private String serverName = "";
		private String info = "";

		/// <summary>
		/// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
		/// </summary>
		protected override void AddParametersToFormat( IrcMessageWriter writer )
		{
			base.AddParametersToFormat( writer );
			writer.AddParameter( this.Nick );
			writer.AddParameter( this.ServerName );
			writer.AddParameter( this.Info );
		}

		/// <summary>
		/// Parses the parameters portion of the message.
		/// </summary>
		protected override void ParseParameters( StringCollection parameters )
		{
			base.ParseParameters( parameters );
			if ( parameters.Count > 3 )
			{
				this.Nick = parameters[ 1 ];
				this.ServerName = parameters[ 2 ];
				this.Info = parameters[ 3 ];
			}
			else
			{
				this.Nick = "";
				this.ServerName = "";
				this.Info = "";
			}
		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MetaBuilders.Irc.Messages.MessageConduit conduit )
		{
			conduit.OnWhoIsServerReply( new IrcMessageEventArgs<WhoIsServerReplyMessage>( this ) );
		}

	}
}
