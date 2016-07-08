# Running your ink

## Quick Start

*Note that although these instructions are written with Unity in mind, it's possible (and straightforward) to run your ink in a non-Unity C# environment.*

* Download the [latest version of the ink-unity-integration Unity package](https://github.com/inkle/ink/releases), and add to your project.
* Select your `.ink` file in Unity, and you should see a *Play* button in the file's inspector.
* Click it, and you should get an Editor window that lets you play (preview) your story.
* To integrate into your game, see **Getting started with the runtime API**, below.

## Further information

Ink uses an intermediate `.json` format, which is compiled from the original `.ink` files. ink's Unity integration package automatically compiles ink files for you, but you can also compile them on the command line. See **Using inklecate on the command line** in the [README](http://www.github.com/inkle/ink) for more information.

The main runtime code is included in the `ink-engine.dll`, and we also have a dependency on the `Newtonsoft.Json.dll` library, which is also included in the integration package.

You may need to change API compatibility for .NET: Go into `Edit -> Project settings -> Player -> Other settings` and change API compatibility level from .NET 2.0 subset (the default) to .NET 2.0. (If you get the error `TypeLoadException: Could not load type 'Newtonsoft.Json.Linq.JArray' from assembly 'Newtonsoft.Json`..., this is what's wrong.)

We recommend that you create a wrapper MonoBehaviour component for the **ink** `Story`. Here, we'll call the component "Script" - in the "film script" sense, rather than the "Unity script" sense!

	using Ink.Runtime;

	public class Script : MonoBehaviour {

		// Set this file to your compiled json asset
		public TextAsset inkAsset;

		// The ink story that we're wrapping
		Story _inkStory;

## Getting started with the runtime API

As mentioned above, your `.ink` file(s) are compiled to a single `.json` file. This is treated by Unity as a TextAsset, that you can then load up in your game.

The API for loading and running your story is very straightforward. Construct a new `Story` object, passing in the JSON string from the TextAsset. For example, in Unity:

    using Ink.Runtime;

    ...

    void Awake()
    {
        _inkStory = new Story(inkAsset.text);
    }
    
From there, you make calls to the story in a loop. There are two repeating stages:

 1. **Present content:** You repeatedly call `Continue()` on it, which returns individual lines of string content, until the `canContinue` property becomes false. For example:
    
        while (_inkStory.canContinue) {
            Debug.Log (_inkStory.Continue ());
        }
        
    A simpler way to achieve the above is through one call to `_inkStory.ContinueMaximally()`. However, in many stories it's useful to pause the story at each line, for example when stepping through dialogue. Also, in such games, there may be state changes that should be reflected in the UI, such as resource counters.
 
 2. **Make choice:** When there isn't any more content, you should check to see whether there any choices to present to the player. To do so, use something like:

        if( _inkStory.currentChoices.Count > 0 ) 
        {
            for (int i = 0; i < _inkStory.currentChoices.Count; ++i) {
                Choice choice = _inkStory.currentChoices [i];
                Debug.Log("Choice " + (i + 1) + ". " + choice.text);
            }
        }
        
    ...and when the player provides input:
    
        _inkStory.ChooseChoiceIndex (index);
        
    And now you're ready to return to step 1, and present content again.
    
### Saving and loading

To save the state of your story within your game, call:

`string savedJson = _inkStory.state.ToJson();`

...and then to load it again:

`_inkStory.state.LoadJson(savedJson);`

### Is that it?
    
That's it! You can achieve a lot with just those simple steps, but for more advanced usage, including deep integration with your game, read on.

For a sample Unity project in action with minimal UI, see [Aaron Broder's Blot repo](https://github.com/abroder/blot).

## Engine usage and philosophy

In Unity, we recommend using your own component class to wrap `Ink.Runtime.Story`. The runtime **ink** engine has been designed to be reasonably general purpose and have a simple API. We also recommend wrapping rather than inheriting from `Story`, so that you can expose to your game only the functionality that you need.

Often when designing the flow for your game, the sequence of interactions between the player and the story may not precisely match the way the **ink** is evaluted. For example, with a classic choose-your-own-adventure type story, you may want to show multiple lines (paragraphs) of text and choices all at once. For a visual novel, you may want to display one line per screen.

Additionally, since the **ink** engine outputs lines of plain text, it can be effectively used for your own simple sub-formats. For example, for a dialog based game, you could write:

    *   Lisa: Where did he go?
        Joe:  I think he jumped over the garden fence.
        * *   Lisa: Let's take a look.
        * *   Lisa: So he's gone for good?
    
As far as the **ink** engine is concerned, the `:` characters are just text. But as the lines of text and choices are produced by the game, you can do some simple text parsing of your own to turn the string `Joe: What's up?` into a game-specific dialog object that references the speaker and the text (or even audio).

This approach can be taken even further to text that flexibly indicates non-content directives. Again, these directives come out of the engine as text, but can parsed by your game for a specific purpose:

    PROPLIST table, chair, apple, orange
    
The above approach is used in our current game for the writer to declare the props that they expect to be in the scene. These might be picked up in the game editor in order to automatically fill a scene with placeholder objects, or just to validate that the level designer has populated the scene correctly.

Of course, you can also use *External Functions* - see below, but the above approach is flexible and won't cause the game to crash if everything isn't set up perfectly yet.

## Jumping to a particular "scene"

Top level named sections in **ink** are called knots (see [the writing tutorial](https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md)). You can tell the runtime engine to jump to a particular named knot:

    _inkStory.ChoosePathString("myKnotName");

And then call `Continue()` as usual.

To jump directly to a stitch within a knot, use a `.` as a separator:

    _inkStory.ChoosePathString("myKnotName.theStitchWithin");
    
(Note that this path string is a *runtime* path rather than the path as used within the **ink** format. It's just been designed so that for the basics of knots and stitches, the format works out the same. Unfortunately however, you can't reference gather or choice labels this way.)

## Setting/getting ink variables

The state of the variables in the **ink** engine is, appropriately enough, stored within the `variablesState` object within the `story`. You can both get and set variables directly on this object:

    _inkStory.variablesState["player_health"] = 100
    
    int health = (int) _inkStory.variablesState["player_health"]
    
## Read/Visit counts

To find out the number of times that a knot or stitch has been visited by the ink engine, you can use this API:

    _inkStory.state.VisitCountAtPathString("...");
    
The path string is in the form `"yourKnot"` for knots, and `"yourKnot.yourStitch"` for stitches.

## Variable observers

You can register a delegate function to be called whenever a particular variable changes. This can be useful to reflect the state of certain **ink** variables directly in the UI. For example:

    _inkStory.ObserveVariable ("health", (string varName, object newValue) => {
        SetHealthInUI((int)newValue);
    });

The reason that the variable name is passed in is so that you can have a single observer function that observes multiple different variables.


## Exernal functions

You can define game-side functions in C# that can be called directly from **ink**. To do so:

1. Declare an external function using something like this at the top of one of your **ink** files, in global scope:

        EXTERNAL multiply(x,y)

2. Bind your C# function. For example:

        _inkStory.BindExternalFunction ("multiply", (int arg1, float arg2) => {
            return arg1 * arg2;
        });  

   There are convenience overloads for BindExternalFunction, for up to three parameters, for both generic `System.Func` and `System.Action`. There is also a general purpose `BindExternalFunctionGeneral` that takes an object array for more than 3 parameters.

3. You can then call that function within the **ink**:

        3 times 4 is {multiply(3, 4)}.

The types you can use as parameters and return values are int, float, bool (automatically converted from **ink**’s internal ints) and string.

    
## Debugging ink engine issues

The **ink** engine is still in a nascent stage (alpha!), and you may well encounter bugs, or unhelpful error messages and exceptions.

We recommend we debug the compiler so that you get a breakpoint in its code when compiling and/or playing your ink file. To do so, open **ink.sln** in Xamarin or Visual Studio, and run the compiler code in the Test configuration. You should supply command line parameters in the project settings. (In Xamarin, right-click on *inklecate*, click *Options*, and in *Run > General* and modify the parameters field.) You could use something like:

    -p path/to/yourMainFile.ink
    
The `-p` switch puts the compiler in Play mode, so that it will execute your story immediately.

When your story hits an assertion, you may be able to glean a little more information from the state of the ink engine. See the [Architecture and Development](https://github.com/inkle/ink/blob/master/Documentation/ArchitectureAndDevOverview.md) document for help understanding and debugging the engine code.
