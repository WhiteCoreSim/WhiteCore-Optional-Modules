using System;
using System.Collections.Specialized;
using System.Text;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

	/// <summary>
	/// A Watch system notification that a user is online.
	/// </summary>
	[Serializable]
	public class WatchedUserIsOnlineMessage : WatchedUserOnlineMessage
	{

		/// <summary>
		/// Creates a new instance of the <see cref="WatchedUserIsOnlineMessage"/>.
		/// </summary>
		public WatchedUserIsOnlineMessage()
			: base()
		{
			this.InternalNumeric = 604;
		}

		/// <exclude />
		protected override string ChangeMessage
		{
			get
			{
				return "is online";
			}
		}

	}
}
