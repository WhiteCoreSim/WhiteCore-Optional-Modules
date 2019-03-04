using System;
using System.Collections.Generic;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Provides clients with a correct <see cref="IrcMessage"/> for a given raw message string
    /// </summary>
    public sealed class MessageParserService
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageParserService"/> class.
        /// </summary>
        /// <remarks>
        /// This is private because this class is a Singleton.
        /// Use the <see cref="Service"/> to get the only instance of this class.
        /// </remarks>
        MessageParserService ()
        {
            foreach (Type type in GetType ().Assembly.GetTypes ()) {
                if (type.IsSubclassOf (typeof (IrcMessage))) {
                    if (type.IsSubclassOf (typeof (CommandMessage))) {
                        if (!type.IsAbstract) {
                            commands.AddLast ((IrcMessage)Activator.CreateInstance (type));
                        }
                    } else if (type.IsSubclassOf (typeof (NumericMessage))) {
                        if (!type.IsAbstract && type != typeof (GenericNumericMessage) && type != typeof (GenericErrorMessage)) {
                            numerics.AddLast ((IrcMessage)Activator.CreateInstance (type));
                        }
                    } else if (type.IsSubclassOf (typeof (CtcpMessage))) {
                        if (!type.IsAbstract && type != typeof (GenericCtcpRequestMessage) && type != typeof (GenericCtcpReplyMessage)) {
                            ctcps.AddLast ((IrcMessage)Activator.CreateInstance (type));
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Provides access to clients to lone instance of the <see cref="MessageParserService"/>.
        /// </summary>
        /// <returns>The Singleton-patterned service.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly MessageParserService Service = new MessageParserService ();


        /// <summary>
        /// Adds a custom message to consider for parsing raw messages recevied from the server.
        /// </summary>
        /// <param name="msg"></param>
        public void AddCustomMessage (IrcMessage msg)
        {
            if (customs == null) {
                customs = new PrioritizedMessageList ();
            }
            customs.AddLast (msg);
        }


        /// <summary>
        /// Parses the given string into an <see cref="IrcMessage"/>.
        /// </summary>
        /// <param name="unparsedMessage">The string to parse.</param>
        /// <returns>An <see cref="IrcMessage"/> which represents the given string.</returns>
        public IrcMessage Parse (string unparsedMessage)
        {
            if (unparsedMessage == null) {
                unparsedMessage = "";
            }
            if (unparsedMessage.Length < MinMessageLength || MaxMessageLength < unparsedMessage.Length) {
                InvalidMessageException e = new InvalidMessageException (string.Format (CultureInfo.InvariantCulture, NeboResources.MessageEmptyOrTooLong, unparsedMessage.Length), unparsedMessage);
                throw (e);
            }

            IrcMessage msg = null;
            try {
                msg = DetermineMessage (unparsedMessage);
                msg.Parse (unparsedMessage);
            }
#pragma warning disable 0168
            catch (InvalidMessageException ex) {
                throw;
            }
#pragma warning restore 0168
            catch (Exception ex) {
                throw new InvalidMessageException (NeboResources.CouldNotParseMessage, unparsedMessage, ex);
            }
            return msg;
        }

        /// <summary>
        /// Determines and instantiates the correct subclass of <see cref="IrcMessage"/> for the given given string.
        /// </summary>
        IrcMessage DetermineMessage (string unparsedMessage)
        {
            IrcMessage msg = null;

            if (customs != null) {
                msg = GetMessage (unparsedMessage, customs);
                if (msg != null) {
                    return msg;
                }
            }

            string command = MessageUtil.GetCommand (unparsedMessage);
            if (char.IsDigit (command [0])) {
                msg = GetMessage (unparsedMessage, numerics);
                if (msg == null) {
                    int numeric = Convert.ToInt32 (command, CultureInfo.InvariantCulture);
                    if (NumericMessage.IsError (numeric)) {
                        msg = new GenericErrorMessage ();
                    } else {
                        msg = new GenericNumericMessage ();
                    }
                }
            } else {
                if (CtcpUtil.IsCtcpMessage (unparsedMessage)) {
                    msg = GetMessage (unparsedMessage, ctcps);
                    if (msg == null) {
                        if (CtcpUtil.IsRequestMessage (unparsedMessage)) {
                            msg = new GenericCtcpRequestMessage ();
                        } else {
                            msg = new GenericCtcpReplyMessage ();
                        }
                    }
                } else {
                    msg = GetMessage (unparsedMessage, commands);
                    if (msg == null) {
                        msg = new GenericMessage ();
                    }
                }
            }
            return msg;
        }

        static IrcMessage GetMessage (string unparsedMessage, PrioritizedMessageList potentialHandlers)
        {
            IrcMessage handler = null;
            LinkedListNode<IrcMessage> nodeToPrioritize = null;

            LinkedListNode<IrcMessage> node = potentialHandlers.First;
            if (node != null) {
                do {
                    IrcMessage msg = node.Value;

                    try {
                        if (msg.CanParse (unparsedMessage)) {
                            nodeToPrioritize = node;
                            handler = msg.CreateInstance ();
                            break;
                        }
                        node = node.Next;
                    } catch {
                        System.Diagnostics.Trace.WriteLine ("Error testing CanParse on { " + unparsedMessage + " }", "Parse Error");
                        throw;
                    }
                }
                while (node != null && node.Next != potentialHandlers.First);
            }


            if (nodeToPrioritize != null) {
                potentialHandlers.Prioritize (nodeToPrioritize);
            }

            return handler;
        }

        const int MinMessageLength = 1;
        const int MaxMessageLength = 512;

        PrioritizedMessageList numerics = new PrioritizedMessageList ();
        PrioritizedMessageList commands = new PrioritizedMessageList ();
        PrioritizedMessageList ctcps = new PrioritizedMessageList ();
        PrioritizedMessageList customs = null;

    }
}
