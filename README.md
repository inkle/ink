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
    
We'd recommend downloading [Inky, our ink editor](https://github.com/inkle/inky), and the follow [the tutorial](https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md), if you want to give ink a try.

Broadly, the engine is made up of two components:

 * **inklecate** is the command-line compiler for ink. It takes one or more text files with an `.ink` extension, and produces a `.json` file. It can also be used in *play* mode, for testing a story on the command line.
 * The **ink runtime engine** is a C# library that can be used within Unity or any other C# environment.

We also have an [ink Unity integration](https://github.com/inkle/ink-unity-integration) package so that you don't have to worry about the details of how to compile your ink files for Unity games.

**Warning:** **ink** is in alpha. Features may change, bugs may be encountered. We're yet to complete a project with this major rewrite of ink - it's a work in progress!


# Getting started

If you're happy to use Unity, we'd recommend following **Writing with Unity**, below.

If you would prefer a more barebones and technical approach, (or you aren't using Unity at all), you can also compile and play ink stories on the command line.

To keep up to date with the latest news about ink [sign up for the mailing list](http://www.inklestudios.com/ink#signup).

## Writing with Unity

* Download the latest [ink-unity-integration package](https://github.com/inkle/ink/releases), or grab it from the Unity AssetStore, and place in your project.
* Create a `.ink` text file such as `myStory.ink`, containing the text `Hello, world!`.
* Select the file in Unity, and you should see a *Play* button in the file's inspector.
* Click it, and you should get an Editor window that lets you play (preview) your story.
* Follow the tutorial: [Writing with Ink](https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md).

## Using inklecate on the command line

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


## Building

### Requirements

**Windows:**
    
 * [Visual Studio](https://www.visualstudio.com/) (e.g. Community edition), [Xamarin](https://xamarin.com/download), or Unity's own version of MonoDevelop.
    
**Mac:**
    
 * [Xamarin](https://xamarin.com/download), or Unity's own version of MonoDevelop

**Linux:**

  * [Mono](http://www.mono-project.com/). For detailed installation instructions, see [Installing Mono on Linux](http://www.mono-project.com/docs/getting-started/install/linux/).

### Build

1. Load up the solution file - `ink.sln`.
2. Select the *Release* configuration and choose *Build -> Build All* (or *Build Solution* in Visual Studio).
3. The compiler binary should be built in `inklecate/bin/Release` (or `x86`), while the runtime engine DLL will be built in `ink-engine-dll/bin/Release/ink-engine.dll`

Note that the executable requires Mono on Mac or .NET on Windows. On Windows this isn't a problem since it ships with .NET, but on Mac you need Xamarin for Mono. The `build_release.command` file in the repo is a Mac script that will bundle up both Mac and Windows versions, and the Mac version will be bundled with the Mono runtime so that the end user doesn't need Xamarin/Mono installed.

# The development of ink

## How to contribute

We’d of course appreciate any bug fixes you might find! Also see the roadmap below for future planned features and optimisations that you might be able to help out with.

We're using GitHub issues both as a discussion forum and as a bug tracker, so [create a GitHub issue](https://github.com/inkle/ink/issues/new) if you want to start a discussion or request a feature, and please label appropriately. Or if you want to get in touch with us directly, [email us](mailto:info@inklestudios.com).

We also have a [Discord server](https://discord.gg/MUXj7Md), where you may find other people using ink to chat with (as well as inklers!)

In terms of related projects outside of the scope of this repo, we'd love to see the following since we don't have time to do it ourselves right now, and we think it would substantially help the community:

 * A more friendly install-edit-play flow for writers. For example, a downloadable GUI-based app with an editor pane on the left and a play pane on the right. Bonus points if the play pane automatically reloads the state of the story as you type!
 * Implementations of the runtime engine in other languages - for example JavaScript, so that stories can be run on the web. (Note that although we'd be excited to see that right now, we'd probably recommend that you don't embark on such a project just yet, since the runtime may still change substantially, so it could create unnecessary maintenance work.)
 * Unity template project to demonstrate how to set up a particular style of game.

## Roadmap

Internally we've been thinking about the following. We can't guarantee we'll implement them any time soon though, or indeed at all!

### Definitely coming

 - Punctuation and whitespace cleaner. Although the ink engine does the best it can at fixing various issues such including the right amount of whitespace, there are certain things that are hard or impossible to deal with, due to the text being inherently interactive and unpredictable. For *Sorcery!* and *80 Days* we had a cleaning function which tidied up spacing and punctuation, and we intend to do the same with this latest version of the ink engine. A similar feature exists in HTML due to the inclusion of markup within text - for example, multiple spaces are collapsed down into one.
 - Tighten implementation to prevent certain "features" that aren't intentional (a wide class of bugs). For example, we currently allow content on the same line after the closing brace of a multi-line piece of logic.
 - Other bug fixes!
 
### Probably coming

 - A scheme to split up the monolithic JSON output into smaller files that can be loaded on the fly in the runtime. This was necessary on Sorcery! and 80 Days as the quantity of content increased substantially, and we were running our games on low-end iOS devices at the time.
     -  Our implementation for the previous version of ink was to have two files: one huge text file with lots of JSON snippets that never got loaded in one go, and an index file, which contained byte offsets and lengths for all the compiled knots in the game. This worked pretty well, although it meant that the compiled JSON was still in one huge file.
     -  A possible alternative we could consider for ink2 is to (optionally?) be able to have a one-to-one mapping between source `.ink` files and output `.json` files, so that the size and arrangement is predictable and controllable.
 - General refactoring and improvements to code structure and optimisation of the compiler.
 - Structured JSON-like data objects within ink format. Exact design still to be determined, but goals are for it to be a superset of JSON, so that it’s compatible, but can be simpler (a bit like YAML, though not YAML for various reasons). Would allow more complex hierarchical game state to be stored within the ink engine.

### To investigate

 - Consider changing multi-bullet weave indentation to Python-style whitespace indentation. This would be a huge syntax-breaking change, but we'd welcome a discussion and/or an experimental implementation.
 - Plugin architecture, to allow you to extract information from the ink while it's being compiled. Currently there's a basic example in the codebase, but it currently has to be built directly into the compiler, rather than via DLLs.
 - Audio and localisation. Difficult problems that need some thought.
 - Further succinctness improvements in JSON representation. We've rewritten it from scratch, but it could still do with a bit of work. Size can be a problem when you have 10MB+ of source ink, as we've had on past games.

## Architectural overview

See the [architectural overview documentation](https://github.com/inkle/ink/blob/master/Documentation/ArchitectureAndDevOverview.md) for information about the pipeline of the **ink** engine, and a birds-eye view of the project's code structure.

# License

**ink** is released under the [MIT license](https://github.com/inkle/ink/blob/master/LICENSE.txt). Although we don't require attribution, we'd love to know if you decide to use **ink** a project! Let us know on [Twitter](http://www.twitter.com/inkleStudios) or [by email](mailto:info@inklestudios.com).

# Support us!

**ink** is free forever, but represents multiple years of thought, design, development and testing. Please consider supporting us via [Patreon](http://www.patreon.com/inkle). Thank you, and have fun!
