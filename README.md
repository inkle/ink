# ink

[![CI Status](http://img.shields.io/travis/inkle/ink.svg?style=flat)](https://travis-ci.org/inkle/ink)

[Ink](http://www.inklestudios.com/ink) is [inkle](http://www.inklestudios.com/)'s scripting language for writing interactive narrative, both for text-centric games as well as more graphical games that contain highly branching stories. It's designed to be easy to learn, but with powerful enough features to allow an advanced level of structuring.

Here's a taster [from the tutorial](https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md).

    - I looked at Monsieur Fogg 
    *   ... and I could contain myself no longer.
        'What is the purpose of our journey, Monsieur?'
        'A wager,' he replied.
        * *     'A wager!'[] I returned.
                He nodded. 
                * * *   'But surely that is foolishness!'
                * * *  'A most serious matter then!'
                - - -   He nodded again.
                * * *   'But can we win?'
                        'That is what we will endeavour to find out,' he answered.
                * * *   'A modest wager, I trust?'
                        'Twenty thousand pounds,' he replied, quite flatly.
                * * *   I asked nothing further of him then[.], and after a final, polite cough, he offered nothing more to me. <>
        * *     'Ah[.'],' I replied, uncertain what I thought.
        - -     After that, <>
    *   ... but I said nothing[] and <> 
    - we passed the day in silence.
    - -> END



# Getting started

**Download [Inky, our ink editor](https://github.com/inkle/inky), and the follow either:**

 * [The basics tutorial](https://www.inklestudios.com/ink/web-tutorial/) if you're non-technical and/or if you'd like to use ink to make a web-based interactive fiction game
 *  [The full tutorial](https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md) if you want to see everything that ink has to offer.


For those who are very technically-minded, you can also use *inklecate* directly, our ink command line compiler (and player).

To keep up to date with the latest news about ink [sign up for the mailing list](http://www.inklestudios.com/ink#signup).

## Writing with Unity

* Download the latest [ink-unity-integration package](https://github.com/inkle/ink-unity-integration), or grab it from the Unity AssetStore, and place in your project.
* Create a `.ink` text file such as `myStory.ink`, containing the text `Hello, world!`.
* Select the file in Unity, and you should see a *Play* button in the file's inspector.
* Click it, and you should get an Editor window that lets you play (preview) your story.
* Follow the tutorial: [Writing with Ink](https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md).

## Ink, Inky, ink-unity-integration, inkjs, inklecate, inkle oh my!

* [Ink](https://www.inklestudios.com/ink) is the core narrative engine itself, written in C#. It includes the code for the compiler. If you're not technical, you don't need to worry about this.
* [Inky](https://www.github.com/inkle/inky) is our ink editor, which is a text editor with support for playing as you write. If you're just starting out with ink, this is all you need.
* [ink-unity-integration](https://www.github.com/inkle/ink-unity-integration) is a plugin to allow you integrate the ink engine with a Unity game. It includes the full Ink engine source.
* **inklecate** is a command-line compiler for ink. Inky uses it behind the scenes.
* [inkjs](https://github.com/y-lohse/inkjs) is a JavaScript port of the ink engine, useful for powering web-based game. This is included when you export a story for web within Inky.
* [inkle](https://www.inklestudios.com) is the game development studio that created ink
* [inklewriter](https://www.inklewriter.com) is an unrelated interactive story creation tool that designed to be easy to use, but is far less powerful. It's possible to export inklewriter stories to ink, but not vice versa.

### What you need if you are a:

* **Writer**: Inky
* **Unity game developer**: ink-unity-integration plugin. Optionally, Inky if you're reading/writing the ink too.
* **Web-game author**: Inky

### Versioning

The intention is the following:

- Each latest ink/inky/ink-unity-integration release on each Github release page should work together. Ink and Inky version *numbering* are separate though. You can see which version of the ink engine Inky has in the About box.
- ink / ink-unity-integration should effectively have the same version of the same engine, except that the integration might have additional Unity-specific extra minor releases. Their X.0.0 and 0.Y.0 version numbers should match. The 0.0.Z version number in ink-unity-integration may diverge to reflect Unity-specific changes.
- inkjs is maintained by the community (primarily @y-lohse and @ephread). It's usually one major version behind the main ink engine, but they work hard to catch up after each release!
- The ink engine also has story-format and save-format versions that are internal to the code (see Story.cs and StoryState.cs).



## Advanced: Using inklecate on the command line

 * [Download the latest version of **inklecate**](https://github.com/inkle/ink/releases) (or build it yourself, see below.)
 * Create a text file called `myStory.ink`, containing the text `Hello, world!`.
 * On the command line, run the following:

    **Mac:** `./inklecate -p myStory.ink`
    
    **Windows:** `inklecate.exe -p myStory.ink`
    
    **Linux:** `mono inklecate.exe -p myStory.ink`
    
    * To run on Linux, you need the Mono runtime and the Mono System.Core library (for CLI 4.0). If you have access to the debian repository, you can install these using: <br>
    `sudo apt install mono-complete`

    The `-p` option uses play mode so that you can see the result immediately. If you want to get a compiled `.json` file, just remove the `-p` option from the examples above.
    
 * Follow the tutorial: [Writing with Ink](https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md).

## Integrating into your game

*Full article: see [Running Your Ink](https://github.com/inkle/ink/blob/master/Documentation/RunningYourInk.md).*

*For a sample Unity project, see [The Intercept](http://www.inklestudios.com/ink/theintercept).*

Ink comes with a C#-based (or [JavaScript-based](https://www.github.com/y-lohse/inkjs) runtime engine that can load and run a compiled ink story in JSON format.

To compile the ink, either export from Inky (File -> Export to JSON). Or if you're using Unity, you can use the [ink-unity-integration package](https://github.com/inkle/ink-unity-integration) which will automatically compile your ink for you whenever you edit it either in Inky or in an editor of your choice.

*Advanced: You can also use the inklecate command line tool to compile ink stories, or you can call the compiler from C# code yourself.*

**ink** isn't designed as an end-to-end narrative game engine. Rather, it's designed to be flexible, so that it can slot into your own game and UI with ease. Here's a taster of the code you need to get started:

    using Ink.Runtime;
    
    // 1) Load story
    _story = new Story(sourceJsonString);
    
    // 2) Game content, line by line
    while(_story.canContinue)
        Debug.Log(story.Continue());
    
    // 3) Display story.currentChoices list, allow player to choose one
    Debug.Log(_story.currentChoices[0].choiceText);
    _story.ChooseChoiceIndex(0);
    
    // 4) Back to 2
    ...



# The development of ink

## Build Requirements

**All Environments:**
 * [.NET Core SDK 3.1](https://dotnet.microsoft.com/download) or newer
 * Optionally [Visual Studio Code](https://code.visualstudio.com/)


**Windows (Optional):**
    
 * [Visual Studio](https://www.visualstudio.com/) (e.g. Community edition); required to build nuget package with multi-targeting of .NET Framework 3.5
 * [Xamarin](https://xamarin.com/download), or Unity's own version of MonoDevelop
   
**Mac (Optional):**
    
 * [Visual Studio for Mac](https://www.visualstudio.com/)
 * [Xamarin](https://xamarin.com/download), or Unity's own version of MonoDevelop

### Building with Visual Studio

1. Load up the solution file - `ink.sln`.
2. Select the *Release* configuration and choose *Build -> Build All* (or *Build Solution* in Visual Studio).
3. The compiler binary should be built in `inklecate/bin/Release` (or `x86`), while the runtime engine DLL will be built in `ink-engine-dll/bin/Release/ink-engine.dll`

### Building with command-line

1. `cd` to the project you want to build (e.g., `cd inklecate`)
2. Build using dotnet: `dotnet build -c Release`
3. To run console apps: `dotnet run -c Release`
    * To produce self-contained executable: `dotnet publish -r win-x64 -c Release --self-contained false`
    * [Recommended RIDs](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog) for the platform (`-r`) are: `win-x64`, `linux-x64`, and `osx-x64`


To run the binaries, you need to install [.NET Core Runtime 2.2]((https://dotnet.microsoft.com/download)) or newer (included in SDK).

## Need help?

* [Discord](https://discord.gg/inkle) - we have an active community of ink users who would be happy to help you out. Discord is probably the best place to find the answer to your question.
* [GitHub Discussions](https://github.com/inkle/ink/discussions) - Or, you can ask a question here on GitHub. (Note: we used to use Issues for general Q&A, but we have now migrated.)

## How to contribute

We’d of course appreciate any bug fixes you might find - feel free to submit a pull request. However, usually we're actively working on a game, so it might take a little while for us to take a look at a non-trivial pull request. Apologies in advance if it takes a while to get a response!

## Architectural overview

See the [architectural overview documentation](https://github.com/inkle/ink/blob/master/Documentation/ArchitectureAndDevOverview.md) for information about the pipeline of the **ink** engine, and a birds-eye view of the project's code structure.

# License

**ink** is released under the [MIT license](https://github.com/inkle/ink/blob/master/LICENSE.txt). Although we don't require attribution, we'd love to know if you decide to use **ink** a project! Let us know on [Twitter](http://www.twitter.com/inkleStudios) or [by email](mailto:info@inklestudios.com).

# Support us!

**ink** is free forever, but represents multiple years of thought, design, development and testing. Please consider supporting us via [Patreon](http://www.patreon.com/inkle). Thank you, and have fun!

![](Epic_MegaGrants_Recipient_logo_horizontal.png)
