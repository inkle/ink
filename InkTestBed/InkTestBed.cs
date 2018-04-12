using System;
using System.IO;
using System.Collections.Generic;
using Ink;


class InkTestBed : IFileHandler
{
    // ---------------------------------------------------------------
    // Main area to test stuff!
    // ---------------------------------------------------------------

    void Run ()
    {
        CompileFile ("test.ink");
        Console.WriteLine(story.ContinueMaximally ());
        JsonRoundtrip ();

        Compile ("Hello {1 + 2}!");
        Console.WriteLine(story.ContinueMaximally ());
    }

    // ---------------------------------------------------------------

    void Compile (string inkSource)
    {
    	var compiler = new Compiler (inkSource, new Compiler.Options {
    		countAllVisits = true,
    		errorHandler = OnError,
    		fileHandler = this
    	});

    	story = compiler.Compile ();

    	PrintAllMessages ();
    }


    void CompileFile (string filename)
    {
        var inkSource = File.ReadAllText (filename);

        var compiler = new Compiler (inkSource, new Compiler.Options {
			sourceFilename = filename,
			countAllVisits = true,
			errorHandler = OnError,
			fileHandler = this
        });

        story = compiler.Compile ();

        PrintAllMessages ();
    }

    void JsonRoundtrip ()
    {
        var jsonStr = story.ToJsonString ();
        Console.WriteLine (jsonStr);

        Console.WriteLine ("---------------------------------------------------");

        var reloadedStory = new Ink.Runtime.Story (jsonStr);
        var newJsonStr = reloadedStory.ToJsonString ();
        Console.WriteLine (newJsonStr);

        story = reloadedStory;
    }

    // ---------------------

    public Ink.Runtime.Story story;

    public InkTestBed () { }

    public static void Main (string [] args)
    {
        new InkTestBed ().Run ();
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

