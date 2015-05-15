using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace MetaBuilders.Irc
{

	/// <summary>
	/// Represents a User on an irc server.
	/// </summary>
	[Serializable]
	public sealed class User : INotifyPropertyChanged
	{

		#region CTor

		/// <summary>
		/// Initializes a new instance of the <see cref="User"/> class.
		/// </summary>
		public User()
		{
			this.modes.CollectionChanged += delegate( object sender, NotifyCollectionChangedEventArgs e ) {
				this.PropChanged( "Modes" );
			};
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="User"/> class with the given mask string
		/// </summary>
		/// <param name="mask">The mask string to parse.</param>
		public User( String mask ) : this()
		{
			Parse( mask );
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the nickname of the User
		/// </summary>
		public String Nick
		{
			get
			{
				return nick;
			}
			set
			{
				if ( nick != value )
				{
					nick = value;
					PropChanged( "Nick" );
				}
			}
		}
		private String nick = "";

		/// <summary>
		/// Gets or sets the supposed real name of the User
		/// </summary>
		public String RealName
		{
			get
			{
				return realName;
			}
			set
			{
				if ( realName != value )
				{
					realName = value;
					PropChanged( "RealName" );
				}
			}
		}
		private String realName = "";

		/// <summary>
		/// Gets or sets the Password the User will use on the server
		/// </summary>
		public String Password
		{
			get
			{
				return password;
			}
			set
			{
				if ( password != value )
				{
					password = value;
					PropChanged( "Password" );
				}
			}
		}
		private String password = "";

		/// <summary>
		/// Gets or sets the username of the User on her local server.
		/// </summary>
		public String UserName
		{
			get
			{
				return username;
			}
			set
			{
				if ( username != value )
				{
					username = value;
					PropChanged( "UserName" );
				}
			}
		}
		private String username = "";

		/// <summary>
		/// Gets or sets the hostname of the local machine of this User
		/// </summary>
		public String HostName
		{
			get
			{
				return hostname;
			}
			set
			{
				if ( hostname != value )
				{
					hostname = value;
					PropChanged( "HostName" );
				}
			}
		}
		private String hostname = "";

		/// <summary>
		/// Gets or sets the online status of this User
		/// </summary>
		public UserOnlineStatus OnlineStatus
		{
			get
			{
				return onlineStatus;
			}
			set
			{
				if ( onlineStatus != value )
				{
					onlineStatus = value;
					PropChanged( "OnlineStatus" );
				}
			}
		}
		private UserOnlineStatus onlineStatus;

		/// <summary>
		/// Gets or sets the away message of this User
		/// </summary>
		public String AwayMessage
		{
			get
			{
				return awayMessage;
			}
			set
			{
				if ( awayMessage != value )
				{
					awayMessage = value;
					PropChanged( "AwayMessage" );
				}
			}
		}
		private String awayMessage;

		/// <summary>
		/// Gets or sets the name of the server which the User is connected to.
		/// </summary>
		public String ServerName
		{
			get
			{
				return serverName;
			}
			set
			{
				if ( serverName != value )
				{
					serverName = value;
					PropChanged( "ServerName" );
				}
			}
		}
		private String serverName;

		/// <summary>
		/// Gets or sets if the User is an IRC Operator
		/// </summary>
		public Boolean IrcOperator
		{
			get
			{
				return ircOperator;
			}
			set
			{
				if ( ircOperator != value )
				{
					ircOperator = value;
					PropChanged( "IrcOperator" );
				}
			}
		}
		private Boolean ircOperator;

		/// <summary>
		/// Gets the modes which apply to the user.
		/// </summary>
		public MetaBuilders.Irc.Messages.Modes.UserModeCollection Modes
		{
			get
			{
				return modes;
			}
		}
		private MetaBuilders.Irc.Messages.Modes.UserModeCollection modes = new MetaBuilders.Irc.Messages.Modes.UserModeCollection();

		#endregion

		#region Methods

		/// <summary>
		/// Represents this User's information as an irc mask
		/// </summary>
		/// <returns></returns>
		public override String ToString()
		{
			StringBuilder result = new StringBuilder();
			result.Append( Nick );
			if ( !String.IsNullOrEmpty( this.UserName ) )
			{
				result.Append( "!" );
				result.Append( this.UserName );
			}
			if ( !String.IsNullOrEmpty( this.HostName ) )
			{
				result.Append( "@" );
				result.Append( this.HostName );
			}

			return result.ToString();
		}

		/// <summary>
		/// Represents this User's information with a guarenteed nick!user@host format.
		/// </summary>
		public String ToNickUserHostString()
		{
			String finalNick = ( String.IsNullOrEmpty( this.Nick ) ) ? "*" : this.Nick;
			String user = ( String.IsNullOrEmpty( this.UserName ) ) ? "*" : this.UserName;
			String host = ( String.IsNullOrEmpty( this.HostName ) ) ? "*" : this.HostName;

			return finalNick + "!" + user + "@" + host;
		}

		/// <summary>
		/// Determines wether the current user mask matches the given user mask.
		/// </summary>
		/// <param name="wildcardMask">The wild-card filled mask to compare with the current.</param>
		/// <returns>True if this mask is described by the given wildcard Mask. False if not.</returns>
		public Boolean IsMatch( User wildcardMask )
		{
			if ( wildcardMask == null )
			{
				return false;
			}

			//Fist we'll return quickly if they are exact matches
			if ( this.Nick == wildcardMask.Nick && this.UserName == wildcardMask.UserName && this.HostName == wildcardMask.HostName )
			{
				return true;
			}

			return ( true
				&& Regex.IsMatch( this.Nick, makeRegexPattern( wildcardMask.Nick ), RegexOptions.IgnoreCase )
				&& Regex.IsMatch( this.UserName, makeRegexPattern( wildcardMask.UserName ), RegexOptions.IgnoreCase )
				&& Regex.IsMatch( this.HostName, makeRegexPattern( wildcardMask.HostName ), RegexOptions.IgnoreCase )
				);
		}

		/// <summary>
		/// Decides if the given user address matches the given address mask.
		/// </summary>
		/// <param name="actualMask">The user address mask to compare match.</param>
		/// <param name="wildcardMask">The address mask containing wildcards to match with.</param>
		/// <returns>True if <parmref>actualMask</parmref> is contained within ( or described with ) the <paramref>wildcardMask</paramref>. False if not.</returns>
		public static Boolean IsMatch( String actualMask, String wildcardMask )
		{
			return new User( actualMask ).IsMatch( new User( wildcardMask ) );
		}

		/// <summary>
		/// Parses the given string as a mask to populate this user object.
		/// </summary>
		/// <param name="rawMask">The mask to parse.</param>
		public void Parse( String rawMask )
		{
			this.Reset();

			String mask = rawMask;
			Int32 indexOfBang = mask.IndexOf( "!", StringComparison.Ordinal );
			Int32 indexOfAt = mask.LastIndexOf( "@", StringComparison.Ordinal );

			if ( indexOfAt > 1 )
			{
				this.HostName = mask.Substring( indexOfAt + 1 );
				mask = mask.Substring( 0, indexOfAt );
			}

			if ( indexOfBang != -1 )
			{
				this.UserName = mask.Substring( indexOfBang + 1 );
				mask = mask.Substring( 0, indexOfBang );
			}

			if ( !string.IsNullOrEmpty( mask ) )
			{
				String newNick = mask;
				String firstLetter = newNick.Substring( 0, 1 );
				if ( ChannelStatus.Exists( firstLetter ) )
				{
					newNick = newNick.Substring( 1 );
				}
				this.Nick = newNick;
			}
		}

		/// <summary>
		/// Resets the User properties to the default values
		/// </summary>
		public void Reset()
		{
			this.Nick = "";
			this.UserName = "";
			this.HostName = "";
			this.OnlineStatus = UserOnlineStatus.Online;
			this.AwayMessage = "";
			this.IrcOperator = false;
			this.Modes.Clear();
			this.Password = "";
			this.RealName = "";
			this.ServerName = "";
			this.UserName = "";

			this.dirtyProperties.Clear();
		}

		/// <summary>
		/// Merges the properties of the given User onto this User.
		/// </summary>
		public void MergeWith( User user )
		{
			if ( user == null )
			{
				return;
			}
			if ( user.IsDirty( "OnlineStatus" ) && !this.IsDirty( "OnlineStatus" ) )
			{
				this.OnlineStatus = user.OnlineStatus;
			}
			if ( user.IsDirty( "AwayMessage" ) && !this.IsDirty( "AwayMessage" ) )
			{
				this.AwayMessage = user.AwayMessage;
			}
			if ( user.IsDirty( "HostName" ) && !this.IsDirty( "HostName" ) )
			{
				this.HostName = user.HostName;
			}
			if ( user.IsDirty( "Nick" ) && !this.IsDirty( "Nick" ) )
			{
				this.Nick = user.Nick;
			}
			if ( user.IsDirty( "Password" ) && !this.IsDirty( "Password" ) )
			{
				this.Password = user.Password;
			}
			if ( user.IsDirty( "RealName" ) && !this.IsDirty( "RealName" ) )
			{
				this.RealName = user.RealName;
			}
			if ( user.IsDirty( "UserName" ) && !this.IsDirty( "UserName" ) )
			{
				this.UserName = user.UserName;
			}
			if ( user.IsDirty( "ServerName" ) && !this.IsDirty( "ServerName" ) )
			{
				this.ServerName = user.ServerName;
			}
			if ( user.IsDirty( "IrcOperator" ) && !this.IsDirty( "IrcOperator" ) )
			{
				this.IrcOperator = user.IrcOperator;
			}
		}

		/// <summary>
		/// Copies the properties of the given User onto this User.
		/// </summary>
		public void CopyFrom( User user )
		{
			if ( user.IsDirty( "OnlineStatus" ) )
			{
				this.OnlineStatus = user.OnlineStatus;
			}
			if ( user.IsDirty( "AwayMessage" ) )
			{
				this.AwayMessage = user.AwayMessage;
			}
			if ( user.IsDirty( "HostName" ) )
			{
				this.HostName = user.HostName;
			}
			if ( user.IsDirty( "Nick" ) )
			{
				this.Nick = user.Nick;
			}
			if ( user.IsDirty( "Password" ) )
			{
				this.Password = user.Password;
			}
			if ( user.IsDirty( "RealName" ) )
			{
				this.RealName = user.RealName;
			}
			if ( user.IsDirty( "UserName" ) )
			{
				this.UserName = user.UserName;
			}
			if ( user.IsDirty( "ServerName" ) )
			{
				this.ServerName = user.ServerName;
			}
			if ( user.IsDirty( "IrcOperator" ) )
			{
				this.IrcOperator = user.IrcOperator;
			}
		}

		private static String makeRegexPattern( String wildcardString )
		{
			return Regex.Escape( wildcardString ).Replace( @"\*", @".*" ).Replace( @"\?", @"." );
		}

		#endregion

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Raised when a property on the instance has changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged( PropertyChangedEventArgs e )
		{
			if ( this.PropertyChanged != null )
			{
				this.PropertyChanged( this, e );
			}
		}

		#endregion

		private void PropChanged( String propertyName )
		{
			if ( !dirtyProperties.Contains( propertyName ) )
			{
				dirtyProperties.Add( propertyName );
			}
			this.OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
		}

		private Boolean IsDirty( String propertyName )
		{
			return dirtyProperties.Contains( propertyName );
		}

		private List<String> dirtyProperties = new List<string>();

	}

}