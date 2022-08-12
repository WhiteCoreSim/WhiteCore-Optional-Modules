using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics;

namespace MetaBuilders.Irc
{

    /// <summary>
    /// Contains information about what irc extensions and such the server supports.
    /// </summary>
    /// <remarks>
    /// This information is sent from a <see cref="Client"/> when it receives a <see cref="MetaBuilders.Irc.Messages.SupportMessage"/>.
    /// This most likely makes it unneccesary to catch this message's received event.
    /// </remarks>
    [Serializable]
    public class ServerSupport
    {

        /// <summary>
        /// The extended parameters which the server can support on a List message.
        /// </summary>
        [
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible"),
        Flags
        ]
        public enum ExtendedListParameters
        {
            /// <summary>
            /// No extended parameters are supported
            /// </summary>
            None = 0,
            /// <summary>
            /// Searching by matching only the given mask is supported
            /// </summary>
            Mask = 1,
            /// <summary>
            /// Searching by not matching the given mask is supported
            /// </summary>
            NotMask = 2,
            /// <summary>
            /// Searching by number of users in the channel is supported
            /// </summary>
            UserCount = 4,
            /// <summary>
            /// Searching by the channel creation time is supported
            /// </summary>
            CreationTime = 8,
            /// <summary>
            /// Searching by the most recent change in a channel's topic is supported
            /// </summary>
            Topic = 16
        }

        #region Default Support

        /// <summary>
        /// 
        /// </summary>
        public static ServerSupport DefaultSupport {
            get {
                if (_defaultSupport == null) {
                    _defaultSupport = new ServerSupport();
                    //TODO Create A Good Default ServerSupport
                }
                return _defaultSupport;
            }
            set {
                _defaultSupport = value;
            }
        }
        static ServerSupport _defaultSupport;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets if the server supports the Deaf user mode
        /// </summary>
        public bool DeafMode {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the standard used by the server.
        /// </summary>
        public string Standard {
            get {
                return (_standard);
            }
            set {
                _standard = value;
            }
        }
        string _standard = "i-d";

        /// <summary>
        /// Gets or sets a list of channel modes a person can get and the respective prefix a channel or nickname will get in case the person has it.
        /// </summary>
        /// <remarks>
        /// The order of the modes goes from most powerful to least powerful. 
        /// Those prefixes are shown in the output of the WHOIS, WHO and NAMES command.
        /// </remarks>
        public string ChannelStatuses {
            get {
                return (_channelStatuses);
            }
            set {
                _channelStatuses = value;
            }
        }
        string _channelStatuses = "(ov)@+";

        /// <summary>
        /// Gets or sets the channel status prefixes supported for matched-status-only messages
        /// </summary>
        public string StatusMessages {
            get {
                return (statusMessages);
            }
            set {
                statusMessages = value;
            }
        }
        string statusMessages = string.Empty;

        /// <summary>
        /// Gets the supported channel prefixes.
        /// </summary>
        public StringCollection ChannelTypes {
            get {
                return (_channelTypes);
            }
        }
        StringCollection _channelTypes = new StringCollection();

        /// <summary>
        /// Gets the modes that require parameters
        /// </summary>
        public StringCollection ModesWithParameters {
            get {
                return (_modesWithParameters);
            }
        }
        StringCollection _modesWithParameters = new StringCollection();

        /// <summary>
        /// Gets the modes that require parameters only when set.
        /// </summary>
        public StringCollection ModesWithParametersWhenSet {
            get {
                return (_modesWithParametersWhenSet);
            }
        }
        StringCollection _modesWithParametersWhenSet = new StringCollection();

        /// <summary>
        /// Gets the modes that do not require parameters.
        /// </summary>
        public StringCollection ModesWithoutParameters {
            get {
                return (_modesWithoutParameters);
            }
        }
        StringCollection _modesWithoutParameters = new StringCollection();

        /// <summary>
        /// Maximum number of channel modes with parameter allowed per <see cref="MetaBuilders.Irc.Messages.ChannelModeMessage"/> command.
        /// </summary>
        public int MaxModes {
            get {
                return (_maxModes);
            }
            set {
                _maxModes = value;
            }
        }
        int _maxModes = 3;

        /// <summary>
        /// Gets or sets the maximum number of channels a client can join.
        /// </summary>
        /// <remarks>
        /// This property is considered obsolete, as most servers use the ChannelLimits property instead.
        /// </remarks>
        public int MaxChannels {
            get {
                return (_maxChannels);
            }
            set {
                _maxChannels = value;
            }
        }
        int _maxChannels = -1;

        /// <summary>
        /// Gets the collection of channel limits, grouped by channel type (ex, #, +).
        /// </summary>
        /// <remarks>This property has replaced MaxChannels becuase of the added flexibility.</remarks>
        public Dictionary<string, int> ChannelLimits {
            get {
                return _channelLimits;
            }
        }
        Dictionary<string, int> _channelLimits = new Dictionary<string, int>();

        /// <summary>
        /// Gets or sets the maximum nickname length.
        /// </summary>
        public int MaxNickLength {
            get {
                return (_maxNickLength);
            }
            set {
                _maxNickLength = value;
            }
        }
        int _maxNickLength = 9;

        /// <summary>
        /// Gets or sets the maximum channel topic length.
        /// </summary>
        public int MaxTopicLength {
            get {
                return (_maxTopicLength);
            }
            set {
                _maxTopicLength = value;
            }
        }
        int _maxTopicLength = -1;

        /// <summary>
        /// Gets or sets the maximum length of the reason in a <see cref="Messages.KickMessage"/>.
        /// </summary>
        public int MaxKickCommentLength {
            get {
                return (_maxKickCommentLength);
            }
            set {
                _maxKickCommentLength = value;
            }
        }
        int _maxKickCommentLength = -1;

        /// <summary>
        /// Gets or sets the maximum length of a channel name.
        /// </summary>
        public int MaxChannelNameLength {
            get {
                return (_maxChannelNameLength);
            }
            set {
                _maxChannelNameLength = value;
            }
        }
        int _maxChannelNameLength = 200;

        /// <summary>
        /// Gets or sets the maximum number of bans that a channel can have.
        /// </summary>
        public int MaxBans {
            get {
                return (_maxBans);
            }
            set {
                _maxBans = value;
            }
        }
        int _maxBans = -1;

        /// <summary>
        /// Gets or sets the Maximum number of invitation exceptions a channel can have.
        /// </summary>
        public int MaxInvitationExceptions {
            get {
                return (maxInvitationsExceptions);
            }
            set {
                maxInvitationsExceptions = value;
            }
        }
        int maxInvitationsExceptions = -1;

        /// <summary>
        /// Gets or sets the maximum number of ban exceptions that a channel can have.
        /// </summary>
        public int MaxBanExceptions {
            get {
                return (maxBanExceptions);
            }
            set {
                maxBanExceptions = value;
            }
        }
        int maxBanExceptions = -1;

        /// <summary>
        /// Gets or sets the name of the network which the server is on.
        /// </summary>
        public string NetworkName {
            get {
                return (_networkName);
            }
            set {
                _networkName = value;
            }
        }
        string _networkName = string.Empty;

        /// <summary>
        /// Gets or sets if the server supports channel ban exceptions. 
        /// </summary>
        public bool BanExceptions {
            get {
                return (_banExceptions);
            }
            set {
                _banExceptions = value;
            }
        }
        bool _banExceptions = false;

        /// <summary>
        /// Gets or sets if the server supports channel invitation exceptions.
        /// </summary>
        public bool InvitationExceptions {
            get {
                return (_invitationExceptions);
            }
            set {
                _invitationExceptions = value;
            }
        }
        bool _invitationExceptions = false;

        /// <summary>
        /// Gets or sets the maximum number of silence ( serverside ignore ) listings a client can store.
        /// </summary>
        public int MaxSilences {
            get {
                return (_maxSilences);
            }
            set {
                _maxSilences = value;
            }
        }
        int _maxSilences = 0;

        /// <summary>
        /// Gets or sets if the server supports messages to channel operators.
        /// </summary>
        /// <remarks>
        /// To send a message to channel operators, use a <see cref="Messages.NoticeMessage"/>
        /// with a target in the format "@#channel".
        /// </remarks>
        public bool MessagesToOperators {
            get {
                return (_messagesToOperators);
            }
            set {
                _messagesToOperators = value;
            }
        }
        bool _messagesToOperators = false;

        /// <summary>
        /// Gets or sets the case mapping supported by the server.
        /// </summary>
        /// <remarks>"ascii", "rfc1459", and "strict-rfc1459" are the only known values.</remarks>
        public string CaseMapping {
            get {
                return (_caseMapping);
            }
            set {
                _caseMapping = value;
            }
        }
        string _caseMapping = "rfc1459";

        /// <summary>
        /// Gets or sets the text encoding used by the server.
        /// </summary>
        public String CharacterSet {
            get {
                return (_characterSet);
            }
            set {
                _characterSet = value;
            }
        }
        string _characterSet = string.Empty;

        /// <summary>
        /// Gets or sets if the server supports the standards declared in rfc 2812.
        /// </summary>
        public bool Rfc2812 {
            get {
                return (_rfc2812);
            }
            set {
                _rfc2812 = value;
            }
        }
        bool _rfc2812 = false;

        /// <summary>
        /// Gets or sets the length of channel ids.
        /// </summary>
        public int ChannelIdLength {
            get {
                return (_channelIdLength);
            }
            set {
                _channelIdLength = value;
            }
        }
        int _channelIdLength = -1;

        /// <summary>
        /// Gets or sets if the server has a message penalty.
        /// </summary>
        public bool Penalties {
            get {
                return (_penalties);
            }
            set {
                _penalties = value;
            }
        }
        bool _penalties = false;

        /// <summary>
        /// Gets or sets if the server will change your nick automatticly when it needs to.
        /// </summary>
        public bool ForcedNickChanges {
            get {
                return (_forcedNickChanges);
            }
            set {
                _forcedNickChanges = value;
            }
        }
        bool _forcedNickChanges = false;

        /// <summary>
        /// Gets or sets if the server supports the USERIP command.
        /// </summary>
        public bool UserIP {
            get {
                return (_userIp);
            }
            set {
                _userIp = value;
            }
        }
        bool _userIp = false;

        /// <summary>
        /// Gets or sets if the server supports the CPRIVMSG command.
        /// </summary>
        public bool ChannelMessages {
            get {
                return (_channelMessages);
            }
            set {
                _channelMessages = value;
            }
        }
        bool _channelMessages = false;

        /// <summary>
        /// Gets or sets if the server supports the CNOTICE command.
        /// </summary>
        public bool ChannelNotices {
            get {
                return (_channelNotices);
            }
            set {
                _channelNotices = value;
            }
        }
        bool _channelNotices = false;

        /// <summary>
        /// Gets or sets the maximum number of targets allowed on targetted messages, grouped by message command
        /// </summary>
        public Dictionary<string, int> MaxMessageTargets {
            get {
                return _maxMessageTargets;
            }
        }
        Dictionary<string, int> _maxMessageTargets = new Dictionary<string, int>();

        /// <summary>
        /// Gets or sets if the server supports the <see cref="Messages.KnockMessage"/>.
        /// </summary>
        public bool Knock {
            get {
                return (_knock);
            }
            set {
                _knock = value;
            }
        }
        bool _knock = false;

        /// <summary>
        /// Gets or sets if the server supports virtual channels.
        /// </summary>
        public bool VirtualChannels {
            get {
                return (_virtualChannels);
            }
            set {
                _virtualChannels = value;
            }
        }
        bool _virtualChannels = false;

        /// <summary>
        /// Gets or sets if the <see cref="Messages.ListReplyMessage"/> is sent in multiple itterations.
        /// </summary>
        public bool SafeList {
            get {
                return (_safeList);
            }
            set {
                _safeList = value;
            }
        }
        bool _safeList = false;

        /// <summary>
        /// Gets or sets the extended parameters the server supports for a <see cref="Messages.ListMessage"/>.
        /// </summary>
        public ExtendedListParameters ExtendedList {
            get {
                return _eList;
            }
            set {
                _eList = value;
            }
        }
        ExtendedListParameters _eList;

        /// <summary>
        /// Gets or sets the maximum number of watches a user is allowed to set.
        /// </summary>
        public int MaxWatches {
            get {
                return (_maxWatches);
            }
            set {
                _maxWatches = value;
            }
        }
        int _maxWatches = -1;

        /// <summary>
        /// Gets or sets if the <see cref="Messages.WhoMessage"/> uses the WHOX protocol
        /// </summary>
        public bool WhoX {
            get {
                return (_whoX);
            }
            set {
                _whoX = value;
            }
        }
        bool _whoX = false;

        /// <summary>
        /// Gets or sets if the server suports callerid-style ignore.
        /// </summary>
        public bool CallerId {
            get {
                return (_callerId);
            }
            set {
                _callerId = value;
            }
        }
        bool _callerId = false;

        /// <summary>
        /// Gets or sets if the server supports ETrace.
        /// </summary>
        public bool ETrace {
            get {
                return (eTrace);
            }
            set {
                eTrace = value;
            }
        }
        bool eTrace = false;

        /// <summary>
        /// Gets or sets the maximum number of user monitors a user is allowed to set.
        /// </summary>
        /// <remarks>
        /// A value of 0 indicates that the server doesn't support the monitor system.
        /// A value of -1 indicates that the server has no limit to the users on the monitor system list.
        /// A value greater than 0 indicates the maximum number of users which can be added to the monitor system list.
        /// </remarks>
        public int MaxMonitors {
            get {
                return _maxMonitors;
            }
            set {
                _maxMonitors = value;
            }
        }
        int _maxMonitors = 0;

        /// <summary>
        /// Gets the collection of safe channel prefix lengths, grouped by the channel type they apply to.
        /// </summary>
        public Dictionary<string, int> SafeChannelPrefixLengths {
            get {
                return _safeChannelPrefixLengths;
            }
        }
        Dictionary<string, int> _safeChannelPrefixLengths = new Dictionary<string, int>();

        /// <summary>
        /// Gets or sets the maximum length of away messages.
        /// </summary>
        public int MaxAwayMessageLength {
            get {
                return _maxAwayMessageLength;
            }
            set {
                _maxAwayMessageLength = value;
            }
        }
        int _maxAwayMessageLength = -1;

        #endregion

        /// <summary>
        /// Gets the list of unknown items supported by the server.
        /// </summary>
        public virtual NameValueCollection UnknownItems {
            get {
                return unknownItems;
            }
        }
        NameValueCollection unknownItems = new NameValueCollection();

        /// <summary>
        /// Loads support information from the given <see cref="Messages.SupportMessage"/>.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void LoadInfo(Messages.SupportMessage msg)
        {
            NameValueCollection items = msg.SupportedItems;
            foreach (string key in items.Keys) {
                string value = items[key] ?? "";
                switch (key) {
                    case "DEAF":
                        DeafMode = true;
                        break;
                    case "AWAYLEN":
                        SetIfNumeric(GetType().GetProperty("MaxAwayMessageLength"), value);
                        break;
                    case "IDCHAN":
                        foreach (InfoPair pair in CreateInfoPairs(value)) {
                            int prefixLength = -1;
                            if (int.TryParse(pair.Value, out prefixLength)) {
                                SafeChannelPrefixLengths.Add(pair.Key, prefixLength);
                            }
                        }
                        break;
                    case "STD":
                        Standard = value;
                        break;
                    case "PREFIX":
                        ChannelStatuses = value;
                        break;
                    case "STATUSMSG":
                    case "WALLVOICES":
                        StatusMessages = value;
                        break;
                    case "CHANTYPES":
                        AddChars(ChannelTypes, value);
                        break;
                    case "CHANMODES":
                        string[] modeGroups = value.Split(',');
                        if (modeGroups.Length >= 4) {
                            AddChars(ModesWithParameters, modeGroups[0]);
                            AddChars(ModesWithParameters, modeGroups[1]);
                            AddChars(ModesWithParametersWhenSet, modeGroups[2]);
                            AddChars(ModesWithoutParameters, modeGroups[3]);
                        } else {
                            Trace.WriteLine("Unknown CHANMODES " + value);
                        }
                        break;
                    case "MODES":
                        SetIfNumeric(GetType().GetProperty("MaxModes"), value);
                        break;
                    case "MAXCHANNELS":
                        SetIfNumeric(GetType().GetProperty("MaxChannels"), value);
                        break;
                    case "CHANLIMIT":
                        foreach (InfoPair chanLimitInfo in CreateInfoPairs(value)) {
                            int limit = -1;
                            if (int.TryParse(chanLimitInfo.Value, out limit)) {
                                foreach (char c in chanLimitInfo.Key) {
                                    ChannelLimits.Add(c.ToString(), limit);
                                }
                            }
                        }
                        break;
                    case "NICKLEN":
                    case "MAXNICKLEN":
                        SetIfNumeric(GetType().GetProperty("MaxNickLength"), value);
                        break;
                    case "TOPICLEN":
                        SetIfNumeric(GetType().GetProperty("MaxTopicLength"), value);
                        break;
                    case "KICKLEN":
                        SetIfNumeric(GetType().GetProperty("MaxKickCommentLength"), value);
                        break;
                    case "CHANNELLEN":
                    case "MAXCHANNELLEN":
                        SetIfNumeric(GetType().GetProperty("MaxChannelNameLength"), value);
                        break;
                    case "MAXBANS":
                        SetIfNumeric(GetType().GetProperty("MaxBans"), value);
                        break;
                    case "NETWORK":
                        NetworkName = value;
                        break;
                    case "EXCEPTS":
                        BanExceptions = true;
                        break;
                    case "INVEX":
                        InvitationExceptions = true;
                        break;
                    case "SILENCE":
                        SetIfNumeric(GetType().GetProperty("MaxSilences"), value);
                        break;
                    case "WALLCHOPS":
                        MessagesToOperators = true;
                        break;
                    case "CASEMAPPING":
                        CaseMapping = value;
                        break;
                    case "CHARSET":
                        CharacterSet = value;
                        break;
                    case "RFC2812":
                        Rfc2812 = true;
                        break;
                    case "CHIDLEN":
                        SetIfNumeric(GetType().GetProperty("ChannelIdLength"), value);
                        break;
                    case "PENALTY":
                        Penalties = true;
                        break;
                    case "FNC":
                        ForcedNickChanges = true;
                        break;
                    case "USERIP":
                        UserIP = true;
                        break;
                    case "CPRIVMSG":
                        ChannelMessages = true;
                        break;
                    case "CNOTICE":
                        ChannelNotices = true;
                        break;
                    case "MAXTARGETS":
                        int maxTargets = -1;
                        if (value != null && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out maxTargets)) {
                            MaxMessageTargets.Add("", maxTargets);
                        }
                        break;
                    case "TARGMAX":
                        foreach (InfoPair targmaxInfo in CreateInfoPairs(value)) {
                            int targmax = -1;
                            if (int.TryParse(targmaxInfo.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out targmax)) {
                                MaxMessageTargets.Add(targmaxInfo.Key, targmax);
                            } else {
                                MaxMessageTargets.Add(targmaxInfo.Key, -1);
                            }
                        }
                        break;
                    case "KNOCK":
                        Knock = true;
                        break;
                    case "VCHANS":
                        VirtualChannels = true;
                        break;
                    case "SAFELIST":
                        SafeList = true;
                        break;
                    case "ELIST":
                        Dictionary<char, ExtendedListParameters> elistMap = new Dictionary<char, ExtendedListParameters>();
                        elistMap.Add('M', ExtendedListParameters.Mask);
                        elistMap.Add('N', ExtendedListParameters.NotMask);
                        elistMap.Add('U', ExtendedListParameters.UserCount);
                        elistMap.Add('C', ExtendedListParameters.CreationTime);
                        elistMap.Add('T', ExtendedListParameters.Topic);

                        ExtendedList = ExtendedListParameters.None;
                        foreach (char c in value.ToUpperInvariant()) {
                            ExtendedList = (ExtendedList | elistMap[c]);
                        }

                        break;
                    case "WATCH":
                        SetIfNumeric(GetType().GetProperty("MaxWatches"), value);
                        break;
                    case "MONITOR":
                        MaxMonitors = -1;
                        SetIfNumeric(GetType().GetProperty("MaxMonitors"), value);
                        break;
                    case "WHOX":
                        WhoX = true;
                        break;
                    case "CALLERID":
                    case "ACCEPT":
                        CallerId = true;
                        break;
                    case "ETRACE":
                        ETrace = true;
                        break;
                    case "MAXLIST":
                        foreach (InfoPair maxListInfoPair in CreateInfoPairs(value)) {
                            int maxLength = -1;
                            if (int.TryParse(maxListInfoPair.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out maxLength)) {
                                if (maxListInfoPair.Key.IndexOf("b", StringComparison.Ordinal) != -1) {
                                    MaxBans = maxLength;
                                }
                                if (maxListInfoPair.Key.IndexOf("e", StringComparison.Ordinal) != -1) {
                                    MaxBanExceptions = maxLength;
                                }
                                if (maxListInfoPair.Key.IndexOf("I", StringComparison.Ordinal) != -1) {
                                    MaxInvitationExceptions = maxLength;
                                }
                            }
                        }
                        break;
                    default:
                        UnknownItems[key] = value;
                        Trace.WriteLine("Unknown ServerSupport key/value " + key + " " + value);
                        break;
                }
            }
        }

        static void AddChars(StringCollection target, string source)
        {
            foreach (char c in source) {
                target.Add(c.ToString());
            }
        }

        void SetIfNumeric(System.Reflection.PropertyInfo property, string value)
        {
            int intValue;
            if (value != null && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out intValue)) {
                property.SetValue(this, intValue, null);
            } else {
                Trace.WriteLine("Expected numeric for ServerSupport target " + property.Name + " but it was '" + (value ?? "") + "'");
            }
        }

        List<InfoPair> CreateInfoPairs(string value)
        {
            List<InfoPair> list = new List<InfoPair>();
            foreach (string chanLimitPair in value.Split(',')) {
                if (chanLimitPair.Contains(":")) {
                    string[] chanLimitInfo = chanLimitPair.Split(':');
                    if (chanLimitInfo.Length == 2 && chanLimitInfo[0].Length > 0) {
                        InfoPair pair = new InfoPair(chanLimitInfo[0], chanLimitInfo[1]);
                        list.Add(pair);
                    }
                }
            }
            return list;
        }

        struct InfoPair
        {
            public InfoPair(string key, string value)
            {
                Key = key;
                Value = value;
            }

            public string Key;
            public string Value;
        }
    }
}
