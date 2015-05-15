using System;
using System.Collections.Specialized;

namespace MetaBuilders.Irc.Contacts
{
	abstract class ContactsTracker
	{
		public ContactsTracker( ContactList contacts )
		{
			this.contacts = contacts;
			this.contacts.Users.CollectionChanged += Users_CollectionChanged;
		}

		void Users_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( e.Action == NotifyCollectionChangedAction.Add )
			{
				foreach ( User newUser in e.NewItems )
				{
					AddNick( newUser.Nick );
				}
			}
			if ( e.Action == NotifyCollectionChangedAction.Remove )
			{
				foreach ( User oldUser in e.OldItems )
				{
					RemoveNick( oldUser.Nick );
				}
			}
		}

		ContactList contacts;

		protected ContactList Contacts
		{
			get
			{
				return contacts;
			}
		}

		public virtual void Initialize()
		{
			StringCollection nicks = new StringCollection();
			foreach ( User u in this.Contacts.Users )
			{
				nicks.Add( u.Nick );
			}
			AddNicks( nicks );
		}

		protected abstract void AddNicks( StringCollection nicks );

		protected abstract void AddNick( String nick );

		protected abstract void RemoveNick( String nick );

	}
}
