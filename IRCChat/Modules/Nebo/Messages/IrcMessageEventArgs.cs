using System;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// The information for a handler of any standard IrcMessage event.
    /// </summary>
    [Serializable]
    public class IrcMessageEventArgs<T> : EventArgs where T : IrcMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IrcMessageEventArgs&lt;T&gt;"/> class with the given <see cref="IrcMessage"/>.
        /// </summary>
        public IrcMessageEventArgs (T msg)
        {
            Message = msg;
        }

        /// <summary>
        /// Gets or sets the Message for the event.
        /// </summary>
        public T Message {
            get;
            set;
        }


    }
}
