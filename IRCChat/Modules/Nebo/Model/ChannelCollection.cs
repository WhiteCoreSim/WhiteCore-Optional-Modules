using System;
using MetaBuilders.Irc.Messages;

namespace MetaBuilders.Irc
{

    /// <summary>
    /// A collection that stores <see cref='MetaBuilders.Irc.Channel'/> objects.
    /// </summary>
    [Serializable()]
    public class ChannelCollection : System.Collections.ObjectModel.ObservableCollection<Channel>
    {

        /// <summary>
        /// Finds the <see href="Channel" /> in the collection with the given name.
        /// </summary>
        /// <returns>The so-named channel, or null.</returns>
        public Channel FindChannel(string channelName)
        {
            foreach (Channel channel in this) {
                if (MessageUtil.IsIgnoreCaseMatch(channel.Name, channelName)) {
                    return channel;
                }
            }
            return null;
        }

        /// <summary>
        /// Either finds or creates the channel by the given name
        /// </summary>
        public Channel EnsureChannel(string name, Client client)
        {
            Channel c = FindChannel(name);
            if (c == null || c.Client != client) {
                c = new Channel(client, name);
                Add(c);
            }
            return c;
        }

    }
}
