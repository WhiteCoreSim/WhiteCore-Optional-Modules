using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The reply to a <see cref="WhoWasMessage"/> query.
    /// </summary>
    [Serializable]
    public class WhoWasUserReplyMessage : NumericMessage
    {

        /// <summary>
        /// Creates a new instance of the <see cref="WhoWasUserReplyMessage"/> class.
        /// </summary>
        public WhoWasUserReplyMessage ()
        {
            InternalNumeric = 314;
        }

        /// <summary>
        /// Gets or sets the User being examined.
        /// </summary>
        public virtual User User {
            get {
                return user;
            }
            set {
                user = value;
            }
        }

        User user = new User ();


        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (User.Nick);
            writer.AddParameter (User.UserName);
            writer.AddParameter (User.HostName);
            writer.AddParameter ("*");
            writer.AddParameter (User.RealName);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            user = new User ();
            if (parameters.Count > 5) {
                user.Nick = parameters [1];
                user.UserName = parameters [2];
                user.HostName = parameters [3];
                user.RealName = parameters [5];
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnWhoWasUserReply (new IrcMessageEventArgs<WhoWasUserReplyMessage> (this));
        }

    }
}
