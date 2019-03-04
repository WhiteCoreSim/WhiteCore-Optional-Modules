using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// A request for some information about the server.
    /// </summary>
    [Serializable]
    public class StatsMessage : ServerQueryBase
    {

        /// <summary>
        /// Gets the Irc command associated with this message.
        /// </summary>
        protected override string Command {
            get {
                return "STATS";
            }
        }

        /// <summary>
        /// Gets or sets the code the what information is requested.
        /// </summary>
        public virtual string Query {
            get {
                return query;
            }
            set {
                query = value;
            }
        }
        string query;

        /// <summary>
        /// Overrides <see cref="IrcMessage.AddParametersToFormat"/>.
        /// </summary>
        protected override void AddParametersToFormat (IrcMessageWriter writer)
        {
            base.AddParametersToFormat (writer);
            if (Query != null && Query.Length != 0) {
                writer.AddParameter (Query);
                writer.AddParameter (Target);
            }
        }

        /// <summary>
        /// Parses the parameters portion of the message.
        /// </summary>
        protected override void ParseParameters (StringCollection parameters)
        {
            base.ParseParameters (parameters);
            if (parameters.Count >= 1) {
                Query = parameters [0];
            } else {
                Query = "";
            }

        }

        /// <summary>
        /// Gets the index of the parameter which holds the server which should respond to the query.
        /// </summary>
        protected override int TargetParsingPosition {
            get {
                return 1;
            }
        }

        /// <summary>
        /// Notifies the given <see cref="MessageConduit"/> by raising the appropriate event for the current <see cref="IrcMessage"/> subclass.
        /// </summary>
        public override void Notify (MessageConduit conduit)
        {
            conduit.OnStats (new IrcMessageEventArgs<StatsMessage> (this));
        }

    }
}
