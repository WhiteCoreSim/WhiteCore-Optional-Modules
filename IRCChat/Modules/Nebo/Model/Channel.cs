using System;
using System.ComponentModel;
using MetaBuilders.Irc.Network;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc
{

	/// <summary>
	/// Represents a single irc channel, with it's users.
	/// </summary>
	public class Channel : INotifyPropertyChanged
	{

		#region ctor

		/// <summary>
		/// Creates a new instance of the <see cref="Channel"/> class on the given client.
		/// </summary>
		public Channel( Client client )
		{
			this.client = client;
			this.users.CollectionChanged += new NotifyCollectionChangedEventHandler( users_CollectionChanged );
			this.Modes.CollectionChanged += new NotifyCollectionChangedEventHandler( Modes_CollectionChanged );
			this.journal.CollectionChanged += new NotifyCollectionChangedEventHandler( journal_CollectionChanged );
		}

		/// <summary>
		/// Creates a new instance of the <see cref="Channel"/> class on the given client with the given name.
		/// </summary>
		public Channel( Client client, String name )
			: this( client )
		{
			this.Name = name;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the collection of general properties assigned to this channel
		/// </summary>
		public virtual NameValueCollection Properties
		{
			get
			{
				return properties;
			}
		}

		/// <summary>
		/// Gets the client which the channel is on.
		/// </summary>
		public virtual Client Client
		{
			get
			{
				return client;
			}
		}


		/// <summary>
		/// Gets or sets whether the channel is currently open
		/// </summary>
		public Boolean Open
		{
			get
			{
				return _open && ( this.Client != null && this.Client.Connection.Status == ConnectionStatus.Connected );
			}
			internal set
			{
				_open = value;
				OnPropertyChanged( new PropertyChangedEventArgs( "Open" ) );
			}
		}
		private Boolean _open = false;

		/// <summary>
		/// Gets or sets the name of the channel.
		/// </summary>
		public virtual String Name
		{
			get
			{
				return this.Properties[ "NAME" ] ?? "";
			}
			set
			{
				String currentValue = this.Name;
				if ( currentValue != value )
				{
					this.Properties[ "NAME" ] = value;
					OnPropertyChanged( new PropertyChangedEventArgs( "Name" ) );
				}
			}
		}

		/// <summary>
		/// Gets or sets the topic of the channel
		/// </summary>
		public virtual String Topic
		{
			get
			{
				return this.Properties[ "TOPIC" ] ?? "";
			}
			set
			{
				String originalValue = this.Topic;
				if ( originalValue != value )
				{
					this.Properties[ "TOPIC" ] = value;
					this.OnPropertyChanged( new PropertyChangedEventArgs( "Topic" ) );
				}
			}
		}

		/// <summary>
		/// Gets or sets the user which set the current topic
		/// </summary>
		public virtual User TopicSetter
		{
			get
			{
				return topicSetter;
			}
			set
			{
				if ( topicSetter != value )
				{
					topicSetter = value;
					this.OnPropertyChanged( new PropertyChangedEventArgs( "TopicSetter" ) );
				}
			}
		}

		/// <summary>
		/// Gets or sets the time which topic was set
		/// </summary>
		public virtual DateTime TopicSetTime
		{
			get
			{
				return topicSetTime;
			}
			set
			{
				if ( topicSetTime != value )
				{
					topicSetTime = value;
					this.OnPropertyChanged( new PropertyChangedEventArgs( "TopicSetTime" ) );
				}
			}
		}

		/// <summary>
		/// Gets the users in the channel.
		/// </summary>
		public virtual UserCollection Users
		{
			get
			{
				return users;
			}
		}

		/// <summary>
		/// Gets the modes in the channel.
		/// </summary>
		public virtual MetaBuilders.Irc.Messages.Modes.ChannelModeCollection Modes
		{
			get
			{
				return modes;
			}
		}

		/// <summary>
		/// Gets the journal of messages on the channel
		/// </summary>
		public virtual Journal Journal
		{
			get
			{
				return journal;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the status for the given <see cref="T:User"/> in the channel.
		/// </summary>
		public virtual ChannelStatus GetStatusForUser( User channelUser )
		{
			VerifyUserInChannel( channelUser );
			if ( userModes.ContainsKey( channelUser ) )
			{
				return userModes[ channelUser ];
			}
			return ChannelStatus.None;
		}

		/// <summary>
		/// Applies the given <see cref="T:ChannelStatus"/> to the given <see cref="T:User"/> in the channel.
		/// </summary>
		public virtual void SetStatusForUser( User channelUser, ChannelStatus status )
		{
			if ( status == ChannelStatus.None && userModes.ContainsKey( channelUser ) )
			{
				userModes.Remove( channelUser );
			}
			else
			{
				VerifyUserInChannel( channelUser );
				userModes[ channelUser ] = status;
			}
		}

		private void VerifyUserInChannel( User channelUser )
		{
			if ( channelUser == null )
			{
				throw new ArgumentNullException( "channelUser" );
			}
			if ( !Users.Contains( channelUser ) )
			{
				throw new ArgumentException( String.Format( CultureInfo.InvariantCulture, NeboResources.UserIsNotInChannel, channelUser.Nick, this.Name ), "channelUser" );
			}
		}

		#endregion

		#region Private

		void users_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			UserCollection users = this.Users;

			if ( e.Action == NotifyCollectionChangedAction.Remove )
			{
				this.userModes.RemoveAll( delegate( KeyValuePair<User, ChannelStatus> keyValue )
				{
					return !users.Contains( keyValue.Key );
				}
				);
			}
			this.OnPropertyChanged( new PropertyChangedEventArgs( "Users" ) );
		}

		void Modes_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			this.OnPropertyChanged( new PropertyChangedEventArgs( "Modes" ) );
		}

		void journal_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			this.OnPropertyChanged( new PropertyChangedEventArgs( "Journal" ) );
		}


		private Client client;
		private User topicSetter;
		private DateTime topicSetTime;
		private UserCollection users = new UserCollection();
		private MetaBuilders.Irc.Messages.Modes.ChannelModeCollection modes = new MetaBuilders.Irc.Messages.Modes.ChannelModeCollection();
		private Journal journal = new Journal();
		private UserStatusMap userModes = new UserStatusMap();
		private NameValueCollection properties = new NameValueCollection();
		
		#endregion

		#region INotifyPropertyChanged Members

		/// <summary>
		/// The event raised when a property on the object changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises the PropertyChanged event.
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnPropertyChanged( PropertyChangedEventArgs e )
		{
			if ( PropertyChanged != null )
			{
				PropertyChanged( this, e );
			}
		}

		#endregion

		#region UserStatusMap

		private class UserStatusMap : Dictionary<User, ChannelStatus>
		{

			/// <summary>
			/// Removes all of the items from the dictionary which the given predictate matches.
			/// </summary>
			/// <returns>The number of items removed from the dictionary.</returns>
			public Int32 RemoveAll( Predicate<System.Collections.Generic.KeyValuePair<User, ChannelStatus>> match )
			{
				if ( match == null )
				{
					throw new ArgumentNullException( "match" );
				}
				Int32 countOfItemsRemoved = 0;

				User[] users = new User[ this.Keys.Count ];
				this.Keys.CopyTo( users, 0 );
				foreach ( User u in users )
				{
					if ( this.ContainsKey( u ) && match( new KeyValuePair<User, ChannelStatus>( u, this[ u ] ) ) )
					{
						this.Remove( u );
						countOfItemsRemoved++;
					}
				}

				return countOfItemsRemoved;

			}
		}

		#endregion

	}
}
