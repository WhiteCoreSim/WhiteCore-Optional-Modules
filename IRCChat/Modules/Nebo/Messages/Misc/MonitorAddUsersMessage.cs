using System;
using System.Collections;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// A Monitor system message that adds users to your monitor list.
	/// </summary>
	[Serializable]
	public class MonitorAddUsersMessage : MonitorMessage
	{

		/// <summary>
		/// Gets the collection of nicks being added to the monitor list.
		/// </summary>
		public StringCollection Nicks
		{
			get
			{
				if ( nicks == null )
				{
					nicks = new StringCollection();
				}
				return nicks;
			}
		}
		private StringCollection nicks;

		/// <summary>
		/// Determines if the message can be parsed by this type.
		/// </summary>
		public override Boolean CanParse( String unparsedMessage )
		{
			if ( !base.CanParse( unparsedMessage ) )
			{
				return false;
			}
			String firstParam = MessageUtil.GetParameter( unparsedMessage, 0 );
			return firstParam.StartsWith( "+", StringComparison.Ordinal );
		}

		/// <summary>
		/// Parses the parameters portion of the message.
		/// </summary>
		protected override void ParseParameters( StringCollection parameters )
		{
			base.ParseParameters( parameters );
			String nicksParam = parameters[ parameters.Count - 1 ];
			String[] splitNicksParam = nicksParam.Split( new String[] { "," }, StringSplitOptions.RemoveEmptyEntries );
			foreach ( String nick in splitNicksParam )
			{
				this.Nicks.Add( nick );
			}
		}

		/// <summary>
		/// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
		/// </summary>
		protected override void AddParametersToFormat( IrcMessageWriter writer )
		{
			base.AddParametersToFormat( writer );
			writer.AddParameter( "+" );

			if ( this.nicks != null )
			{
				writer.AddParameter( MessageUtil.CreateList( this.nicks, "," ), true );
			}
		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MessageConduit conduit )
		{
			conduit.OnMonitorAddUsers( new IrcMessageEventArgs<MonitorAddUsersMessage>( this ) );
		}

	}
}
