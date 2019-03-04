using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{
    /// <summary>
    /// A <see cref="IrcMessage"/> which carries communication from a person to another person or channel.
    /// </summary>
    [Serializable]
    public abstract class TextMessage : CommandMessage, IChannelTargetedMessage, IQueryTargetedMessage
    {

        /// <summary>
        /// Gets the target of this <see cref="TextMessage"/>.
        /// </summary>
        public virtual StringCollection Targets {
            get {
                return _targets;
            }
        }
        StringCollection _targets = new StringCollection ();

        /// <summary>
        /// Gets or sets the actual text of this <see cref="TextMessage"/>.
        /// </summary>
        /// <remarks>
        /// This property holds the core purpose of irc itself... sending text communication to others.
        /// </remarks>
        public virtual string Text {
            get {
                return _text;
            }
            set {
                _text = value;
            }
        }
        string _text = "";

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            writer.AddList (Targets, ",", false);
            writer.AddParameter (Text, true);
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            Targets.Clear ();
            if (parameters.Count >= 2) {
                Targets.AddRange (parameters [0].Split (','));
                Text = parameters [1];
            } else {
                Text = string.Empty;
            }
        }


        #region IChannelTargetedMessage Members

        bool IChannelTargetedMessage.IsTargetedAtChannel (string channelName)
        {
            return IsTargetedAtChannel (channelName);
        }

        /// <summary>
        /// Determines if the the current message is targeted at the given channel.
        /// </summary>
        protected virtual bool IsTargetedAtChannel (string channelName)
        {
            return MessageUtil.ContainsIgnoreCaseMatch (Targets, channelName);
        }

        #endregion

        #region IQueryTargetedMessage Members

        bool IQueryTargetedMessage.IsQueryToUser (User user)
        {
            return IsQueryToUser (user);
        }

        /// <summary>
        /// Determines if the current message is targeted at a query to the given user.
        /// </summary>
        protected virtual bool IsQueryToUser (User user)
        {
            foreach (string target in Targets) {
                if (MessageUtil.IsIgnoreCaseMatch (user.Nick, target)) {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
