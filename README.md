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

* Download the latest [ink-unity-integration package](https://github.com/inkle/ink/releases), or grab it from the Unity AssetStore, and place in your project.
* Create a `.ink` text file such as `myStory.ink`, containing the text `Hello, world!`.
* Select the file in Unity, and you should see a *Play* button in the file's inspector.
* Click it, and you should get an Editor window that lets you play (preview) your story.
* Follow the tutorial: [Writing with Ink](https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md).

## Advanced: Using inklecate on the command line

 * [Download the latest version of **inklecate**](https://github.com/inkle/ink/releases) (or build it yourself, see below.)
 * Create a text file called `myStory.ink`, containing the text `Hello, world!`.
 * On the command line, run the following:

    **Mac:** `./inklecate -p myStory.ink`
    
    **Windows:** `inklecate.exe -p myStory.ink`
    
    **Linux:** `mono inklecate.exe -p myStory.ink`
    
    * To run on Linux, you need the Mono runtime and the Mono System.Core library (for CLI 4.0). If you have access to the debian repository, you can install these using: <br>
    `sudo apt-get install mono-runtime libmono-system-core4.0-cil`

    The `-p` option uses play mode so that you can see the result immediately. If you want to get a compiled `.json` file, just remove the `-p` option from the examples above.
    
 * Follow the tutorial: [Writing with Ink](https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md).

## Integrating into your game

*Full article: see [Running Your Ink](https://github.com/inkle/ink/blob/master/Documentation/RunningYourInk.md).*

*For a sample Unity project, see [The Intercept](http://www.inklestudios.com/ink/theintercept).*

Ink comes with a C#-based (or [JavaScript-based](https://www.github.com/y-lohse/inkjs) runtime engine that can load and run a compiled ink story in JSON format.

To compile the ink, either export from Inky (File -> Export to JSON). Or if you're using Unity, you can use the [ink-Unity-integration package](https://github.com/inkle/ink-unity-integration) which will automatically compile your ink for you whenever you edit it either in Inky or in an editor of your choice.

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

## How to contribute

We’d of course appreciate any bug fixes you might find!

We're using GitHub issues both as a discussion forum and as a bug tracker, so [create a GitHub issue](https://github.com/inkle/ink/issues/new) if you want to start a discussion or request a feature, and please label appropriately. Or if you want to get in touch with us directly, [email us](mailto:info@inklestudios.com).

We also have a [Discord server](https://discord.gg/MUXj7Md), where you may find other people using ink to chat with (as well as inklers!)

## Architectural overview

See the [architectural overview documentation](https://github.com/inkle/ink/blob/master/Documentation/ArchitectureAndDevOverview.md) for information about the pipeline of the **ink** engine, and a birds-eye view of the project's code structure.

# License

**ink** is released under the [MIT license](https://github.com/inkle/ink/blob/master/LICENSE.txt). Although we don't require attribution, we'd love to know if you decide to use **ink** a project! Let us know on [Twitter](http://www.twitter.com/inkleStudios) or [by email](mailto:info@inklestudios.com).

# Support us!

**ink** is free forever, but represents multiple years of thought, design, development and testing. Please consider supporting us via [Patreon](http://www.patreon.com/inkle). Thank you, and have fun!
