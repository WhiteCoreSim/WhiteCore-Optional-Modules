using System;
using System.Collections.Specialized;
using System.Text;

namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// Signals the end of a <see cref="WhoIsMessage"/> reply.
	/// </summary>
	[Serializable]
	public class WhoIsEndReplyMessage : NumericMessage
	{

		/// <summary>
		/// Creates a new instance of the <see cref="WhoIsEndReplyMessage"/> class.
		/// </summary>
		public WhoIsEndReplyMessage()
			: base()
		{
			this.InternalNumeric = 318;
		}

		/// <summary>
		/// Gets or sets the nick for the user examined.
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

		private String nick = "";

		/// <summary>
		/// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
		/// </summary>
		protected override void AddParametersToFormat( IrcMessageWriter writer )
		{
			base.AddParametersToFormat( writer );
			writer.AddParameter( this.Nick );
			writer.AddParameter( "End of /WHOIS list" );
		}

		/// <summary>
		/// Parses the parameters portion of the message.
		/// </summary>
		protected override void ParseParameters( StringCollection parameters )
		{
			base.ParseParameters( parameters );
			if ( parameters.Count == 3 )
			{
				this.Nick = parameters[ 1 ];
			}
		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MetaBuilders.Irc.Messages.MessageConduit conduit )
		{
			conduit.OnWhoIsEndReply( new IrcMessageEventArgs<WhoIsEndReplyMessage>( this ) );
		}

	}
}
