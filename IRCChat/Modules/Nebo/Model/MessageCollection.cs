using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MetaBuilders.Irc.Messages;

namespace MetaBuilders.Irc
{

	/// <summary>
	///     <para>
	///       A collection that stores <see cref='MetaBuilders.Irc.Messages.IrcMessage'/> objects.
	///    </para>
	/// </summary>
	[Serializable()]
	public class MessageCollection : ObservableCollection<IrcMessage>
	{

	}
}
