using System;
using System.Collections.Generic;
using System.Text;

namespace Ink.Runtime
{
    public static class StringExt
    {
        public static string Join<T>(string separator, List<T> objects)
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

