using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Ink;


class InkTestBed : IFileHandler
{
    // ---------------------------------------------------------------
    // Main area to test stuff!
    // ---------------------------------------------------------------

    void Run ()
    {
        Play ();
    }

    // ---------------------------------------------------------------
    // Useful functions when testing
    // ---------------------------------------------------------------

    // Full play loop
    void Play ()
    {
        if (story == null) CompileFile ("test.ink");

        // Errors to the extent that story couldn't be constructed?
        if (story == null) return;

        while (story.canContinue || story.currentChoices.Count > 0) {
            if (story.canContinue)
                ContinueMaximally ();

            PrintAllMessages ();

            if (story.currentChoices.Count > 0)
                PlayerChoice ();
        }
    }

    void Continue ()
    {
        Console.WriteLine(story.Continue ());
        PrintChoicesIfNecessary ();
        PrintAllMessages (); // errors etc
    }

    void ContinueMaximally ()
    {
        Console.WriteLine (story.ContinueMaximally ());
        PrintChoicesIfNecessary ();
        PrintAllMessages (); // errors etc
    }

    void Choose (int choiceIdx)
    {
        story.ChooseChoiceIndex (choiceIdx);
    }

    void PlayerChoice ()
    {
        bool hasValidChoice = false;
        int choiceIndex = -1;

        while (!hasValidChoice) {
            Console.Write (">>> ");

            string userInput = Console.ReadLine ();

            if (userInput == null)
                throw new System.Exception ("<User input stream closed.>");

            int choiceNum;
            if (int.TryParse (userInput, out choiceNum)) {
                choiceIndex = choiceNum - 1;

                if (choiceIndex >= 0 && choiceIndex < story.currentChoices.Count) {
                    hasValidChoice = true;
                } else {
                    Console.WriteLine ("Choice out of range");
                }
            } else {
                Console.WriteLine ("Not a number");
            }
        }

        story.ChooseChoiceIndex (choiceIndex);
    }

    Ink.Runtime.Story Compile (string inkSource)
    {
    	compiler = new Compiler (inkSource, new Compiler.Options {
    		errorHandler = OnError,
    		fileHandler = this
    	});

    	story = compiler.Compile ();

    	PrintAllMessages ();

        return story;
    }

    Compiler CreateCompiler(string filename = null)
    {
        if (filename == null) filename = "test.ink";

        if (Path.IsPathRooted(filename))
        {
            var dir = Path.GetDirectoryName(filename);
            Directory.SetCurrentDirectory(dir);
        }

        var inkSource = File.ReadAllText(filename);

        return new Compiler(inkSource, new Compiler.Options
        {
            sourceFilename = filename,
            errorHandler = OnError,
            fileHandler = this
        });
    }

    Ink.Runtime.Story CompileFile (string filename = null)
    {
        CreateCompiler(filename);

        story = compiler.Compile ();

        PrintAllMessages ();

        return story;
    }

    void JsonRoundtrip ()
    {
        var jsonStr = story.ToJson ();
        Console.WriteLine (jsonStr);

        Console.WriteLine ("---------------------------------------------------");

        var reloadedStory = new Ink.Runtime.Story (jsonStr);
        var newJsonStr = reloadedStory.ToJson ();
        Console.WriteLine (newJsonStr);

        story = reloadedStory;
    }

    // e.g.:
    //
    // Hello world
    // + choice
    //     done!
    //     -> END
    //
    // ------ SECOND INK VERSION ------
    //
    // Hello world
    // + choice
    //     done!
    //     -> END
    //
    void SplitFile (string filename, out string ink1, out string ink2)
    {
        const string splitStr = "------ SECOND INK VERSION ------";

        var fullSource = File.ReadAllText (filename);

        var idx = fullSource.IndexOf (splitStr, StringComparison.InvariantCulture);
        if (idx == -1)
            throw new System.Exception ("Split point not found in " + filename);

        ink1 = fullSource.Substring (0, idx);
        ink2 = fullSource.Substring (idx + splitStr.Length);
    }

    // e.g.:
    //
    //     InkChangingTest (() => {
    //         ContinueMaximally ();
    //         story.ChooseChoiceIndex (0);
    //     }, () => {
    //         ContinueMaximally ();
    //     });
    //
    void InkChangingTest (Action test1, Action test2)
    {
        string ink1, ink2;

		SplitFile ("test.ink", out ink1, out ink2);

        var story1 = Compile (ink1);

        test1 ();

        var saveState = story1.state.ToJson ();

        Console.WriteLine ("------ SECOND INK VERSION ------");

		var story2 = Compile (ink2);

        story2.state.LoadJson (saveState);

        test2 ();
    }

    void SimpleDiff(string s1, string s2)
    {
        if (s1 == s2)
        {
            Console.WriteLine("Identical!");
        }
        else
        {
            bool foundDiff = false;
            for (int i = 0; i < Math.Min(s2.Length, s1.Length); i++)
            {
                if (s2[i] != s1[i])
                {
                    foundDiff = true;
                    int diffI = Math.Max(i - 10, 0);
                    Console.WriteLine("Difference at idx {0}: \n\t{1}\nv.s.\n\t{2}",
                                      i,
                                      s1.Substring(diffI, 40),
                                      s2.Substring(diffI, 40));
                    break;
                }
            }

            if (!foundDiff)
            {
                var startOfExtension = Math.Min(s1.Length, s2.Length);
                var longerText = s1.Length > s2.Length ? s1 : s2;
                Console.WriteLine("Difference in length: {0} v.s. {1}. Extended: {2}",
                                  s1.Length,
                                  s2.Length,
                                  longerText.Substring(startOfExtension));
            }
        }
    }

    // Examples of usage:
    //
    //     var duration = Millisecs(() => DoSomething());
    //
    // Or to take the average after running DoSomething 100 times, but skipping
    // the first time (since we want to know the "warmed caches" time:
    //
    //     var duration = Millisecs((() => DoSomething(), 100, 1);
    //
    float Millisecs(Action action, int times = 1, int ignoreWarmupTimes = 0) {
        var s = new Stopwatch();

        var realTimes = times - ignoreWarmupTimes;

        if (times == 1 && ignoreWarmupTimes == 0)
        {
            s.Start();
            action();
            s.Stop();
        } else {
            if(ignoreWarmupTimes > 0 ) {
                for (int i = 0; i < ignoreWarmupTimes; i++) {
                    action();
                }
            }
            
            s.Start();
            for (int i = 0; i < realTimes; i++) {
                action();
            }
            s.Stop();
        }

        long ticks = s.ElapsedTicks;
        long ticksPerSec = Stopwatch.Frequency;
        double ticksPerMillisec = ticksPerSec / 1000.0;
        double millisecs = ticks / ticksPerMillisec;
        return (float)(millisecs / realTimes);
    }

    // ---------------------

    public Ink.Runtime.Story story;
    public Compiler compiler;

    public InkTestBed () { }

    public static void Main (string [] args)
    {
        new InkTestBed ().Run ();

        Console.WriteLine (">>> TEST BED ENDED <<<");
    }

    void PrintMessages (List<string> messageList, ConsoleColor colour)
    {
        Console.ForegroundColor = colour;

        foreach (string msg in messageList) {
            Console.WriteLine (msg);
        }

        Console.ResetColor ();
    }

    void PrintAllMessages ()
    {
        PrintMessages (_authorMessages, ConsoleColor.Green);
        PrintMessages (_warnings, ConsoleColor.Blue);
        PrintMessages (_errors, ConsoleColor.Red);

        _authorMessages.Clear ();
        _warnings.Clear ();
        _errors.Clear ();

        if (story != null) {
            if( story.hasError )
                PrintMessages (story.currentErrors, ConsoleColor.Red);
            if( story.hasWarning )
                PrintMessages (story.currentWarnings, ConsoleColor.Blue);
            story.ResetErrors ();
        }
    }

    void PrintChoicesIfNecessary ()
    {
        if (!story.canContinue && story.currentChoices != null) {

            int number = 1;
            foreach (var c in story.currentChoices) {
                Console.WriteLine (" {0}) {1}", number, c.text);
                number++;
            }
        }
    }

    void OnError (string message, Ink.ErrorType errorType)
    {
        switch (errorType) {
        case ErrorType.Author:
            _authorMessages.Add (message);
            break;

        case ErrorType.Warning:
            _warnings.Add (message);
            break;

        case ErrorType.Error:
            _errors.Add (message);
            break;
        }
    }

    public string ResolveInkFilename (string includeName)
    {
        var workingDir = Directory.GetCurrentDirectory ();
        var fullRootInkPath = Path.Combine (workingDir, includeName);
        return fullRootInkPath;
    }

    public string LoadInkFileContents (string fullFilename)
    {
        return File.ReadAllText (fullFilename);
    }

    List<string> _errors = new List<string> ();
    List<string> _warnings = new List<string> ();
    List<string> _authorMessages = new List<string> ();
}

