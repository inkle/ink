using System;
using System.Collections.Generic;
using System.Linq;
using Inklewriter.Parsed;

namespace Inklewriter
{
	public partial class InkParser : StringParser
	{
        public InkParser(string str, string filenameForMetadata = null) : base(str) { 
            _filename = filenameForMetadata;
			RegisterExpressionOperators ();
            this.errorHandler = OnError;
		}

        protected override string PreProcessInputString(string str)
        {
            var inputWithCommentsRemoved = (new CommentEliminator (str)).Process();
            return inputWithCommentsRemoved;
        }

        protected override void RuleDidSucceed(object result, StringParserState.Element state)
        {
            // Apply DebugMetadata based on the state at the start of the rule
            // (i.e. use line number as it was at the start of the rule)
            var parsedObj = result as Parsed.Object;
            if ( parsedObj != null) {
                var md = new Runtime.DebugMetadata ();
                md.lineNumber = state.lineIndex + 1;
                md.fileName = _filename;
                parsedObj.debugMetadata = md;
            }
        }

        public void OnError(string message, int index, int lineIndex)
        {
            if (_filename != null) {
                Console.WriteLine ("Error in '{0}' line {1}: {2}", _filename, (lineIndex+1), message);
            } else {
                Console.WriteLine ("Error on line {0}: {1}", (lineIndex+1), message);
            }
        }

        string _filename;
	}
}

