using System;
using System.Collections.Specialized;
using System.Text;

namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// Used to indicate the server name given currently doesn't exist.
	/// </summary>
	[Serializable]
	public class NoSuchServerMessage : ErrorMessage
	{

		/// <summary>
		/// Creates a new instances of the <see cref="NoSuchServerMessage"/> class.
		/// </summary>
		public NoSuchServerMessage()
			: base()
		{
			this.InternalNumeric = 402;
		}

		/// <summary>
		/// Gets or sets the nick which wasn't accepted.
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
			writer.AddParameter( "No such server" );
		}

		/// <summary>
		/// Parses the parameters portion of the message.
		/// </summary>
		protected override void ParseParameters( StringCollection parameters )
		{
			base.ParseParameters( parameters );
			if ( parameters.Count > 1 )
			{
				this.Nick = parameters[ 1 ];
			}
			else
			{
				this.Nick = "";
			}
		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MetaBuilders.Irc.Messages.MessageConduit conduit )
		{
			conduit.OnNoSuchServer( new IrcMessageEventArgs<NoSuchServerMessage>( this ) );
		}

	}
}
