using System;
using Ink;
using Ink.Runtime;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Path = Ink.Runtime.Path;

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
        private List<string> _errorMessages = new List<string>();

        private TestMode _mode;

        private bool _testingErrors;

        private List<string> _warningMessages = new List<string>();

        public Tests(TestMode mode)
        {
            _mode = mode;            
            var codeBase = Assembly.GetExecutingAssembly().Location;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
                path = System.IO.Path.GetDirectoryName(path);
            Directory.SetCurrentDirectory(path);            
        }

        [Test()]
        public void TestArgumentNameCollisions()
        {
            CompileStringWithoutRuntime(@"
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
", testingErrors: true);

            Assert.AreEqual(2, _errorMessages.Count);
            Assert.IsTrue(HadError("conflicts with a Knot"));
            Assert.IsTrue(HadError("conflicts with existing variable"));
        }

        [Test()]
        public void TestArgumentShouldntConflictWithGatherElsewhere()
        {
            // Testing that there are no errors only
            CompileStringWithoutRuntime(@"
== knot ==
- (x) -> DONE

== function f(x) ==
Nothing
");
        }

        [Test()]
        public void TestArithmetic()
        {
            Story story = CompileString(@"
{ 2 * 3 + 5 * 6 }
{8 mod 3}
{13 % 5}
{ 7 / 3 }
{ 7 / 3.0 }
{ 10 - 2 }
{ 2 * (5-1) }
");

            Assert.AreEqual("36\n2\n3\n2\n2"+System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator+"333333\n8\n8\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestBasicStringLiterals()
        {
            var story = CompileString(@"
VAR x = ""Hello world 1""
{x}
Hello {""world""} 2.
");
            Assert.AreEqual("Hello world 1\nHello world 2.\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestBasicTunnel()
        {
            Story story = CompileString(@"
-> f ->
<> world

== f ==
Hello
->->
");

            Assert.AreEqual("Hello world\n", story.Continue());
        }

        [Test()]
        public void TestBlanksInInlineSequences()
        {
            var story = CompileString(@"
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
->->".Replace("\r", ""));

            Assert.AreEqual(
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
".Replace("\r", ""), story.ContinueMaximally().Replace("\r", ""));
        }

        [Test()]
        public void TestCallStackEvaluation()
        {
            var storyStr =
                @"
                   { six() + two() }
                    -> END

                === function six
                    ~ return four() + two()

                === function four
                    ~ return two() + two()

                === function two
                    ~ return 2
                ";

            Story story = CompileString(storyStr);
            Assert.AreEqual("8\n", story.Continue());
        }

        [Test()]
        public void TestChoiceCount()
        {
            Story story = CompileString(@"
* one -> end
* two -> end
{ CHOICE_COUNT() }

= end
-> END
");

            Assert.AreEqual("2\n", story.Continue());
        }

        [Test()]
        public void TestChoiceDivertsToDone()
        {
            var story = CompileString(@"* choice -> DONE");

            story.Continue();

            Assert.AreEqual(1, story.currentChoices.Count);
            story.ChooseChoiceIndex(0);

            Assert.AreEqual("choice\n", story.Continue());
            Assert.IsFalse(story.hasError);
        }

        [Test()]
        public void TestChoiceWithBracketsOnly()
        {
            var storyStr = "*   [Option]\n    Text";

            Story story = CompileString(storyStr);
            story.Continue();

            Assert.AreEqual(1, story.currentChoices.Count);
            Assert.AreEqual("Option", story.currentChoices[0].text);

            story.ChooseChoiceIndex(0);

            Assert.AreEqual("Text\n", story.Continue());
        }

        [Test()]
        public void TestCommentEliminator()
        {
            var testContent =
@"A// C
A /* C */ A

A * A * /* * C *// A/*
C C C

*/";

            CommentEliminator p = new CommentEliminator(testContent);
            var result = p.Process();

            var expected = "A\nA  A\n\nA * A * / A\n\n\n";

            Assert.AreEqual(expected.Replace("\r", ""), result.Replace("\r", "")); //Windows perculiarity
        }

        //------------------------------------------------------------------------
        [Test()]
        public void TestCommentEliminatorMixedNewlines()
        {
            var testContent =
                "A B\nC D // comment\nA B\r\nC D // comment\r\n/* block comment\r\nsecond line\r\n */ ";

            CommentEliminator p = new CommentEliminator(testContent);
            var result = p.Process();

            var expected =
                "A B\nC D \nA B\nC D \n\n\n ";

            Assert.AreEqual(expected, result);
        }

        [Test()]
        public void TestCompareDivertTargets()
        {
            var storyStr = @"
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
    -> DONE

=== two
    Two
    -> DONE";

            Story story = CompileString(storyStr);

            Assert.AreEqual("different knot\nsame knot\nsame knot\ndifferent knot\nsame knot\nsame knot\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestComplexTunnels()
        {
            Story story = CompileString(@"
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

            Assert.AreEqual("one (1)\none and a half (1"+ System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator+"5)\ntwo (2)\nthree (3)\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestConditionalChoiceInWeave()
        {
            var storyStr =
                @"
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

            Story story = CompileString(storyStr);

            // Extra newline is because there's a choice object sandwiched there,
            // so it can't be absorbed :-/
            Assert.AreEqual("start\n", story.Continue());
            Assert.AreEqual(1, story.currentChoices.Count);

            story.ChooseChoiceIndex(0);

            Assert.AreEqual("result\n", story.Continue());
        }

        [Test()]
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

            Story story = CompileString(storyStr);

            Assert.AreEqual("first gather\n", story.Continue());

            Assert.AreEqual(2, story.currentChoices.Count);

            story.ChooseChoiceIndex(0);

            Assert.AreEqual("option 1\nthe main gather\n", story.ContinueMaximally());
            Assert.AreEqual(0, story.currentChoices.Count);
        }

        [Test()]
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

            Story story = CompileString(storyStr);
            story.ContinueMaximally();

            Assert.AreEqual(4, story.currentChoices.Count);
            Assert.AreEqual("one", story.currentChoices[0].text);
            Assert.AreEqual("two", story.currentChoices[1].text);
            Assert.AreEqual("three", story.currentChoices[2].text);
            Assert.AreEqual("four", story.currentChoices[3].text);
        }

        [Test()]
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

            Story story = CompileString(storyStr);

            Assert.AreEqual("true\ntrue\ntrue\ntrue\ntrue\ngreat\nright?\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestConst()
        {
            var story = CompileString(@"
VAR x = c

CONST c = 5

{x}
");
            Assert.AreEqual("5\n", story.Continue());
        }

        [Test()]
        public void TestDefaultChoices()
        {
            Story story = CompileString(@"
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

            Assert.AreEqual("", story.Continue());
            Assert.AreEqual(2, story.currentChoices.Count);

            story.ChooseChoiceIndex(0);
            Assert.AreEqual("After choice\n", story.Continue());

            Assert.AreEqual(1, story.currentChoices.Count);

            story.ChooseChoiceIndex(0);
            Assert.AreEqual("After choice\nThis is default.\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestDefaultSimpleGather()
        {
            var story = CompileString(@"
* ->
- x
-> DONE");

            Assert.AreEqual("x\n", story.Continue());
        }

        [Test()]
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

            Story story = CompileString(storyStr);
            Assert.AreEqual("", story.ContinueMaximally());
        }

        [Test()]
        public void TestDivertNotFoundError()
        {
            CompileStringWithoutRuntime(@"
-> knot

== knot ==
Knot.
-> next
", testingErrors: true);

            Assert.IsTrue(HadError("not found"));
        }

        [Test()]
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

            Story story = CompileString(storyStr);

            Assert.AreEqual("gather\ntest\nchoice content\ngather\nsecond time round\n", story.ContinueMaximally());
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

            Story story = CompileString(storyStr);

            Assert.AreEqual("other\nother\nother\nother\n", story.currentText);
        }

        [Test()]
        public void TestEmpty()
        {
            Story story = CompileString(@"");

            Assert.AreEqual(string.Empty, story.currentText);
        }

        [Test()]
        public void TestEmptyChoice()
        {
            int warningCount = 0;
            InkParser parser = new InkParser("*", null, (string message, ErrorType errorType) =>
            {
                if (errorType == ErrorType.Warning)
                {
                    warningCount++;
                    Assert.IsTrue(message.Contains("completely empty"));
                }
                else
                {
                    Assert.Fail("Shouldn't have had any errors");
                }
            });

            parser.Parse();

            Assert.AreEqual(1, warningCount);
        }

        [Test()]
        public void TestEmptyMultilineConditionalBranch()
        {
            var story = CompileString(@"
{ 3:
    - 3:
    - 4:
        txt
}
");

            Assert.AreEqual("", story.Continue());
        }

        [Test()]
        public void TestEmptySequenceContent()
        {
            var story = CompileString(@"
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
            Assert.AreEqual("Wait for it....\nSurprise!\nDone.\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestEnd()
        {
            Story story = CompileString(@"
hello
-> END
world
-> END
");

            Assert.AreEqual("hello\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestEnd2()
        {
            Story story = CompileString(@"
-> test

== test ==
hello
-> END
world
-> END
");

            Assert.AreEqual("hello\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestEndOfContent()
        {
            Story story = CompileString("Hello world", false, true);
            story.ContinueMaximally();
            Assert.IsFalse(HadError());

            story = CompileString("== test ==\nContent\n-> END");
            story.ContinueMaximally();
            Assert.IsFalse(story.hasError);

            // Should have runtime error due to running out of content
            // (needs a -> END)
            story = CompileString("== test ==\nContent", false, true);
            story.ContinueMaximally();
            Assert.IsTrue(HadWarning());

            // Should have warning that there's no "-> END"
            CompileStringWithoutRuntime("== test ==\nContent", true);
            Assert.IsFalse(HadError());
            Assert.IsTrue(HadWarning());

            CompileStringWithoutRuntime("== test ==\n~return", testingErrors: true);
            Assert.IsTrue(HadError("Return statements can only be used in knots that are declared as functions"));

            CompileStringWithoutRuntime("== function test ==\n-> END", testingErrors: true);
            Assert.IsTrue(HadError("Functions may not contain diverts"));
        }

        [Test()]
        public void TestEscapeCharacter()
        {
            var storyStr = @"{true:this is a '\|' character|this isn't}";

            Story story = CompileString(storyStr);

            Assert.AreEqual("this is a '|' character\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestExternalBinding()
        {
            var story = CompileString(@"
EXTERNAL message(x)
EXTERNAL multiply(x,y)
EXTERNAL times(i,str)
~ message(""hello world"")
{multiply(5.0, 3)}
{times(3, ""knock "")}
");
            string message = null;

            story.BindExternalFunction("message", (string arg) =>
            {
                message = "MESSAGE: " + arg;
            });

            story.BindExternalFunction("multiply", (float arg1, int arg2) =>
            {
                return arg1 * arg2;
            });

            story.BindExternalFunction("times", (int numberOfTimes, string str) =>
            {
                string result = "";
                for (int i = 0; i < numberOfTimes; i++)
                {
                    result += str;
                }
                return result;
            });

            Assert.AreEqual("15\n", story.Continue());

            Assert.AreEqual("knock knock knock \n", story.Continue());

            Assert.AreEqual("MESSAGE: hello world", message);
        }

        [Test()]
        public void TestFactorialByReference()
        {
            var storyStr = @"
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

            Story story = CompileString(storyStr);

            Assert.AreEqual("120\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestFactorialRecursive()
        {
            var storyStr = @"
{ factorial(5) }

== function factorial(n) ==
 { n == 1:
    ~ return 1
 - else:
    ~ return (n * factorial(n-1))
 }
";

            Story story = CompileString(storyStr);

            Assert.AreEqual("120\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestFunctionCallRestrictions()
        {
            CompileStringWithoutRuntime(@"
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
", testingErrors: true);

            Assert.AreEqual(2, _errorMessages.Count);
            Assert.IsTrue(_errorMessages[0].Contains("hasn't been marked as a function"));
            Assert.IsTrue(_errorMessages[1].Contains("can only be called as a function"));
        }

        [Test()]
        public void TestFunctionPurityChecks()
        {
            CompileStringWithoutRuntime(@"
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
", testingErrors: true);

            Assert.AreEqual(7, _errorMessages.Count);
            Assert.IsTrue(_errorMessages[0].Contains("Return statements can only be used in knots that"));
            Assert.IsTrue(_errorMessages[1].Contains("Functions cannot be stitches"));
            Assert.IsTrue(_errorMessages[2].Contains("Functions may not contain stitches"));
            Assert.IsTrue(_errorMessages[3].Contains("Functions may not contain diverts"));
            Assert.IsTrue(_errorMessages[4].Contains("Functions may not contain choices"));
            Assert.IsTrue(_errorMessages[5].Contains("Functions may not contain choices"));
            Assert.IsTrue(_errorMessages[6].Contains("Return statements can only be used in knots that"));
        }

        [Test()]
        public void TestGatherAtFlowEnd()
        {
            // The final "->" doesn't have anywhere to go, so it should
            // happily just go to the end of the flow.
            var storyStr = "- nothing ->";

            Story story = CompileString(storyStr);

            // Hrm: terminating space is a little bit silly
            // (it's because the divert arrow forces a little bit of
            // whitespace in case you're diverting straight into another line)
            Assert.AreEqual("nothing ", story.ContinueMaximally());
        }

        [Test()]
        public void TestGatherChoiceSameLine()
        {
            var storyStr = "- * hello\n- * world";

            Story story = CompileString(storyStr);
            story.Continue();

            Assert.AreEqual("hello", story.currentChoices[0].text);

            story.ChooseChoiceIndex(0);
            story.Continue();

            Assert.AreEqual("world", story.currentChoices[0].text);
        }

        [Test()]
        public void TestGatherReadCountWithInitialSequence()
        {
            var story = CompileString(@"
- (opts)
{test:seen test}
- (test)
{ -> opts |}
");

            Assert.AreEqual("seen test\n", story.Continue());
        }

        [Test()]
        public void TestHasReadOnChoice()
        {
            var storyStr =
                @"
* { not test } visible choice
* { test } visible choice

== test ==
-> END
                ";

            Story story = CompileString(storyStr);
            story.ContinueMaximally();

            Assert.AreEqual(1, story.currentChoices.Count);
            Assert.AreEqual("visible choice", story.currentChoices[0].text);
        }

        [Test()]
        public void TestHelloWorld()
        {
            Story story = CompileString("Hello world");
            Assert.AreEqual("Hello world\n", story.Continue());
        }

        [Test()]
        public void TestIdentifersCanStartWithNumbers()
        {
            var story = CompileString(@"
-> 2tests
== 2tests ==
~ temp 512x2 = 512 * 2
~ temp 512x2p2 = 512x2 + 2
512x2 = {512x2}
512x2p2 = {512x2p2}
-> DONE
");

            Assert.AreEqual("512x2 = 1024\n512x2p2 = 1026\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestImplicitInlineGlue()
        {
            var story = CompileString(@"
I have {five()} eggs.

== function five ==
{false:
    Don't print this
}
five
");

            Assert.AreEqual("I have five eggs.\n", story.Continue());
        }

        [Test ()]
        public void TestImplicitInlineGlueB ()
        {
            var story = CompileString (@"
A {f():B} 
X

=== function f() ===
{true: 
    ~ return false
}
");

            Assert.AreEqual ("A \nX\n", story.ContinueMaximally ());
        }

        [Test ()]
        public void TestImplicitInlineGlueC ()
        {
            var story = CompileString (@"
A
{f():X}
C

=== function f()
{ true: 
    ~ return false
}
");

            Assert.AreEqual ("A\nC\n", story.ContinueMaximally ());
        }

        [Test()]
        public void TestInclude()
        {
            var storyStr =
                @"
INCLUDE test_included_file.ink
  INCLUDE test_included_file2.ink

This is the main file.
                ";

            Story story = CompileString(storyStr);
            Assert.AreEqual("This is include 1.\nThis is include 2.\nThis is the main file.\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestIncrement()
        {
            Story story = CompileString(@"
VAR x = 5
~ x++
{x}

~ x--
{x}
");

            Assert.AreEqual("6\n5\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestKnotDotGather()
        {
            var story = CompileString(@"
-> knot
=== knot
-> knot.gather
- (gather) g
-> DONE");

            Assert.AreEqual("g\n", story.Continue());
        }

        // Although VAR and CONST declarations are parsed as being
        // part of the knot, they're extracted, so that the null
        // termination detection shouldn't see this as a loose end.
        [Test()]
        public void TestKnotTerminationSkipsGlobalObjects()
        {
            CompileStringWithoutRuntime(@"
=== stuff ===
-> END

VAR X = 1
CONST Y = 2
", testingErrors: true);

            Assert.IsTrue(_warningMessages.Count == 0);
        }

        [Test()]
        public void TestKnotThreadInteraction()
        {
            Story story = CompileString(@"
-> knot
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

            Assert.AreEqual("blah blah\n", story.ContinueMaximally());

            Assert.AreEqual(2, story.currentChoices.Count);
            Assert.IsTrue(story.currentChoices[0].text.Contains("option"));
            Assert.IsTrue(story.currentChoices[1].text.Contains("wigwag"));

            story.ChooseChoiceIndex(1);
            Assert.AreEqual("wigwag\n", story.Continue());
            Assert.AreEqual("THE END\n", story.Continue());
            Assert.IsFalse(story.hasError);
        }

        [Test()]
        public void TestKnotThreadInteraction2()
        {
            Story story = CompileString(@"
-> knot
=== knot
    <- threadA
    When should this get printed?
    -> DONE

=== threadA
    -> tunnel ->
    Finishing thread.
    -> DONE

=== tunnel
    -   I’m in a tunnel
    *   I’m an option
    -   ->->

");

            Assert.AreEqual("I’m in a tunnel\nWhen should this get printed?\n", story.ContinueMaximally());
            Assert.AreEqual(1, story.currentChoices.Count);
            Assert.AreEqual(story.currentChoices[0].text, "I’m an option");

            story.ChooseChoiceIndex(0);
            Assert.AreEqual("I’m an option\nFinishing thread.\n", story.ContinueMaximally());
            Assert.IsFalse(story.hasError);
        }

        [Test()]
        public void TestLeadingNewlineMultilineSequence()
        {
            var story = CompileString(@"
{stopping:

- a line after an empty line
- blah
}
");

            Assert.AreEqual("a line after an empty line\n", story.Continue());
        }

        [Test()]
        public void TestLiteralUnary()
        {
            var story = CompileString(@"
VAR negativeLiteral = -1
VAR negativeLiteral2 = not not false
VAR negativeLiteral3 = !(0)

{negativeLiteral}
{negativeLiteral2}
{negativeLiteral3}
");
            Assert.AreEqual("-1\n0\n1\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestLogicInChoices()
        {
            var story = CompileString(@"
* 'Hello {name()}[, your name is {name()}.'],' I said, knowing full well that his name was {name()}.
-> DONE

== function name ==
Joe
");

            story.ContinueMaximally();

            Assert.AreEqual("'Hello Joe, your name is Joe.'", story.currentChoices[0].text);
            story.ChooseChoiceIndex(0);

            Assert.AreEqual("'Hello Joe,' I said, knowing full well that his name was Joe.\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestMultipleConstantReferences()
        {
            var story = CompileString(@"
CONST CONST_STR = ""ConstantString""
VAR varStr = CONST_STR
{varStr == CONST_STR:success}
");

            Assert.AreEqual("success\n", story.Continue());
        }

        [Test()]
        public void TestMultiThread()
        {
            Story story = CompileString(@"
-> start
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
            Assert.AreEqual("This is place 1.\nThis is place 2.\n", story.ContinueMaximally());

            story.ChooseChoiceIndex(0);
            Assert.AreEqual("choice in place 1\nThe end\n", story.ContinueMaximally());
            Assert.IsFalse(story.hasError);
        }

        [Test()]
        public void TestNestedInclude()
        {
            var storyStr =
                @"
INCLUDE test_included_file3.ink

This is the main file

-> knot_in_2
                ";

            Story story = CompileString(storyStr);
            Assert.AreEqual("The value of a variable in test file 2 is 5.\nThis is the main file\nThe value when accessed from knot_in_2 is 5.\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestNestedPassByReference()
        {
            var storyStr = @"
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

            Story story = CompileString(storyStr);

            // Bloody whitespace
            Assert.AreEqual("5\n \n625\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestNonTextInChoiceInnerContent()
        {
            var storyStr =
                @"
-> knot
== knot
   *   option text[]. {true: Conditional bit.} -> next
   -> DONE

== next
    Next.
    -> DONE
                ";

            Story story = CompileString(storyStr);
            story.Continue();

            story.ChooseChoiceIndex(0);
            Assert.AreEqual("option text. Conditional bit.\n", story.Continue());
            Assert.AreEqual("Next.\n", story.Continue());
        }

        [Test()]
        public void TestOnceOnlyChoicesCanLinkBackToSelf()
        {
            var story = CompileString(@"
-> opts
= opts
*   (firstOpt) [First choice]   ->  opts
*   {firstOpt} [Second choice]  ->  opts
* -> end

- (end)
    -> END
");

            story.ContinueMaximally();

            Assert.AreEqual(1, story.currentChoices.Count);
            Assert.AreEqual("First choice", story.currentChoices[0].text);

            story.ChooseChoiceIndex(0);
            story.ContinueMaximally();

            Assert.AreEqual(1, story.currentChoices.Count);
            Assert.AreEqual("Second choice", story.currentChoices[0].text);

            story.ChooseChoiceIndex(0);
            story.ContinueMaximally();

            Assert.AreEqual(null, story.currentErrors);
        }

        [Test()]
        public void TestOnceOnlyChoicesWithOwnContent()
        {
            Story story = CompileString(@"
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

            story.ContinueMaximally();

            Assert.AreEqual(3, story.currentChoices.Count);

            story.ChooseChoiceIndex(0);
            story.ContinueMaximally();

            Assert.AreEqual(2, story.currentChoices.Count);

            story.ChooseChoiceIndex(0);
            story.ContinueMaximally();

            Assert.AreEqual(1, story.currentChoices.Count);

            story.ChooseChoiceIndex(0);
            story.ContinueMaximally();

            Assert.AreEqual(0, story.currentChoices.Count);
        }

        [Test()]
        public void TestPaths()
        {
            // Different instances should ensure different instances of individual components
            var path1 = new Path("hello.1.world");
            var path2 = new Path("hello.1.world");

            var path3 = new Path(".hello.1.world");
            var path4 = new Path(".hello.1.world");

            Assert.AreEqual(path1, path2);

            Assert.AreEqual(path3, path4);

            Assert.AreNotEqual(path1, path3);
        }

        [Test()]
        public void TestPathToSelf()
        {
            var story = CompileString(@"
- (dododo)
-> tunnel ->
-> dododo

== tunnel
+ A
->->
");
            // We're only checking that the story copes
            // okay without crashing
            // (internally the "-> dododo" ends up generating
            //  a very short path: ".^", and after walking into
            // the parent, it didn't cope with the "." before
            // I fixed it!)
            story.Continue();
            story.ChooseChoiceIndex(0);
            story.Continue();
            story.ChooseChoiceIndex(0);
        }

        [Test()]
        public void TestPrintNum()
        {
            var story = CompileString(@"
. {print_num(4)} .
. {print_num(15)} .
. {print_num(37)} .
. {print_num(101)} .
. {print_num(222)} .
. {print_num(1234)} .

=== function print_num(x) ===
{
    - x >= 1000:
        {print_num(x / 1000)} thousand { x mod 1000 > 0:{print_num(x mod 1000)}}
    - x >= 100:
        {print_num(x / 100)} hundred { x mod 100 > 0:and {print_num(x mod 100)}}
    - x == 0:
        zero
    - else:
        { x >= 20:
            { x / 10:
                - 2: twenty
                - 3: thirty
                - 4: forty
                - 5: fifty
                - 6: sixty
                - 7: seventy
                - 8: eighty
                - 9: ninety
            }
            { x mod 10 > 0:<>-<>}
        }
        { x < 10 || x > 20:
            { x mod 10:
                - 1: one
                - 2: two
                - 3: three
                - 4: four
                - 5: five
                - 6: six
                - 7: seven
                - 8: eight
                - 9: nine
            }
        - else:
            { x:
                - 10: ten
                - 11: eleven
                - 12: twelve
                - 13: thirteen
                - 14: fourteen
                - 15: fifteen
                - 16: sixteen
                - 17: seventeen
                - 18: eighteen
                - 19: nineteen
            }
        }
}
");

            Assert.AreEqual(
@". four .
. fifteen .
. thirty-seven .
. one hundred and one .
. two hundred and twenty-two .
. one thousand two hundred and thirty-four .
".Replace("\r", ""), story.ContinueMaximally().Replace("\r", ""));
        }

        [Test()]
        public void TestQuoteCharacterSignificance()
        {
            // Confusing escaping + ink! Actual ink string is:
            // My name is "{"J{"o"}e"}"
            //  - First and last quotes are insignificant - they're part of the content
            //  - Inner quotes are significant - they're part of the syntax for string expressions
            // So output is: My name is "Joe"
            var story = CompileString(@"My name is ""{""J{""o""}e""}""");
            Assert.AreEqual("My name is \"Joe\"\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestReadCountAcrossCallstack()
        {
            var story = CompileString(@"
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
            Assert.AreEqual("1) Seen first 1 times.\nIn second.\n2) Seen first 1 times.\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestReadCountAcrossThreads()
        {
            var story = CompileString(@"
    -> top

= top
    {top}
    <- aside
    {top}
    -> DONE

= aside
    * {false} DONE
");
            Assert.AreEqual("1\n1\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestReadCountDotSeparatedPath()
        {
            Story story = CompileString(@"
-> hi ->
-> hi ->
-> hi ->

{ hi.stitch_to_count }

== hi ==
= stitch_to_count
hi
->->
");

            Assert.AreEqual("hi\nhi\nhi\n3\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestRequireVariableTargetsTyped()
        {
            CompileStringWithoutRuntime(@"
-> test(-> elsewhere)

== test(varTarget) ==
-> varTarget ->
-> DONE

== elsewhere ==
->->
", testingErrors: true);
            Assert.IsTrue(HadError("it should be marked as: ->"));
        }

        [Test()]
        public void TestReturnTextWarning()
        {
            InkParser parser = new InkParser("== test ==\n return something",
                null,
                (string message, ErrorType errorType) =>
                {
                    if (errorType == ErrorType.Warning)
                    {
                        throw new TestWarningException();
                    }
                });

            Assert.Throws<TestWarningException>(() => parser.Parse());
        }

        [Test()]
        public void TestSameLineDivertIsInline()
        {
            var story = CompileString(@"
-> hurry_home
=== hurry_home ===
We hurried home to Savile Row -> as_fast_as_we_could

=== as_fast_as_we_could ===
as fast as we could.
-> DONE
");

            Assert.AreEqual("We hurried home to Savile Row as fast as we could.\n", story.Continue());
        }

        [Test()]
        public void TestShouldntGatherDueToChoice()
        {
            Story story = CompileString(@"
* opt
    - - text
    * * {false} impossible
- gather");

            story.ContinueMaximally();
            story.ChooseChoiceIndex(0);

            // Shouldn't go to "gather"
            Assert.AreEqual("opt\ntext\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestShuffleStackMuddying()
        {
            var story = CompileString (@"
* {condFunc()} [choice 1]
* {condFunc()} [choice 2]
* {condFunc()} [choice 3]
* {condFunc()} [choice 4]


=== function condFunc() ===
{shuffle:
    - ~ return false
    - ~ return true
    - ~ return true
    - ~ return false
}
");

            story.Continue ();

            Assert.AreEqual (2, story.currentChoices.Count);
        }

        [Test()]
        public void TestSimpleGlue()
        {
            var storyStr = "Some <> \ncontent<> with glue.\n";

            Story story = CompileString(storyStr);

            Assert.AreEqual("Some content with glue.\n", story.Continue());
        }

        [Test()]
        public void TestStickyChoicesStaySticky()
        {
            var story = CompileString(@"
-> test
== test ==
First line.
Second line.
+ Choice 1
+ Choice 2
- -> test
");

            story.ContinueMaximally();
            Assert.AreEqual(2, story.currentChoices.Count);

            story.ChooseChoiceIndex(0);
            story.ContinueMaximally();
            Assert.AreEqual(2, story.currentChoices.Count);
        }

        [Test()]
        public void TestStringConstants()
        {
            var story = CompileString(@"
{x}
VAR x = kX
CONST kX = ""hi""
");

            Assert.AreEqual("hi\n", story.Continue());
        }

        [Test()]
        public void TestStringParserA()
        {
            StringParser p = new StringParser("A");
            var results = p.Interleave<string>(
                () => p.ParseString("A"),
                () => p.ParseString("B"));

            var expected = new[] { "A" };
            Assert.AreEqual(expected, results);
        }

        [Test()]
        public void TestStringParserABAB()
        {
            StringParser p = new StringParser("ABAB");
            var results = p.Interleave<string>(
                () => p.ParseString("A"),
                () => p.ParseString("B"));

            var expected = new[] { "A", "B", "A", "B" };
            Assert.AreEqual(expected, results);
        }

        [Test()]
        public void TestStringParserABAOptional()
        {
            StringParser p = new StringParser("ABAA");
            var results = p.Interleave<string>(
                () => p.ParseString("A"),
                p.Optional(() => p.ParseString("B")));

            var expected = new[] { "A", "B", "A", "A" };
            Assert.AreEqual(expected, results);
        }

        [Test()]
        public void TestStringParserABAOptional2()
        {
            StringParser p = new StringParser("BABB");
            var results = p.Interleave<string>(
                p.Optional(() => p.ParseString("A")),
                () => p.ParseString("B"));

            var expected = new[] { "B", "A", "B", "B" };
            Assert.AreEqual(expected, results);
        }

        [Test()]
        public void TestStringParserB()
        {
            StringParser p = new StringParser("B");
            var result = p.Interleave<string>(
                () => p.ParseString("A"),
                () => p.ParseString("B"));

            Assert.IsNull(result);
        }

        [Test()]
        public void TestStringsInChoices()
        {
            var story = CompileString(@"
* \ {""test1""} [""test2 {""test3""}""] {""test4""}
-> DONE
");
            story.ContinueMaximally();

            Assert.AreEqual(1, story.currentChoices.Count);
            Assert.AreEqual(@" test1 ""test2 test3""", story.currentChoices[0].text);

            story.ChooseChoiceIndex(0);
            Assert.AreEqual(" test1  test4\n", story.Continue());
        }

        [Test()]
        public void TestStringTypeCoersion()
        {
            var story = CompileString(@"
{""5"" == 5:same|different}
{""blah"" == 5:same|different}
");

            // Not sure that "5" should be equal to 5, but hmm.
            Assert.AreEqual("same\ndifferent\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestTemporariesAtGlobalScope()
        {
            var story = CompileString(@"
VAR x = 5
~ temp y = 4
{x}{y}
");
            Assert.AreEqual("54\n", story.Continue());
        }

        [Test()]
        public void TestThreadDone()
        {
            Story story = CompileString(@"
This is a thread example
<- example_thread
The example is now complete.

== example_thread ==
Hello.
-> DONE
World.
-> DONE
");

            Assert.AreEqual("This is a thread example\nHello.\nThe example is now complete.\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestTunnelOnwardsAfterTunnel()
        {
            var story = CompileString(@"
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

            Assert.AreEqual("Hello...\n...world.\nThe End.\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestTunnelVsThreadBehaviour()
        {
            Story story = CompileString(@"
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

            Assert.IsFalse(story.ContinueMaximally().Contains("Finished tunnel"));

            // Choices should be A, B
            Assert.AreEqual(2, story.currentChoices.Count);

            story.ChooseChoiceIndex(0);

            // Choices should be C, D, E
            Assert.IsTrue(story.ContinueMaximally().Contains("Finished tunnel"));
            Assert.AreEqual(3, story.currentChoices.Count);

            story.ChooseChoiceIndex(2);

            Assert.IsTrue(story.ContinueMaximally().Contains("Done."));
        }

        [Test()]
        public void TestTurnsSince()
        {
            Story story = CompileString(@"
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
            Assert.AreEqual("-1\n0\n", story.ContinueMaximally());

            story.ChooseChoiceIndex(0);
            Assert.AreEqual("1\n", story.ContinueMaximally());

            story.ChooseChoiceIndex(0);
            Assert.AreEqual("2\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestTurnsSinceNested()
        {
            var story = CompileString(@"
-> empty_world
=== empty_world ===
    {TURNS_SINCE(-> then)} = -1
    * (then) stuff
        {TURNS_SINCE(-> then)} = 0
        * * (next) more stuff
            {TURNS_SINCE(-> then)} = 1
        -> DONE
");
            Assert.AreEqual("-1 = -1\n", story.ContinueMaximally());

            Assert.AreEqual(1, story.currentChoices.Count);
            story.ChooseChoiceIndex(0);

            Assert.AreEqual("stuff\n0 = 0\n", story.ContinueMaximally());

            Assert.AreEqual(1, story.currentChoices.Count);
            story.ChooseChoiceIndex(0);

            Assert.AreEqual("more stuff\n1 = 1\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestTurnsSinceWithVariableTarget()
        {
            // Count all visits must be switched on for variable count targets
            var story = CompileString(@"
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
", countAllVisits: true);

            Assert.AreEqual("0\n0\n", story.ContinueMaximally());

            story.ChooseChoiceIndex(0);
            Assert.AreEqual("1\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestUnbalancedWeaveIndentation()
        {
            var story = CompileString(@"
* * * First
* * * * Very indented
- - End
-> END
");
            story.ContinueMaximally();

            Assert.AreEqual(1, story.currentChoices.Count);
            Assert.AreEqual("First", story.currentChoices[0].text);

            story.ChooseChoiceIndex(0);
            Assert.AreEqual("First\n", story.ContinueMaximally());
            Assert.AreEqual(1, story.currentChoices.Count);
            Assert.AreEqual("Very indented", story.currentChoices[0].text);

            story.ChooseChoiceIndex(0);
            Assert.AreEqual("Very indented\nEnd\n", story.ContinueMaximally());
            Assert.AreEqual(0, story.currentChoices.Count);
        }

        [Test()]
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

            Story story = CompileString(storyStr);

            // Extra newline is because there's a choice object sandwiched there,
            // so it can't be absorbed :-/
            Assert.AreEqual("5\n", story.Continue());
        }

        [Test()]
        public void TestVariableDivertTarget()
        {
            var story = CompileString(@"
VAR x = -> here

-> there

== there ==
-> x

== here ==
Here.
-> DONE
");
            Assert.AreEqual("Here.\n", story.Continue());
        }

        [Test()]
        public void TestVariableGetSetAPI()
        {
            var story = CompileString(@"
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
            Assert.AreEqual("5\n", story.ContinueMaximally());
            Assert.AreEqual(5, story.variablesState["x"]);

            story.variablesState["x"] = 10;
            story.ChooseChoiceIndex(0);
            Assert.AreEqual("10\n", story.ContinueMaximally());
            Assert.AreEqual(10, story.variablesState["x"]);

            story.variablesState["x"] = 8.5f;
            story.ChooseChoiceIndex(0);
            Assert.AreEqual("8"+ System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator+"5\n", story.ContinueMaximally());
            Assert.AreEqual(8.5f, story.variablesState["x"]);

            story.variablesState["x"] = "a string";
            story.ChooseChoiceIndex(0);
            Assert.AreEqual("a string\n", story.ContinueMaximally());
            Assert.AreEqual("a string", story.variablesState["x"]);

            Assert.AreEqual(null, story.variablesState["z"]);

            // Not allowed arbitrary types
            Assert.Throws<StoryException>(() =>
            {
                story.variablesState["x"] = new System.Text.StringBuilder();
            });
        }

        [Test()]
        public void TestVariableObserver()
        {
            var story = CompileString(@"
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

            story.ObserveVariable("testVar", (string varName, object newValue) =>
            {
                currentVarValue = (int)newValue;
                observerCallCount++;
            });

            story.ContinueMaximally();

            Assert.AreEqual(15, currentVarValue);
            Assert.AreEqual(1, observerCallCount);
            Assert.AreEqual(1, story.currentChoices.Count);

            story.ChooseChoiceIndex(0);
            story.Continue();

            Assert.AreEqual(25, currentVarValue);
            Assert.AreEqual(2, observerCallCount);
        }

        [Test()]
        public void TestVariablePointerRefFromKnot()
        {
            var story = CompileString(@"
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

            Assert.AreEqual("6\n", story.Continue());
        }

        [Test()]
        public void TestVariableSwapRecurse()
        {
            var storyStr = @"
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

            Story story = CompileString(storyStr);

            Assert.AreEqual("1 2\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestVariableTunnel()
        {
            var story = CompileString(@"
-> one_then_tother(-> tunnel)

=== one_then_tother(-> x) ===
    -> x -> end

=== tunnel ===
    STUFF
    ->->

=== end ===
    -> END
");

            Assert.AreEqual("STUFF\n", story.ContinueMaximally());
        }

        [Test()]
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

            Story story = CompileString(storyStr);

            story.ContinueMaximally();

            Assert.AreEqual(2, story.currentChoices.Count);
            Assert.AreEqual("one", story.currentChoices[0].text);
            Assert.AreEqual("four", story.currentChoices[1].text);

            story.ChooseChoiceIndex(0);
            story.ContinueMaximally();

            Assert.AreEqual(1, story.currentChoices.Count);
            Assert.AreEqual("two", story.currentChoices[0].text);

            story.ChooseChoiceIndex(0);
            Assert.AreEqual("two\nthree\nsix\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestWeaveOptions()
        {
            var storyStr =
                @"
					-> test
                    === test
                        * Hello[.], world.
                        -> END
                ";

            Story story = CompileString(storyStr);
            story.Continue();

            Assert.AreEqual("Hello.", story.currentChoices[0].text);

            story.ChooseChoiceIndex(0);
            Assert.AreEqual("Hello, world.\n", story.Continue());
        }

        [Test()]
        public void TestWhitespace()
        {
            var storyStr =
@"
-> firstKnot
=== firstKnot
    Hello!
    -> anotherKnot

=== anotherKnot
    World.
    -> END
";

            Story story = CompileString(storyStr);
            Assert.AreEqual("Hello!\nWorld.\n", story.ContinueMaximally());
        }

        [Test()]
        public void TestVisitCountsWhenChoosing()
        {
            var storyStr =
                @"
== TestKnot ==
this is a test
+ [Next] -> TestKnot2

== TestKnot2 ==
this is the end
-> END
";

            Story story = CompileString(storyStr);

            Assert.AreEqual (0, story.state.VisitCountAtPathString ("TestKnot"));
            Assert.AreEqual (0, story.state.VisitCountAtPathString ("TestKnot2"));

            story.ChoosePathString ("TestKnot");

            Assert.AreEqual (1, story.state.VisitCountAtPathString ("TestKnot"));
            Assert.AreEqual (0, story.state.VisitCountAtPathString ("TestKnot2"));

            story.Continue ();

            Assert.AreEqual (1, story.state.VisitCountAtPathString ("TestKnot"));
            Assert.AreEqual (0, story.state.VisitCountAtPathString ("TestKnot2"));

            story.ChooseChoiceIndex (0);

            Assert.AreEqual (1, story.state.VisitCountAtPathString ("TestKnot"));

            // At this point, we have made the choice, but the divert *within* the choice
            // won't yet have been evaluated.
            Assert.AreEqual (0, story.state.VisitCountAtPathString ("TestKnot2"));

            story.Continue ();

            Assert.AreEqual (1, story.state.VisitCountAtPathString ("TestKnot"));
            Assert.AreEqual (1, story.state.VisitCountAtPathString ("TestKnot2"));
        }

        [Test()]
        public void TestTempGlobalConflict()
        {
            // Test bug where temp was being treated as a global
            var storyStr =
                @"
-> outer
=== outer
~ temp x = 0
~ f(x)
{x}
-> DONE

=== function f(ref x)
~temp local = 0
~x=x
{setTo3(local)}

=== function setTo3(ref x)
~x = 3
";

            Story story = CompileString(storyStr);

            Assert.AreEqual ("0\n", story.Continue ());
        }

        [Test()]
        public void TestThreadInLogic()
        {
            var storyStr =
                @"
-> once ->
-> once ->

== once ==
{<- content|}
->->

== content ==
Content
-> DONE
";

            Story story = CompileString(storyStr);

            Assert.AreEqual ("Content\n", story.Continue ());
        }

        [Test ()]
        public void TestTempUsageInOptions ()
        {
            var storyStr =
                @"
~ temp one = 1
* \ {one}
- End of choice 
    -> another
* (another) this [is] another
 -> DONE
";

            Story story = CompileString (storyStr);
            story.Continue ();
                 
            Assert.AreEqual (1, story.currentChoices.Count);
            Assert.AreEqual (" 1", story.currentChoices[0].text);
            story.ChooseChoiceIndex (0);

            Assert.AreEqual (" 1\nEnd of choice\nthis  another\n", story.ContinueMaximally ());

            Assert.AreEqual (0, story.currentChoices.Count);
        }


        [Test ()]
        public void TestEvaluatingInkFunctionsFromGame ()
        {
            var storyStr =
                @"
Top level content
* choice

== somewhere ==
= else
-> DONE

== function test ==
~ return -> somewhere.else
";

            Story story = CompileString (storyStr);
            story.Continue ();

            var returnedDivertTarget = story.EvaluateFunction ("test");

            // Divert target should get returned as a string
            Assert.AreEqual ("somewhere.else", returnedDivertTarget);
        }

        [Test ()]
        public void TestEvaluatingInkFunctionsFromGame2 ()
        {
            var storyStr =
                @"
One
Two
Three

== function func1 ==
This is a function
~ return 5

== function func2 ==
This is a function without a return value
~ return

== function add(x,y) ==
x = {x}, y = {y}
~ return x + y
";

            Story story = CompileString (storyStr);

            string textOutput;
            var funcResult = story.EvaluateFunction ("func1", out textOutput);
            Assert.AreEqual ("This is a function\n", textOutput);
            Assert.AreEqual (5, funcResult);

            Assert.AreEqual ("One\n", story.Continue());

            funcResult = story.EvaluateFunction ("func2", out textOutput);
            Assert.AreEqual ("This is a function without a return value\n", textOutput);
            Assert.AreEqual (null, funcResult);

            Assert.AreEqual ("Two\n", story.Continue ());

            funcResult = story.EvaluateFunction ("add", out textOutput, 1, 2);
            Assert.AreEqual ("x = 1, y = 2\n", textOutput);
            Assert.AreEqual (3, funcResult);

            Assert.AreEqual ("Three\n", story.Continue ());
        }

        [Test ()]
        public void TestEvaluatingFunctionVariableStateBug ()
        {
            var storyStr =
                @"
Start
-> tunnel ->
End
-> END

== tunnel ==
In tunnel.
->->

=== function function_to_evaluate() ===
    { zero_equals_(1):
        ~ return ""WRONG""
    - else:
        ~ return ""RIGHT""
    }

=== function zero_equals_(k) ===
    ~ do_nothing(0)
    ~ return  (0 == k)

=== function do_nothing(k)
    ~ return 0
";

            Story story = CompileString (storyStr);

            Assert.AreEqual ("Start\n", story.Continue ());
            Assert.AreEqual ("In tunnel.\n", story.Continue ());

            var funcResult = story.EvaluateFunction ("function_to_evaluate");
            Assert.AreEqual ("RIGHT", funcResult);

            Assert.AreEqual ("End\n", story.Continue ());
        }

        [Test ()]
        public void TestDoneStopsThread ()
        {
            var storyStr =
                @"
-> DONE
This content is inaccessible.
";

            Story story = CompileString (storyStr);

            Assert.AreEqual (string.Empty, story.ContinueMaximally ());
        }

        [Test ()]
        public void TestWrongVariableDivertTargetReference ()
        {
            var storyStr =
                @"
-> go_to_broken(-> SOMEWHERE)

== go_to_broken(-> b)
 -> go_to(-> b) // INSTEAD OF: -> go_to(b)

== go_to(-> a)
  -> a

== SOMEWHERE ==
Should be able to get here!
-> DONE
";
            CompileStringWithoutRuntime (storyStr, testingErrors:true);

            Assert.IsTrue (HadError ("it shouldn't be preceded by '->'"));
        }

        [Test ()]
        public void TestLeftRightGlueMatching ()
        {
            var storyStr =
                @"
A line.
{ f():
    Another line.
}

== function f ==
{false:nothing}
~ return true

";
            var story = CompileString (storyStr);

            Assert.AreEqual ("A line.\nAnother line.\n", story.ContinueMaximally ());
        }

        [Test ()]
        public void TestSetNonExistantVariable ()
        {
            var storyStr =
                @"
VAR x = ""world""
Hello {x}.
";
            var story = CompileString (storyStr);

            Assert.AreEqual ("Hello world.\n", story.Continue());

            Assert.Throws<StoryException>(() => {
                story.variablesState ["y"] = "earth";
            });
        }

        [Test ()]
        public void TestConstRedefinition ()
        {
            var storyStr =
                @"
CONST pi = 3.1415
CONST pi = 3.1415

CONST x = ""Hello""
CONST x = ""World""

CONST y = 3
CONST y = 3.0

CONST z = -> somewhere
CONST z = -> elsewhere

== somewhere ==
-> DONE

== elsewhere ==
-> DONE
";
            CompileStringWithoutRuntime (storyStr, testingErrors:true);

            Assert.IsFalse (HadError ("'pi' has been redefined"));
            Assert.IsTrue (HadError ("'x' has been redefined"));
            Assert.IsTrue (HadError ("'y' has been redefined"));
            Assert.IsTrue (HadError ("'z' has been redefined"));
        }

        [Test ()]
        public void TestTags ()
        {
            var storyStr =
                @"
VAR x = 2 
# author: Joe
# title: My Great Story
This is the content

== knot ==
# knot tag
Knot content
# end of knot tag
-> END

= stitch
# stitch tag
Stitch content
# this tag is below some content so isn't included in the static tags for the stitch
-> END
";
            var story = CompileString (storyStr);

            var globalTags = new List<string> ();
            globalTags.Add ("author: Joe");
            globalTags.Add ("title: My Great Story");

            var knotTags = new List<string> ();
            knotTags.Add ("knot tag");

            var knotTagWhenContinuedTwice = new List<string> ();
            knotTagWhenContinuedTwice.Add ("end of knot tag");

            var stitchTags = new List<string> ();
            stitchTags.Add ("stitch tag");

            Assert.AreEqual (globalTags, story.globalTags);
            Assert.AreEqual ("This is the content\n", story.Continue ());
            Assert.AreEqual (globalTags, story.currentTags);

            Assert.AreEqual (knotTags, story.TagsForContentAtPath ("knot"));
            Assert.AreEqual (stitchTags, story.TagsForContentAtPath ("knot.stitch"));

            story.ChoosePathString ("knot");
            Assert.AreEqual ("Knot content\n", story.Continue ());
            Assert.AreEqual (knotTags, story.currentTags);
            Assert.AreEqual ("", story.Continue ());
            Assert.AreEqual (knotTagWhenContinuedTwice, story.currentTags);
        }

        [Test ()]
        public void TestTunnelOnwardsDivertOverride ()
        {
            var storyStr =
                @"
-> A ->
We will never return to here!

== A ==
This is A
->-> B

== B ==
Now in B.
-> END
";
            var story = CompileString (storyStr);

            Assert.AreEqual ("This is A\nNow in B.\n", story.ContinueMaximally());
        }

        [Test ()]
        public void TestListBasicOperations ()
        {
            var storyStr =
                @"
LIST list = a, (b), c, (d), e
{list}
{(a, c) and (b, e)}
{(a, b, c) ^ (c, b, e)}
{list ? (b, d, e)}
{list ? (d, b)}
{list !? (c)}
";
            var story = CompileString (storyStr);

            Assert.AreEqual ("b, d\na, b, c, e\nb, c\n0\n1\n1\n", story.ContinueMaximally ());
        }


        [Test ()]
        public void TestListMixedItems ()
        {
            var storyStr =
                @"
LIST list = (a), b, (c), d, e
LIST list2 = x, (y), z
{list && list2}
";
            var story = CompileString (storyStr);

            Assert.AreEqual ("a, y, c\n", story.ContinueMaximally ());
        }


        [Test ()]
        public void TestMoreListOperations ()
        {
            var storyStr =
                @"
LIST list = l, m = 5, n
{LIST_VALUE(l)}

{list(1)}

~ temp t = list()
~ t += n
{t}
~ t = LIST_ALL(t)
~ t -= n
{t}
~ t = LIST_INVERT(t)
{t}
";
            var story = CompileString (storyStr);

            Assert.AreEqual ("1\nl\nn\nl, m\nn\n", story.ContinueMaximally ());
        }

        [Test ()]
        public void TestEmptyListOrigin ()
        {
            var storyStr =
                @"
LIST list = a, b
{LIST_ALL(list)}

";
            var story = CompileString (storyStr);

            Assert.AreEqual ("a, b\n", story.ContinueMaximally ());
        }

        [Test ()]
        public void TestEmptyListOriginAfterAssignment ()
        {
            var storyStr =
                @"
LIST x = a, b, c
~ x = ()
{LIST_ALL(x)}
";
            var story = CompileString (storyStr);

            Assert.AreEqual ("a, b, c\n", story.ContinueMaximally ());
        }

        [Test ()]
        public void TestListSaveLoad ()
        {
            var storyStr =
                @"
LIST l1 = (a), b, (c)
LIST l2 = (x), y, z

VAR t = ()
~ t = l1 and l2
{t}

== elsewhere ==
~ t += z
{t}
-> END
";
            var story = CompileString (storyStr);

            Assert.AreEqual ("a, x, c\n", story.ContinueMaximally ());

            var savedState = story.state.ToJson ();

            // Compile new version of the story
            story = CompileString (storyStr);

            // Load saved game
            story.state.LoadJson (savedState);

            story.ChoosePathString ("elsewhere");
            Assert.AreEqual ("a, x, c, z\n", story.ContinueMaximally ());
        }

        [Test ()]
        public void TestEmptyThreadError ()
        {
            CompileStringWithoutRuntime ("<-", testingErrors:true);
            Assert.IsTrue (HadError ("Expected target for new thread"));
        }

        [Test ()]
        public void TestAuthorWarningsInsideContentListBug ()
        {
            var storyStr =
                @"
{ once:
- a
TODO: b
}
";
            CompileString (storyStr, testingErrors:true);
            Assert.IsFalse (HadError ());
        }

        [Test ()]
        public void TestWeaveWithinSequence ()
        {
            var storyStr =
                @"
{ shuffle:
-   * choice
    nextline
}
";
            var story = CompileString (storyStr);

            story.Continue ();

            Assert.IsTrue (story.currentChoices.Count == 1);

            story.ChooseChoiceIndex (0);

            Assert.AreEqual ("choice\nnextline\n", story.ContinueMaximally ());
        }

        // Helper compile function
        protected Story CompileString(string str, bool countAllVisits = false, bool testingErrors = false)
        {
            _testingErrors = testingErrors;
            _errorMessages.Clear();
            _warningMessages.Clear();

            InkParser parser = new InkParser(str, null, TestErrorHandler);
            var parsedStory = parser.Parse();
            parsedStory.countAllVisits = countAllVisits;

            Story story = parsedStory.ExportRuntime(TestErrorHandler);
            Assert.AreNotEqual(null, story);

            // Convert to json and back again
            if (_mode == TestMode.JsonRoundTrip)
            {
                var jsonStr = story.ToJsonString();
                story = new Story(jsonStr);
            }

            return story;
        }

        protected Ink.Parsed.Story CompileStringWithoutRuntime(string str, bool testingErrors = false)
        {
            _testingErrors = testingErrors;
            _errorMessages.Clear();
            _warningMessages.Clear();

            InkParser parser = new InkParser(str, null, TestErrorHandler);
            var parsedStory = parser.Parse();

            if (!testingErrors) {
                Assert.IsNotNull (parsedStory);
                Assert.IsFalse (parsedStory.hadError);
            }

            if (parsedStory) {
                parsedStory.ExportRuntime (TestErrorHandler);
            }

            return parsedStory;
        }

        private bool HadError(string matchStr = null)
        {
            return HadErrorOrWarning(matchStr, _errorMessages);
        }

        private bool HadErrorOrWarning(string matchStr, List<string> list)
        {
            if (matchStr == null)
                return list.Count > 0;

            foreach (var str in list)
            {
                if (str.Contains(matchStr))
                    return true;
            }
            return false;
        }

        private bool HadWarning(string matchStr = null)
        {
            return HadErrorOrWarning(matchStr, _warningMessages);
        }

        private void TestErrorHandler(string message, ErrorType errorType)
        {
            if (_testingErrors)
            {
                if (errorType == ErrorType.Error)
                    _errorMessages.Add(message);
                else
                    _warningMessages.Add(message);
            }
            else
                Assert.Fail(message);
        }

        private class TestWarningException : System.Exception
        { }
    }
}