using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The ErrorMessage received when a user tries to kill, kick, or deop a bot which provides channel services.
    /// </summary>
    [Serializable]
    public class CannotRemoveServiceBotMessage : ErrorMessage
    {
        //:irc.dkom.at 484 artificer chanserv #NeboBot :Cannot kill, kick or deop channel service

        /// <summary>
        /// Creates a new instances of the <see cref="CannotRemoveServiceBotMessage"/> class.
        /// </summary>
        public CannotRemoveServiceBotMessage ()
        {
            InternalNumeric = 484;
        }

        /// <summary>
        /// Gets or sets the nick of the bot
        /// </summary>
        public string Nick {
            get {
                return nick;
            }
            set {
                nick = value;
            }
        }
        string nick;

        /// <summary>
        /// Gets or sets the channel on which the bot resides
        /// </summary>
        public string Channel {
            get {
                return channel;
            }
            set {
                channel = value;
            }
        }
        string channel;


        /// <exclude />
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddParameter (Nick);
            writer.AddParameter (Channel);
            writer.AddParameter ("Cannot kill, kick or deop channel service");
        }

        /// <exclude />
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            Nick = "";
            Channel = "";
            if (parameters.Count > 2) {
                Nick = parameters [1];
                Channel = parameters [2];
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnCannotRemoveServiceBot (new IrcMessageEventArgs<CannotRemoveServiceBotMessage> (this));
        }

    }
}
