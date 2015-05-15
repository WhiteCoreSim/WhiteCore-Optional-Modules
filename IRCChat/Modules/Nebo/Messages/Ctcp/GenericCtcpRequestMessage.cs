using System;

namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// An unknown <see cref="CtcpRequestMessage"/>.
	/// </summary>
	[Serializable]
	public class GenericCtcpRequestMessage : CtcpRequestMessage
	{

		/// <summary>
		/// Gets or sets the information packaged with the ctcp command.
		/// </summary>
		public virtual String DataPackage
		{
			get
			{
				return this.dataPackage;
			}
			set
			{
				this.dataPackage = value;
			}
		}
		private String dataPackage = "";


		/// <summary>
		/// Gets the data payload of the Ctcp request.
		/// </summary>
		protected override String ExtendedData
		{
			get
			{
				return this.dataPackage;
			}
		}

		/// <summary>
		/// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
		/// </summary>
		public override void Notify( MetaBuilders.Irc.Messages.MessageConduit conduit )
		{
			conduit.OnGenericCtcpRequest( new IrcMessageEventArgs<GenericCtcpRequestMessage>( this ) );
		}

		/// <summary>
		/// Gets or sets the Ctcp command.
		/// </summary>
		public virtual String Command
		{
			get
			{
				return this.InternalCommand;
			}
			set
			{
				this.InternalCommand = value;
			}
		}

		/// <summary>
		/// Parses the given string to populate this <see cref="IrcMessage"/>.
		/// </summary>
		public override void Parse( String unparsedMessage )
		{
			base.Parse( unparsedMessage );
			this.Command = CtcpUtil.GetInternalCommand( unparsedMessage );
			this.DataPackage = CtcpUtil.GetExtendedData( unparsedMessage );
		}

	}

}
