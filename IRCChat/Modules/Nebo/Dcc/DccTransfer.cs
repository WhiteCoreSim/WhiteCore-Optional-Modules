using System;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace MetaBuilders.Irc.Dcc
{

	/// <summary>
	/// Handles the networks level communication protocols for sending and receiving files over dcc.
	/// </summary>
	
	public class DccTransfer
	{
        #region Private Members

        Byte[] buffer;

        FileStream file;
        Int64 startPosition;
        Socket transferSocket;
        Int32 bufferSize = 4096;
        Boolean turboMode;
        Boolean secure;
        Boolean sendAhead = true;
        Int64 bytesTransferred;
        Int64 fileSize = -1;

        #endregion

		#region Properties
		/// <summary>
		/// Gets or sets a stream to the file being transfered.
		/// </summary>
		public FileStream File
		{
			get
			{
				return file;
			}
			set
			{
				file = value;
			}
		}

		/// <summary>
		/// Gets or sets the startposition in the file to transfer the information.
		/// </summary>
		public Int64 StartPosition
		{
			get
			{
				return startPosition;
			}
			set
			{
				startPosition = value;
			}
		}

		/// <summary>
		/// Gets or sets the socket the file transfer will use.
		/// </summary>
		public Socket TransferSocket
		{
			get
			{
				return transferSocket;
			}
			set
			{
				transferSocket = value;
			}
		}

		/// <summary>
		/// Gets or sets the size of the buffer for transfer of the file.
		/// </summary>
		public Int32 BufferSize
		{
			get
			{
				return bufferSize;
			}
			set
			{
				if ( value > 8192 )
				{
					throw new ArgumentException( NeboResources.BufferSizeIsLimited, "value" );
				}
				bufferSize = value;
			}
		}


		/// <summary>
		/// Gets or sets if the transfer uses the "turbo" extension in increase transfer speed.
		/// </summary>
		public Boolean TurboMode
		{
			get
			{
				return turboMode;
			}
			set
			{
				turboMode = value;
			}
		}

		/// <summary>
		/// Gets or sets if the transfer uses SSL to secure the transfer.
		/// </summary>
		public Boolean Secure
		{
			get
			{
				return secure;
			}
			set
			{
				secure = value;
			}
		}

		/// <summary>
		/// Gets or sets if the transfer uses the "send ahead" extension to increase transfer speed.
		/// </summary>
		public Boolean SendAhead
		{
			get
			{
				return sendAhead;
			}
			set
			{
				sendAhead = value;
			}
		}

		/// <summary>
		/// Gets the number of bytes transfered so far.
		/// </summary>
		public Int64 BytesTransferred
		{
			get
			{
				return bytesTransferred;
			}
		}

		/// <summary>
		/// Gets or sets the size of the file being transfered.
		/// </summary>
		public Int64 FileSize
		{
			get
			{
				return fileSize;
			}
			set
			{
				fileSize = value;
			}
		}
		#endregion

		#region Events
		/// <summary>
		/// The TransferInterruption event occurs when the file has not completely transfered, but the connection has been stopped.
		/// </summary>
		public event EventHandler TransferInterruption;
		/// <summary>
		/// Raises the <see cref="TransferInterruption"/> event.
		/// </summary>
		protected void OnTransferInterruption( EventArgs e )
		{
			if ( TransferInterruption != null )
			{
				TransferInterruption( this, e );
			}
		}

		/// <summary>
		/// The TransferComplete event occurs when the file has been completely transfered.
		/// </summary>
		public event EventHandler TransferComplete;
		/// <summary>
		/// Raises the <see cref="TransferComplete"/> event.
		/// </summary>
		protected void OnTransferComplete( EventArgs e )
		{
			if ( TransferComplete != null )
			{
				TransferComplete( this, e );
			}
		}
		#endregion

		#region Methods

		/// <summary>
		/// Sends the file over the current socket.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
		internal void Send()
		{
			if ( !File.CanRead )
			{
				throw new InvalidOperationException( NeboResources.CannotReadFromFile );
			}

			bytesTransferred = 0;

			buffer = new Byte[ BufferSize ];
			Byte[] acknowledgment = new Byte[ 4 ];
			Int32 bytesSent;


			while ( ( bytesSent = File.Read( buffer, 0, buffer.Length ) ) != 0 )
			{
				try
				{
					transferSocket.Send( buffer, bytesSent, SocketFlags.None );
					bytesTransferred += bytesSent;
					if ( !TurboMode && !this.SendAhead )
					{
						transferSocket.Receive( acknowledgment );
					}
				}
				catch
				{
					OnTransferInterruption( EventArgs.Empty );
				}
			}

			if ( !TurboMode )
			{
				while ( !AllAcknowledgmentsReceived( acknowledgment ) )
				{
					transferSocket.Receive( acknowledgment );
				}
			}
			OnTransferComplete( EventArgs.Empty );
		}

		/// <summary>
		/// Receives the file over the current socket.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
		internal void Receive()
		{
			bytesTransferred = 0;

			buffer = new Byte[ BufferSize ];
			Int32 bytesReceived;

			while ( !IsTransferComplete )
			{
				bytesReceived = transferSocket.Receive( buffer );
				if ( bytesReceived == 0 )
				{
					OnTransferInterruption( EventArgs.Empty );
					return;
				}
				bytesTransferred += bytesReceived;
				if ( File.CanWrite )
				{
					File.Write( buffer, 0, bytesReceived );
				}
				SendAcknowledgement();
			}
			File.Flush();
			OnTransferComplete( EventArgs.Empty );
		}

		#endregion

		#region Helpers
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
		void SendAcknowledgement()
		{
			if ( !TurboMode )
			{
				//Convert BytesTransfered to a 4 byte array containing the number
				Byte[] bytesAck = DccBytesReceivedFormat();

				// Send it over the socket.
				transferSocket.Send( bytesAck );
			}
		}

		Boolean AllAcknowledgmentsReceived( Byte[] lastAck )
		{
			Int64 acknowledgedBytes = DccBytesToLong( lastAck );
			return acknowledgedBytes >= BytesTransferred;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
		Boolean IsTransferComplete
		{
			get
			{
				if ( fileSize == -1 )
				{
					return false;
				}
				return startPosition + bytesTransferred >= fileSize;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
		byte[] DccBytesReceivedFormat()
		{
			byte[] size = new byte[4];
			byte[] longBytes = BitConverter.GetBytes( NetworkUnsignedLong( this.BytesTransferred ) );
			Array.Copy( longBytes, 0, size, 0, 4 );
			return size;
		}

		static long DccBytesToLong( byte[] received )
		{
			return IPAddress.NetworkToHostOrder( BitConverter.ToInt32( received, 0 ) );
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
		static long NetworkUnsignedLong( long hostOrderLong )
		{
			long networkLong = IPAddress.HostToNetworkOrder( hostOrderLong );
			return ( networkLong >> 32 ) & 0x00000000ffffffff;
		}

		#endregion
	}
}
