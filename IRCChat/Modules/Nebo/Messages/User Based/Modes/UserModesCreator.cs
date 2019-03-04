using System.Diagnostics;

namespace MetaBuilders.Irc.Messages.Modes
{

    /// <summary>
    /// UserModesCreator parses, builds, and writes the modes used by the <see cref="UserModeMessage"/> class.
    /// </summary>
    public class UserModesCreator
    {

        /// <summary>
        /// Creates a new instance of the <see cref="UserModesCreator"/> class.
        /// </summary>
        public UserModesCreator () { }

        #region Parsing

        /// <summary>
        /// Loads the given mode data into this <see cref="UserModesCreator"/>
        /// </summary>
        public void Parse (UserModeMessage msg)
        {
            if (msg == null) {
                return;
            }
            Parse (msg.ModeChanges);
        }

        /// <summary>
        /// Loads the given mode data into this <see cref="UserModesCreator"/>
        /// </summary>
        public void Parse (string modeChanges)
        {
            modes.Clear ();
            if (string.IsNullOrEmpty (modeChanges)) {
                return;
            }
            ModeAction currentAction = ModeAction.Add;
            foreach (char c in modeChanges) {
                if (ModeAction.IsDefined (c.ToString ())) {
                    currentAction = ModeAction.Parse (c.ToString ());
                } else {
                    // PONDER This probably won't correctly parse incorrect mode messages, should I?
                    switch (c) {
                    case 'a':
                        modes.Add (new AwayMode (currentAction));
                        break;
                    case 'g':
                        modes.Add (new CallerIdMode (currentAction));
                        break;
                    case 'i':
                        modes.Add (new InvisibleMode (currentAction));
                        break;
                    case 'o':
                        modes.Add (new NetworkOperatorMode (currentAction));
                        break;
                    case 'O':
                        modes.Add (new ServerOperatorMode (currentAction));
                        break;
                    case 'k':
                        modes.Add (new ReceiveServerKillsMode (currentAction));
                        break;
                    case 's':
                        modes.Add (new ReceiveServerKillsMode (currentAction));
                        break;
                    case 'w':
                        modes.Add (new ReceiveWallopsMode (currentAction));
                        break;
                    case 'r':
                        modes.Add (new RestrictedMode (currentAction));
                        break;
                    default:
                        modes.Add (new UnknownUserMode (currentAction, c.ToString ()));
                        Trace.WriteLine ("Unknown UserMode '" + c.ToString () + "'");
                        break;
                    }
                }
            }
            CollapseModes ();
        }
        #endregion

        /// <summary>
        /// Removes redundant or overridden modes from the modes collection.
        /// </summary>
        void CollapseModes ()
        {
            //TODO Implement CollapseModes
        }


        /// <summary>
        /// Applies the current modes to the given <see cref="UserModeMessage"/>.
        /// </summary>
        /// <param name="msg">The message to be altered.</param>
        public virtual void ApplyTo (UserModeMessage msg)
        {
            if (msg == null) {
                return;
            }

            msg.ModeChanges = "";
            if (modes.Count > 0) {
                // The first one always adds its mode
                UserMode currentMode = modes [0];
                ModeAction currentAction = currentMode.Action;
                currentMode.ApplyTo (msg, true);
                // The rest compare to the current
                for (int i = 1; i < modes.Count; i++) {
                    currentMode = modes [i];
                    currentMode.ApplyTo (msg, currentAction != currentMode.Action);
                    currentAction = currentMode.Action;
                }
            }
        }


        /// <summary>
        /// Gets the collection of modes parsed or to be applied.
        /// </summary>
        public virtual UserModeCollection Modes {
            get {
                return modes;
            }
        }

        UserModeCollection modes = new UserModeCollection ();

    }
}
