using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

namespace MetaBuilders.Irc.Network
{

    /// <summary>
    /// Represents a network connection to an irc server.
    /// </summary>
    /// <remarks>
    /// Use the <see cref="ClientConnection"/> class to send a <see cref="MetaBuilders.Irc.Messages.IrcMessage"/> to an irc server, and to be notified when it returns a <see cref="MetaBuilders.Irc.Messages.IrcMessage"/>.
    /// </remarks>
    [DesignerCategory ("Code")]
    public class ClientConnection : Component
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientConnection"/> class.
        /// </summary>
        /// <remarks>With this Constructor, the <see cref="Address"/> default to 127.0.0.1, and the <see cref="Port"/> defaults to 6667.</remarks>
        public ClientConnection ()
            : this ("127.0.0.1", 6667)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientConnection"/> class with the given address on the given port.
        /// </summary>
        /// <param name="address">The network address to connect to.</param>
        /// <param name="port">The port to connect on.</param>
        public ClientConnection (string address, int port)
        {
            Encoding = System.Text.Encoding.ASCII;
            Ssl = false;
            Address = address;
            Port = port;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the <see cref="ClientConnection"/> recieves data.
        /// </summary>
        internal event EventHandler<ConnectionDataEventArgs> DataReceived;

        /// <summary>
        /// Occurs when the <see cref="ClientConnection"/> sends data.
        /// </summary>
        internal event EventHandler<ConnectionDataEventArgs> DataSent;

        /// <summary>
        /// Occurs when starting the connecting sequence to a server
        /// </summary>
        public event EventHandler Connecting;

        /// <summary>
        /// Occurs after the connecting sequence is successful.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Occurs when the disconnecting sequence is successful.
        /// </summary>
        public event EventHandler<ConnectionDataEventArgs> Disconnected;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the internet address which the current <see cref="ClientConnection"/> uses.
        /// </summary>
        /// <remarks>A <see cref="NotSupportedException"/> will be thrown if an attempt is made to change the <see cref="ClientConnection.Address"/> if the <see cref="ClientConnection.Status"/> is not <see cref="ConnectionStatus.Disconnected"/>.</remarks>
        public string Address {
            get {
                return address;
            }
            set {
                if (Status == ConnectionStatus.Disconnected) {
                    address = value;
                } else {
                    throw new NotSupportedException (NeboResources.AddressCannotBeChanged);
                }
            }
        }

        /// <summary>
        /// Gets or sets the port which the <see cref="ClientConnection"/> will communicate over.
        /// </summary>
        /// <remarks>
        /// <para>For irc, the <see cref="Port"/> is generally between 6667 and 7000</para>
        /// <para>A <see cref="NotSupportedException"/> will be thrown if an attempt is made to change the <see cref="ClientConnection.Port"/> if the <see cref="ClientConnection.Status"/> is not <see cref="ConnectionStatus.Disconnected"/>.</para>
        /// </remarks>
        public int Port {
            get {
                return port;
            }
            set {
                if (Status == ConnectionStatus.Disconnected) {
                    port = value;
                } else {
                    throw new NotSupportedException (NeboResources.PortCannotBeChanged);
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ConnectionStatus"/> of the <see cref="ClientConnection"/>.
        /// </summary>
        public ConnectionStatus Status {
            get {
                return status;
            }
            private set {
                status = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ISynchronizeInvoke"/> implementor which will be used to synchronize threads and events.
        /// </summary>
        /// <remarks>
        /// This is usually the main form of the application.
        /// </remarks>
        public ISynchronizeInvoke SynchronizationObject {
            get {
                return synchronizationObject;
            }
            set {
                synchronizationObject = value;
            }
        }

        /// <summary>
        /// Gets or sets the encoding used by stream reader and writer.
        /// </summary>
        /// <remarks>
        /// Generally, only ASCII and UTF-8 are supported.
        /// </remarks>
        public System.Text.Encoding Encoding {
            get {
                return _encoding;
            }
            set {
                if (Status == ConnectionStatus.Disconnected) {
                    _encoding = value;
                } else {
                    throw new NotSupportedException (NeboResources.EncodingCannotBeChanged);
                }
            }
        }

        /// <summary>
        /// Gets or sets if the connection will use SSL to connect to the server
        /// </summary>
        public bool Ssl {
            get {
                return _ssl;
            }
            set {
                if (Status == ConnectionStatus.Disconnected) {
                    _ssl = value;
                } else {
                    throw new NotSupportedException (NeboResources.SslCannotBeChanged);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a network connection to the current <see cref="Address"/> and <see cref="Port"/>
        /// </summary>
        /// <remarks>
        /// Only use this overload if your application is not a Windows.Forms application, you've set the <see cref="SynchronizationObject"/> property, or you want to handle threading issues yourself.
        /// </remarks>
        public virtual void Connect ()
        {
            lock (lockObject) {
                if (Status != ConnectionStatus.Disconnected) {
                    throw new InvalidOperationException (NeboResources.AlreadyConnected);
                }

                Status = ConnectionStatus.Connecting;
                OnConnecting (EventArgs.Empty);
            }

            connectionWorker = new Thread (new ThreadStart (ReceiveData));
            connectionWorker.IsBackground = true;
            connectionWorker.Start ();
        }

        /// <summary>
        /// Creates a network connection to the current <see cref="ClientConnection.Address"/> and <see cref="ClientConnection.Port"/>
        /// </summary>
        /// <remarks>
        /// <p>When using this class from an application, 
        /// you need to pass in a control so that data-receiving thread can sync with your application.</p>
        /// <p>If calling this from a form or other control, just pass in the current instance.</p>
        /// </remarks>
        /// <example>
        /// <code>
        /// [C#]
        /// client.Connection.Connect(this);
        /// 
        /// [VB]
        /// client.Connection.Connect(Me)
        /// </code>
        /// </example>
        public virtual void Connect (ISynchronizeInvoke syncObject)
        {
            SynchronizationObject = syncObject;
            Connect ();
        }

        /// <summary>
        /// Closes the current network connection.
        /// </summary>
        public virtual void Disconnect ()
        {
            Status = ConnectionStatus.Disconnected;
        }

        /// <summary>
        /// Forces closing the current network connection and kills the thread running it.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public virtual void DisconnectForce ()
        {
            Disconnect ();
            if (connectionWorker != null) {
                try {
                    connectionWorker.Abort ();
                } catch {
                }
            }
        }

        /// <summary>
        /// Sends the given string over the network
        /// </summary>
        /// <param name="data">The <see cref="System.String"/> to send.</param>
        public virtual void Write (string data)
        {
            if (chatWriter == null || chatWriter.BaseStream == null || !chatWriter.BaseStream.CanWrite) {
                throw new InvalidOperationException (NeboResources.ConnectionCanNotBeWrittenToYet);
            }

            if (data == null) {
                return;
            }


            if (!data.EndsWith ("\r\n", StringComparison.Ordinal)) {
                data += "\r\n";
            }
            if (data.Length > 512) {
                throw new Messages.InvalidMessageException (NeboResources.MessagesAreLimitedInSize, data);
            }

            try {
                chatWriter.WriteLine (data);
                chatWriter.Flush ();
                OnDataSent (new ConnectionDataEventArgs (data));
            } catch (Exception ex) {
                System.Diagnostics.Trace.WriteLine ("Couldn't Send '" + data + "'. " + ex.ToString ());
                throw;
            }
        }

        /// <summary>
        /// Releases the resources used by the <see cref="ClientConnection"/>
        /// </summary>
        protected override void Dispose (bool disposing)
        {
            try {
                if (disposing) {
                    if (chatClient != null) {
                        ((IDisposable)chatClient).Dispose ();
                    }
                    if (chatReader != null) {
                        ((IDisposable)chatReader).Dispose ();
                    }
                    if (chatWriter != null) {
                        ((IDisposable)chatWriter).Dispose ();
                    }
                }
            } finally {
                base.Dispose (disposing);
            }
        }

        #endregion

        #region Protected Event Raisers

        /// <summary>
        /// Raises the <see cref="ClientConnection.Connecting"/> event of the <see cref="ClientConnection"/> object.
        /// </summary>
        protected virtual void OnConnecting (EventArgs e)
        {
            if (Connecting != null) {
                Connecting (this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="ClientConnection.Connected"/> event of the <see cref="ClientConnection"/> object.
        /// </summary>
        protected virtual void OnConnected (EventArgs e)
        {
            if (synchronizationObject != null && synchronizationObject.InvokeRequired) {
                SyncInvoke del = delegate {
                    OnConnected (e);
                };
                synchronizationObject.Invoke (del, null);
                return;
            }

            if (Connected != null) {
                Connected (this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="ClientConnection.DataReceived"/> event of the <see cref="ClientConnection"/> object.
        /// </summary>
        /// <param name="e">A <see cref="ConnectionDataEventArgs"/> that contains the data.</param>
        protected virtual void OnDataReceived (ConnectionDataEventArgs e)
        {
            if (synchronizationObject != null && synchronizationObject.InvokeRequired) {
                SyncInvoke del = delegate {
                    OnDataReceived (e);
                };
                synchronizationObject.Invoke (del, null);
                return;
            }

            if (DataReceived != null) {
                DataReceived (this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="ClientConnection.DataSent"/> event of the <see cref="ClientConnection"/> object.
        /// </summary>
        /// <param name="data">A <see cref="ConnectionDataEventArgs"/> that contains the data.</param>
        protected virtual void OnDataSent (ConnectionDataEventArgs data)
        {
            if (DataSent != null) {
                DataSent (this, data);
            }
        }

        /// <summary>
        /// Raises the <see cref="ClientConnection.Disconnected"/> event of the <see cref="ClientConnection"/> object.
        /// </summary>
        protected virtual void OnDisconnected (ConnectionDataEventArgs e)
        {
            if (synchronizationObject != null && synchronizationObject.InvokeRequired) {
                SyncInvoke del = delegate {
                    OnDisconnected (e);
                };
                synchronizationObject.Invoke (del, null);
                return;
            }

            if (Disconnected != null) {
                Disconnected (this, e);
            }
        }

        #endregion

        #region Private

        static bool ValidateServerCertificate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None) {
                return true;
            }

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        /// <summary>
        /// This method listens for data over the network until the Connection.State is Disconnected.
        /// </summary>
        /// <remarks>
        /// ReceiveData runs in its own thread.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void ReceiveData ()
        {

            try {
                chatClient = new TcpClient (Address, Port);
                Stream dataStream = null;
                if (Ssl) {
                    dataStream = new SslStream (chatClient.GetStream (), false, ValidateServerCertificate, null);
                    ((SslStream)dataStream).AuthenticateAsClient (Address);
                } else {
                    dataStream = chatClient.GetStream ();
                }


                chatReader = new StreamReader (dataStream, Encoding);
                chatWriter = new StreamWriter (dataStream, Encoding);
                chatWriter.AutoFlush = true;
            } catch (AuthenticationException e) {
                if (chatClient != null) {
                    chatClient.Close ();
                }
                Status = ConnectionStatus.Disconnected;
                OnDisconnected (new ConnectionDataEventArgs (e.Message));
                return;
            } catch (Exception ex) {
                Status = ConnectionStatus.Disconnected;
                OnDisconnected (new ConnectionDataEventArgs (ex.Message));
                return;
            }

            Status = ConnectionStatus.Connected;
            OnConnected (EventArgs.Empty);

            string disconnectReason = "";

            try {
                string incomingMessageLine;

                while (Status == ConnectionStatus.Connected && ((incomingMessageLine = chatReader.ReadLine ()) != null)) {
                    try {
                        incomingMessageLine = incomingMessageLine.Trim ();
                        OnDataReceived (new ConnectionDataEventArgs (incomingMessageLine));
                    } catch (ThreadAbortException ex) {
                        System.Diagnostics.Trace.WriteLine (ex.Message);
                        Thread.ResetAbort ();
                        disconnectReason = "Thread Aborted";
                        break;
                    }
                }
            } catch (Exception ex) {
                System.Diagnostics.Trace.WriteLine (ex.ToString ());
                disconnectReason = ex.Message;
            }
            Status = ConnectionStatus.Disconnected;

            if (chatClient != null) {
                chatClient.Close ();
                chatClient = null;
            }

            ConnectionDataEventArgs disconnectArgs = new ConnectionDataEventArgs (disconnectReason);
            OnDisconnected (disconnectArgs);
        }

        object lockObject = new object ();

        string address;
        int port;
        ConnectionStatus status = ConnectionStatus.Disconnected;
        System.Text.Encoding _encoding;
        bool _ssl;

        TcpClient chatClient;
        StreamReader chatReader;
        StreamWriter chatWriter;
        Thread connectionWorker;

        ISynchronizeInvoke synchronizationObject = null;
        delegate void SyncInvoke ();

        #endregion

    }
}