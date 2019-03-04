using System;
using System.Runtime.Serialization;
using System.Globalization;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Possible actions for each mode change in a <see cref="ChannelModeMessage"/> or <see cref="UserModeMessage"/> message.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Usage", "CA2229:ImplementSerializationConstructors", Justification = "Using IObjectReference instead"), Serializable]
    public sealed class ModeAction : MarshalByRefObject, IComparable, ISerializable
    {

        #region Static Instances

        /// <summary>
        /// Gets the <see cref="ModeAction"/> representing the addition of a mode to a user or channel.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly ModeAction Add = new ModeAction ("+");
        /// <summary>
        /// Gets the <see cref="ModeAction"/> representing the removal of a mode from a user or channel.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly ModeAction Remove = new ModeAction ("-");

        #endregion

        #region Static Properties

        /// <summary>
        /// Gets an array of <see cref="ModeAction"/> instances representing all the possible actions.
        /// </summary>
        public static IList<ModeAction> Values {
            get {
                if (values == null) {
                    values = new ReadOnlyCollection<ModeAction> (new List<ModeAction> { Add, Remove });
                }
                return values;
            }
        }

        /// <summary>
        /// Determines if the given string value is representative of any defined ModeActions.
        /// </summary>
        public static bool IsDefined (string value)
        {
            foreach (ModeAction modeAction in Values) {
                if (modeAction._ircName == value) {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns the correct <see cref="ModeAction"/> for the given string value.
        /// </summary>
        /// <param name="value">The String to parse.</param>
        public static ModeAction Parse (string value)
        {
            return Parse (value, false);
        }

        /// <summary>
        /// Returns the correct <see cref="ModeAction"/> for the given string value.
        /// </summary>
        /// <param name="value">The String to parse.</param>
        /// <param name="ignoreCase">Decides whether the parsing is case-specific.</param>
        public static ModeAction Parse (string value, bool ignoreCase)
        {
            if (value == null) {
                throw new ArgumentNullException ("value");
            }
            foreach (ModeAction modeAction in Values) {
                StringComparison compareMode = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                if (string.Compare (modeAction._ircName, value, compareMode) == 0) {
                    return modeAction;
                }
            }
            throw new ArgumentException (string.Format (CultureInfo.InvariantCulture, NeboResources.ModeActionDoesNotExist, value), "value");
        }

        #endregion

        #region CTor

        /// <summary>
        /// Creates a new instance of the <see cref="ModeAction"/> class.
        /// </summary>
        /// <remarks>
        /// This is private so that only the Enum-like static references can ever be used.
        /// </remarks>
        ModeAction (string ircName)
        {
            _ircName = ircName;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a string representing the <see cref="ModeAction"/> in irc format.
        /// </summary>
        public override string ToString ()
        {
            return _ircName;
        }

        #endregion

        #region Equality/Operators

        /// <summary>
        /// Implements Equals based on a string value.
        /// </summary>
        public override bool Equals (object obj)
        {
            ModeAction other = obj as ModeAction;
            if (other == null) {
                return base.Equals (obj);
            }
            return _ircName.Equals (other._ircName);
        }

        /// <summary>
        /// Implements Equals based on a string value.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode ()
        {
            return _ircName.GetHashCode ();
        }

        /// <summary>
        /// Implements the operator based on a string value.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
        public static bool operator == (ModeAction x, ModeAction y)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals (x, y)) {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)x == null) || ((object)y == null)) {
                return false;
            }

            return x._ircName == y._ircName;
        }

        /// <summary>
        /// Implements the operator based on a string value.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
        public static bool operator != (ModeAction x, ModeAction y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implements the operator based on a string value.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
        public static bool operator < (ModeAction x, ModeAction y)
        {
            return string.CompareOrdinal (x._ircName, y._ircName) < 0;
        }

        /// <summary>
        /// Implements the operator based on a string value.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
        public static bool operator > (ModeAction x, ModeAction y)
        {
            return string.CompareOrdinal (x._ircName, y._ircName) > 0;
        }

        #endregion

        #region IComparable

        /// <summary>
        /// Implements <see cref="IComparable.CompareTo"/>
        /// </summary>
        public int CompareTo (object obj)
        {
            if (obj == null) {
                return 1;
            }
            ModeAction ma = obj as ModeAction;
            if (ma == null) {
                throw new ArgumentException (string.Format (CultureInfo.InvariantCulture, NeboResources.ObjectMustBeOfType, "ModeAction"), "obj");
            }
            return string.Compare (_ircName, ma._ircName, StringComparison.Ordinal);
        }

        #endregion

        #region ISerializable

        /// <summary>
        /// Implements ISerializable.GetObjectData
        /// </summary>
        [SecurityPermission (SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        [SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData (SerializationInfo info, StreamingContext context)
        {
            if (info != null) {
                info.SetType (typeof (ModeActionProxy));
                info.AddValue ("IrcName", _ircName);
            }
        }

        [Serializable]
        sealed class ModeActionProxy : IObjectReference, ISerializable
        {
            ModeActionProxy (SerializationInfo info, StreamingContext context)
            {
                ircName = info.GetString ("IrcName");
            }
            string ircName = "";
            public object GetRealObject (StreamingContext context)
            {
                return Parse (ircName);
            }
            [SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
            public void GetObjectData (SerializationInfo info, StreamingContext context)
            {
                throw new NotImplementedException ();
            }
        }

        #endregion

        string _ircName;
        static ReadOnlyCollection<ModeAction> values;
    }
}
