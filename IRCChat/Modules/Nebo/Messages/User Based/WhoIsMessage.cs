using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Requests information from the server about the users specified.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Possible reply messages include:
    /// <see cref="T:NoSuchServerMessage"/>
    /// <see cref="T:NoNickGivenMessage"/>
    /// <see cref="T:NoSuchNickMessage"/>
    /// 
    /// <see cref="T:WhoIsUserReplyMessage"/>
    /// <see cref="T:WhoIsChannelsReplyMessage"/>
    /// <see cref="T:WhoIsServerReplyMessage"/>
    /// <see cref="T:WhoIsOperReplyMessage"/>
    /// <see cref="T:WhoIsIdleReplyMessage"/>
    /// 
    /// <see cref="T:UserAwayMessage" />
    /// <see cref="T:WhoIsEndReplyMessage"/>
    /// </para>
    /// </remarks>
    [Serializable]
    public class WhoIsMessage : CommandMessage
    {

        /// <summary>
        /// Gets the collection of users that information is requested for.
        /// </summary>
        public virtual UserCollection Masks {
            get {
                return masks;
            }
        }
        UserCollection masks = new UserCollection ();

        /// <summary>
        /// Gets or sets the server which should return the information.
        /// </summary>
        public virtual string Server {
            get {
                return server;
            }
            set {
                server = value;
            }
        }

        /// <summary>
        /// Gets the Irc command associated with this message.
        /// </summary>
        protected override string Command {
            get {
                return "WHOIS";
            }
        }

        string server = "";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Server);
            writer.AddList (Masks, ",", true);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            Masks.Clear ();
            Server = "";
            if (parameters.Count >= 1) {
                if (parameters.Count > 1) {
                    Server = parameters [0];
                }
                foreach (string maskString in parameters [parameters.Count - 1].Split (',')) {
                    Masks.Add (new User (maskString));
                }
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnWhoIs (new IrcMessageEventArgs<WhoIsMessage> (this));
        }

    }
}
