using System.Collections.Specialized;

namespace MetaBuilders.Irc.Dcc
{

    /// <summary>
    /// Holds a few utilities for dcc message parsing.
    /// </summary>
    public sealed class DccUtil
    {

        /// <summary>
        /// Do not use under penalty of death
        /// </summary>
        DccUtil()
        {
        }

        /// <summary>
        /// Gets the Dcc Command of the message, such as CHAT or SEND.
        /// </summary>
        public static string GetCommand(string rawMessage)
        {
            return GetParameters(rawMessage)[0];
        }

        /// <summary>
        /// Gets the Dcc Argument of the message, such as the filename of a SEND.
        /// </summary>
        public static string GetArgument(string rawMessage)
        {
            return GetParameters(rawMessage)[1];
        }

        /// <summary>
        /// Gets the address of the connection instantiator in Int64 format as a String.
        /// </summary>
        public static string GetAddress(string rawMessage)
        {
            return GetParameters(rawMessage)[2];
        }

        /// <summary>
        /// Gets the port of the connection instantiator.
        /// </summary>
        public static string GetPort(string rawMessage)
        {
            return GetParameters(rawMessage)[3];
        }

        /// <summary>
        /// Gets the inner parameters of a dcc data area.
        /// </summary>
        public static StringCollection GetParameters(string rawMessage)
        {
            string extendedData = Messages.CtcpUtil.GetExtendedData(rawMessage);
            return Messages.MessageUtil.Tokenize(extendedData, 0);
        }
    }
}
