using System;
using System.Collections.Generic;
using System.IO;

namespace Ink.InkParser
{
	public partial class InkParser : StringParser
    {
        #region Events

        public event ParserErrorEventHandler ParserError;
        protected virtual void OnParserError(ParserErrorEventArgs e)
        {
            var handler = ParserError;
            handler?.Invoke(this, e);
        }

        #endregion Events

        #region Constructor

        public InkParser(string str, string filenameForMetadata = null, IFileHandler fileHandler = null) 
            : this(str, filenameForMetadata, null, fileHandler) 
        {  }

        InkParser(string str, string inkFilename = null, InkParser rootParser = null, IFileHandler fileHandler = null) : base(str) { 
            _filename = inkFilename;
			RegisterExpressionOperators ();
            GenerateStatementLevelRules ();

            // Built in handler for all standard parse errors and warnings
            this.errorHandler = OnStringParserError;
            
            // The above parse errors are then formatted as strings and passed
            // to the Ink.ErrorHandler, or it throws an exception

            _fileHandler = fileHandler ?? new CurrentDirectoryFileHandler();

            if (rootParser == null) {
                _rootParser = this;

                _openFilenames = new HashSet<string> ();

                if (inkFilename != null) {
                    var fullRootInkPath = _fileHandler.ResolveInkFilename (inkFilename);
                    _openFilenames.Add (fullRootInkPath);
                }

            } else {
                _rootParser = rootParser;
            }

        }

        #endregion Constructor

        // Main entry point
        public Parsed.Fiction Parse()
        {
            List<Parsed.Object> topLevelContent = StatementsAtLevel (StatementLevel.Top);

            // Note we used to return null if there were any errors, but this would mean
            // that include files would return completely empty rather than attempting to
            // continue with errors. Returning an empty include files meant that anything
            // that *did* compile successfully would otherwise be ignored, generating way
            // more errors than necessary.
            return new Parsed.Fiction (topLevelContent, isInclude:_rootParser != this);
        }

        protected List<T> SeparatedList<T> (SpecificParseRule<T> mainRule, ParseRule separatorRule) where T : class
        {
            T firstElement = Parse (mainRule);
            if (firstElement == null) return null;

            var allElements = new List<T> ();
            allElements.Add (firstElement);

            do {

                int nextElementRuleId = BeginRule ();

                var sep = separatorRule ();
                if (sep == null) {
                    FailRule (nextElementRuleId);
                    break;
                }

                var nextElement = Parse (mainRule);
                if (nextElement == null) {
                    FailRule (nextElementRuleId);
                    break;
                }

                SucceedRule (nextElementRuleId);

                allElements.Add (nextElement);

            } while (true);

            return allElements;
        }

        protected override string PreProcessInputString(string str)
        {
            var inputWithCommentsRemoved = (new CommentEliminator (str)).Process();
            return inputWithCommentsRemoved;
        }

        protected override void RuleDidSucceed(object result, StringParserState.Element stateAtStart, StringParserState.Element stateAtEnd)
        {
            // Apply DebugMetadata based on the state at the start of the rule
            // (i.e. use line number as it was at the start of the rule)
            var parsedObj = result as Parsed.Object;
            if ( parsedObj) {
                var md = new Runtime.DebugMetadata ();
                md.startLineNumber = stateAtStart.lineIndex + 1;
                md.endLineNumber = stateAtEnd.lineIndex + 1;
                md.fileName = _filename;
                parsedObj.debugMetadata = md;
                return;
            }

            // A list of objects that doesn't already have metadata?
            var parsedListObjs = result as List<Parsed.Object>;
            if (parsedListObjs != null) {
                foreach (var parsedListObj in parsedListObjs) {
                    if (!parsedListObj.hasOwnDebugMetadata) {
                        var md = new Runtime.DebugMetadata ();
                        md.startLineNumber = stateAtStart.lineIndex + 1;
                        md.endLineNumber = stateAtEnd.lineIndex + 1;
                        md.fileName = _filename;
                        parsedListObj.debugMetadata = md;
                    }
                }
            }
        }
            
        protected bool parsingStringExpression
        {
            get {
                return GetFlag ((uint)CustomFlags.ParsingString);
            } 
            set {
                SetFlag ((uint)CustomFlags.ParsingString, value);
            }
        }

        protected enum CustomFlags {
            ParsingString = 0x1
        }

        void OnStringParserError(string message, int index, int lineIndex, bool isWarning)
        {
            var warningType = isWarning ? "WARNING:" : "ERROR:";
            string fullMessage;

            if (_filename != null) {
                fullMessage = string.Format(warningType+" '{0}' line {1}: {2}",  _filename, (lineIndex+1), message);
            } else {
                fullMessage = string.Format(warningType+" line {0}: {1}", (lineIndex+1), message);
            }

            OnParserError(new ParserErrorEventArgs() { Message = fullMessage , ErrorType = isWarning ? ParserErrorType.Warning : ParserErrorType.Error });
        }

        IFileHandler _fileHandler;



        string _filename;
	}
}

