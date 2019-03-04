namespace MetaBuilders.Irc.Messages
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Globalization;


    /// <summary>
    /// Exception thrown when a message parsed from a string is invalid.
    /// </summary>
    [
    Serializable (),
    ]
    public class InvalidMessageException : Exception, ISerializable
    {

        /// <summary>
        /// Initializes a new instance of the InvalidMessageException class.
        /// </summary>
        public InvalidMessageException ()
        {
        }

        /// <summary>
        /// Intializes a new instance of the InvalidMessageException class with the given message.
        /// </summary>
        /// <param name="message"></param>
        public InvalidMessageException (string message)
            :
            base (message)
        {
        }

        /// <summary>
        /// Intializes a new instance of the InvalidMessageException class with the given message and inner exception.
        /// </summary>
        public InvalidMessageException (string message, Exception innerException)
            :
            base (message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the InvalidMessageException class with the given Message and RecivedMessage.
        /// </summary>
        /// <param name="message">Message explaining the exception.</param>
        /// <param name="receivedMessage">The Message received which was invalid.</param>
        public InvalidMessageException (string message, string receivedMessage)
            :
            base (message)
        {
            _receivedMessage = receivedMessage;
        }

        /// <summary>
        /// Initializes a new instance of the InvalidMessageException class with the given Message and RecivedMessage.
        /// </summary>
        /// <param name="message">Message explaining the exception.</param>
        /// <param name="receivedMessage">The Message received which was invalid.</param>
        /// <param name="innerException">The exception that exists as the child exception to this one.</param>
        public InvalidMessageException (string message, string receivedMessage, Exception innerException)
            :
            base (message, innerException)
        {
            _receivedMessage = receivedMessage;
        }

        #region ISerializable
        /// <summary>
        /// deserialization constructor
        /// </summary>
        protected InvalidMessageException (SerializationInfo info, StreamingContext context)
            :
            base (info, context)
        {
            //if ( info == null )
            //{
            //	return;
            //}
            _receivedMessage = info.GetString ("ReceivedMessage");
        }


        /// <summary>
        /// Implements <see cref="ISerializable.GetObjectData"/>.
        /// </summary>
        [SecurityPermission (SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        [SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData (SerializationInfo info, StreamingContext context)
        {
            if (info != null) {
                base.GetObjectData (info, context);
                info.AddValue ("ReceivedMessage", _receivedMessage);
            }
        }
        #endregion


        #region Properties

        /// <summary>
        /// Gets the message string that caused the exception.
        /// </summary>
        public virtual string ReceivedMessage {
            get {
                return _receivedMessage;
            }
        }
        string _receivedMessage = "";

        #endregion

        /// <summary>
        /// Gets the string content of the invalid message.
        /// </summary>
        public override string Message {
            get {
                string s = string.Format (CultureInfo.InvariantCulture, "ReceivedMessage: {0}", _receivedMessage);
                return base.Message + Environment.NewLine + s;
            }
        }


    }
}
