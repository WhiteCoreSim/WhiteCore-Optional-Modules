using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// A Message that edits the list of users on your watch list.
    /// </summary>
    [Serializable]
    public class WatchListEditorMessage : WatchMessage
    {

        #region Properties

        /// <summary>
        /// Gets the collection of nicks being added to the watch list.
        /// </summary>
        public StringCollection AddedNicks {
            get {
                if (addedNicks == null) {
                    addedNicks = new StringCollection();
                }
                return addedNicks;
            }
        }
        StringCollection addedNicks;

        /// <summary>
        /// Gets the collection of nicks being removed from the watch list.
        /// </summary>
        public StringCollection RemovedNicks {
            get {
                if (removedNicks == null) {
                    removedNicks = new StringCollection();
                }
                return removedNicks;
            }
        }
        StringCollection removedNicks;

        #endregion

        #region Parsing

        /// <summary>
        /// Determines if the message can be parsed by this type.
        /// </summary>
        public override bool CanParse(string unparsedMessage)
        {
            if (!base.CanParse(unparsedMessage)) {
                return false;
            }
            string firstParam = MessageUtil.GetParameter(unparsedMessage, 0);
            return (firstParam.StartsWith("+", StringComparison.Ordinal) || firstParam.StartsWith("-", StringComparison.Ordinal));
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters(StringCollection parameters)
        {
            base.ParseParameters(parameters);
            foreach (string param in parameters) {
                if (param.StartsWith("+", StringComparison.Ordinal)) {
                    AddedNicks.Add(param.Substring(1));
                }
                if (param.StartsWith("-", StringComparison.Ordinal)) {
                    RemovedNicks.Add(param.Substring(1));
                }
            }
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>
        /// </summary>
        protected override void AddParametersToFormat(IrcMessageWriter writer)
        {
            base.AddParametersToFormat(writer);
            if (addedNicks != null) {
                foreach (string aNick in addedNicks) {
                    writer.AddParameter("+" + aNick, true);
                }
            }
            if (removedNicks != null) {
                foreach (string rNick in removedNicks) {
                    writer.AddParameter("-" + rNick, true);
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify(MessageConduit conduit)
        {
            conduit.OnWatchListEditor(new IrcMessageEventArgs<WatchListEditorMessage>(this));
        }

        #endregion

    }
}
