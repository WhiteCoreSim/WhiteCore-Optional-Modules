using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Globalization;

namespace MetaBuilders.Irc.Network
{
	/// <summary>
	/// An Ident daemon which is still used by some
	/// IRC networks for authentication.
	/// </summary>
	[System.ComponentModel.DesignerCategory( "Code" )]
	public sealed class Ident : Component
	{

		/// <summary>
		/// The singleton Ident service.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes" )]
		public static readonly Ident Service = new Ident();
		private Ident()
		{
		}



		/// <summary>
		/// Gets or sets the <see cref="MetaBuilders.Irc.User"/> to respond to an ident request with.
		/// </summary>
		public MetaBuilders.Irc.User User
		{
			get
			{
				if ( user == null )
				{
					user = new User();
				}
				return user;
			}
			set
			{
				user = value;
			}
		}

		/// <summary>
		/// Gets the status of the Ident service.
		/// </summary>
		public ConnectionStatus Status
		{
			get
			{
				return status;
			}
		}


		/// <summary>
		/// Starts the Ident server.
		/// </summary>
		public void Start()
		{
			this.Start( false );
		}

		/// <summary>
		/// Starts the Ident server.
		/// </summary>
		/// <param name="stopAfterFirstAnswer">If true, Ident will stop immediately after answering. If false, will continue until <see cref="Ident.Stop"/> is called.</param>
		public void Start( Boolean stopAfterFirstAnswer )
		{
			lock ( lockObject )
			{
				if ( this.status != ConnectionStatus.Disconnected )
				{
					System.Diagnostics.Trace.WriteLine( "Ident Already Started" );
					return;
				}
				else
				{
					this.stopAfter = stopAfterFirstAnswer;
					socketThread = new Thread( new ThreadStart( Run ) );
					socketThread.Name = "Identd";
					socketThread.IsBackground = true;
					socketThread.Start();
				}
			}
		}

		private Object lockObject = new Object();

		/// <summary>
		/// Stops the Ident server.
		/// </summary>
		public void Stop()
		{
			lock ( lockObject )
			{
				this.status = ConnectionStatus.Disconnected;
				if ( this.listener != null )
				{
					listener.Stop();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase" )]
		private void Run()
		{
			this.status = ConnectionStatus.Connecting;

			try
			{
				listener = new TcpListener( IPAddress.Any, port );
				listener.Start();
			}
			catch ( Exception ex )
			{
				System.Diagnostics.Trace.WriteLine( "Error Opening Ident Listener On Port " + port.ToString(CultureInfo.InvariantCulture) + ", " + ex.ToString(), "Ident" );
				this.status = ConnectionStatus.Disconnected;
				throw;
			}

			try
			{
				while ( this.status != ConnectionStatus.Disconnected )
				{
					try
					{
						TcpClient client = listener.AcceptTcpClient();
						this.status = ConnectionStatus.Connected;


						//Read query
						StreamReader reader = new StreamReader( client.GetStream() );
						string identRequest = reader.ReadLine();

						//Send back reply
						StreamWriter writer = new StreamWriter( client.GetStream() );
						String identName = this.User.UserName;
						if ( identName.Length == 0 )
						{
							if ( this.User.Nick.Length != 0 )
							{
								identName = this.User.Nick;
							}
							else
							{
								identName = "nebo";
							}
						}
						String identReply = identRequest.Trim() + this.reply + identName.ToLower( CultureInfo.InvariantCulture );
						writer.WriteLine( identReply );
						writer.Flush();

						//Close connection with client
						client.Close();

						if ( stopAfter )
						{
							this.status = ConnectionStatus.Disconnected;
						}
					}
					catch ( IOException ex )
					{
						System.Diagnostics.Trace.WriteLine( "Error Processing Ident Request: " + ex.Message, "Ident" );
					}
				}
			}
			catch ( SocketException ex )
			{
				switch ( (SocketError)ex.ErrorCode )
				{
					case SocketError.InterruptedFunctionCall:
						System.Diagnostics.Trace.WriteLine( "Ident Stopped By Thread Abort", "Ident" );
						break;
					default:
						System.Diagnostics.Trace.WriteLine( "Ident Abnormally Stopped: " + ex.ToString(), "Ident" );
						break;
				}
				//throw( ex );
			}

			if ( listener != null )
			{
				listener.Stop();
			}
		}

		/// <summary>
		/// Releases the resources used by <see cref="Ident"/>
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			try
			{
				if ( disposing )
				{
					if ( listener != null )
					{
						( (IDisposable)listener ).Dispose();
					}
					if ( socketThread != null )
					{
						socketThread.Abort();
					}
				}
			}
			finally
			{
				base.Dispose( disposing );
			}
		}


		private User user;
		private TcpListener listener;
		private Thread socketThread;
		private string reply = " : USERID : UNIX : ";
		private int port = 113;
		private ConnectionStatus status = ConnectionStatus.Disconnected;
		private Boolean stopAfter = true;


	}
}
