
namespace MetaBuilders.Irc.Messages.Modes
{

    /// <summary>
    /// Servers use this mode to give the user creating a safe channel the status of "channel creator".
    /// </summary>
    public class CreatorMode : MemberStatusMode
    {

        /// <summary>
        /// Creates a new instance of the <see cref="CreatorMode"/> class.
        /// </summary>
        public CreatorMode()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CreatorMode"/> class with the given <see cref="ModeAction"/>.
        /// </summary>
        public CreatorMode(ModeAction action)
        {
            Action = action;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CreatorMode"/> class 
        /// with the given <see cref="ModeAction"/> and member's nick.
        /// </summary>
        public CreatorMode(ModeAction action, string nick)
        {
            Action = action;
            Nick = nick;
        }

        /// <summary>
        /// Gets the irc string representation of the mode being changed or applied.
        /// </summary>
        protected override string Symbol {
            get {
                return "O";
            }
        }

    }
}
