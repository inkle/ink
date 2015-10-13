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
            Assert.AreEqual ("Hello world\n", story.currentText);
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
            Assert.AreEqual ("Hello!\nWorld.\n", story.currentText);
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
            Assert.AreEqual ("8\n", story.currentText);
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
            Assert.AreEqual ("Hello.", story.currentChoices[0].choiceText);

            story.ContinueWithChoiceIndex (0);
            Assert.AreEqual ("Hello, world.\n", story.currentText);
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
            Assert.AreEqual (4, story.currentChoices.Count);
            Assert.AreEqual ("one", story.currentChoices[0].choiceText);
            Assert.AreEqual ("two", story.currentChoices[1].choiceText);
            Assert.AreEqual ("three", story.currentChoices[2].choiceText);
            Assert.AreEqual ("four", story.currentChoices[3].choiceText);
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
            Assert.AreEqual ("option text. Conditional bit.\nNext.\n", story.currentText);
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
            Assert.AreEqual ("\n", story.currentText);
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
            Assert.AreEqual ("This is include 1.\nThis is include 2.\nThis is the main file.\n", story.currentText);
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
            Assert.AreEqual ("The value of a variable in test file 2 is 5.\nThis is the main file\nThe value when accessed from knot_in_2 is 5.\n", story.currentText);
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

            Assert.AreEqual ("true\ntrue\ntrue\ntrue\ntrue\ngreat\nright?\n", story.currentText);
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

            Assert.AreEqual ("other\nother\nother\nother\n", story.currentText);
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
            Assert.AreEqual ("start\n\n", story.currentText);
            Assert.AreEqual (1, story.currentChoices.Count);

            story.ContinueWithChoiceIndex (0);

            Assert.AreEqual ("result\n", story.currentText);
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

            Assert.AreEqual (1, story.currentChoices.Count);
            Assert.AreEqual ("visible choice", story.currentChoices[0].choiceText);
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

            Assert.AreEqual ("first gather\n", story.currentText);
            Assert.AreEqual (2, story.currentChoices.Count);

            story.ContinueWithChoiceIndex (0);

            Assert.AreEqual ("option 1\nthe main gather\n", story.currentText);
            Assert.AreEqual (0, story.currentChoices.Count);
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
            Assert.AreEqual ("\n5\n", story.currentText);
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

            Assert.AreEqual ("gather\ntest\nchoice content\ngather\nsecond time round\n", story.currentText);
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
            Assert.AreEqual (2, story.currentChoices.Count);
            Assert.AreEqual ("one", story.currentChoices[0].choiceText);
            Assert.AreEqual ("four", story.currentChoices[1].choiceText);

            story.ContinueWithChoiceIndex (0);
            Assert.AreEqual (1, story.currentChoices.Count);
            Assert.AreEqual ("two", story.currentChoices[0].choiceText);

            story.ContinueWithChoiceIndex (0);
            Assert.AreEqual ("two\nthree\nsix\n", story.currentText);
        }

        [Test ()]
        public void TestGatherAtFlowEnd()
        {
            // The final "->" doesn't have anywhere to go, so it should
            // happily just go to the end of the flow.
            var storyStr = "- nothing ->";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual ("nothing\n", story.currentText);
        }

        [Test ()]
        public void TestChoiceWithBracketsOnly()
        {
            // The final "->" doesn't have anywhere to go, so it should
            // happily just go to the end of the flow.
            var storyStr = "*   [Option]\n    Text";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual (1, story.currentChoices.Count);
            Assert.AreEqual ("Option", story.currentChoices[0].choiceText);

            story.ContinueWithChoiceIndex (0);

            Assert.AreEqual ("Text\n", story.currentText);
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

            Assert.AreEqual ("one\ntwo\nthree\n", story.currentText);
        }
            
        [Test ()]
        public void TestGatherChoiceSameLine()
        {
            var storyStr =  "- * hello\n- * world";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual ("hello", story.currentChoices [0].choiceText);

            story.ContinueWithChoiceIndex (0);

            Assert.AreEqual ("world", story.currentChoices [0].choiceText);
        }

        [Test ()]
        public void TestSimpleGlue()
        {
            var storyStr =  "Some <> \ncontent<> with glue.";

            Story story = CompileString (storyStr);
            story.Begin ();

            Assert.AreEqual ("Some content with glue.\n", story.currentText);
        }

        [Test ()]
        public void TestEscapeCharacter()
        {
            var storyStr =  @"{true:this is a '\|' character|this isn't}";

            Story story = CompileString (storyStr);
            story.Begin ();

            // Unfortunate leading newline...
            Assert.AreEqual ("this is a '|' character\n", story.currentText);
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
            Assert.AreEqual ("Hello world\n", story.currentText);
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

            Assert.AreEqual ("different knot\nsame knot\nsame knot\ndifferent knot\nsame knot\nsame knot\n", story.currentText);
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

            Assert.AreEqual ("120\n", story.currentText);
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
            Assert.AreEqual ("5\n \n625\n", story.currentText);
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

            Assert.AreEqual ("\n120\n", story.currentText);
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

            Assert.AreEqual ("1 2\n", story.currentText);
        }

        [Test ()]
        public void TestEmpty()
        {
            Story story = CompileString (@"");
            story.Begin ();

            Assert.AreEqual (string.Empty, story.currentText);
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

            Assert.AreEqual ("6\n5\n", story.currentText);
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
->->
");
            story.Begin ();

            Assert.AreEqual ("hi\nhi\nhi\n3\n", story.currentText);
        }

        [Test ()]
        public void TestChoiceCount()
        {
            Story story = CompileString (@"
* one -> end
* two -> end
{ CHOICE_COUNT() }

= end
~ ~ ~
");
            story.Begin ();

            Assert.AreEqual ("2\n", story.currentText);
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

            Assert.AreEqual ("Some content.\nDefault choice chosen.\n", story.currentText);
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

            Assert.AreEqual ("36\n2\n3\n2\n2.333333\n8\n8\n", story.currentText);
        }

        [Test ()]
        public void TestBeatsSince()
        {
            Story story = CompileString (@"
{ BEATS_SINCE(test) }
~ test()
{ BEATS_SINCE(test) }
* [choice 1]
- { BEATS_SINCE(test) }
* [choice 2]
- { BEATS_SINCE(test) }

== function test ==
~ ~ ~
");
            story.Begin ();
            Assert.AreEqual ( "-1\n0\n", story.currentText);

            story.ContinueWithChoiceIndex (0);
            Assert.AreEqual ("1\n", story.currentText);

            story.ContinueWithChoiceIndex (0);
            Assert.AreEqual ("2\n", story.currentText);
        }

        [Test ()]
        public void TestEndOfContent()
        {
            Story story = CompileString ("Hello world");
            story.Begin ();
            Assert.IsFalse (story.hasError);

            story = CompileString ("== test ==\nContent\n-> DONE");
            story.Begin ();
            Assert.IsFalse (story.hasError);

            // No errors, but should have warning (see below)
            story = CompileString ("== test ==\nContent");
            story.Begin ();
            Assert.IsFalse (story.hasError);

            // Should have warning that there's no "-> DONE"
            var parsedStory = CompileStringWithoutRuntime ("== test ==\nContent");
            Assert.IsTrue (parsedStory.hadWarning);
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
            Assert.AreEqual ("opt\ntext\n", story.currentText);
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

            Assert.AreEqual (3, story.currentChoices.Count);

            story.ContinueWithChoiceIndex (0);

            Assert.AreEqual (2, story.currentChoices.Count);

            story.ContinueWithChoiceIndex (0);

            Assert.AreEqual (1, story.currentChoices.Count);

            story.ContinueWithChoiceIndex (0);

            Assert.AreEqual (0, story.currentChoices.Count);
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

            Assert.AreEqual (5,errors.Count);
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

            Assert.AreEqual (2, errors.Count);
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
->->
");

            story.Begin ();

            Assert.AreEqual ("Hello world\n", story.currentText);
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

            Assert.AreEqual ("one (1)\none and a half (1.5)\ntwo (2)\nthree (3)\n", story.currentText);
        }

        [Test ()]
        public void TestTunnelVsPasteBehaviour()
        {
            Story story = CompileString (@"
-> knot_with_options ->
Finished tunnel.

Starting paste.
<- paste_with_options
* E
-
Done.


== knot_with_options ==
* A
* B
-
->->

== paste_with_options ==
* C
* D
");
            story.Begin ();

            // Choices should be A, B
            Assert.AreEqual (2, story.currentChoices.Count);
            Assert.IsFalse (story.currentText.Contains ("Finished tunnel"));

            story.ContinueWithChoiceIndex (0);

            // Choices should be C, D, E
            Assert.IsTrue (story.currentText.Contains ("Finished tunnel"));
            Assert.AreEqual (3, story.currentChoices.Count);

            story.ContinueWithChoiceIndex (2);

            Assert.IsTrue (story.currentText.Contains ("Done."));
        }



        [Test ()]
        public void TestStop()
        {
            Story story = CompileString (@"
hello
-> DONE
world
");
            story.Begin ();

            Assert.AreEqual ("hello\n", story.currentText);
        }


        [Test ()]
        public void TestStop2()
        {
            Story story = CompileString (@"
-> test

== test ==
hello
-> DONE
world
");
            story.Begin ();

            Assert.AreEqual ("hello\n", story.currentText);
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

