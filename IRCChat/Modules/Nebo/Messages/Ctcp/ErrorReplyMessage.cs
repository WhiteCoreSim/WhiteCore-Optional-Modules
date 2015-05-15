using System;
using System.Collections.Specialized;
using System.Text;


namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// This reply should be sent whenever a client receives a <see cref="CtcpRequestMessage"/> that is not understood or is malformed.
	/// </summary>
	[Serializable]
	public class ErrorReplyMessage : CtcpReplyMessage
	{

		/// <summary>
		/// Creates a new instance of the <see cref="ErrorReplyMessage"/> class.
		/// </summary>
		public ErrorReplyMessage()
			: base()
		{
			this.InternalCommand = "ERRMSG";
		}

		/// <summary>
		/// Gets or sets the text of the query which couldn't be processed.
		/// </summary>
		public virtual String Query
		{
			get
			{
				return this.query;
			}
			set
			{
				this.query = value;
			}
		}
		private String query = "";

		/// <summary>
		/// Gets or sets the reason the request couldn't be processed.
		/// </summary>
		public virtual String Reason
		{
			get
			{
				return this.reason;
			}
			set
			{
				this.reason = value;
			}
		}
		private String reason = "";

		/// <summary>
		/// Gets the data payload of the Ctcp request.
		/// </summary>
		protected override String ExtendedData
		{
			get
			{
				return this.query + " " + this.reason;
			}
		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MetaBuilders.Irc.Messages.MessageConduit conduit )
		{
			conduit.OnErrorReply( new IrcMessageEventArgs<ErrorReplyMessage>( this ) );
		}

		/// <summary>
		/// Parses the given string to populate this <see cref="IrcMessage"/>.
		/// </summary>
		public override void Parse( String unparsedMessage )
		{
			base.Parse( unparsedMessage );
			String eData = CtcpUtil.GetExtendedData( unparsedMessage );
			StringCollection p = MessageUtil.GetParameters( eData );
			if ( p.Count == 2 )
			{
				this.Query = p[ 0 ];
				this.Reason = p[ 1 ];
			}
		}

	}
}
