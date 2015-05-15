using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MetaBuilders.Irc.Network;

namespace MetaBuilders.Irc
{
    
	/// <summary>
	///     <para>
	///       A collection that stores <see cref='MetaBuilders.Irc.Client'/> objects.
	///    </para>
	/// </summary>
	/// <seealso cref='MetaBuilders.Irc.ClientCollection'/>
	[Serializable()]
	public class ClientCollection : ObservableCollection<Client> {

		//public Client FindClient( String serverName )
		//{
		//    foreach ( Client c in this )
		//    {
		//        if ( c.ServerName == name )
		//        {
		//            return c;
		//        }
		//    }
		//    return null;
		//}

	}
}
