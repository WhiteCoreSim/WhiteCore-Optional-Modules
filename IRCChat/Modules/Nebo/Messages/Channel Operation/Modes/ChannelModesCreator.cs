using System;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc.Messages.Modes
{

    /// <summary>
    /// ChannelModesCreator parses, builds, and writes the modes used by the <see cref="ChannelModeMessage"/> class.
    /// </summary>
    public class ChannelModesCreator
    {
        readonly ChannelModeCollection modes = new ChannelModeCollection();

        /// <summary>
        /// Creates a new instance of the <see cref="ChannelModesCreator"/> class.
        /// </summary>
        public ChannelModesCreator()
        {
        }

        /// <summary>
        /// A <see cref="Irc.ServerSupport"/> instance is required in order to parse non-standard modes.
        /// </summary>
        public ServerSupport ServerSupport {
            get {
                return serverSupports;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }
                serverSupports = value;
            }
        }
        ServerSupport serverSupports = new ServerSupport();

        #region Parsing

        /// <summary>
        /// Loads the given mode data into this <see cref="ChannelModesCreator"/>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void Parse(string modeChanges, StringCollection modeArguments)
        {
            if (string.IsNullOrEmpty(modeChanges)) {
                return;
            }
            if (modeArguments == null) {
                modeArguments = new StringCollection();
            }

            modes.Clear();
            ModeAction currentAction = ModeAction.Add;
            int argIndex = 0;
            foreach (char c in modeChanges) {
                if (ModeAction.IsDefined(c.ToString())) {
                    currentAction = ModeAction.Parse(c.ToString());
                } else {
                    // PONDER This probably won't correctly parse incorrect mode messages, should I?
                    switch (c) {
                        case 'a':
                            modes.Add(new AnonymousMode(currentAction));
                            break;
                        case 'b':
                            BanMode banMode = new BanMode(currentAction, new User(modeArguments[argIndex]));
                            argIndex++;
                            modes.Add(banMode);
                            break;
                        case 'e':
                            BanExceptionMode banExceptionMode = new BanExceptionMode(currentAction, new User(modeArguments[argIndex]));
                            argIndex++;
                            modes.Add(banExceptionMode);
                            break;
                        case 'h':
                            HalfOpMode halfOpMode = new HalfOpMode(currentAction, modeArguments[argIndex]);
                            argIndex++;
                            modes.Add(halfOpMode);
                            break;
                        case 'I':
                            InvitationExceptionMode invitationExceptionMode = new InvitationExceptionMode(currentAction, new User(modeArguments[argIndex]));
                            argIndex++;
                            modes.Add(invitationExceptionMode);
                            break;
                        case 'i':
                            InviteOnlyMode inviteOnlyMode = new InviteOnlyMode(currentAction);
                            modes.Add(inviteOnlyMode);
                            break;
                        case 'k':
                            KeyMode keyMode = new KeyMode(currentAction);
                            if (currentAction == ModeAction.Add) {
                                keyMode.Password = modeArguments[argIndex];
                                argIndex++;
                            }
                            modes.Add(keyMode);
                            break;
                        case 'l':
                            LimitMode limitMode = new LimitMode(currentAction);
                            if (currentAction == ModeAction.Add) {
                                limitMode.UserLimit = Convert.ToInt32(modeArguments[argIndex], CultureInfo.InvariantCulture);
                                argIndex++;
                            }
                            modes.Add(limitMode);
                            break;
                        case 'm':
                            modes.Add(new ModeratedMode(currentAction));
                            break;
                        case 'n':
                            modes.Add(new NoOutsideMessagesMode(currentAction));
                            break;
                        case 'O':
                            CreatorMode creatorMode = new CreatorMode(currentAction, modeArguments[argIndex]);
                            argIndex++;
                            modes.Add(creatorMode);
                            break;
                        case 'o':
                            OperatorMode operatorMode = new OperatorMode(currentAction, modeArguments[argIndex]);
                            argIndex++;
                            modes.Add(operatorMode);
                            break;
                        case 'p':
                            modes.Add(new PrivateMode(currentAction));
                            break;
                        case 'q':
                            modes.Add(new QuietMode(currentAction));
                            break;
                        case 's':
                            modes.Add(new SecretMode(currentAction));
                            break;
                        case 'r':
                            modes.Add(new ServerReopMode(currentAction));
                            break;
                        case 'R':
                            modes.Add(new RegisteredNicksOnlyMode(currentAction));
                            break;
                        case 't':
                            modes.Add(new TopicGuardedMode(currentAction));
                            break;
                        case 'v':
                            VoiceMode voiceMode = new VoiceMode(currentAction, modeArguments[argIndex]);
                            argIndex++;
                            modes.Add(voiceMode);
                            break;
                        default:
                            string unknownMode = c.ToString();
                            if (serverSupports.ModesWithParameters.Contains(unknownMode) ||
                               (serverSupports.ModesWithParametersWhenSet.Contains(unknownMode) &&
                                currentAction == ModeAction.Add)) {
                                // I want to yank a parameter	
                                modes.Add(new UnknownChannelMode(currentAction, unknownMode, modeArguments[argIndex]));
                                argIndex++;
                            } else {
                                // I don't
                                modes.Add(new UnknownChannelMode(currentAction, unknownMode));
                            }
                            break;
                    }
                }
            }
            CollapseModes();
        }

        /// <summary>
        /// Loads the given mode data into this <see cref="ChannelModesCreator"/>
        /// </summary>
        public void Parse(ChannelModeMessage msg)
        {
            if (msg == null) {
                return;
            }
            Parse(msg.ModeChanges, msg.ModeArguments);
        }

        /// <summary>
        /// Loads the given mode data into this <see cref="ChannelModesCreator"/>
        /// </summary>
        public void Parse(string modeChanges)
        {
            if (string.IsNullOrEmpty(modeChanges)) {
                return;
            }
            Parse(modeChanges, new StringCollection());
        }

        #endregion

        /// <summary>
        /// Removes redundant or overridden modes from the modes collection.
        /// </summary>
        void CollapseModes()
        {
            // TODO: Implement CollapseModes
        }


        /// <summary>
        /// Applies the current modes to the given <see cref="ChannelModeMessage"/>.
        /// </summary>
        /// <param name="msg">The message to be altered.</param>
        public virtual void ApplyTo(ChannelModeMessage msg)
        {
            if (msg == null) {
                return;
            }
            msg.ModeChanges = "";
            msg.ModeArguments.Clear();
            if (modes.Count > 0) {
                // The first one always adds its mode
                ChannelMode currentMode = modes[0];
                ModeAction currentAction = currentMode.Action;
                currentMode.ApplyTo(msg, true);
                // The rest compare to the current
                for (int i = 1; i < modes.Count; i++) {
                    currentMode = modes[i];
                    currentMode.ApplyTo(msg, currentAction != currentMode.Action);
                    currentAction = currentMode.Action;
                }
            }
        }


        /// <summary>
        /// Gets the collection of modes parsed or to be applied.
        /// </summary>
        public virtual ChannelModeCollection Modes {
            get {
                return modes;
            }
        }

    }
}
