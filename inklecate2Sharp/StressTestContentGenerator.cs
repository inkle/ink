using System;
using System.Text;

namespace Inklewriter
{
    public class StressTestContentGenerator
    {
        public string content { get; }
        public int sizeInKiloChars { get { return content.Length / 1024; } }

        public StressTestContentGenerator (int repetitions)
        {
            var initialContent = "~ var myVar = 5 + 5 + (3 * 2 * 9) + 1.2\n\n";

            var repeatingContent = @"== test__X__ ==
This is some content in a test knot with the number {myVar}.
{ myVar > 2:
    - This evaluation is true.
    - This evaluation is false.
}
This is some more content.
 * A choice
    * * A sub-choice ->
 * Another choice ==> somewhere_else__X__
 - A gather
 Some more content within the gather. Some more content within the gather. Some more content within the gather. Some more content within the gather. Some more content within the gather. 
 Some more content within the gather. Some more content within the gather. Some more content within the gather. 
 myVar is {myVar}.
 * Another choice[.] which is obviously really cool
    And some content within that choice
    * * Another sub choice
     * * * Another another sub choice
      * * * * Yet another choice.
              With more content in that choice.
 - A final gather.
 Nice one.
 Isn't this great?
 ==> test__Y__
 ~ done


== somewhere_else__X__ ==
 This is somewhere else
 ~ done

";

            var sb = new StringBuilder ();
            sb.Append (initialContent);

            for (int i = 1; i <= repetitions; ++i) {

                var content = repeatingContent.Replace ("__X__", i.ToString());
                content = content.Replace ("__Y__", (i + 1).ToString());

                sb.Append (content);
            }

            var finalContent = @" == test__X__
    Done!
    ~ done";

            finalContent = finalContent.Replace ("__X__", (repetitions+1).ToString ());
            sb.AppendFormat (finalContent, repetitions.ToString());

            this.content = sb.ToString ();
        }
    }
}

