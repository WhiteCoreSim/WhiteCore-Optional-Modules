using System.Globalization;

namespace MetaBuilders.Irc.Messages.Modes
{

    /// <summary>
    /// A user limit may be set on channels by using this mode.
    /// </summary>
    public class LimitMode : FlagMode
    {

        /// <summary>
        /// Creates a new instance of the <see cref="LimitMode"/> class.
        /// </summary>
        public LimitMode()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LimitMode"/> class with the given <see cref="ModeAction"/>.
        /// </summary>
        public LimitMode(ModeAction action)
        {
            Action = action;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LimitMode"/> class 
        /// with the given <see cref="ModeAction"/> and user limit.
        /// </summary>
        public LimitMode(ModeAction action, int userLimit)
        {
            Action = action;
            user_Limit = userLimit;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LimitMode"/> class 
        /// with the given user limit.
        /// </summary>
        public LimitMode(int userLimit)
        {
            user_Limit = userLimit;
        }


        /// <summary>
        /// Gets the irc string representation of the mode being changed or applied.
        /// </summary>
        protected override string Symbol {
            get {
                return "l";
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of users allowed to join the channel.
        /// </summary>
        public virtual int UserLimit {
            get {
                return user_Limit;
            }
            set {
                user_Limit = value;
            }
        }
        int user_Limit = -1;

        /// <summary>
        /// Applies this mode to the ModeArguments property of the given <see cref="ChannelModeMessage" />.
        /// </summary>
        /// <param name="msg">The message which will be modified to include this mode.</param>
        protected override void AddParameter(ChannelModeMessage msg)
        {
            if (UserLimit != -1) {
                msg.ModeArguments.Add(user_Limit.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}
