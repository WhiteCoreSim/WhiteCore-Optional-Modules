using System;

namespace MetaBuilders.Irc
{
    public class UserEventArgs : EventArgs
    {
        public UserEventArgs (User u)
        {
            _user = u;
        }

        User _user;

        public User User {
            get {
                return _user;
            }
        }
    }
}
