using System;
using System.Collections.ObjectModel;

namespace MetaBuilders.Irc.Messages.Modes
{

    /// <summary>
    ///     <para>
    ///       A collection that stores <see cref='ChannelMode'/> objects.
    ///    </para>
    /// </summary>
    /// <seealso cref='ChannelModeCollection'/>
    [Serializable ()]
    public class ChannelModeCollection : ObservableCollection<ChannelMode>
    {

        /// <summary>
        /// Clears the current collection and adds the given modes
        /// </summary>
        /// <param name="newModes"></param>
        public void ResetWith (ChannelModeCollection newModes)
        {
            Clear ();
            foreach (ChannelMode mode in newModes) {
                Add (mode);
            }
        }

    }
}
