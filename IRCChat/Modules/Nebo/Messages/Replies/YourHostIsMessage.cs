using System;
using System.Collections.Specialized;
using System.Text;


namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// This message is sent directly after connecting, 
	/// giving the client information about the server software in use.
	/// </summary>
	[Serializable]
	public class YourHostMessage : NumericMessage
	{

		/// <summary>
		/// Creates a new instance of the <see cref="YourHostMessage"/> class.
		/// </summary>
		public YourHostMessage()
			: base()
		{
			this.InternalNumeric = 002;
		}

		/// <summary>
		/// Gets or sets the name of the software the server is running.
		/// </summary>
		public virtual String ServerName
		{
			get
			{
				return serverName;
			}
			set
			{
				serverName = value;
			}
		}
		private String serverName = "";

		/// <summary>
		/// Gets or sets the version of the software the server is running.
		/// </summary>
		public virtual String Version
		{
			get
			{
				return version;
			}
			set
			{
				version = value;
			}
		}
		private String version = "";

		private const String yourHostIs = "Your host is ";
		private const String runningVersion = ", running version ";

		/// <summary>
		/// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
		/// </summary>
		protected override void AddParametersToFormat( IrcMessageWriter writer )
		{
			base.AddParametersToFormat( writer );
			writer.AddParameter( yourHostIs + this.ServerName + runningVersion + this.Version );
		}

		/// <summary>
		/// Parses the parameters portion of the message.
		/// </summary>
		protected override void ParseParameters( StringCollection parameters )
		{
			base.ParseParameters( parameters );
			String reply = parameters[ 1 ];
			if ( reply.IndexOf( yourHostIs, StringComparison.Ordinal ) != -1 && reply.IndexOf( runningVersion, StringComparison.Ordinal ) != -1 )
			{
				Int32 startOfServerName = yourHostIs.Length;
				Int32 startOfVersion = reply.IndexOf( runningVersion, StringComparison.Ordinal ) + runningVersion.Length;
				Int32 lengthOfServerName = reply.IndexOf( runningVersion, StringComparison.Ordinal ) - startOfServerName;

				this.ServerName = reply.Substring( startOfServerName, lengthOfServerName );
				this.Version = reply.Substring( startOfVersion );
			}
		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MetaBuilders.Irc.Messages.MessageConduit conduit )
		{
			conduit.OnYourHost( new IrcMessageEventArgs<YourHostMessage>( this ) );
		}

	}
}
