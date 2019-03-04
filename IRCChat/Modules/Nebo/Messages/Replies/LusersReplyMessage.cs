using System;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// One of the responses to the <see cref="LusersMessage"/> query.
    /// </summary>
    [Serializable]
    public class LusersReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="LusersReplyMessage"/> class.
        /// </summary>
        public LusersReplyMessage ()
        {
            InternalNumeric = 251;
        }


        /// <summary>
        /// Gets or sets the number of users connected to irc.
        /// </summary>
        public virtual int UserCount {
            get {
                return userCount;
            }
            set {
                userCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of invisible users connected to irc.
        /// </summary>
        public virtual int InvisibleCount {
            get {
                return invisibleCount;
            }
            set {
                invisibleCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of servers connected on the network.
        /// </summary>
        public virtual int ServerCount {
            get {
                return serverCount;
            }
            set {
                serverCount = value;
            }
        }

        int userCount = -1;
        int invisibleCount = -1;
        int serverCount = -1;
        string thereAre = "There are ";
        string usersAnd = " users and ";
        string invisibleOn = " invisible on ";
        string servers = " servers";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (thereAre + UserCount + usersAnd + InvisibleCount + invisibleOn + serverCount + servers);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            string payload = parameters [1];
            UserCount = Convert.ToInt32 (MessageUtil.StringBetweenStrings (payload, thereAre, usersAnd), CultureInfo.InvariantCulture);
            InvisibleCount = Convert.ToInt32 (MessageUtil.StringBetweenStrings (payload, usersAnd, invisibleOn), CultureInfo.InvariantCulture);
            ServerCount = Convert.ToInt32 (MessageUtil.StringBetweenStrings (payload, invisibleOn, servers), CultureInfo.InvariantCulture);

        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnLusersReply (new IrcMessageEventArgs<LusersReplyMessage> (this));
        }

    }
}
