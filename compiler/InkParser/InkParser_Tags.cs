using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Ink
{
    public partial class InkParser
    {
        protected Parsed.ContentList StartTag ()
        {
            Whitespace ();

            if (ParseString ("#") == null)
                return null;

            var result = new Parsed.ContentList();

            // End previously active tag before starting new one
            EndTagIfNecessary(result);

            tagActive = true;

            Whitespace ();
            
            result.AddContent(new Parsed.Tag(isStart:true));
            return result;
        }

        protected void EndTagIfNecessary(List<Parsed.Object> outputContentList)
        {
            if( tagActive ) {
                if( outputContentList != null )
                    outputContentList.Add(new Parsed.Tag(isStart:false));
                tagActive = false;
            }
        }

        protected void EndTagIfNecessary(Parsed.ContentList outputContentList)
        {
            if( tagActive ) {
                if( outputContentList != null )
                    outputContentList.AddContent(new Parsed.Tag(isStart:false));
                tagActive = false;
            }
        }
    }
    }


