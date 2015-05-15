using System;
using System.Collections.Specialized;
using System.Text;
using System.Globalization;


namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// The reply to the <see cref="FingerRequestMessage"/>, containing the user's name and idle time.
	/// </summary>
	[Serializable]
	public class FingerReplyMessage : CtcpReplyMessage
	{

		/// <summary>
		/// Creates a new instance of the <see cref="FingerReplyMessage"/> class.
		/// </summary>
		public FingerReplyMessage()
			: base()
		{
			this.InternalCommand = "FINGER";
		}


		/// <summary>
		/// Gets or sets the real name of the user.
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

		/// <summary>
		/// Gets or sets the login name of the user.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login" )]
		public virtual String LoginName
		{
			get
			{
				return this.loginName;
			}
			set
			{
				this.loginName = value;
			}
		}

		/// <summary>
		/// Gets or sets the number of seconds that the user has been idle.
		/// </summary>
		public virtual Double IdleSeconds
		{
			get
			{
				return this.idleSeconds;
			}
			set
			{
				this.idleSeconds = value;
			}
		}


		/// <summary>
		/// Gets the data payload of the Ctcp request.
		/// </summary>
		protected override String ExtendedData
		{
			get
			{
				StringBuilder result = new StringBuilder();
				result.Append( ":" );
				result.Append( this.RealName );
				result.Append( " (" );
				result.Append( this.LoginName );
				result.Append( ") - Idle " );
				result.Append( this.IdleSeconds.ToString( CultureInfo.InvariantCulture ) );
				result.Append( " seconds" );
				return result.ToString();
			}
		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MetaBuilders.Irc.Messages.MessageConduit conduit )
		{
			conduit.OnFingerReply( new IrcMessageEventArgs<FingerReplyMessage>( this ) );
		}

		private String realName = "";
		private String loginName = "";
		private Double idleSeconds;

		/// <summary>
		/// Parses the given string to populate this <see cref="IrcMessage"/>.
		/// </summary>
		public override void Parse( String unparsedMessage )
		{
			base.Parse( unparsedMessage );
			String payload = CtcpUtil.GetExtendedData( unparsedMessage );
			if ( payload.StartsWith( ":", StringComparison.Ordinal ) )
			{
				payload = payload.Substring( 1 );
			}
			this.RealName = payload.Substring( 0, payload.IndexOf( " ", StringComparison.Ordinal ) );

			Int32 startOfLoginName = payload.IndexOf( " (", StringComparison.Ordinal );
			Int32 endOfLoginName = payload.IndexOf( ")", StringComparison.Ordinal );
			if ( startOfLoginName > 0 )
			{
				startOfLoginName += 2;
				this.LoginName = payload.Substring( startOfLoginName, endOfLoginName - startOfLoginName );

				Int32 startOfIdle = payload.IndexOf( "- Idle ", StringComparison.Ordinal );
				if ( startOfIdle > 0 )
				{
					startOfIdle += 6;
					String idleSecs = payload.Substring( startOfIdle, payload.Length - startOfIdle - 8 );
					Double foo;
					if ( Double.TryParse( idleSecs, System.Globalization.NumberStyles.Any, null, out foo ) )
					{
						this.IdleSeconds = foo;
					}
					else
					{
						this.IdleSeconds = -1;
					}

				}
			}
		}

	}
}
