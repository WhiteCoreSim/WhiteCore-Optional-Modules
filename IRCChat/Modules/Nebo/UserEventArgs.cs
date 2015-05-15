using System;
using System.Collections.Generic;
using System.Text;

namespace MetaBuilders.Irc
{
	public class UserEventArgs : EventArgs
	{
		public UserEventArgs( User u )
		{
			this._user = u;
		}

		private User _user;

		public User User
		{
			get
			{
				return _user;
			}
		}
	}
}
