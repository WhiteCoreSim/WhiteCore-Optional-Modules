using System;

namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// This is much like the <see cref="ChatMessage"/> message, 
	/// except the intent is to describe an "action" that the sender is doing.
	/// </summary>
	[Serializable]
	public class ActionRequestMessage : CtcpRequestMessage
	{

		/// <summary>
		/// Creates a new instance of the <see cref="ActionRequestMessage"/> class.
		/// </summary>
		public ActionRequestMessage()
			: base()
		{
			this.InternalCommand = "ACTION";
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ActionRequestMessage"/> class with the given text.
		/// </summary>
		/// <param name="text">The text of the action.</param>
		public ActionRequestMessage( String text )
			: this()
		{
			this.text = text;
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ActionRequestMessage"/> class with the given text and target.
		/// </summary>
		/// <param name="text">The text of the action.</param>
		/// <param name="target">The target of the action.</param>
		public ActionRequestMessage( String text, String target )
			: this( text )
		{
			this.Target = target;
		}

		/// <summary>
		/// Gets or sets the communicated text of this <see cref="ActionRequestMessage"/>.
		/// </summary>
		public virtual String Text
		{
			get
			{
				return this.text;
			}
			set
			{
				this.text = value;
			}
		}
		private String text = "";

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MessageConduit conduit )
		{
			conduit.OnActionRequest( new IrcMessageEventArgs<ActionRequestMessage>( this ) );
		}

		/// <summary>
		/// Gets the data payload of the Ctcp request.
		/// </summary>
		protected override string ExtendedData
		{
			get
			{
				return this.Text;
			}
		}

		/// <summary>
		/// Parses the given string to populate this <see cref="IrcMessage"/>.
		/// </summary>
		public override void Parse( String unparsedMessage )
		{
			base.Parse( unparsedMessage );
			this.Text = CtcpUtil.GetExtendedData( unparsedMessage );
		}

	}

}
