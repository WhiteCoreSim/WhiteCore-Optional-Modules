using System;
using System.Collections.Generic;
using System.Text;

namespace MetaBuilders.Irc.Messages
{
	/// <summary>
	/// A delegate which provides custom format rendering for the items in a list.
	/// </summary>
	public delegate String CustomListItemRendering<T>( T item );
}
