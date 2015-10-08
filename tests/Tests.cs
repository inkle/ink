using NUnit.Framework;
using Inklewriter;
using Inklewriter.Runtime;

namespace Tests
{
    public enum TestMode
    {
        Normal,
        JsonRoundTrip
    }

    [TestFixture(TestMode.Normal)]
    [TestFixture(TestMode.JsonRoundTrip)]
	internal class Tests
	{
        public Tests(TestMode mode)
        {
            _mode = mode;
        }
        TestMode _mode;

		// Helper compile function
		protected Story CompileString(string str)
		{
			InkParser parser = new InkParser(str);
            parser.errorHandler += (string message, int index, int lineIndex, bool isWarning) => {
                Assert.Fail(message + " on line " + lineIndex);
            };
			var parsedStory = parser.Parse();
            Assert.IsFalse (parsedStory.hadError);
                
			Story story = parsedStory.ExportRuntime ();

            // Convert to json and back again
            if (_mode == TestMode.JsonRoundTrip) {
                var jsonStr = story.ToJsonString (indented:true);
                story = Story.CreateWithJson (jsonStr);
            }

			return story;
		}

        protected Inklewriter.Parsed.Story CompileStringWithoutRuntime(string str)
        {
            InkParser parser = new InkParser(str);
            parser.errorHandler += (string message, int index, int lineIndex, bool isWarning) => {
                Assert.Fail(message + " on line " + lineIndex);
            };
            var parsedStory = parser.Parse();
            Assert.IsFalse (parsedStory.hadError);

            parsedStory.ExportRuntime ();
            return parsedStory;
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
    -> anotherKnot

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

                === function six
                    ~ return four() + two()

                === function four
                    ~ return two() + two()

                === function two
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
        public void TestConditionalChoices()
        {
            var storyStr =
                @"
* { true } { false } not displayed
* { true } { true }
  { true and true }  one
* { false } not displayed
* (name) { true } two
* { true }
  { true }
  three
* { true }
  four
                ";

            Story story = CompileString (storyStr);
            story.Begin ();
            Assert.AreEqual (story.currentChoices.Count, 4);
            Assert.AreEqual (story.currentChoices[0].choiceText, "one");
            Assert.AreEqual (story.currentChoices[1].choiceText, "two");
            Assert.AreEqual (story.currentChoices[2].choiceText, "three");
            Assert.AreEqual (story.currentChoices[3].choiceText, "four");
        }

        [Test ()]
        public void TestNonTextInChoiceInnerContent()
        {
            var storyStr =
                @"
== knot 
   *   option text[]. {true: Conditional bit.} -> next
   ~ done

== next
    Next.
    ~ done
                ";

            Story story = CompileString (storyStr);
            story.Begin ();
            story.ContinueWithChoiceIndex (0);
            Assert.AreEqual (story.currentText, "option text. Conditional bit.\nNext.\n");
        }

        [Test ()]
        public void TestDivertInConditional()
        {
            var storyStr =
                @"
=== intro
= top
    { main: -> done }
    ~ done
= main 
    -> top 
= done 
    ~ done
                ";

            Story story = CompileString (storyStr);
            story.Begin ();
            Assert.AreEqual (story.currentText, "\n");
        }

        [Test ()]
        public void TestInclude()
        {
            var storyStr =
                @"
~ include test_included_file.ink
~ include test_included_file2.ink

This is the main file.
                ";

            Story story = CompileString (storyStr);
            story.Begin ();
            Assert.AreEqual (story.currentText, "This is include 1.\nThis is include 2.\nThis is the main file.\n");
        }

        [Test ()]
        public void TestNestedInclude()
        {
            var storyStr =
                @"
~ include test_included_file3.ink

This is the main file

-> knot_in_2
                ";

            Story story = CompileString (storyStr);
            story.Begin ();
            Assert.AreEqual (story.currentText, "The value of a variable in test file 2 is 5.\nThis is the main file\nThe value when accessed from knot_in_2 is 5.\n");
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

        public void TestElseBranches()
        {
            var storyStr =
                @"
~ var x = 3

{ 
    - x == 1: one
    - x == 2: two
    - else: other
}

{ 
    - x == 1: one
    - x == 2: two
    - other
}

{ x == 4:
  - The main clause
  - else: other
}

{ x == 4:
  The main clause
- else:
  other
}
                ";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual (story.currentText, "other\nother\nother\nother\n");
        }

        [Test ()]
        public void TestConditionalChoiceInWeave()
        {
            var storyStr =
                @"
== test ==
- start
 { 
    - true: * go to a stitch -> a_stitch
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
        public void TestHasReadOnChoice()
        {
            var storyStr =
                @"
* { not test } visible choice
* { test } visible choice

== test ==
~ done
                ";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual (story.currentChoices.Count, 1);
            Assert.AreEqual (story.currentChoices[0].choiceText, "visible choice");
        }

        [Test ()]
        public void TestConditionalChoiceInWeave2()
        {
            var storyStr =
                @"
- first gather
    * option 1
    * option 2
- the main gather
{false:
    * unreachable option
}
- unrechable gather
                ";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual (story.currentText, "first gather\n");
            Assert.AreEqual (story.currentChoices.Count, 2);

            story.ContinueWithChoiceIndex (0);

            Assert.AreEqual (story.currentText, "option 1\nthe main gather\n");
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
-> knot.stitch.gather

== knot ==
= stitch
- hello
    * (choice) test
        choice content
- (gather)
  gather

  {stopping:
    - -> knot.stitch.choice
    - second time round
  }

~ done
                ";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual (story.currentText, "gather\ntest\nchoice content\ngather\nsecond time round\n");
        }


        [Test ()]
        public void TestWeaveGathers()
        {
            var storyStr =
                @"
- 
 * one
    * * two
   - - three
 *  four
   - - five
- six
                ";

            Story story = CompileString (storyStr);

            story.Begin ();
            Assert.AreEqual (story.currentChoices.Count, 2);
            Assert.AreEqual (story.currentChoices[0].choiceText, "one");
            Assert.AreEqual (story.currentChoices[1].choiceText, "four");

            story.ContinueWithChoiceIndex (0);
            Assert.AreEqual (story.currentChoices.Count, 1);
            Assert.AreEqual (story.currentChoices[0].choiceText, "two");

            story.ContinueWithChoiceIndex (0);
            Assert.AreEqual (story.currentText, "two\nthree\nsix\n");
        }

        [Test ()]
        public void TestGatherAtFlowEnd()
        {
            // The final "->" doesn't have anywhere to go, so it should
            // happily just go to the end of the flow.
            var storyStr = "- nothing ->";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual (story.currentText, "nothing\n");
        }

        [Test ()]
        public void TestChoiceWithBracketsOnly()
        {
            // The final "->" doesn't have anywhere to go, so it should
            // happily just go to the end of the flow.
            var storyStr = "*   [Option]\n    Text";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual (story.currentChoices.Count, 1);
            Assert.AreEqual (story.currentChoices[0].choiceText, "Option");

            story.ContinueWithChoiceIndex (0);

            Assert.AreEqual (story.currentText, "Text\n");
        }

        [Test ()]
        public void TestDivertWeaveArrowTypes()
        {
            var storyStr =
                @"
- (one) one -> two
- (two) two -> three
- (three) three
                ";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual (story.currentText, "one\ntwo\nthree\n");
        }
            
        [Test ()]
        public void TestGatherChoiceSameLine()
        {
            var storyStr =  "- * hello\n- * world";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual (story.currentChoices [0].choiceText, "hello");

            story.ContinueWithChoiceIndex (0);

            Assert.AreEqual (story.currentChoices [0].choiceText, "world");
        }

        [Test ()]
        public void TestSimpleGlue()
        {
            var storyStr =  "Some <> \ncontent<> with glue.";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual (story.currentText, "Some content with glue.\n");
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

        [Test ()]
        public void TestSectionEnd()
        {
            var storyStr =  @"
== knot
Hello world
 ~ ~ ~~";

            Story story = CompileString (storyStr);
            story.Begin ();

            // Unfortunate leading newline...
            Assert.AreEqual (story.currentText, "Hello world\n");
        }

        [Test ()]
        public void TestCompareDivertTargets()
        {
            var storyStr =  @"~ var to_one = -> one
~ var to_two = -> two

{to_one == to_two:same knot|different knot}
{to_one == to_one:same knot|different knot}
{to_two == to_two:same knot|different knot}
{ -> one == -> two:same knot|different knot}
{ -> one == to_one:same knot|different knot}
{ to_one == -> one:same knot|different knot}

== one
    One

=== two 
    Two";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual (story.currentText, "different knot\nsame knot\nsame knot\ndifferent knot\nsame knot\nsame knot\n");
        }

        [Test ()]
        public void TestFactorialRecursive()
        {
            var storyStr =  @"
{ factorial(5) }

== function factorial(n) ==
 { n == 1:
    ~ return 1
 - else:
    ~ return (n * factorial(n-1))
 }
";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual (story.currentText, "120\n");
       }

        [Test ()]
        public void TestNestedPassByReference()
        {
            var storyStr =  @"
~ var x = 5

{x}

~ squaresquare(x)

{x}

== function squaresquare(ref x) ==
 {square(x)} {square(x)}
 ~ ~ ~

== function square(ref x) ==
 ~ x = x * x
 ~ ~ ~
";

            Story story = CompileString (storyStr);
            story.Begin ();

            // Bloody whitespace
            Assert.AreEqual (story.currentText, "5\n \n625\n");
        }


        [Test ()]
        public void TestFactorialByReference()
        {
            var storyStr =  @"
~ var result
~ factorialByRef(result, 5)
{ result }


== function factorialByRef(ref r, n) ==
{ r == 0:
    ~ r = 1
}
{ n > 1:
    ~ r = r * n
    ~ factorialByRef(r, n-1)
}
~ ~ ~
";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual (story.currentText, "\n120\n");
        }

        [Test ()]
        public void TestVariableSwapRecurse()
        {
            var storyStr =  @"
~ f(1, 1)

== function f(x, y) ==
{ x == 1 and y == 1:
  ~ x = 2
  ~ f(y, x)
- else:
  {x} {y}
}
~ ~ ~
";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual (story.currentText, "1 2\n");
        }

        [Test ()]
        public void TestEmpty()
        {
            Story story = CompileString (@"");
            story.Begin ();

            Assert.AreEqual (story.currentText, string.Empty);
        }

        [Test ()]
        public void TestIncrement()
        {
            Story story = CompileString (@"
~ var x = 5
~ x++
{x}

~ x--
{x}
");
            story.Begin ();

            Assert.AreEqual (story.currentText, "6\n5\n");
        }

        [Test ()]
        public void TestReadCountDotSeparatedPath()
        {
            Story story = CompileString (@"
-> hi ->
-> hi ->
-> hi ->

{ hi.stitch_to_count }

== hi ==
= stitch_to_count
hi
~ ~ ~
");
            story.Begin ();

            Assert.AreEqual (story.currentText, "hi\nhi\nhi\n3\n");
        }

        [Test ()]
        public void TestChoiceCount()
        {
            Story story = CompileString (@"
* one -> end
* two -> end
{ choice_count() }

= end
~ ~ ~
");
            story.Begin ();

            Assert.AreEqual (story.currentText, "2\n");
        }


        [Test ()]
        public void TestDefaultChoices()
        {
            Story story = CompileString (@"
Some content.

* {false} impossible choice
* -> default_target

= default_target
Default choice chosen.
~ ~ ~
");
            story.Begin ();

            Assert.AreEqual (story.currentText, "Some content.\nDefault choice chosen.\n");
        }


        class TestWarningException : System.Exception {}

        [Test ()]
        public void TestReturnTextWarning()
        {
            InkParser parser = new InkParser("== test ==\n return something");
            parser.errorHandler += (string message, int index, int lineIndex, bool isWarning) => {
                if( isWarning ) {
                    throw new TestWarningException();
                }
            };

            Assert.Throws<TestWarningException>(() => parser.Parse ());
        }

        [Test ()]
        public void TestArithmetic()
        {
            Story story = CompileString (@"
{ 2 * 3 + 5 * 6 }
{8 mod 3}
{13 % 5}
{ 7 / 3 }
{ 7 / 3.0 }
{ 10 - 2 }
{ 2 * (5-1) }
");
            story.Begin ();

            Assert.AreEqual (story.currentText, "36\n2\n3\n2\n2.333333\n8\n8\n");
        }

        [Test ()]
        public void TestBeatsSince()
        {
            Story story = CompileString (@"
{ beats_since(test) }
~ test()
{ beats_since(test) }
* [choice 1]
- { beats_since(test) }
* [choice 2]
- { beats_since(test) }

== function test ==
~ ~ ~
");
            story.Begin ();
            Assert.AreEqual (story.currentText, "-1\n0\n");

            story.ContinueWithChoiceIndex (0);
            Assert.AreEqual (story.currentText, "1\n");

            story.ContinueWithChoiceIndex (0);
            Assert.AreEqual (story.currentText, "2\n");
        }

        [Test ()]
        public void TestEndOfContent()
        {
            Story story = CompileString ("Hello world");
            story.Begin ();
            Assert.AreEqual (story.hasError, false);

            story = CompileString ("== test ==\nContent\n~ ~ ~");
            story.Begin ();
            Assert.AreEqual (story.hasError, false);

            story = CompileString ("== test ==\nContent");
            story.Begin ();
            Assert.AreEqual (story.hasError, true);
        }

        [Test ()]
        public void TestShouldntGatherDueToChoice()
        {
            Story story = CompileString (@"
* opt
    - - text
    * * {false} impossible
- gather");
            
            story.Begin ();
            story.ContinueWithChoiceIndex (0);

            // Shouldn't go to "gather"
            Assert.AreEqual (story.currentText, "opt\ntext\n");

            // Should run out of content
            Assert.IsTrue(story.hasError);
        }

        [Test ()]
        public void TestOnceOnlyChoicesWithOwnContent()
        {
            Story story = CompileString (@"
~ var times = 3
-> home

== home ==
~ times = times - 1
{times >= 0:-> eat}
I've finished eating now.
~ ~ ~

== eat ==
This is the {first|second|third} time.
 * Eat ice-cream[]
 * Drink coke[]
 * Munch cookies[]
-
-> home
");

            story.Begin ();

            Assert.AreEqual (story.currentChoices.Count, 3);

            story.ContinueWithChoiceIndex (0);

            Assert.AreEqual (story.currentChoices.Count, 2);

            story.ContinueWithChoiceIndex (0);

            Assert.AreEqual (story.currentChoices.Count, 1);

            story.ContinueWithChoiceIndex (0);

            Assert.AreEqual (story.currentChoices.Count, 0);
        }

        [Test ()]
        public void TestFunctionPurityChecks()
        {
            Inklewriter.Parsed.Story parsedStory = CompileStringWithoutRuntime (@"
-> test

== test ==
~ myFunc()
= function myBadInnerFunc
Not allowed!
~ ~ ~


== function myFunc ==
Hello world
* a choice
* another choice
-
-> myFunc
= testStitch
    This is a stitch
~ ~ ~
");
            var errors = parsedStory.errors;

            Assert.AreEqual (errors.Count,5);
            Assert.IsTrue(errors[0].Contains("Functions cannot be stitches"));
            Assert.IsTrue(errors[1].Contains("Functions may not contain stitches"));
            Assert.IsTrue(errors[2].Contains("Functions may not contain diverts"));
            Assert.IsTrue(errors[3].Contains("Functions may not contain choices"));
            Assert.IsTrue(errors[4].Contains("Functions may not contain choices"));
        }


        [Test ()]
        public void TestFunctionCallRestrictions()
        {
            Inklewriter.Parsed.Story parsedStory = CompileStringWithoutRuntime (@"
// Allowed to do this
~ myFunc()

// Not allowed to to this
~ aKnot()

// Not allowed to do this
-> myFunc

== function myFunc ==
This is a function.
~ ~ ~

== aKnot ==
This is a normal knot.
~ ~ ~
");
            var errors = parsedStory.errors;

            Assert.AreEqual (errors.Count,2);
            Assert.IsTrue(errors[0].Contains("hasn't been marked as a function"));
            Assert.IsTrue(errors[1].Contains("can only be called as a function"));
        }

        [Test ()]
        public void TestBasicTunnel()
        {
            Story story = CompileString (@"
-> f ->
<> world

== f ==
Hello
");

            story.Begin ();

            Assert.AreEqual (story.currentText, "Hello world\n");
        }

        [Test ()]
        public void TestComplexTunnels()
        {
            Story story = CompileString (@"
-> one (1) -> two (2) ->
three (3)

== one(num) ==
one ({num})
-> oneAndAHalf (1.5) ->
->->

== oneAndAHalf(num) ==
one and a half ({num})
->->

== two (num) ==
two ({num})
->->
");
            story.Begin ();

            Assert.AreEqual (story.currentText, "one (1)\none and a half (1.5)\ntwo (2)\nthree (3)\n");
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

