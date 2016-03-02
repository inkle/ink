using NUnit.Framework;
using Ink;
using Ink.Runtime;

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

        protected void CheckParsedStoryForErrors(Ink.Parsed.Story story) {
            if (story.hadError) {
                foreach (string error in story.errors) {
                    Assert.Fail ("Story compilation error: " + error);
                }
            }
        }

		// Helper compile function
        protected Story CompileString(string str, bool countAllVisits=false)
		{
			InkParser parser = new InkParser(str);
            parser.errorHandler += (string message, int index, int lineIndex, bool isWarning) => {
                Assert.Fail(message + " on line " + lineIndex);
            };
			var parsedStory = parser.Parse();
            parsedStory.countAllVisits = countAllVisits;
            CheckParsedStoryForErrors (parsedStory);
                
			Story story = parsedStory.ExportRuntime ();
            CheckParsedStoryForErrors (parsedStory);
            Assert.AreNotEqual (null, story);

            // Convert to json and back again
            if (_mode == TestMode.JsonRoundTrip) {
                var jsonStr = story.ToJsonString (indented:true);
                story = Story.CreateWithJson (jsonStr);
            }

			return story;
		}

        protected Ink.Parsed.Story CompileStringWithoutRuntime(string str)
        {
            InkParser parser = new InkParser(str);
            parser.errorHandler += (string message, int index, int lineIndex, bool isWarning) => {
                if( !isWarning )
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
            Assert.AreEqual ("Hello world\n", story.Continue());
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
    -> END
";
            
            Story story = CompileString (storyStr);
            Assert.AreEqual ("Hello!\nWorld.\n", story.ContinueMaximally());
        }

        [Test ()]
        public void TestCallStackEvaluation()
        {
            var storyStr =
                @"
                === eight
                   { six() + two() }
                    -> END

                === function six
                    ~ return four() + two()

                === function four
                    ~ return two() + two()

                === function two
                    ~ return 2
                ";

            Story story = CompileString (storyStr);
            Assert.AreEqual ("8\n", story.Continue());
        }

        [Test ()]
        public void TestWeaveOptions()
        {
            var storyStr =
                @"
                    === test
                        * Hello[.], world.
                        -> END
                ";



            Story story = CompileString (storyStr);
            story.Continue ();

            Assert.AreEqual ("Hello.", story.currentChoices[0].choiceText);

            story.ChooseChoiceIndex (0);
            Assert.AreEqual ("Hello, world.\n", story.Continue());
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
            story.ContinueMaximally ();

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
   -> END

== next
    Next.
    -> END
                ";

            Story story = CompileString (storyStr);
            story.Continue ();

            story.ChooseChoiceIndex (0);
            Assert.AreEqual ("option text. Conditional bit.\n", story.Continue());
            Assert.AreEqual ("Next.\n", story.Continue());
        }

        [Test ()]
        public void TestDivertInConditional()
        {
            var storyStr =
                @"
=== intro
= top
    { main: -> done }
    -> END
= main 
    -> top 
= done 
    -> END
                ";

            Story story = CompileString (storyStr);
            Assert.AreEqual ("", story.ContinueMaximally());
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
            Assert.AreEqual ("This is include 1.\nThis is include 2.\nThis is the main file.\n", story.ContinueMaximally());
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
            Assert.AreEqual ("The value of a variable in test file 2 is 5.\nThis is the main file\nThe value when accessed from knot_in_2 is 5.\n", story.ContinueMaximally());
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

            Assert.AreEqual ("true\ntrue\ntrue\ntrue\ntrue\ngreat\nright?\n", story.ContinueMaximally());
        }

        public void TestElseBranches()
        {
            var storyStr =
                @"
VAR x = 3

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
    - true: * [go to a stitch] -> a_stitch
 }
- gather shouldn't be seen
-> END

= a_stitch
    result
    -> END
                ";

            Story story = CompileString (storyStr);

            // Extra newline is because there's a choice object sandwiched there,
            // so it can't be absorbed :-/
            Assert.AreEqual ("start\n", story.Continue());
            Assert.AreEqual (1, story.currentChoices.Count);

            story.ChooseChoiceIndex (0);

            Assert.AreEqual ("result\n", story.Continue());
        }

        [Test ()]
        public void TestHasReadOnChoice()
        {
            var storyStr =
                @"
* { not test } visible choice
* { test } visible choice

== test ==
-> END
                ";

            Story story = CompileString (storyStr);
            story.ContinueMaximally ();

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

            Assert.AreEqual ("first gather\n", story.Continue());

            Assert.AreEqual (2, story.currentChoices.Count);

            story.ChooseChoiceIndex (0);

            Assert.AreEqual ("option 1\nthe main gather\n", story.ContinueMaximally());
            Assert.AreEqual (0, story.currentChoices.Count);
        }

        [Test ()]
        public void TestVariableDeclarationInConditional()
        {
            var storyStr =
                @"
VAR x = 0
{true:
    - ~ x = 5
}
{x}
                ";

            Story story = CompileString (storyStr);

            // Extra newline is because there's a choice object sandwiched there,
            // so it can't be absorbed :-/
            Assert.AreEqual ("5\n", story.Continue());
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

-> END
                ";

            Story story = CompileString (storyStr);

            Assert.AreEqual ("gather\ntest\nchoice content\ngather\nsecond time round\n", story.ContinueMaximally());
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

            story.ContinueMaximally ();

            Assert.AreEqual (2, story.currentChoices.Count);
            Assert.AreEqual ("one", story.currentChoices[0].choiceText);
            Assert.AreEqual ("four", story.currentChoices[1].choiceText);

            story.ChooseChoiceIndex (0);
            story.ContinueMaximally ();

            Assert.AreEqual (1, story.currentChoices.Count);
            Assert.AreEqual ("two", story.currentChoices[0].choiceText);

            story.ChooseChoiceIndex (0);
            Assert.AreEqual ("two\nthree\nsix\n", story.ContinueMaximally());
        }

        [Test ()]
        public void TestGatherAtFlowEnd()
        {
            // The final "->" doesn't have anywhere to go, so it should
            // happily just go to the end of the flow.
            var storyStr = "- nothing ->";

            Story story = CompileString (storyStr);

            // Hrm: terminating space is a little bit silly
            // (it's because the divert arrow forces a little bit of
            // whitespace in case you're diverting straight into another line)
            Assert.AreEqual ("nothing ", story.ContinueMaximally());
        }

        [Test ()]
        public void TestChoiceWithBracketsOnly()
        {
            // The final "->" doesn't have anywhere to go, so it should
            // happily just go to the end of the flow.
            var storyStr = "*   [Option]\n    Text";

            Story story = CompileString (storyStr);
            story.Continue ();

            Assert.AreEqual (1, story.currentChoices.Count);
            Assert.AreEqual ("Option", story.currentChoices[0].choiceText);

            story.ChooseChoiceIndex (0);

            Assert.AreEqual ("Text\n", story.Continue());
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

            Assert.AreEqual ("one two three\n", story.ContinueMaximally());
        }
            
        [Test ()]
        public void TestGatherChoiceSameLine()
        {
            var storyStr =  "- * hello\n- * world";

            Story story = CompileString (storyStr);
            story.Continue ();

            Assert.AreEqual ("hello", story.currentChoices [0].choiceText);

            story.ChooseChoiceIndex (0);
            story.Continue ();

            Assert.AreEqual ("world", story.currentChoices [0].choiceText);
        }

        [Test ()]
        public void TestSimpleGlue()
        {
            var storyStr =  "Some <> \ncontent<> with glue.\n";

            Story story = CompileString (storyStr);

            Assert.AreEqual ("Some content with glue.\n", story.Continue());
        }

        [Test ()]
        public void TestEscapeCharacter()
        {
            var storyStr =  @"{true:this is a '\|' character|this isn't}";

            Story story = CompileString (storyStr);

            // Unfortunate leading newline...
            Assert.AreEqual ("this is a '|' character\n", story.ContinueMaximally());
        }

        [Test ()]
        public void TestCompareDivertTargets()
        {
            var storyStr =  @"
VAR to_one = -> one
VAR to_two = -> two

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

            Assert.AreEqual ("different knot\nsame knot\nsame knot\ndifferent knot\nsame knot\nsame knot\n", story.ContinueMaximally());
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

            Assert.AreEqual ("120\n", story.ContinueMaximally());
       }

        [Test ()]
        public void TestNestedPassByReference()
        {
            var storyStr =  @"
VAR globalVal = 5

{globalVal}

~ squaresquare(globalVal)

{globalVal}

== function squaresquare(ref x) ==
 {square(x)} {square(x)}
 ~ return

== function square(ref x) ==
 ~ x = x * x
 ~ return
";

            Story story = CompileString (storyStr);

            // Bloody whitespace
            Assert.AreEqual ("5\n \n625\n", story.ContinueMaximally());
        }


        [Test ()]
        public void TestFactorialByReference()
        {
            var storyStr =  @"
VAR result = 0
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
~ return
";

            Story story = CompileString (storyStr);

            Assert.AreEqual ("120\n", story.ContinueMaximally());
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
~ return
";

            Story story = CompileString (storyStr);

            Assert.AreEqual ("1 2\n", story.ContinueMaximally());
        }

        [Test ()]
        public void TestEmpty()
        {
            Story story = CompileString (@"");

            Assert.AreEqual (string.Empty, story.currentText);
        }

        [Test ()]
        public void TestIncrement()
        {
            Story story = CompileString (@"
VAR x = 5
~ x++
{x}

~ x--
{x}
");

            Assert.AreEqual ("6\n5\n", story.ContinueMaximally());
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

            Assert.AreEqual ("hi\nhi\nhi\n3\n", story.ContinueMaximally());
        }

        [Test ()]
        public void TestChoiceCount()
        {
            Story story = CompileString (@"
* one -> end
* two -> end
{ CHOICE_COUNT() }

= end
-> END
");

            Assert.AreEqual ("2\n", story.Continue());
        }


        [Test ()]
        public void TestDefaultChoices()
        {
            Story story = CompileString (@"
 - (start)
 * [Choice 1]
 * [Choice 2]
 * {false} Impossible choice
 * -> default
 - After choice
 -> start

== default ==
This is default.
-> DONE
");

            Assert.AreEqual ("", story.Continue());
            Assert.AreEqual (2, story.currentChoices.Count);


            story.ChooseChoiceIndex (0);
            Assert.AreEqual ("After choice\n", story.Continue());

            Assert.AreEqual (1, story.currentChoices.Count);

            story.ChooseChoiceIndex (0);
            Assert.AreEqual ("After choice\nThis is default.\n", story.ContinueMaximally());
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

            Assert.AreEqual ("36\n2\n3\n2\n2.333333\n8\n8\n", story.ContinueMaximally());
        }

        [Test ()]
        public void TestTurnsSince()
        {
            Story story = CompileString (@"
{ TURNS_SINCE(-> test) }
~ test()
{ TURNS_SINCE(-> test) }
* [choice 1]
- { TURNS_SINCE(-> test) }
* [choice 2]
- { TURNS_SINCE(-> test) }

== function test ==
~ return
");
            Assert.AreEqual ( "-1\n0\n", story.ContinueMaximally());

            story.ChooseChoiceIndex (0);
            Assert.AreEqual ("1\n", story.ContinueMaximally());

            story.ChooseChoiceIndex (0);
            Assert.AreEqual ("2\n", story.ContinueMaximally());
        }

        [Test ()]
        public void TestEndOfContent()
        {
            Story story = CompileString ("Hello world");
            story.ContinueMaximally ();
            Assert.IsFalse (story.hasError);

            story = CompileString ("== test ==\nContent\n-> END");
            story.ContinueMaximally ();
            Assert.IsFalse (story.hasError);

            // Should have runtime error due to running out of content
            // (needs a -> END)
            story = CompileString ("== test ==\nContent");
            story.ContinueMaximally ();
            Assert.IsTrue (story.hasError);

            // Should have warning that there's no "-> END"
            var parsedStory = CompileStringWithoutRuntime ("== test ==\nContent");
            Assert.IsTrue (parsedStory.hadWarning);

            parsedStory = CompileStringWithoutRuntime ("== test ==\n~return");
            Assert.IsTrue (parsedStory.hadError);
            parsedStory.errors [0].Contains ("Return statements can only be used in knots that are declared as functions");

            parsedStory = CompileStringWithoutRuntime ("== function test ==\n-> END");
            Assert.IsTrue (parsedStory.hadError);
            parsedStory.errors [0].Contains ("Functions may not contain diverts");
        }

        [Test ()]
        public void TestShouldntGatherDueToChoice()
        {
            Story story = CompileString (@"
* opt
    - - text
    * * {false} impossible
- gather");
            
            story.ContinueMaximally ();
            story.ChooseChoiceIndex (0);

            // Shouldn't go to "gather"
            Assert.AreEqual ("opt\ntext\n", story.ContinueMaximally());
        }

        [Test ()]
        public void TestOnceOnlyChoicesWithOwnContent()
        {
            Story story = CompileString (@"
VAR times = 3
-> home

== home ==
~ times = times - 1
{times >= 0:-> eat}
I've finished eating now.
-> END

== eat ==
This is the {first|second|third} time.
 * Eat ice-cream[]
 * Drink coke[]
 * Munch cookies[]
-
-> home
");

            story.ContinueMaximally ();

            Assert.AreEqual (3, story.currentChoices.Count);

            story.ChooseChoiceIndex (0);
            story.ContinueMaximally ();

            Assert.AreEqual (2, story.currentChoices.Count);

            story.ChooseChoiceIndex (0);
            story.ContinueMaximally ();

            Assert.AreEqual (1, story.currentChoices.Count);

            story.ChooseChoiceIndex (0);
            story.ContinueMaximally ();

            Assert.AreEqual (0, story.currentChoices.Count);
        }

        [Test ()]
        public void TestFunctionPurityChecks()
        {
            Ink.Parsed.Story parsedStory = CompileStringWithoutRuntime (@"
-> test

== test ==
~ myFunc()
= function myBadInnerFunc
Not allowed!
~ return


== function myFunc ==
Hello world
* a choice
* another choice
-
-> myFunc
= testStitch
    This is a stitch
~ return
");
            var errors = parsedStory.errors;

            Assert.AreEqual (7,errors.Count);
            Assert.IsTrue(errors[0].Contains("Return statements can only be used in knots that"));
            Assert.IsTrue(errors[1].Contains("Functions cannot be stitches"));
            Assert.IsTrue(errors[2].Contains("Functions may not contain stitches"));
            Assert.IsTrue(errors[3].Contains("Functions may not contain diverts"));
            Assert.IsTrue(errors[4].Contains("Functions may not contain choices"));
            Assert.IsTrue(errors[5].Contains("Functions may not contain choices"));
            Assert.IsTrue(errors[6].Contains("Return statements can only be used in knots that"));
        }


        [Test ()]
        public void TestFunctionCallRestrictions()
        {
            Ink.Parsed.Story parsedStory = CompileStringWithoutRuntime (@"
// Allowed to do this
~ myFunc()

// Not allowed to to this
~ aKnot()

// Not allowed to do this
-> myFunc

== function myFunc ==
This is a function.
~ return

== aKnot ==
This is a normal knot.
-> END
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


            Assert.AreEqual ("Hello world\n", story.Continue());
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

            Assert.AreEqual ("one (1)\none and a half (1.5)\ntwo (2)\nthree (3)\n", story.ContinueMaximally());
        }

        [Test ()]
        public void TestTunnelVsThreadBehaviour()
        {
            Story story = CompileString (@"
-> knot_with_options ->
Finished tunnel.

Starting thread.
<- thread_with_options
* E
-
Done.


== knot_with_options ==
* A
* B
-
->->

== thread_with_options ==
* C
* D
");

            Assert.IsFalse (story.ContinueMaximally ().Contains ("Finished tunnel"));

            // Choices should be A, B
            Assert.AreEqual (2, story.currentChoices.Count);


            story.ChooseChoiceIndex (0);

            // Choices should be C, D, E
            Assert.IsTrue (story.ContinueMaximally ().Contains ("Finished tunnel"));
            Assert.AreEqual (3, story.currentChoices.Count);

            story.ChooseChoiceIndex (2);

            Assert.IsTrue (story.ContinueMaximally ().Contains ("Done."));
        }



        [Test ()]
        public void TestEnd()
        {
            Story story = CompileString (@"
hello
-> END
world
");

            Assert.AreEqual ("hello\n", story.ContinueMaximally());
        }


        [Test ()]
        public void TestEnd2()
        {
            Story story = CompileString (@"
-> test

== test ==
hello
-> END
world
");

            Assert.AreEqual ("hello\n", story.ContinueMaximally());
        }

        [Test ()]
        public void TestThreadDone()
        {
            Story story = CompileString (@"
This is a thread example
<- example_thread
The example is now complete.


== example_thread ==
Hello.
-> DONE
World.
-> DONE
");

            Assert.AreEqual ("This is a thread example\nHello.\nThe example is now complete.\n", story.ContinueMaximally());
        }


        [Test ()]
        public void TestMultiThread()
        {
            Story story = CompileString (@"
== start ==
-> tunnel ->
The end
-> END

== tunnel ==
<- place1
<- place2
-> DONE

== place1 ==
This is place 1.
* choice in place 1
- ->->

== place2 ==
This is place 2.
* choice in place 2
- ->->
");
            Assert.AreEqual ("This is place 1.\nThis is place 2.\n", story.ContinueMaximally());

            story.ChooseChoiceIndex (0);
            Assert.AreEqual ("choice in place 1\nThe end\n", story.ContinueMaximally());
            Assert.IsFalse (story.hasError);
        }

        [Test ()]
        public void TestKnotThreadInteraction()
        {
            Story story = CompileString (@"
=== knot 
    <- threadB
    -> tunnel -> 
    THE END
    -> END

=== tunnel 
    - blah blah 
    * wigwag
    - ->->

=== threadB
    *   option 
    -   something
        -> DONE
");
            
            Assert.AreEqual ("blah blah\n", story.ContinueMaximally());

            Assert.AreEqual (2, story.currentChoices.Count);
            Assert.IsTrue(story.currentChoices[0].choiceText.Contains("option"));
            Assert.IsTrue(story.currentChoices[1].choiceText.Contains("wigwag"));

            story.ChooseChoiceIndex (1);
            Assert.AreEqual ("wigwag\n", story.Continue());
            Assert.AreEqual ("THE END\n", story.Continue());
            Assert.IsFalse (story.hasError);
        }

        [Test ()]
        public void TestKnotThreadInteraction2()
        {
            Story story = CompileString (@"
=== knot 
    <- threadA 
    When should this get printed?
    -> END

=== threadA 
    -> tunnel ->
    Finishing thread.
    -> DONE


=== tunnel
    -   I’m in a tunnel 
    *   I’m an option   
    -   ->->

");

            Assert.AreEqual ("I’m in a tunnel\nWhen should this get printed?\n", story.ContinueMaximally());
            Assert.AreEqual (1, story.currentChoices.Count);
            Assert.AreEqual(story.currentChoices[0].choiceText, "I’m an option");

            story.ChooseChoiceIndex (0);
            Assert.AreEqual ("I’m an option\nFinishing thread.\n", story.ContinueMaximally());
            Assert.IsFalse (story.hasError);
        }

        [Test ()]
        public void TestDivertNotFoundError()
        {
            var parsedStory = CompileStringWithoutRuntime (@"
-> knot

== knot ==
Knot.
-> next
");

            Assert.IsTrue (parsedStory.hadError);
            Assert.IsTrue(parsedStory.errors[0].Contains("not found"));
        }

        [Test ()]
        public void TestVariableDivertTarget()
        {
            var story = CompileString (@"
VAR x = -> here

-> there

== there ==
-> x

== here ==
Here.
-> DONE
");
            Assert.AreEqual ("Here.\n", story.Continue());
        }

        [Test ()]
        public void TestConst()
        {
            var story = CompileString (@"
VAR x = c

CONST c = 5

{x}
");
            Assert.AreEqual ("5\n", story.Continue());
        }

        [Test ()]
        public void TestReadCountAcrossCallstack()
        {
            var story = CompileString (@"
-> first

== first ==
1) Seen first {first} times.
-> second ->
2) Seen first {first} times.
-> DONE

== second ==
In second.
->->
");
            Assert.AreEqual ("1) Seen first 1 times.\nIn second.\n2) Seen first 1 times.\n", story.ContinueMaximally ());
        }

        [Test ()]
        public void TestReadCountAcrossThreads()
        {
            var story = CompileString (@"
=== empty_world ===
    -> top

= top 
    {top}
    <- aside
    {top}
    -> DONE

= aside 
    * {false} DONE
");
            Assert.AreEqual ("1\n1\n", story.ContinueMaximally());
        }


        [Test ()]
        public void TestTurnsSinceNested()
        {
            var story = CompileString (@"
=== empty_world ===
    {TURNS_SINCE(-> then)} = -1
    * (then) stuff
        {TURNS_SINCE(-> then)} = 0
        * * (next) more stuff
            {TURNS_SINCE(-> then)} = 1
        -> DONE
");
            Assert.AreEqual ("-1 = -1\n", story.ContinueMaximally());

            Assert.AreEqual (1, story.currentChoices.Count);
            story.ChooseChoiceIndex (0);

            Assert.AreEqual ("stuff\n0 = 0\n", story.ContinueMaximally());

            Assert.AreEqual (1, story.currentChoices.Count);
            story.ChooseChoiceIndex (0);

            Assert.AreEqual ("more stuff\n1 = 1\n", story.ContinueMaximally());
        }


        [Test ()]
        public void TestEmptySequenceContent()
        {
            var story = CompileString (@"
-> thing ->
-> thing ->
-> thing ->
-> thing ->
-> thing ->
Done.

== thing ==
{once:
  - Wait for it....
  -
  -
  -  Surprise!
}
->->
");
            Assert.AreEqual ("Wait for it....\nSurprise!\nDone.\n", story.ContinueMaximally());
        }

        [Test ()]
        public void TestTurnsSinceWithVariableTarget()
        {
            // Count all visits must be switched on for variable count targets
            var story = CompileString (@"
-> start


=== start ===
    {beats(-> start)}
    {beats(-> start)}
    *   [Choice]  -> next 
= next 
    {beats(-> start)}
    -> END

=== function beats(x) ===
    ~ return TURNS_SINCE(x)
", countAllVisits:true);
            
            Assert.AreEqual ("0\n0\n", story.ContinueMaximally());

            story.ChooseChoiceIndex(0);
            Assert.AreEqual ("1\n", story.ContinueMaximally());
        }


        [Test ()]
        public void TestLiteralUnary()
        {
            var story = CompileString (@"
VAR negativeLiteral = -1
VAR negativeLiteral2 = not not false
VAR negativeLiteral3 = !(0)

{negativeLiteral}
{negativeLiteral2}
{negativeLiteral3}
");
            Assert.AreEqual ("-1\n0\n1\n", story.ContinueMaximally());
        }

        [Test ()]
        public void TestVariableGetSetAPI()
        {
            var story = CompileString (@"
VAR x = 5

{x}

* [choice]
-
{x}

* [choice]
-

{x}

* [choice]
-

{x}

-> DONE
");

            // Initial state
            Assert.AreEqual ("5\n", story.ContinueMaximally());
            Assert.AreEqual (5, story.variablesState["x"]);

            story.variablesState["x"] = 10;
            story.ChooseChoiceIndex(0);
            Assert.AreEqual ("10\n", story.ContinueMaximally());
            Assert.AreEqual (10, story.variablesState["x"]);

            story.variablesState["x"] = 8.5f;
            story.ChooseChoiceIndex(0);
            Assert.AreEqual ("8.5\n", story.ContinueMaximally());
            Assert.AreEqual (8.5f, story.variablesState["x"]);

            story.variablesState["x"] = "a string";
            story.ChooseChoiceIndex(0);
            Assert.AreEqual ("a string\n", story.ContinueMaximally());
            Assert.AreEqual ("a string", story.variablesState["x"]);

            Assert.AreEqual (null, story.variablesState["z"]);

            // Not allowed arbitrary types
            Assert.Throws<StoryException>(() => {
                story.variablesState["x"] = new System.Text.StringBuilder();
            });
        }


        [Test ()]
        public void TestArgumentNameCollisions()
        {
            var parsedStory = CompileStringWithoutRuntime (@"
VAR global_var = 5

~ pass_divert(-> knot_name)
{variable_param_test(10)}

=== function aTarget() ===
   ~ return true

=== function pass_divert(aTarget) === 
    Should be a divert target, but is a read count:- {aTarget}

=== function variable_param_test(global_var) ===
    ~ return global_var

=== knot_name ===
    -> END
");
            //parsedStory.ExportRuntime ();

            Assert.AreEqual (2, parsedStory.errors.Count);
            Assert.IsTrue (parsedStory.errors [0].Contains ("conflicts with a Knot"));
            Assert.IsTrue (parsedStory.errors [1].Contains ("conflicts with existing variable"));

        }



        [Test ()]
        public void TestLogicInChoices()
        {
            var story = CompileString (@"
* 'Hello {name()}[, your name is {name()}.'],' I said, knowing full well that his name was {name()}.
-> DONE

== function name ==
Joe
");

            story.ContinueMaximally ();

            Assert.AreEqual ("'Hello Joe, your name is Joe.'", story.currentChoices[0].choiceText);
            story.ChooseChoiceIndex (0);

            Assert.AreEqual ("'Hello Joe,' I said, knowing full well that his name was Joe.\n", story.ContinueMaximally());
        }

        [Test ()]
        public void TestOnceOnlyChoicesCanLinkBackToSelf()
        {
            var story = CompileString (@"
= opts
*   (firstOpt) [First choice]   ->  opts
*   {firstOpt} [Second choice]  ->  opts
* -> end

- (end)
    -> END
");

            story.ContinueMaximally ();

            Assert.AreEqual (1, story.currentChoices.Count);
            Assert.AreEqual ("First choice", story.currentChoices[0].choiceText);

            story.ChooseChoiceIndex (0);
            story.ContinueMaximally ();

            Assert.AreEqual (1, story.currentChoices.Count);
            Assert.AreEqual ("Second choice", story.currentChoices[0].choiceText);

            story.ChooseChoiceIndex (0);
            story.ContinueMaximally ();

            Assert.AreEqual (null, story.currentErrors);
        }


        [Test ()]
        public void TestVariableTunnel()
        {
            var story = CompileString (@"
-> one_then_tother(-> tunnel)

=== one_then_tother(-> x) ===
    -> x -> end

=== tunnel === 
    STUFF
    ->->

=== end ===
    -> END
");

            Assert.AreEqual ("STUFF\n", story.ContinueMaximally());
        }

        [Test ()]
        public void TestRequireVariableTargetsTyped()
        {
            var parsedStory = CompileStringWithoutRuntime (@"
-> test(-> elsewhere)

== test(varTarget) ==
-> varTarget ->
-> DONE

== elsewhere ==
->->
");
            Assert.AreEqual (1, parsedStory.errors.Count);
            Assert.IsTrue (parsedStory.errors[0].Contains("it should be marked as: ->"));
        }

        [Test ()]
        public void TestIdentifersCanStartWithNumbers()
        {
            var story = CompileString (@"
-> 2tests
== 2tests ==
~ temp 512x2 = 512 * 2
~ temp 512x2p2 = 512x2 + 2
512x2 = {512x2}
512x2p2 = {512x2p2}
-> DONE
");

            Assert.AreEqual ("512x2 = 1024\n512x2p2 = 1026\n", story.ContinueMaximally());
        }


        [Test ()]
        public void TestUnbalancedWeaveIndentation()
        {
            var story = CompileString (@"
* * * First
* * * * Very indented
- - End
-> END
");
            story.ContinueMaximally ();

            Assert.AreEqual (1, story.currentChoices.Count);
            Assert.AreEqual ("First", story.currentChoices[0].choiceText);

            story.ChooseChoiceIndex (0);
            Assert.AreEqual ("First\n", story.ContinueMaximally());
            Assert.AreEqual (1, story.currentChoices.Count);
            Assert.AreEqual ("Very indented", story.currentChoices[0].choiceText);

            story.ChooseChoiceIndex (0);
            Assert.AreEqual ("Very indented\nEnd\n", story.ContinueMaximally());
            Assert.AreEqual (0, story.currentChoices.Count);
        }

        [Test ()]
        public void TestBasicStringLiterals()
        {
            var story = CompileString (@"
VAR x = ""Hello world 1""
{x}
Hello {""world""} 2.
");
            Assert.AreEqual ("Hello world 1\nHello world 2.\n", story.ContinueMaximally());
        }


        [Test ()]
        public void TestQuoteCharacterSignificance()
        {
            // Confusing escaping + ink! Actual ink string is:
            // My name is "{"J{"o"}e"}"
            //  - First and last quotes are insignificant - they're part of the content
            //  - Inner quotes are significant - they're part of the syntax for string expressions
            // So output is: My name is "Joe"
            var story = CompileString (@"My name is ""{""J{""o""}e""}""");
            Assert.AreEqual ("My name is \"Joe\"\n", story.ContinueMaximally());
        }

        [Test ()]
        public void TestStringTypeCoersion()
        {
            var story = CompileString (@"
{""5"" == 5:same|different}
{""blah"" == 5:same|different}
");

            // Not sure that "5" should be equal to 5, but hmm.
            Assert.AreEqual ("same\ndifferent\n", story.ContinueMaximally());
        }

        [Test ()]
        public void TestStringsInChoices()
        {
            var story = CompileString (@"
* \ {""test1""} [""test2 {""test3""}""] {""test4""}
-> DONE
");
            story.ContinueMaximally ();

            Assert.AreEqual (1, story.currentChoices.Count);
            Assert.AreEqual (@" test1 ""test2 test3""", story.currentChoices [0].choiceText);

            story.ChooseChoiceIndex (0);
            Assert.AreEqual (" test1  test4\n", story.Continue());
        }

        [Test ()]
        public void TestEmptyChoice()
        {
            InkParser parser = new InkParser("*");

            int warningCount = 0;
            parser.errorHandler += (string message, int index, int lineIndex, bool isWarning) => {
                if( isWarning ) {
                    warningCount++;
                    Assert.IsTrue (message.Contains ("completely empty"));
                } else {
                    Assert.Fail("Shouldn't have had any errors");
                }
            };
            parser.Parse();

            Assert.AreEqual (1, warningCount);
        }

        [Test ()]
        public void TestTemporariesAtGlobalScope()
        {
            var story = CompileString (@"
VAR x = 5
~ temp y = 4
{x}{y}
");
            Assert.AreEqual ("54\n", story.Continue());
        }


        [Test ()]
        public void TestExternalBinding()
        {
            var story = CompileString (@"
EXTERNAL message(x)
EXTERNAL multiply(x,y)
~ message(""hello world"")
{multiply(5.0, 3)}
");
            string message = null;

            story.BindExternalFunction ("message", (string arg) => {
                message = "MESSAGE: "+arg;
            });

            story.BindExternalFunction ("multiply", (int arg1, float arg2) => {
                return arg1 * arg2;
            });

            
            Assert.AreEqual ("15\n", story.ContinueMaximally());

            Assert.AreEqual ("MESSAGE: hello world", message);
        }

        [Test ()]
        public void TestSameLineDivertIsInline()
        {
            var story = CompileString (@"
=== hurry_home ===
We hurried home to Savile Row -> as_fast_as_we_could
    
=== as_fast_as_we_could ===
as fast as we could.
-> DONE
");
            
            Assert.AreEqual ("We hurried home to Savile Row as fast as we could.\n", story.Continue());
        }

        [Test ()]
        public void TestVariableObserver()
        {
            var story = CompileString (@"
VAR testVar = 5
VAR testVar2 = 10

Hello world!

~ testVar = 15
~ testVar2 = 100

Hello world 2!

* choice

    ~ testVar = 25
    ~ testVar2 = 200

    -> END
");


            int currentVarValue = 0;
            int observerCallCount = 0;

            story.ObserveVariable ("testVar", (string varName, object newValue) => {
                currentVarValue = (int)newValue;
                observerCallCount++;
            });

            story.ContinueMaximally ();

            Assert.AreEqual (15, currentVarValue);
            Assert.AreEqual (1, observerCallCount);
            Assert.AreEqual (1, story.currentChoices.Count);

            story.ChooseChoiceIndex (0);
            story.Continue ();

            Assert.AreEqual (25, currentVarValue);
            Assert.AreEqual (2, observerCallCount);
        }


        [Test ()]
        public void TestVariablePointerRefFromKnot()
        {
            var story = CompileString (@"
VAR val = 5

-> knot ->

-> END

== knot ==
~ inc(val)
{val}
->->

== function inc(ref x) ==
    ~ x = x + 1
");


            Assert.AreEqual ("6\n", story.Continue());
        }

        [Test ()]
        public void TestTunnelOnwardsAfterTunnel()
        {
            var story = CompileString (@"
-> tunnel1 ->
The End.
-> END


== tunnel1 ==
Hello...
-> tunnel2 ->->

== tunnel2 ==
...world.
->->
");


            Assert.AreEqual ("Hello...\n...world.\nThe End.\n", story.ContinueMaximally());
        }

        [Test ()]
        public void TestChoiceDivertsToDone()
        {
            var story = CompileString (@"* choice -> DONE");

            story.Continue ();

            Assert.AreEqual (1, story.currentChoices.Count);
            story.ChooseChoiceIndex (0);

            Assert.AreEqual ("choice\n", story.Continue());
            Assert.IsFalse (story.hasError);
        }

        [Test ()]
        public void TestBlanksInInlineSequences()
        {
            var story = CompileString (@"
1. -> seq1 ->
2. -> seq1 ->
3. -> seq1 ->
4. -> seq1 ->
\---
1. -> seq2 ->
2. -> seq2 ->
3. -> seq2 ->
\---
1. -> seq3 ->
2. -> seq3 ->
3. -> seq3 ->
\---
1. -> seq4 ->
2. -> seq4 ->
3. -> seq4 ->

== seq1 ==
{a||b}
->->

== seq2 ==
{|a}
->->

== seq3 ==
{a|}
->->

== seq4 ==
{|}
->->");

            Assert.AreEqual (
@"1. a
2. 
3. b
4. b
---
1. 
2. a
3. a
---
1. a
2. 
3. 
---
1. 
2. 
3. 
", story.ContinueMaximally());
        }

        [Test ()]
        public void TestDefaultSimpleGather()
        {
            var story = CompileString (@"
* -> 
- x
-> DONE");

            Assert.AreEqual("x\n", story.Continue ());
        }

        [Test ()]
        public void TestKnotDotGather()
        {
            var story = CompileString (@"
=== knot
-> knot.gather
- (gather) g
-> DONE");

            Assert.AreEqual("g\n", story.Continue ());
        }

        [Test ()]
        public void TestImplicitInlineGlue()
        {
            var story = CompileString (@"
I have {five()} eggs.

== function five ==
{false:
    Don't print this
}
five
");

            Assert.AreEqual("I have five eggs.\n", story.Continue ());
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

