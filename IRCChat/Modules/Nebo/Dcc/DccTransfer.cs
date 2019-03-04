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

        byte [] buffer;

        FileStream file;
        long startPosition;
        Socket transferSocket;
        int bufferSize = 4096;
        bool turboMode;
        bool secure;
        bool sendAhead = true;
        long bytesTransferred;
        long fileSize = -1;

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a stream to the file being transfered.
        /// </summary>
        public FileStream File {
            get {
                return file;
            }
            set {
                file = value;
            }
        }

        /// <summary>
        /// Gets or sets the startposition in the file to transfer the information.
        /// </summary>
        public long StartPosition {
            get {
                return startPosition;
            }
            set {
                startPosition = value;
            }
        }

        /// <summary>
        /// Gets or sets the socket the file transfer will use.
        /// </summary>
        public Socket TransferSocket {
            get {
                return transferSocket;
            }
            set {
                transferSocket = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the buffer for transfer of the file.
        /// </summary>
        public int BufferSize {
            get {
                return bufferSize;
            }
            set {
                if (value > 8192) {
                    throw new ArgumentException (NeboResources.BufferSizeIsLimited, nameof (value));
                }
                bufferSize = value;
            }
        }


        /// <summary>
        /// Gets or sets if the transfer uses the "turbo" extension in increase transfer speed.
        /// </summary>
        public bool TurboMode {
            get {
                return turboMode;
            }
            set {
                turboMode = value;
            }
        }

        /// <summary>
        /// Gets or sets if the transfer uses SSL to secure the transfer.
        /// </summary>
        public bool Secure {
            get {
                return secure;
            }
            set {
                secure = value;
            }
        }

        /// <summary>
        /// Gets or sets if the transfer uses the "send ahead" extension to increase transfer speed.
        /// </summary>
        public bool SendAhead {
            get {
                return sendAhead;
            }
            set {
                sendAhead = value;
            }
        }

        /// <summary>
        /// Gets the number of bytes transfered so far.
        /// </summary>
        public long BytesTransferred {
            get {
                return bytesTransferred;
            }
        }

        /// <summary>
        /// Gets or sets the size of the file being transfered.
        /// </summary>
        public long FileSize {
            get {
                return fileSize;
            }
            set {
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
        protected void OnTransferInterruption (EventArgs e)
        {
            if (TransferInterruption != null) {
                TransferInterruption (this, e);
            }
        }

        /// <summary>
        /// The TransferComplete event occurs when the file has been completely transfered.
        /// </summary>
        public event EventHandler TransferComplete;
        /// <summary>
        /// Raises the <see cref="TransferComplete"/> event.
        /// </summary>
        protected void OnTransferComplete (EventArgs e)
        {
            if (TransferComplete != null) {
                TransferComplete (this, e);
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Sends the file over the current socket.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal void Send ()
        {
            if (!File.CanRead) {
                throw new InvalidOperationException (NeboResources.CannotReadFromFile);
            }

            bytesTransferred = 0;

            buffer = new byte [BufferSize];
            byte [] acknowledgment = new byte [4];
            int bytesSent;


            while ((bytesSent = File.Read (buffer, 0, buffer.Length)) != 0) {
                try {
                    transferSocket.Send (buffer, bytesSent, SocketFlags.None);
                    bytesTransferred += bytesSent;
                    if (!TurboMode && !SendAhead) {
                        transferSocket.Receive (acknowledgment);
                    }
                } catch {
                    OnTransferInterruption (EventArgs.Empty);
                }
            }

            if (!TurboMode) {
                while (!AllAcknowledgmentsReceived (acknowledgment)) {
                    transferSocket.Receive (acknowledgment);
                }
            }
            OnTransferComplete (EventArgs.Empty);
        }

        /// <summary>
        /// Receives the file over the current socket.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal void Receive ()
        {
            bytesTransferred = 0;

            buffer = new byte [BufferSize];
            int bytesReceived;

            while (!IsTransferComplete) {
                bytesReceived = transferSocket.Receive (buffer);
                if (bytesReceived == 0) {
                    OnTransferInterruption (EventArgs.Empty);
                    return;
                }
                bytesTransferred += bytesReceived;
                if (File.CanWrite) {
                    File.Write (buffer, 0, bytesReceived);
                }
                SendAcknowledgement ();
            }
            File.Flush ();
            OnTransferComplete (EventArgs.Empty);
        }

        #endregion

        #region Helpers
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        void SendAcknowledgement ()
        {
            if (!TurboMode) {
                //Convert BytesTransfered to a 4 byte array containing the number
                byte [] bytesAck = DccBytesReceivedFormat ();

                // Send it over the socket.
                transferSocket.Send (bytesAck);
            }
        }

        bool AllAcknowledgmentsReceived (byte [] lastAck)
        {
            long acknowledgedBytes = DccBytesToLong (lastAck);
            return acknowledgedBytes >= BytesTransferred;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        bool IsTransferComplete {
            get {
                if (fileSize == -1) {
                    return false;
                }
                return startPosition + bytesTransferred >= fileSize;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        byte [] DccBytesReceivedFormat ()
        {
            byte [] size = new byte [4];
            byte [] longBytes = BitConverter.GetBytes (NetworkUnsignedLong (BytesTransferred));
            Array.Copy (longBytes, 0, size, 0, 4);
            return size;
        }

        static long DccBytesToLong (byte [] received)
        {
            return IPAddress.NetworkToHostOrder (BitConverter.ToInt32 (received, 0));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        static long NetworkUnsignedLong (long hostOrderLong)
        {
            long networkLong = IPAddress.HostToNetworkOrder (hostOrderLong);
            return (networkLong >> 32) & 0x00000000ffffffff;
        }

        #endregion
    }
}
