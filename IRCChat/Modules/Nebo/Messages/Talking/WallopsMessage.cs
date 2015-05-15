using System;
using System.Collections.Specialized;
using System.Text;

namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// This message is sent to all users with <see cref="MetaBuilders.Irc.Messages.Modes.ReceiveWallopsMode"/>,
	/// <see cref="MetaBuilders.Irc.Messages.Modes.NetworkOperatorMode"/>, or <see cref="MetaBuilders.Irc.Messages.Modes.ServerOperatorMode"/> user modes.
	/// </summary>
	[Serializable]
	public class WallopsMessage : CommandMessage
	{

		/// <summary>
		/// Gets or sets the text of the <see cref="WallopsMessage"/>.
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
		/// Gets the Irc command associated with this message.
		/// </summary>
		protected override String Command
		{
			get
			{
				return "WALLOPS";
			}
		}

		/// <summary>
		/// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
		/// </summary>
		protected override void AddParametersToFormat( IrcMessageWriter writer )
		{
			base.AddParametersToFormat( writer );
			writer.AddParameter( this.Text, true );
		}

		/// <summary>
		/// Parses the parameters portion of the message.
		/// </summary>
		protected override void ParseParameters( StringCollection parameters )
		{
			base.ParseParameters( parameters );
			if ( parameters.Count >= 1 )
			{
				this.Text = parameters[ 0 ];
			}
			else
			{
				this.Text = String.Empty;
			}
		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MetaBuilders.Irc.Messages.MessageConduit conduit )
		{
			conduit.OnWallops( new IrcMessageEventArgs<WallopsMessage>( this ) );
		}

	}
}
