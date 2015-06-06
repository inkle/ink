using NUnit.Framework;
using System;
using Inklewriter;
using Inklewriter.Runtime;

namespace Tests
{
	[TestFixture ()]
	public class StringParserTests
	{
		// Helper compile function
		protected Story CompileString(string str)
		{
			InkParser parser = new InkParser(str);
			var parsedStory = parser.Parse();
			Story story = parsedStory.ExportRuntime ();
			return story;
		}

		[Test ()]
		public void TestHelloWorld()
		{
			Story story = CompileString ("Hello world");
			story.Begin ();
			Assert.AreEqual (story.currentText, "Hello world\n");
		}

        [Test ()]
        public void TestWhitespace()
        {
            var storyStr =
@"
=== firstKnot
    Hello!
    ==> anotherKnot

=== anotherKnot
    World.
";
            
            Story story = CompileString (storyStr);
            story.Begin ();
            Assert.AreEqual (story.currentText, "Hello!\nWorld.\n");
        }

        [Test ()]
        public void TestCallStackEvaluation()
        {
            var storyStr =
                @"
                === eight
                   { six() + two() }

                === six
                    ~ return four() + two()

                === four
                    ~ return two() + two()

                === two
                    ~ return 2
                ";

            Story story = CompileString (storyStr);
            story.Begin ();
            Assert.AreEqual (story.currentText, "8\n");
        }

        [Test ()]
        public void TestWeaveOptions()
        {
            var storyStr =
                @"
                    === test
                        * Hello[.], world.
                ";



            Story story = CompileString (storyStr);
            story.Begin ();
            Assert.AreEqual (story.currentChoices[0].choiceText, "Hello.");

            story.ContinueWithChoiceIndex (0);
            Assert.AreEqual (story.currentText, "Hello, world.\n");
        }

        [Test ()]
        public void TestConditionals()
        {
            var storyStr =
                @"
{false:not true|true}
{
   - 4 > 5: not true
   - 5 > 4: true
}
{ 2*2 > 3:
   - true
   - not true
}
{
   - 1 > 3: not true
   - { 2+2 == 4:
        - true
        - not true
   }
}
{ 2*3:
   - 1+7: not true
   - 9: not true
   - 1+1+1+3: true
   - 9-3: also true but not printed
}
                ";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual (story.currentText, "true\ntrue\ntrue\ntrue\ntrue\n");
        }

		//------------------------------------------------------------------------

		[Test ()]
		public void TestStringParserABAB ()
		{
			StringParser p = new StringParser ("ABAB");
			var results = p.Interleave<string>(
				() => p.ParseString ("A"),
				() => p.ParseString ("B"));

			var expected = new [] { "A", "B", "A", "B" };
			Assert.AreEqual(expected, results);
		}

		[Test ()]
		public void TestStringParserA ()
		{
			StringParser p = new StringParser ("A");
			var results = p.Interleave<string>(
				() => p.ParseString ("A"),
				() => p.ParseString ("B"));

			var expected = new [] { "A" };
			Assert.AreEqual(expected, results);
		}

		[Test ()]
		public void TestStringParserB ()
		{
			StringParser p = new StringParser ("B");
			var result = p.Interleave<string>(
				() => p.ParseString ("A"),
				() => p.ParseString ("B"));

			Assert.IsNull (result);
		}

		[Test ()]
		public void TestStringParserABAOptional ()
		{
			StringParser p = new StringParser ("ABAA");
			var results = p.Interleave<string>(
				() => p.ParseString ("A"),
				p.Optional(() => p.ParseString ("B")));

			var expected = new [] { "A", "B", "A", "A" };
			Assert.AreEqual(expected, results);
		}

		[Test ()]
		public void TestStringParserABAOptional2 ()
		{
			StringParser p = new StringParser ("BABB");
			var results = p.Interleave<string>(
				p.Optional(() => p.ParseString ("A")),
				() => p.ParseString ("B"));

			var expected = new [] { "B", "A", "B", "B" };
			Assert.AreEqual(expected, results);
		}

        //------------------------------------------------------------------------

        [Test ()]
        public void TestCommentEliminator ()
        {
            var testContent = 
@"A   // C
A /* C */ A

A * A * /* * C *// A
/*
C C C

*/

";

            CommentEliminator p = new CommentEliminator (testContent);
            var result = p.Process ();

            var expected = 
@"A   
A  A

A * A * / A





";

            Assert.AreEqual(expected, result);
        }

	}
}

