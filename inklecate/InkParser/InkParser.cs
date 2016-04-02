using System;
using System.Collections.Generic;

namespace Ink
{
	internal partial class InkParser : StringParser
	{
        public InkParser(string str, string filenameForMetadata = null, string rootDirectory = null, Ink.ErrorHandler externalErrorHandler = null) : base(str) { 
            _filename = filenameForMetadata;
            _rootDirectory = rootDirectory;
			RegisterExpressionOperators ();
            GenerateStatementLevelRules ();
            this.errorHandler = OnError;
            _externalErrorHandler = externalErrorHandler;
		}

        // Main entry point
        public Parsed.Story Parse()
        {
            List<Parsed.Object> topLevelContent = StatementsAtLevel (StatementLevel.Top);
            if (hadError) {
                return null;
            }

            return new Parsed.Story (topLevelContent);
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

        void OnError(string message, int index, int lineIndex, bool isWarning)
        {
            var warningType = isWarning ? "WARNING:" : "ERROR:";
            string fullMessage;

            if (_filename != null) {
                fullMessage = string.Format(warningType+" '{0}' line {1}: {2}",  _filename, (lineIndex+1), message);
            } else {
                fullMessage = string.Format(warningType+" line {0}: {1}", (lineIndex+1), message);
            }

            if (_externalErrorHandler != null) {
                _externalErrorHandler (fullMessage, isWarning ? ErrorType.Warning : ErrorType.Error);
            } else {
                Console.WriteLine (fullMessage);
            }
        }

        Ink.ErrorHandler _externalErrorHandler;

        string _filename;
        string _rootDirectory;
	}
}

