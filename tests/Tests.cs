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
            parser.errorHandler += (string message, int index, int lineIndex) => {
                Assert.Fail(message + " on line " + lineIndex);
            };
			var parsedStory = parser.Parse();
            Assert.IsFalse (parsedStory.hadError);
                
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
    ~ done
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
                    ~ done

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
                        ~ done
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
{ true:
    great
    right?
}
                ";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual (story.currentText, "true\ntrue\ntrue\ntrue\ntrue\ngreat\nright?\n");
        }

        [Test ()]
        public void TestConditionalChoiceInWeave()
        {
            var storyStr =
                @"
== test ==
- start
 { 
    - true: * go to a stitch => a_stitch
 }
- gather shouldn't be seen
~ done

= a_stitch
    result
    ~ done
                ";

            Story story = CompileString (storyStr);
            story.Begin ();

            // Extra newline is because there's a choice object sandwiched there,
            // so it can't be absorbed :-/
            Assert.AreEqual (story.currentText, "start\n\n");
            Assert.AreEqual (story.currentChoices.Count, 1);

            story.ContinueWithChoiceIndex (0);

            Assert.AreEqual (story.currentText, "result\n");
        }

        [Test ()]
        public void TestConditionalChoiceInWeave2()
        {
            var storyStr =
                @"
== test ==
- start
 { 
    - false: * go to a stitch => a_stitch
 }
- gather should be seen
~ done

= a_stitch
    result
    ~ done
                ";

            Story story = CompileString (storyStr);
            story.Begin ();

            // Extra newline is because there's a choice object sandwiched there,
            // so it can't be absorbed :-/
            Assert.AreEqual (story.currentText, "start\ngather should be seen\n");
            Assert.AreEqual (story.currentChoices.Count, 0);
        }

        [Test ()]
        public void TestVariableDeclarationInConditional()
        {
            var storyStr =
                @"
{true:
    - ~ var x = 5
}
{x}
                ";

            Story story = CompileString (storyStr);
            story.Begin ();

            // Extra newline is because there's a choice object sandwiched there,
            // so it can't be absorbed :-/
            Assert.AreEqual (story.currentText, "\n5\n");
        }

        [Test ()]
        public void TestDivertToWeavePoints()
        {
            var storyStr =
                @"
==> knot => stitch -> gather

== knot ==
= stitch
- hello
    * (choice) test
        choice content
- (gather)
  gather

  {stopping:
    - ==> knot => stitch -> choice
    - second time round
  }

~ done
                ";

            Story story = CompileString (storyStr);
            story.Begin ();

            // Unfortunate leading newline...
            Assert.AreEqual (story.currentText, "\ngather\nchoice content\ngather\nsecond time round\n");
        }
            
        [Test ()]
        public void TestEscapeCharacter()
        {
            var storyStr =  @"{true:this is a '\|' character|this isn't}";

            Story story = CompileString (storyStr);
            story.Begin ();

            // Unfortunate leading newline...
            Assert.AreEqual (story.currentText, "this is a '|' character\n");
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

