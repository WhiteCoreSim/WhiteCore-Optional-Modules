using System;
using System.Collections.Specialized;
using System.Text;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// The base class for all numeric messages sent from the server to the client.
	/// </summary>
	[Serializable]
	public abstract class NumericMessage : IrcMessage
	{

		/// <summary>
		/// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
		/// </summary>
		protected override void AddParametersToFormat( IrcMessageWriter writer )
		{
			base.AddParametersToFormat( writer );
			writer.AddParameter( this.internalNumeric.ToString( "000", CultureInfo.InvariantCulture ) );
			if ( this.Target.Length != 0 )
			{
				writer.AddParameter( this.Target );
			}
		}

		/// <summary>
		/// Gets or sets the target of the message.
		/// </summary>
		public virtual String Target
		{
			get
			{
				return target;
			}
			set
			{
				target = value;
			}
		}
		private String target = "";

		/// <summary>
		/// Determines if the given numeric is an error message.
		/// </summary>
		public static Boolean IsError( Int32 numeric )
		{
			Int32 normalStart = 400;
			Int32 normalEnd = 599;
			Int32 ircxStart = 900;
			Int32 ircxEnd = 998;
			return ( ( normalStart <= numeric && numeric <= normalEnd ) || ( ircxStart <= numeric && numeric <= ircxEnd ) );
		}

		/// <summary>
		/// Determines if the given numeric is a command reply message.
		/// </summary>
		public static Boolean IsCommandReply( Int32 numeric )
		{
			return ( !IsError( numeric ) && !IsDirect( numeric ) );
		}

		/// <summary>
		/// Determines if the given numeric is a direct message.
		/// </summary>
		public static Boolean IsDirect( Int32 numeric )
		{
			return ( 0 < numeric && numeric < 100 );
		}

		/// <summary>
		/// Gets the Numeric command of the Message
		/// </summary>
		public virtual Int32 InternalNumeric
		{
			get
			{
				return internalNumeric;
			}
			protected set
			{
				internalNumeric = value;
			}
		}
		private Int32 internalNumeric = -1;

		/// <summary>
		/// Determines if the message can be parsed by this type.
		/// </summary>
		public override Boolean CanParse( String unparsedMessage )
		{
			Double foo;
			if ( Double.TryParse( MessageUtil.GetCommand( unparsedMessage ), System.Globalization.NumberStyles.Integer, null, out foo ) )
			{
				if ( 0 <= foo && foo < 1000 )
				{
					if ( InternalNumeric == -1 )
					{
						return true;
					}
					else
					{
						Int32 parsedNumeric = Convert.ToInt32( foo );
						return ( InternalNumeric == parsedNumeric );
					}
				}
				else
				{
					return false;
				}
			}
			return false;
		}

		/// <summary>
		/// Parses the parameters portion of the message.
		/// </summary>
		protected override void ParseParameters( StringCollection parameters )
		{
			base.ParseParameters( parameters );
			if ( parameters.Count > 0 )
			{
				this.Target = parameters[ 0 ];
			}
			else
			{
				this.Target = "";
			}
		}

	}

}
