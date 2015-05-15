using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Collections.Generic;
using System.Globalization;


namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// Provides simple utilities for parsing and generating messages.
	/// </summary>
	/// <remarks>
	/// Client code will probably not need to use most of these routines.
	/// </remarks>
	public static class MessageUtil
	{

		/// <summary>
		/// Takes the given channel name, and returns a name that is valid according to the given server support.
		/// </summary>
		/// <param name="channelName">The channel name to examine</param>
		/// <param name="support">The feature support of an irc server</param>
		/// <returns>A valid channel name on the given server.</returns>
		public static String EnsureValidChannelName( String channelName, ServerSupport support )
		{
			if ( channelName == null || channelName.Length == 0 )
			{
				return "#irc";
			}
			if ( support == null )
			{
				support = ServerSupport.DefaultSupport;
			}

			String result = channelName.Replace( " ", "_" );
			result = result.Replace( ",", "_" );
			result = result.Replace( ":", "_" );
			String firstChar = result.Substring( 0, 1 );
			if ( firstChar != "#" && firstChar != "&" && firstChar != "+" && firstChar != "!" )
			{
				result = "#" + result;
			}

			if ( result.Length > support.MaxChannelNameLength )
			{
				return result.Substring( 0, support.MaxChannelNameLength );
			}
			else
			{
				return result;
			}
		}

		/// <summary>
		/// Determines if the given channel name has a valid namespace prefix.
		/// </summary>
		/// <remarks>
		/// This is according to the IRC spec, and is not representative of what a particular server may support.
		/// </remarks>
		public static Boolean HasValidChannelPrefix( String channelName )
		{
			if ( String.IsNullOrEmpty( channelName ) )
			{
				return false;
			}

			if ( channelName.StartsWith( "#", StringComparison.Ordinal ) )
			{
				return true;
			}
			if ( channelName.StartsWith( "&", StringComparison.Ordinal ) )
			{
				return true;
			}
			if ( channelName.StartsWith( "+", StringComparison.Ordinal ) )
			{
				return true;
			}
			if ( channelName.StartsWith( "!", StringComparison.Ordinal ) )
			{
				return true;
			}

			return false;
		}


		#region Parameters To String

		/// <summary>
		/// Creates a list of irc parameters from the given collection of strings.
		/// </summary>
		public static String ParametersToString( Boolean useColon, StringCollection parameters )
		{
			if ( parameters == null )
			{
				return "";
			}
			String[] foo = new String[ parameters.Count ];
			parameters.CopyTo( foo, 0 );
			return ParametersToString( useColon, foo );
		}

		/// <summary>
		/// Creates a list of irc parameters from the given array of strings.
		/// </summary>
		public static String ParametersToString( Boolean useColon, params String[] parameters )
		{
			StringBuilder result = new StringBuilder();
			if ( parameters.Length > 1 )
			{
				for ( Int32 i = 0; i < parameters.Length - 1; i++ )
				{
					result.Append( parameters[ i ] );
					result.Append( " " );
				}
			}
			if ( parameters.Length != 0 )
			{
				if ( useColon )
				{
					result.Append( ":" );
				}
				result.Append( parameters[ parameters.Length - 1 ] );
			}
			return result.ToString();
		}

		/// <summary>
		/// Creates a list of irc parameters from the given collection of strings.
		/// </summary>
		public static String ParametersToString( StringCollection parameters )
		{
			return MessageUtil.ParametersToString( true, parameters );
		}

		/// <summary>
		/// Creates a list of irc parameters from the given array of strings.
		/// </summary>
		public static String ParametersToString( params String[] parameters )
		{
			return MessageUtil.ParametersToString( true, parameters );
		}

		#endregion

		#region Create Lists

		/// <summary>
		/// Creates a space-delimited list from the given StringCollection, using delimiter.
		/// </summary>
		public static String CreateList( StringCollection items, String delimiter )
		{
			if ( items == null )
			{
				return String.Empty;
			}
			String[] itemsArray = new String[ items.Count ];
			items.CopyTo( itemsArray, 0 );
			return CreateList( itemsArray, delimiter );
		}

		/// <summary>
		/// Creates a char-delimited list from the given String[], using delimiter.
		/// </summary>
		public static String CreateList( String[] items, String delimiter )
		{
			return String.Join( delimiter, items );
		}

		/// <summary>
		/// Creates a char-delimited list from the given IList of objects, using delimiter.
		/// </summary>
		public static String CreateList( IList items, String delimiter )
		{
			if ( items == null || items.Count == 0 )
			{
				return "";
			}

			StringBuilder result = new StringBuilder();
			result.Append( items[ 0 ].ToString() );
			for ( Int32 i = 1; i < items.Count; i++ )
			{
				result.Append( delimiter );
				result.Append( items[ i ].ToString() );
			}
			return result.ToString();
		}

		/// <summary>
		/// Creates a char-delimited list from the given IEnumerable of objects, using delimiter.
		/// </summary>
		public static String CreateList<T>( IEnumerable<T> items, String delimiter, CustomListItemRendering<T> render )
		{
			if ( items == null )
			{
				return "";
			}
			StringBuilder result = new StringBuilder();
			foreach ( T item in items )
			{
				String itemValue = render( item );
				result.Append( itemValue );
				result.Append( delimiter );
			}
			if ( result.Length > delimiter.Length )
			{
				result.Remove( result.Length - delimiter.Length, delimiter.Length );
			}
			return result.ToString();
			
		}

		#endregion


		/// <summary>
		/// Extracts the Prefix from a string message.
		/// </summary>
		public static String GetPrefix( String rawMessage )
		{
			if ( !String.IsNullOrEmpty( rawMessage ) && rawMessage.StartsWith( ":", StringComparison.Ordinal ) )
			{
				return rawMessage.Substring( 1, rawMessage.IndexOf( " ", StringComparison.Ordinal ) ).Trim();
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Extracts the Command from a string message.
		/// </summary>
		public static String GetCommand( String rawMessage )
		{
			if ( String.IsNullOrEmpty( rawMessage ) )
			{
				return "";
			}

			// remove prefix
			if ( rawMessage.StartsWith( ":", StringComparison.Ordinal ) )
			{
				rawMessage = rawMessage.Substring( rawMessage.IndexOf( " ", 1, StringComparison.Ordinal ) + 1 );
				if ( String.IsNullOrEmpty( rawMessage ) )
				{
					return "";
				}
			}

			// the first token is the command
			Int32 indexOfFirstSpace = rawMessage.IndexOf( " ", StringComparison.Ordinal );
			if ( indexOfFirstSpace == -1 )
			{
				return rawMessage;
			}
			return rawMessage.Substring( 0, indexOfFirstSpace );
		}

		/// <summary>
		/// Gets the parameters of the raw message.
		/// </summary>
		/// <param name="rawMessage">the message string which has the parameters.</param>
		public static StringCollection GetParameters( String rawMessage )
		{
			if ( String.IsNullOrEmpty( rawMessage ) )
			{
				return new StringCollection();
			}

			Int32 startIndex;

			if ( rawMessage.StartsWith( ":", StringComparison.Ordinal ) )
			{
				// then the params start after space 2
				startIndex = MessageUtil.NthIndexOf( rawMessage, " ", 0, 2 ) + 1;
			}
			else
			{
				// then the params start after space 1
				startIndex = MessageUtil.NthIndexOf( rawMessage, " ", 0, 1 ) + 1;
			}

			if ( startIndex == 0 )
			{
				return new StringCollection();
			}
			return Tokenize( rawMessage, startIndex );
		}

		/// <summary>
		/// Seperates the given space-delimted parameter string into a collection.
		/// </summary>
		public static StringCollection Tokenize( String rawMessage, Int32 startIndex )
		{
			if ( rawMessage == null )
			{
				throw new ArgumentNullException( "rawMessage" );
			}

			if ( rawMessage == cachedRawMessage )
			{
				return cachedParams;
			}

			StringCollection parameters = new StringCollection();
			StringBuilder param = new StringBuilder();
			for ( int i = startIndex; i < rawMessage.Length; i++ )
			{
				Char c = rawMessage[ i ];
				if ( c.ToString() == " " )
				{
					parameters.Add( param.ToString() );
					param = new StringBuilder();
				}
				else if ( i + 1 == rawMessage.Length )
				{
					param.Append( c );
					parameters.Add( param.ToString() );
					param = new StringBuilder();
				}
				else if ( c.ToString() == ":" && param.Length == 0 )
				{
					parameters.Add( rawMessage.Substring( i + 1 ) );
					break;
				}
				else
				{
					param.Append( c );
				}
			}

			cachedRawMessage = rawMessage;
			cachedParams = parameters;

			return parameters;
		}

		private static String cachedRawMessage = "";
		private static StringCollection cachedParams;


		/// <summary>
		/// Gets the last parameter in the parameters collection of the given unparsed message.
		/// </summary>
		public static String GetLastParameter( String rawMessage )
		{
			StringCollection p = MessageUtil.GetParameters( rawMessage );
			if ( p.Count > 0 )
			{
				return p[ p.Count - 1 ];
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the nth parameter in the parameters collection of the given unparsed message.
		/// </summary>
		public static String GetParameter( String rawMessage, Int32 index )
		{
			StringCollection p = MessageUtil.GetParameters( rawMessage );
			if ( p.Count > index )
			{
				return p[ index ];
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the substring in the input string existing between the given two strings.
		/// </summary>
		/// <param name="input">The string to search in.</param>
		/// <param name="before">The string before the one you want.</param>
		/// <param name="after">The string after the one you want.</param>
		public static String StringBetweenStrings( String input, String before, String after )
		{
			if ( input == null )
			{
				throw new ArgumentNullException( "input" );
			}
			if ( before == null )
			{
				throw new ArgumentNullException( "before" );
			}
			if ( after == null )
			{
				throw new ArgumentNullException( "after" );
			}

			Int32 startOfBetween = 0;
			if ( before.Length != 0 )
			{
				Int32 indexOfBefore = input.IndexOf( before, StringComparison.Ordinal );
				if ( indexOfBefore > -1 )
				{
					startOfBetween = indexOfBefore + before.Length;
				}
			}

			if ( after.Length == 0 )
			{
				return input.Substring( startOfBetween );
			}
			else
			{
				Int32 startOfEnd = input.LastIndexOf( after, StringComparison.Ordinal );
				if ( startOfEnd == -1 )
				{
					return input.Substring( startOfBetween );
				}
				Int32 lengthOfBetween = startOfEnd - startOfBetween;
				return input.Substring( startOfBetween, lengthOfBetween );
			}
		}

		/// <summary>
		/// Gets the index of the nth time that searchValue shows up in text.
		/// </summary>
		/// <param name="text">The string to search in.</param>
		/// <param name="searchValue">The string to search for.</param>
		/// <param name="startIndex">The place to start looking.</param>
		/// <param name="nthItem">The item to stop at.</param>
		public static Int32 NthIndexOf( String text, String searchValue, Int32 startIndex, Int32 nthItem )
		{
			if ( text == null )
			{
				throw new ArgumentNullException( "text" );
			}

			Int32 result = -1;
			Int32 currentStartIndex = startIndex;
			for ( Int32 i = 0; i < nthItem; i++ )
			{
				result = text.IndexOf( searchValue, currentStartIndex, StringComparison.Ordinal );
				if ( result == -1 )
					return result;
				else
					currentStartIndex = result + 1;
			}
			return result;
		}

		/// <summary>
		/// Turns a <see cref="DateTime"/> into the Int32 representation as is commonly used on Unix machines
		/// </summary>
		public static Int32 ConvertToUnixTime( DateTime dt )
		{
			DateTime unixEpochStartDate = new DateTime( 1970, 1, 1, 0, 0, 0, 0 );
			long ticksSinceEpochStart = dt.Ticks - unixEpochStartDate.Ticks;
			TimeSpan ts = new TimeSpan( ticksSinceEpochStart );
			return Convert.ToInt32( ts.TotalSeconds );
		}

		/// <summary>
		/// Turns the given Unix representation of a datetime into a <see cref="DateTime"/>.
		/// </summary>
		public static DateTime ConvertFromUnixTime( Int32 ut )
		{
			DateTime unixEpochStartDate = new DateTime( 1970, 1, 1, 0, 0, 0, 0 );
			return unixEpochStartDate.AddSeconds( ut );
		}

		/// <summary>
		/// Converts the given Unix representation of a datetime into a <see cref="Nullable&lt;DateTime&gt;"/>.
		/// </summary>
		/// <remarks>
		/// If the string can't be parsed, a null DateTime is returned.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string" )]
		public static DateTime? ConvertFromUnixTime( String unixTimeString )
		{
			if ( String.IsNullOrEmpty( unixTimeString ) )
			{
				return null;
			}

			Int32 unixTime;
			if ( Int32.TryParse( unixTimeString, System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture, out unixTime ) )
			{
				return ConvertFromUnixTime( unixTime );
			}
			return null;
		}

		/// <summary>
		/// Determines if the given strings match eachother using <see href="StringComparison.InvariantCultureIgnoreCase" /> matching.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "b" ), System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "a" ), System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Globalization", "CA1309:UseOrdinalStringComparison", MessageId = "System.String.Compare(System.String,System.String,System.StringComparison)" )]
		public static Boolean IsIgnoreCaseMatch( String a, String b )
		{
			return ( String.Compare( a, b, StringComparison.InvariantCultureIgnoreCase ) == 0 );
		}

		/// <summary>
		/// Determines if the given collection of strings contains a string which matches the given string using <see href="StringComparison.InvariantCultureIgnoreCase" /> matching.
		/// </summary>
		/// <param name="strings">The list to look in</param>
		/// <param name="match">The string to look for</param>
		public static Boolean ContainsIgnoreCaseMatch( StringCollection strings, String match )
		{
			foreach ( String item in strings )
			{
				if ( IsIgnoreCaseMatch( item, match ) )
				{
					return true;
				}
			}
			return false;
		}
	}
}
