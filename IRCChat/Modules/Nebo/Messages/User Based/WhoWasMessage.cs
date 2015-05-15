using System;
using System.Collections.Specialized;
using System.Text;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// Requests information about a user who is no longer connected to irc.
	/// </summary>
	[Serializable]
	public class WhoWasMessage : CommandMessage
	{

		/// <summary>
		/// Gets or sets the nick of the user being examined.
		/// </summary>
		public virtual String Nick
		{
			get
			{
				return this.nick;
			}
			set
			{
				this.nick = value;
			}
		}

		/// <summary>
		/// Gets or sets the server that should search for the information.
		/// </summary>
		public virtual String Server
		{
			get
			{
				return server;
			}
			set
			{
				server = value;
			}
		}

		/// <summary>
		/// Gets or sets the maximum number of results to receive.
		/// </summary>
		public virtual Int32 MaximumResults
		{
			get
			{
				return maximumResults;
			}
			set
			{
				maximumResults = value;
			}
		}

		/// <summary>
		/// Gets the Irc command associated with this message.
		/// </summary>
		protected override String Command
		{
			get
			{
				return "WHOWAS";
			}
		}

		private String nick = "";
		private String server = "";
		private Int32 maximumResults = 1;

		/// <summary>
		/// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
		/// </summary>
		protected override void AddParametersToFormat( IrcMessageWriter writer )
		{
			base.AddParametersToFormat( writer );
			writer.AddParameter( this.Nick );

			if ( this.MaximumResults > 0 )
			{
				writer.AddParameter( this.MaximumResults.ToString( CultureInfo.InvariantCulture ) );
				if ( this.Server.Length != 0 )
				{
					writer.AddParameter( this.Server );
				}
			}

		}

		/// <summary>
		/// Parses the parameters portion of the message.
		/// </summary>
		protected override void ParseParameters( StringCollection parameters )
		{
			base.ParseParameters( parameters );

			this.Nick = "";
			this.Server = "";
			this.MaximumResults = 1;

			if ( parameters.Count > 0 )
			{
				this.Nick = parameters[ 0 ];
				if ( parameters.Count > 1 )
				{
					this.Server = parameters[ 1 ];
					if ( parameters.Count > 2 )
					{
						this.MaximumResults = Convert.ToInt32( parameters[ 2 ], CultureInfo.InvariantCulture );
					}
				}
			}
		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MetaBuilders.Irc.Messages.MessageConduit conduit )
		{
			conduit.OnWhoWas( new IrcMessageEventArgs<WhoWasMessage>( this ) );
		}
	}
}
