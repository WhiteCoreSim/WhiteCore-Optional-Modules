using System;
using System.Collections.Specialized;
using System.Text;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// A single reply to the <see cref="ListMessage"/> query.
	/// </summary>
	[Serializable]
	public class ListReplyMessage : NumericMessage, IChannelTargetedMessage
	{

		/// <summary>
		/// Creates a new instance of the <see cref="ListReplyMessage"/> class.
		/// </summary>
		public ListReplyMessage()
			: base()
		{
			this.InternalNumeric = 322;
		}

		/// <summary>
		/// Gets or sets the channel for this reply.
		/// </summary>
		public virtual String Channel
		{
			get
			{
				return channel;
			}
			set
			{
				channel = value;
			}
		}

		/// <summary>
		/// Gets or sets the number of people in the channel.
		/// </summary>
		public virtual Int32 MemberCount
		{
			get
			{
				return memberCount;
			}
			set
			{
				memberCount = value;
			}
		}

		/// <summary>
		/// Gets or sets the topic of the channel.
		/// </summary>
		public virtual String Topic
		{
			get
			{
				return topic;
			}
			set
			{
				topic = value;
			}
		}

		private String topic = "";
		private Int32 memberCount = -1;
		private String channel = "";

		/// <summary>
		/// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
		/// </summary>
		protected override void AddParametersToFormat( IrcMessageWriter writer )
		{
			base.AddParametersToFormat( writer );
			writer.AddParameter( this.Channel );
			writer.AddParameter( this.MemberCount.ToString( CultureInfo.InvariantCulture ) );
			writer.AddParameter( this.Topic );
		}

		/// <summary>
		/// Parses the parameters portion of the message.
		/// </summary>
		protected override void ParseParameters( StringCollection parameters )
		{
			base.ParseParameters( parameters );
			if ( parameters.Count == 4 )
			{
				this.Channel = parameters[ 1 ];
				this.MemberCount = Convert.ToInt32( parameters[ 2 ], CultureInfo.InvariantCulture );
				this.Topic = parameters[ 3 ];
			}
			else
			{
				this.Channel = "";
				this.MemberCount = -1;
				this.Topic = "";
			}
		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MetaBuilders.Irc.Messages.MessageConduit conduit )
		{
			conduit.OnListReply( new IrcMessageEventArgs<ListReplyMessage>( this ) );
		}


		#region IChannelTargetedMessage Members

		bool IChannelTargetedMessage.IsTargetedAtChannel( string channelName )
		{
			return IsTargetedAtChannel( channelName );
		}

		/// <summary>
		/// Determines if the the current message is targeted at the given channel.
		/// </summary>
		protected virtual bool IsTargetedAtChannel( string channelName )
		{
			return MessageUtil.IsIgnoreCaseMatch( this.Channel, channelName );
			;
		}

		#endregion
	}
}
