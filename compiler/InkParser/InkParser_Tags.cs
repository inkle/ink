using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Ink
{
    public partial class InkParser
    {
        protected Parsed.Object StartTag ()
        {
            Whitespace ();

            if (ParseString ("#") == null)
                return null;

            if( parsingStringExpression ) {
                Error("Tags aren't allowed inside of strings. Please use \\# if you want a hash symbol.");
                // but allow us to continue anyway...
            }

            var result = (Parsed.Object)null;

            // End previously active tag before starting new one
            if( tagActive ) {
                var contentList = new Parsed.ContentList();
                contentList.AddContent(new Parsed.Tag { isStart = false });
                contentList.AddContent(new Parsed.Tag { isStart = true });
                result = contentList;
            }
            
            // Otherwise, just start a tag, no need for a content list
            else {
                result = new Parsed.Tag { isStart = true };
            }

            tagActive = true;

            Whitespace ();
            
            return result;
        }

        protected void EndTagIfNecessary(List<Parsed.Object> outputContentList)
        {
            if( tagActive ) {
                if( outputContentList != null )
                    outputContentList.Add(new Parsed.Tag { isStart = false });
                tagActive = false;
            }
        }

        protected void EndTagIfNecessary(Parsed.ContentList outputContentList)
        {
            if( tagActive ) {
                if( outputContentList != null )
                    outputContentList.AddContent(new Parsed.Tag { isStart = false });
                tagActive = false;
            }
        }
    }
    }


