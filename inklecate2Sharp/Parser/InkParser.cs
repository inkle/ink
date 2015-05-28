using System;
using System.Collections.Generic;
using System.Linq;
using Inklewriter.Parsed;

namespace Inklewriter
{
	public partial class InkParser : StringParser
	{

        public InkParser(string str) : base(str) { 
			RegisterExpressionOperators ();
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
                var md = new DebugMetadata ();
                md.lineNumber = state.lineIndex + 1;
                parsedObj.debugMetadata = md;
            }
        }

	}
}

