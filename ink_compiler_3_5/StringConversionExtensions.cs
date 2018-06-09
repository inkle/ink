using System.Collections.Generic;

namespace Ink
{
    public static class StringConversionExtensions
    {
        public static string[] ToStringsArray<T>(this List<T> objects)
        {
            int count = objects.Count;
            string[] strings = new string[count];

            for(int i = 0; i < count; i++)
            {
                strings[i] = objects.ToString();
            }

            return strings;
        }
    }
}
