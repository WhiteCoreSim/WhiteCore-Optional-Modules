using System;
using System.Collections.Specialized;
using System.Text;


namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// The reply for the <see cref="UserInfoRequestMessage"/> query.
	/// </summary>
	[Serializable]
	public class UserInfoReplyMessage : CtcpReplyMessage
	{

		/// <summary>
		/// Creates a new instance of the <see cref="UserInfoReplyMessage"/> class.
		/// </summary>
		public UserInfoReplyMessage()
			: base()
		{
			this.InternalCommand = "USERINFO";
		}

		/// <summary>
		/// The information that the client wants to return.
		/// </summary>
		public virtual String Response
		{
			get
			{
				return this.response;
			}
			set
			{
				this.response = value;
			}
		}
		private String response = "";

		/// <summary>
		/// Gets the data payload of the Ctcp request.
		/// </summary>
		protected override String ExtendedData
		{
			get
			{
				return this.response;
			}
		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MetaBuilders.Irc.Messages.MessageConduit conduit )
		{
			conduit.OnUserInfoReply( new IrcMessageEventArgs<UserInfoReplyMessage>( this ) );
		}

		/// <summary>
		/// Parses the given string to populate this <see cref="IrcMessage"/>.
		/// </summary>
		public override void Parse( String unparsedMessage )
		{
			base.Parse( unparsedMessage );
			this.Response = CtcpUtil.GetExtendedData( unparsedMessage );
		}

	}
}
