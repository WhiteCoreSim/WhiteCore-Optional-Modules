using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;

namespace MetaBuilders.Irc.Messages
{

    /// <summary>
    /// Writes <see cref="IrcMessage"/> data to a <see cref="TextWriter"/> in irc protocol format.
    /// </summary>
    public class IrcMessageWriter : IDisposable
    {

        #region Constructors

        /// <summary>
        /// Creates a new instance of the IrcMessageWriter class without an <see cref="InnerWriter"/> to write to.
        /// </summary>
        public IrcMessageWriter ()
        {
            resetDefaults ();
        }

        /// <summary>
        /// Creates a new instance of the IrcMessageWriter class with the given <see cref="InnerWriter"/> to write to.
        /// </summary>
        public IrcMessageWriter (TextWriter writer)
        {
            _writer = writer;
            resetDefaults ();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="TextWriter"/> which will written to.
        /// </summary>
        public TextWriter InnerWriter {
            get {
                return _writer;
            }
            set {
                _writer = value;
            }
        }

        TextWriter _writer;

        /// <summary>
        /// Gets or sets the ID of the sender of the message.
        /// </summary>
        public string Sender {
            get {
                return _sender;
            }
            set {
                _sender = value;
            }
        }
        string _sender;

        /// <summary>
        /// Gets or sets if a new line is appended to the end of messages when they are written.
        /// </summary>
        public bool AppendNewLine {
            get {
                return addNewLine;
            }
            set {
                addNewLine = value;
            }
        }
        bool addNewLine;

        #endregion

        #region Methods

        /// <summary>
        /// Adds the given possibly-splittable parameter to the writer.
        /// </summary>
        /// <param name="value">The parameter to add to the writer</param>
        /// <param name="splittable">Indicates if the parameter can be split across messages written.</param>
        public void AddParameter (string value, bool splittable)
        {
            parameters.Add (value);
            if (splittable) {
                AddSplittableParameter ();
            }
        }

        /// <summary>
        /// Adds the given non-splittable parameter to the writer.
        /// </summary>
        /// <param name="value">The parameter to add to the writer</param>
        public void AddParameter (string value)
        {
            AddParameter (value, false);
        }

        /// <summary>
        /// Adds a possibly-splittable list of parameters to the writer.
        /// </summary>
        /// <param name="value">The list of parameters to add</param>
        /// <param name="separator">The seperator to write between values in the list</param>
        /// <param name="splittable">Indicates if the parameters can be split across messages written.</param>
        public void AddList (IList value, string separator, bool splittable)
        {
            parameters.Add (value);
            listParams [(parameters.Count - 1).ToString (CultureInfo.InvariantCulture)] = separator;
            if (splittable) {
                AddSplittableParameter ();
            }
        }

        /// <summary>
        /// Adds a splittable list of parameters to the writer.
        /// </summary>
        /// <param name="value">The list of parameters to add</param>
        /// <param name="separator">The seperator to write between values in the list</param>
        public void AddList (IList value, string separator)
        {
            AddList (value, separator, true);
        }

        /// <summary>
        /// Writes the current message data to the inner writer in irc protocol format.
        /// </summary>
        public void Write ()
        {
            //TODO Implement message splitting on IrcMessageWriter.Write
            if (_writer == null) {
                _writer = new StringWriter (CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty (_sender)) {
                _writer.Write (":");
                _writer.Write (_sender);
                _writer.Write (" ");
            }


            int paramCount = parameters.Count;
            if (paramCount > 0) {
                for (int i = 0; i < paramCount - 1; i++) {
                    _writer.Write (GetParamValue (i));
                    _writer.Write (" ");
                }
                string lastParam = GetParamValue (paramCount - 1);
                if (lastParam.IndexOf (" ", StringComparison.Ordinal) > 0) {
                    _writer.Write (":");
                }
                _writer.Write (lastParam);
            }
            if (addNewLine) {
                _writer.Write (Environment.NewLine);
            }

            resetDefaults ();
        }


        #endregion

        #region Helpers

        void resetDefaults ()
        {
            addNewLine = true;
            _sender = null;
            parameters.Clear ();
            listParams.Clear ();
        }

        ArrayList parameters = new ArrayList ();
        NameValueCollection listParams = new NameValueCollection ();
        NameValueCollection splitParams = new NameValueCollection ();

        void AddSplittableParameter ()
        {
            splitParams [parameters.Count.ToString (CultureInfo.InvariantCulture)] = string.Empty;
        }

        string GetParamValue (int index)
        {
            object thisParam = parameters [index];

            StringCollection thisParamAsCollection = thisParam as StringCollection;
            if (thisParamAsCollection != null) {
                string seperator = listParams [index.ToString (CultureInfo.InvariantCulture)];
                return MessageUtil.CreateList (thisParamAsCollection, seperator);
            }

            IList thisParamAsList = thisParam as IList;
            if (thisParamAsList != null) {
                string seperator = listParams [index.ToString (CultureInfo.InvariantCulture)];
                return MessageUtil.CreateList (thisParamAsList, seperator);
            }

            return thisParam.ToString ();
        }

        #endregion

        #region IDisposable Members

        bool disposed = false;

        /// <summary>
        /// Implements IDisposable.Dispose
        /// </summary>
        public void Dispose ()
        {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        void Dispose (bool disposing)
        {
            if (!disposed) {
                if (disposing) {
                    if (_writer != null) {
                        _writer.Dispose ();
                    }
                }
                disposed = true;
            }
        }

        /// <summary>
        /// The IrcMessageWriter destructor
        /// </summary>
        ~IrcMessageWriter ()
        {
            Dispose (false);
        }

        #endregion
    }
}
