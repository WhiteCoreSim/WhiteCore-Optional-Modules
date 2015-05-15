using System;
using System.Collections.Specialized;
using System.Text;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// One of the responses to the <see cref="LusersMessage"/> query.
	/// </summary>
	[Serializable]
	public class LusersReplyMessage : NumericMessage
	{

		/// <summary>
		/// Creates a new instance of the <see cref="LusersReplyMessage"/> class.
		/// </summary>
		public LusersReplyMessage()
			: base()
		{
			this.InternalNumeric = 251;
		}


		/// <summary>
		/// Gets or sets the number of users connected to irc.
		/// </summary>
		public virtual Int32 UserCount
		{
			get
			{
				return userCount;
			}
			set
			{
				userCount = value;
			}
		}

		/// <summary>
		/// Gets or sets the number of invisible users connected to irc.
		/// </summary>
		public virtual Int32 InvisibleCount
		{
			get
			{
				return invisibleCount;
			}
			set
			{
				invisibleCount = value;
			}
		}

		/// <summary>
		/// Gets or sets the number of servers connected on the network.
		/// </summary>
		public virtual Int32 ServerCount
		{
			get
			{
				return serverCount;
			}
			set
			{
				serverCount = value;
			}
		}

		private Int32 userCount = -1;
		private Int32 invisibleCount = -1;
		private Int32 serverCount = -1;
		private String thereAre = "There are ";
		private String usersAnd = " users and ";
		private String invisibleOn = " invisible on ";
		private String servers = " servers";

		/// <summary>
		/// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
		/// </summary>
		protected override void AddParametersToFormat( IrcMessageWriter writer )
		{
			base.AddParametersToFormat( writer );
			writer.AddParameter( this.thereAre + this.UserCount + this.usersAnd + this.InvisibleCount + this.invisibleOn + this.serverCount + this.servers );
		}

		/// <summary>
		/// Parses the parameters portion of the message.
		/// </summary>
		protected override void ParseParameters( StringCollection parameters )
		{
			base.ParseParameters( parameters );
			String payload = parameters[ 1 ];
			this.UserCount = Convert.ToInt32( MessageUtil.StringBetweenStrings( payload, this.thereAre, this.usersAnd ), CultureInfo.InvariantCulture );
			this.InvisibleCount = Convert.ToInt32( MessageUtil.StringBetweenStrings( payload, this.usersAnd, this.invisibleOn ), CultureInfo.InvariantCulture );
			this.ServerCount = Convert.ToInt32( MessageUtil.StringBetweenStrings( payload, this.invisibleOn, this.servers ), CultureInfo.InvariantCulture );

		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MetaBuilders.Irc.Messages.MessageConduit conduit )
		{
			conduit.OnLusersReply( new IrcMessageEventArgs<LusersReplyMessage>( this ) );
		}

	}
}
