using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// A Monitor system notification that contains a list of nicks
    /// </summary>
    [Serializable]
    public abstract class MonitoredNicksListMessage : NumericMessage
    {

        /// <summary>
        /// Gets the collection of nicks of users for the message.
        /// </summary>
        public StringCollection Nicks {
            get {
                if (nicks == null) {
                    nicks = new StringCollection ();
                }
                return nicks;
            }
        }
        StringCollection nicks;

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddList (Nicks, ",", true);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);

            Nicks.Clear ();

            if (parameters.Count > 1) {
                string userListParam = parameters [1];
                string [] userList = userListParam.Split (new string [] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string nick in userList) {
                    Nicks.Add (nick);
                }
            }
        }

    }
}
