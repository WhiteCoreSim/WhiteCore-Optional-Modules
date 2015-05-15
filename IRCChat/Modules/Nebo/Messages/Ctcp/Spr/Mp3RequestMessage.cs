using System;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// An SPR Jukebox message that notifies the recipient of the senders available mp3 file.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Mp" ), Serializable]
	public class Mp3RequestMessage : CtcpRequestMessage
	{

		/// <summary>
		/// Creates a new instance of the <see cref="Mp3RequestMessage"/> class.
		/// </summary>
		public Mp3RequestMessage()
			: base()
		{
			this.InternalCommand = "MP3";
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ActionRequestMessage"/> class with the given text and target.
		/// </summary>
		/// <param name="target">The target of the action.</param>
		public Mp3RequestMessage( String target )
			: this()
		{
			this.Target = target;
		}

		/// <summary>
		/// Gets or sets the filename of the mp3 being shared.
		/// </summary>
		public String FileName
		{
			get
			{
				return filename;
			}
			set
			{
				filename = value;
			}
		}
		private String filename;

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MessageConduit conduit )
		{
			conduit.OnMp3Request( new IrcMessageEventArgs<Mp3RequestMessage>( this ) );
		}

		/// <summary>
		/// Gets the data payload of the Ctcp request.
		/// </summary>
		protected override string ExtendedData
		{
			get
			{
				return this.FileName;
			}
		}

		/// <summary>
		/// Parses the given string to populate this <see cref="IrcMessage"/>.
		/// </summary>
		public override void Parse( String unparsedMessage )
		{
			base.Parse( unparsedMessage );
			this.FileName = CtcpUtil.GetExtendedData( unparsedMessage );
		}

	}

}
