using System;
using System.Collections.Specialized;
using System.Text;

namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// Requests information from the server about the users specified.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Possible reply messages include:
	/// <see cref="T:NoSuchServerMessage"/>
	/// <see cref="T:NoNickGivenMessage"/>
	/// <see cref="T:NoSuchNickMessage"/>
	/// 
	/// <see cref="T:WhoIsUserReplyMessage"/>
	/// <see cref="T:WhoIsChannelsReplyMessage"/>
	/// <see cref="T:WhoIsServerReplyMessage"/>
	/// <see cref="T:WhoIsOperReplyMessage"/>
	/// <see cref="T:WhoIsIdleReplyMessage"/>
	/// 
	/// <see cref="T:UserAwayMessage" />
	/// <see cref="T:WhoIsEndReplyMessage"/>
	/// </para>
	/// </remarks>
	[Serializable]
	public class WhoIsMessage : CommandMessage
	{

		/// <summary>
		/// Gets the collection of users that information is requested for.
		/// </summary>
		public virtual UserCollection Masks
		{
			get
			{
				return this.masks;
			}
		}
		private UserCollection masks = new UserCollection();

		/// <summary>
		/// Gets or sets the server which should return the information.
		/// </summary>
		public virtual String Server
		{
			get
			{
				return server;
			}
			set
			{
				server = value;
			}
		}

		/// <summary>
		/// Gets the Irc command associated with this message.
		/// </summary>
		protected override String Command
		{
			get
			{
				return "WHOIS";
			}
		}

		private String server = "";

		/// <summary>
		/// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
		/// </summary>
		protected override void AddParametersToFormat( IrcMessageWriter writer )
		{
			base.AddParametersToFormat( writer );
			writer.AddParameter( this.Server );
			writer.AddList( this.Masks, ",", true );
		}

		/// <summary>
		/// Parses the parameters portion of the message.
		/// </summary>
		protected override void ParseParameters( StringCollection parameters )
		{
			base.ParseParameters( parameters );
			this.Masks.Clear();
			this.Server = "";
			if ( parameters.Count >= 1 )
			{
				if ( parameters.Count > 1 )
				{
					this.Server = parameters[ 0 ];
				}
				foreach ( String maskString in parameters[ parameters.Count - 1 ].Split( ',' ) )
				{
					this.Masks.Add( new User( maskString ) );
				}
			}
		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MetaBuilders.Irc.Messages.MessageConduit conduit )
		{
			conduit.OnWhoIs( new IrcMessageEventArgs<WhoIsMessage>( this ) );
		}

	}
}
