using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;


namespace MetaBuilders.Irc.Network
{

    /// <summary>
    /// Represents a persistent network connection where this side waits for connections.
    /// </summary>
    [DesignerCategory ("Code")]
    public class ServerConnection : Component
    {

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerConnection"/> class.
        /// </summary>
        public ServerConnection ()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerConnection"/> class on the given port.
        /// </summary>
        /// <param name="port">The port to listen on.</param>
        public ServerConnection (int port)
        {
            Port = port;
        }
        #endregion

        #region Events

        /// <summary>
        /// Occurs when the <see cref="ServerConnection"/> recieves data.
        /// </summary>
        internal event EventHandler<ConnectionDataEventArgs> DataReceived;

        /// <summary>
        /// Occurs when the <see cref="ServerConnection"/> sends data.
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
        /// Gets or sets the port which the <see cref="ServerConnection"/> will communicate over.
        /// </summary>
        /// <remarks>
        /// <para>A <see cref="NotSupportedException"/> will be thrown if an attempt is made to change the <see cref="ServerConnection.Port"/> if the <see cref="ServerConnection.Status"/> is not <see cref="ConnectionStatus.Disconnected"/>.</para>
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
        /// Gets or sets the length of time to wait after calling <see cref="Listen"/> before the thread will stop waiting for a connection.
        /// </summary>
        public TimeSpan TimeOut {
            get {
                return timeOut;
            }
            set {
                timeOut = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="ConnectionStatus"/> of the <see cref="ServerConnection"/>.
        /// </summary>
        public ConnectionStatus Status {
            get {
                return status;
            }
        }
        void setStatus (ConnectionStatus value)
        {
            status = value;
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
        #endregion

        #region Methods

        /// <summary>
        /// Waits for a network connection on the current <see cref="ServerConnection.Port"/>.
        /// </summary>
        /// <remarks>
        /// Only use this overload if your application is not a Windows.Forms application, you've set the <see cref="SynchronizationObject"/> property, or you want to handle threading issues yourself.
        /// </remarks>
        public virtual void Listen ()
        {
            lock (lockObject) {
                if (Status != ConnectionStatus.Disconnected) {
                    throw new InvalidOperationException (NeboResources.AlreadyConnectToAnotherClient);
                }

                setStatus (ConnectionStatus.Connecting);
                OnConnecting (EventArgs.Empty);

                InitializeDelegates ();

            }

            connectionWorker = new Thread (new ThreadStart (Run));
            connectionWorker.IsBackground = true;
            connectionWorker.Start ();

            if (TimeOut != TimeSpan.Zero) {
                timeoutTimer = new Timer (new TimerCallback (checkTimeOut), null, TimeOut, TimeSpan.Zero);
            }
        }

        object lockObject = new object ();

        Timer timeoutTimer;

        /// <summary>
        /// Waits for a network connection on the current <see cref="Port"/>.
        /// </summary>
        /// <param name="syncObject">
        /// The <see cref="System.Windows.Forms.Control"/> which this will synchronize with when calling events.
        /// </param>
        /// <remarks>
        /// <p>When using this class from a Windows.Forms application, 
        /// you need to pass in a control so that data-receiving thread can sync with your application.</p>
        /// <p>If calling this from a form or other control, just pass in the current instance.</p>
        /// </remarks>
        /// <example>
        /// <code>
        /// [C#]
        /// myServerConnection.Listen(this);
        /// 
        /// [VB]
        /// myServerConnection.Listen(Me)
        /// </code>
        /// </example>
        public virtual void Listen (ISynchronizeInvoke syncObject)
        {
            synchronizationObject = syncObject;
            Listen ();
        }



        void checkTimeOut (object state)
        {
            if (Status == ConnectionStatus.Connecting) {
                DisconnectForce ();
            }
            if (timeoutTimer != null) {
                timeoutTimer.Dispose ();
                timeoutTimer = null;
            }
        }

        /// <summary>
        /// Closes the current network connection.
        /// </summary>
        public virtual void Disconnect ()
        {
            setStatus (ConnectionStatus.Disconnected);
            ConnectionDataEventArgs disconnectArgs = new ConnectionDataEventArgs ("Disconnect Called");
            if (synchronizationObject != null && synchronizationObject.InvokeRequired) {
                synchronizationObject.Invoke (onDisconnectedDelegate, new object [] { disconnectArgs });
            } else {
                OnDisconnected (disconnectArgs);
            }

        }

        /// <summary>
        /// Forces closing the current network connection and kills the thread running it.
        /// </summary>
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
            if (data == null) {
                return;
            }

            if (!data.EndsWith ("\n")) {
                data += "\n";
            }

            try {
                if (chatWriter == null || !chatWriter.BaseStream.CanWrite) {
                    Debug.WriteLine ("Couldn't Send '" + data + "'");
                    return;
                }
                chatWriter.WriteLine (data);
                chatWriter.Flush ();
                OnDataSent (new ConnectionDataEventArgs (data));
            } catch {
                Debug.WriteLine ("Couldn't Send '" + data + "'");
                throw;
            }
        }

        /// <summary>
        /// Releases the resources used by the <see cref="ServerConnection"/>
        /// </summary>
        protected override void Dispose (bool disposing)
        {
            try {
                if (disposing) {
                    if (chatListener != null) {
                        ((IDisposable)chatListener).Dispose ();
                    }
                    if (chatReader != null) {
                        ((IDisposable)chatReader).Dispose ();
                    }
                    if (chatWriter != null) {
                        ((IDisposable)chatWriter).Dispose ();
                    }
                    if (timeoutTimer != null) {
                        timeoutTimer.Dispose ();
                    }
                }
            } finally {
                base.Dispose (disposing);
            }
        }

        #endregion

        #region Protected Event Raisers
        delegate void ConnectionDataEventRaiser (ConnectionDataEventArgs e);
        delegate void EventRaiser (EventArgs e);

        /// <summary>
        /// Raises the <see cref="DataReceived"/> event of the <see cref="ServerConnection"/> object.
        /// </summary>
        /// <param name="data">A <see cref="ConnectionDataEventArgs"/> that contains the data.</param>
        protected virtual void OnDataReceived (ConnectionDataEventArgs data)
        {
            if (DataReceived != null) {
                DataReceived (this, data);
            }
        }
        ConnectionDataEventRaiser onDataReceivedDelegate;


        /// <summary>
        /// Raises the <see cref="DataSent"/> event of the <see cref="ServerConnection"/> object.
        /// </summary>
        /// <param name="data">A <see cref="ConnectionDataEventArgs"/> that contains the data.</param>
        protected virtual void OnDataSent (ConnectionDataEventArgs data)
        {
            if (DataSent != null) {
                DataSent (this, data);
            }
        }

        /// <summary>
        /// Raises the <see cref="Connecting"/> event of the <see cref="ServerConnection"/> object.
        /// </summary>
        protected virtual void OnConnecting (EventArgs e)
        {
            if (Connecting != null) {
                Connecting (this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="Connected"/> event of the <see cref="ServerConnection"/> object.
        /// </summary>
        protected virtual void OnConnected (EventArgs e)
        {
            if (Connected != null) {
                Connected (this, e);
            }
        }
        EventRaiser onConnectedDelegate;


        /// <summary>
        /// Raises the <see cref="Disconnected"/> event of the <see cref="ServerConnection"/> object.
        /// </summary>
        protected virtual void OnDisconnected (ConnectionDataEventArgs e)
        {
            if (Disconnected != null) {
                Disconnected (this, e);
            }
        }
        ConnectionDataEventRaiser onDisconnectedDelegate;




        #endregion

        #region Private

        /// <summary>
        /// This method listens for data over the network until the Connection.State is Disconnected.
        /// </summary>
        /// <remarks>
        /// Run runs in its own thread.
        /// </remarks>
        void Run ()
        {

            ConnectionDataEventArgs disconnectArgs;
            string disconnectReason = "";


            try {
                chatListener = new TcpListener (System.Net.IPAddress.Any, Port);
                chatListener.Start ();

                Debug.WriteLine ("Starting AcceptTcpClient", "ServerConnection");

                TcpClient client = chatListener.AcceptTcpClient ();
                chatReader = new StreamReader (client.GetStream (), System.Text.Encoding.UTF8);
                chatWriter = new StreamWriter (client.GetStream (), System.Text.Encoding.UTF8);

                setStatus (ConnectionStatus.Connected);
                if (synchronizationObject != null && synchronizationObject.InvokeRequired) {
                    synchronizationObject.Invoke (onConnectedDelegate, new object [] { EventArgs.Empty });
                } else {
                    OnConnected (EventArgs.Empty);
                }

                Debug.WriteLine ("TcpClient Accepted", "ServerConnection");

            } catch (Exception ex) {
                Debug.WriteLine ("Error Opening ServerConnection On Port " + port.ToString () + ", " + ex.ToString (), "ServerConnection");
                setStatus (ConnectionStatus.Disconnected);
                disconnectArgs = new ConnectionDataEventArgs (disconnectReason);
                if (synchronizationObject != null && synchronizationObject.InvokeRequired) {
                    synchronizationObject.Invoke (onDisconnectedDelegate, new object [] { disconnectArgs });
                } else {
                    OnDisconnected (disconnectArgs);
                }
                throw;
            }



            try {
                string incomingMessageLine;

                while (Status == ConnectionStatus.Connected && ((incomingMessageLine = chatReader.ReadLine ()) != null)) {
                    try {
                        incomingMessageLine = incomingMessageLine.Trim ();
                        if (synchronizationObject != null && synchronizationObject.InvokeRequired) {
                            synchronizationObject.Invoke (onDataReceivedDelegate, new object [] { new ConnectionDataEventArgs (incomingMessageLine) });
                        } else {
                            OnDataReceived (new ConnectionDataEventArgs (incomingMessageLine));
                        }
                    } catch (ThreadAbortException ex) {
                        Debug.WriteLine (ex.Message);
                        Thread.ResetAbort ();
                        disconnectReason = "Thread Aborted";
                        break;
                    }
                }
            } catch (Exception ex) {
                Debug.WriteLine (ex.ToString ());
                disconnectReason = ex.Message;
            }
            setStatus (ConnectionStatus.Disconnected);

            if (chatListener != null) {
                chatListener.Stop ();
                chatListener = null;
            }

            disconnectArgs = new ConnectionDataEventArgs (disconnectReason);

            if (synchronizationObject != null && synchronizationObject.InvokeRequired) {
                try {
                    synchronizationObject.Invoke (onDisconnectedDelegate, new object [] { disconnectArgs });
                } catch {
                    OnDisconnected (disconnectArgs);
                }
            } else {
                OnDisconnected (disconnectArgs);
            }
        }



        void InitializeDelegates ()
        {
            onDataReceivedDelegate = new ConnectionDataEventRaiser (OnDataReceived);
            onDisconnectedDelegate = new ConnectionDataEventRaiser (OnDisconnected);
            onConnectedDelegate = new EventRaiser (OnConnected);
        }

        TimeSpan timeOut = TimeSpan.Zero;
        int port;
        ConnectionStatus status = ConnectionStatus.Disconnected;

        TcpListener chatListener;
        StreamReader chatReader;
        StreamWriter chatWriter;
        Thread connectionWorker;
        ISynchronizeInvoke synchronizationObject;
        #endregion

    }
}