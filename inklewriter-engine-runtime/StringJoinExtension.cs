using System;
using System.Collections;
using System.Text;

namespace Inklewriter.Runtime
{
    public static class StringJoinExtension
    {
        public static string Join(this string str, string separator, IEnumerable objects)
        {
            var sb = new StringBuilder ();

            var isFirst = true;
            foreach (var o in objects) {

                if (!isFirst)
                    sb.Append (separator);

                sb.Append (o.ToString ());

                isFirst = false;
            }
            
            return sb.ToString ();
        }
    }
}

