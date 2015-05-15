using System;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// An SPR Jukebox message that notifies the recipient of the senders available mp3 serving capabilities.
	/// </summary>
	[Serializable]
	public class SlotsRequestMessage : CtcpRequestMessage
	{

		/// <summary>
		/// Creates a new instance of the <see cref="SlotsRequestMessage"/> class.
		/// </summary>
		public SlotsRequestMessage()
			: base()
		{
			this.InternalCommand = "SLOTS";
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ActionRequestMessage"/> class with the given text and target.
		/// </summary>
		/// <param name="target">The target of the action.</param>
		public SlotsRequestMessage( String target )
			: this()
		{
			this.Target = target;
		}

		/// <summary>
		/// TotalSendSlots
		/// </summary>
		public int TotalSendSlots
		{
			get
			{
				return totalSendSlots;
			}
			set
			{
				totalSendSlots = value;
			}
		}
		private int totalSendSlots;

		/// <summary>
		/// AvailableSendSlots
		/// </summary>
		public int AvailableSendSlots
		{
			get
			{
				return availableSendSlots;
			}
			set
			{
				availableSendSlots = value;
			}
		}
		private int availableSendSlots;

		/// <summary>
		/// NextSend
		/// </summary>
		public String NextSend
		{
			get
			{
				return nextSend;
			}
			set
			{
				nextSend = value;
			}
		}
		private String nextSend;

		/// <summary>
		/// TakenQueueSlots
		/// </summary>
		public int TakenQueueSlots
		{
			get
			{
				return takenQueueSlots;
			}
			set
			{
				takenQueueSlots = value;
			}
		}
		private int takenQueueSlots;

		/// <summary>
		/// TotalQueueSlots
		/// </summary>
		public int TotalQueueSlots
		{
			get
			{
				return totalQueueSlots;
			}
			set
			{
				totalQueueSlots = value;
			}
		}
		private int totalQueueSlots;

		/// <summary>
		/// CpsRecord
		/// </summary>
		public int CpsRecord
		{
			get
			{
				return cpsRecord;
			}
			set
			{
				cpsRecord = value;
			}
		}
		private int cpsRecord;

		/// <summary>
		/// TotalFiles
		/// </summary>
		public int TotalFiles
		{
			get
			{
				return totalFiles;
			}
			set
			{
				totalFiles = value;
			}
		}
		private int totalFiles;


		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MessageConduit conduit )
		{
			conduit.OnSlotsRequest( new IrcMessageEventArgs<SlotsRequestMessage>( this ) );
		}

		/// <summary>
		/// Gets the data payload of the Ctcp request.
		/// </summary>
		protected override string ExtendedData
		{
			get
			{
				return String.Format( CultureInfo.InvariantCulture, "{0} {1} {2} {3} {4} {5} {6}", TotalSendSlots, AvailableSendSlots, NextSend, TakenQueueSlots, TotalQueueSlots, CpsRecord, TotalFiles );
			}
		}

		/// <summary>
		/// Parses the given string to populate this <see cref="IrcMessage"/>.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Int32.TryParse(System.String,System.Globalization.NumberStyles,System.IFormatProvider,System.Int32@)" )]
		public override void Parse( String unparsedMessage )
		{
			base.Parse( unparsedMessage );
			String slotInfo = CtcpUtil.GetExtendedData( unparsedMessage );
			String[] slotInfoItems = slotInfo.Split( ' ' );
			if ( slotInfoItems.Length >= 7 )
			{
				Int32.TryParse( slotInfoItems[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out this.totalSendSlots );
				Int32.TryParse( slotInfoItems[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out this.availableSendSlots );
				this.nextSend = slotInfoItems[2];
				Int32.TryParse( slotInfoItems[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out this.takenQueueSlots );
				Int32.TryParse( slotInfoItems[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out this.totalQueueSlots );
				Int32.TryParse( slotInfoItems[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out this.cpsRecord );
				Int32.TryParse( slotInfoItems[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out this.totalFiles );
			}
		}

	}

}
