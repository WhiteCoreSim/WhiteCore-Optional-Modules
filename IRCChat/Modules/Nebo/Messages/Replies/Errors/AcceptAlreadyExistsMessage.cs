using System;
using System.Collections.Specialized;
using System.Text;

namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// The ErrorMessage received when the client attempts to add a nick to his accept list
	/// when that nick is already on the list.
	/// </summary>
	[Serializable]
	public class AcceptAlreadyExistsMessage : ErrorMessage
	{

		/// <summary>
		/// Creates a new instances of the <see cref="AcceptAlreadyExistsMessage"/> class.
		/// </summary>
		public AcceptAlreadyExistsMessage()
			: base()
		{
			this.InternalNumeric = 457;
		}

		/// <summary>
		/// Gets or sets the nick which wasn't added
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
			// actual message received from efnet, 
			// although the text isn't the same as what appears in references.
			// :irc.pte.hu 457 artificer azure :is already on your accept list

			base.AddParametersToFormat( writer );
			writer.AddParameter( this.Nick );
			writer.AddParameter( "is already on your accept list" );
		}

		/// <summary>
		/// Parses the parameters portion of the message.
		/// </summary>
		protected override void ParseParameters( StringCollection parameters )
		{
			// :irc.pte.hu 457 artificer azure :is already on your accept list

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
			conduit.OnAcceptAlreadyExists( new IrcMessageEventArgs<AcceptAlreadyExistsMessage>( this ) );
		}

	}
}
