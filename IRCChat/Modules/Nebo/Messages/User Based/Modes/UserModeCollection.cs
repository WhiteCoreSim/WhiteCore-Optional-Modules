using System;
using System.Collections.ObjectModel;

namespace MetaBuilders.Irc.Messages.Modes
{
    
    
	/// <summary>
	///     <para>
	///       A collection that stores <see cref='MetaBuilders.Irc.Messages.Modes.UserMode'/> objects.
	///    </para>
	/// </summary>
	/// <seealso cref='MetaBuilders.Irc.Messages.Modes.UserModeCollection'/>
	[Serializable()]
	public class UserModeCollection : ObservableCollection<UserMode>
	{
        
	}
}
