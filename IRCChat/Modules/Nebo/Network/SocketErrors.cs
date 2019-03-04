

namespace MetaBuilders.Irc.Network
{

    /// <summary>
    /// A partial list of the codes that can exist in the <see cref="System.Net.Sockets.SocketException.ErrorCode"/> property.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum SocketError
    {
        /// <summary>
        /// A blocking operation was interrupted by a call to WSACancelBlockingCall.
        /// </summary>
        InterruptedFunctionCall = 10004,
        /// <summary>
        /// An attempt was made to access a socket in a way forbidden by its access permissions. 
        /// </summary>
        PermissionDenied = 10013,
        /// <summary>
        /// The system detected an invalid pointer address in attempting to use a pointer argument of a call.
        /// </summary>
        BadAddress = 10014,
        /// <summary>
        /// Some invalid argument was supplied (for example, specifying an invalid level to the setsockopt function). 
        /// </summary>
        InvalidArgument = 10022,
        /// <summary>
        /// Too many open sockets. 
        /// </summary>
        TooManyOpenFiles = 10024,
        /// <summary>
        /// This error is returned from operations on nonblocking sockets that cannot be completed immediately.
        /// </summary>
        ResourceTemporarilyUnavailable = 10035,
        /// <summary>
        /// A blocking operation is currently executing. 
        /// </summary>
        OperationNowInProgress = 10036,
        /// <summary>
        /// An operation was attempted on a nonblocking socket with an operation already in progress
        /// </summary>
        OperationAlreadyInProgress = 10037,
        /// <summary>
        /// An operation was attempted on something that is not a socket.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Nonsocket")]
        SocketOperationOnNonsocket = 10038,
        /// <summary>
        /// A required address was omitted from an operation on a socket.
        /// </summary>
        DestinationAddressRequired = 10039,
        /// <summary>
        /// A message sent on a datagram socket was larger than the internal message buffer or some other network limit, or the buffer used to receive a datagram was smaller than the datagram itself. 
        /// </summary>
        MessageTooLong = 10040,
        /// <summary>
        /// A protocol was specified in the socket function call that does not support the semantics of the socket type requested.
        /// </summary>
        ProtocolWrongTypeForSocket = 10041,
        /// <summary>
        /// An unknown, invalid or unsupported option or level was specified
        /// </summary>
        BadProtocolOption = 10042,
        /// <summary>
        /// The requested protocol has not been configured into the system, or no implementation for it exists.
        /// </summary>
        ProtocolNotSupported = 10043,
        /// <summary>
        /// The support for the specified socket type does not exist in this address family. 
        /// </summary>
        SocketTypeNotSupported = 10044,
        /// <summary>
        /// The attempted operation is not supported for the type of object referenced.
        /// </summary>
        OperationNotSupported = 10045,
        /// <summary>
        /// The protocol family has not been configured into the system or no implementation for it exists.
        /// </summary>
        ProtocolFamilyNotSupported = 10046,
        /// <summary>
        /// An address incompatible with the requested protocol was used.
        /// </summary>
        AddressFamilyNotSupportedByProtocolFamily = 10047,
        /// <summary>
        /// Typically, only one usage of each socket address (protocol/IP address/port) is permitted. This error occurs if an application attempts to bind a socket to an IP address/port that has already been used for an existing socket, or a socket that was not closed properly, or one that is still in the process of closing.
        /// </summary>
        AddressAlreadyInUse = 10048,
        /// <summary>
        /// The requested address is not valid in its context.
        /// </summary>
        CannotAssignRequestedAddress = 10049,
        /// <summary>
        /// A socket operation encountered a dead network.
        /// </summary>
        NetworkIsDown = 10050,
        /// <summary>
        /// A socket operation was attempted to an unreachable network.
        /// </summary>
        NetworkIsUnreachable = 10051,
        /// <summary>
        /// The connection has been broken due to keep-alive activity detecting a failure while the operation was in progress.
        /// </summary>
        NetworkDroppedConnectionOnReset = 10052,
        /// <summary>
        /// An established connection was aborted by the software in your host computer, possibly due to a data transmission time-out or protocol error
        /// </summary>
        SoftwareCausedConnectionAbort = 10053,
        /// <summary>
        /// An existing connection was forcibly closed by the remote host.
        /// </summary>
        ConnectionResetByPeer = 10054,
        /// <summary>
        /// An operation on a socket could not be performed because the system lacked sufficient buffer space or because a queue was full. 
        /// </summary>
        NoBufferSpaceAvailable = 10055,
        /// <summary>
        /// A connect request was made on an already-connected socket. 
        /// </summary>
        SocketIsAlreadyConnected = 10056,
        /// <summary>
        /// A request to send or receive data was disallowed because the socket is not connected and no address was supplied.
        /// </summary>
        SocketIsNotConnected = 10057,
        /// <summary>
        /// A request to send or receive data was disallowed because the socket had already been shut down in that direction with a previous shutdown call.
        /// </summary>
        CannotSendAfterSocketShutdown = 10058,
        /// <summary>
        /// A connection attempt failed because the connected party did not properly respond after a period of time, or the established connection failed because the connected host has failed to respond. 
        /// </summary>
        ConnectionTimedOut = 10060,
        /// <summary>
        /// No connection could be made because the target computer actively refused it. 
        /// </summary>
        ConnectionRefused = 10061,
        /// <summary>
        /// A socket operation failed because the destination host is down.
        /// </summary>
        HostIsDown = 10064,
        /// <summary>
        /// A socket operation was attempted to an unreachable host.
        /// </summary>
        NoRouteToHost = 10065,
        /// <summary>
        /// A Windows Sockets implementation may have a limit on the number of applications that can use it simultaneously.
        /// </summary>
        TooManyProcesses = 10067,
        /// <summary>
        /// This error is returned if the Windows Sockets implementation cannot function at this time because the underlying system it uses to provide network services is currently unavailable. 
        /// </summary>
        NetworkSubsystemIsUnavailable = 10091,
        /// <summary>
        /// The current Windows Sockets implementation does not support the Windows Sockets specification version requested by the application.
        /// </summary>
        WinsockDllVersionOutOfRange = 10092,
        /// <summary>
        /// Successful WSAStartup not yet performed. 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "WSA")]
        SuccessfulWSAStartupNotYetPerformed = 10093,
        /// <summary>
        /// Returned to indicate that the remote party has initiated a graceful shutdown sequence. 
        /// </summary>
        GracefulShutdownInProgress = 10101,
        /// <summary>
        /// The specified class was not found. 
        /// </summary>
        ClassTypeNotFound = 10109,
        /// <summary>
        /// No such host is known. The name is not an official host name or alias, or it cannot be found in the database(s) being queried.
        /// </summary>
        HostNotFound = 11001,
        /// <summary>
        /// This is usually a temporary error during host name resolution and means that the local server did not receive a response from an authoritative server. A retry at some time later may be successful. 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Nonauthoritative")]
        NonauthoritativeHostNotFound = 11002,
        /// <summary>
        /// This indicates some sort of nonrecoverable error occurred during a database lookup.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Nonrecoverable")]
        ThisIsANonrecoverableError = 11003,
        /// <summary>
        /// The requested name is valid and was found in the database, but it does not have the correct associated data being resolved for.
        /// </summary>
        NoDataRecordOfRequestedType = 11004,
    }
}
