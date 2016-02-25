# ink

[![Gitter](https://badges.gitter.im/inkle/ink.svg)](https://gitter.im/inkle/ink?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

Ink is [inkle](http://www.inklestudios.com/)'s scripting language for writing interactive narrative, both for text-centric games as well as more graphical games that contain highly branching stories. It's designed to be easy to learn, but with powerful enough features to allow an advanced level of structuring.

Want to know what **ink** looks like? Take a look at the [writing documentation](https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md).

Broadly, the engine is made up of two components:

 * **inklecate** is the command-line compiler for ink. It takes one or more text files with an `.ink` extension, and produces a `.json` file. It can also be used in *play* mode, for testing a story on the command line.
 * The **ink runtime engine** is a C# library that can be used within Unity or other C# environment.

**Warning:** **ink** is in alpha. Features may change, bugs may be encountered. We're yet to complete a project with this major rewrite of ink - it's a work in progress!


# Getting started

## Writing

**Warning:** Since the engine is in alpha, it hasn't been neatly packaged up for non-technical writers. Right now, you need to build the compiler yourself, and you need basic knowledge of the command line to try out your stories.

 * Build **inklecate** by following the instructions below
 * Create a text file called `myStory.ink`, containing the text `Hello, world!`.
 * On the command line, run: `inklecate -p myStory.ink`. The `-p` option uses play mode so that you can see the result immediately.
 * Follow the tutorial: [Writing with Ink](https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md).

## Building

### Requirements

**Windows:**
    
 * [Visual Studio](https://www.visualstudio.com/) (e.g. Community edition), [Xamarin](https://xamarin.com/download), or Unity's own version of MonoDevelop.
    
**Mac:**
    
 * [Xamarin](https://xamarin.com/download), or Unity's own version of MonoDevelop

### Build

1. Load up the solution file - `ink.sln`.
2. Select the *Release* configuration and choose *Build -> Build All* (or *Build Solution* in Visual Studio).
3. The compiler binary should be built in `inklecate/bin/Release` (or `x86`), while the runtime engine DLL will be built in `ink-engine-dll/bin/Release/ink-engine.dll`.
4. Optionally, you may want to install **inklecate** at a system level (e.g. on Mac copy to `/usr/local/bin`).

### Build Mac binary of *inklecate* with no dependencies

The above build instructions will build a binary that relies on the Mono runtime being available on the user's system. The following script is a way of building the mono runtime directly into the binary. (Perhaps we should do this in future.)

    # Build the release code
    xbuild /p:Configuration=Release ink.sln
    
    # Prepare to bundle up compiled binary
    export PATH=/Library/Frameworks/Mono.framework/Commands:$PATH
    export AS="as -arch i386"
    export CC="cc -arch i386 -framework CoreFoundation -lobjc -liconv"
    mkdir -p ReleaseBinary
    
    # "Bundles in addition support a –static flag. The –static flag causes mkbundle to generate a static executable that statically links the Mono runtime. Be advised that this option will trigger the LGPL requirement that you still distribute the independent pieces to your user so he can manually upgrade his Mono runtime if he chooses to do so. Alternatively, you can obtain a proprietary license of Mono by contacting Xamarin."
    # http://www.mono-project.com/archived/guiderunning_mono_applications/
    mkbundle ./inklecate/bin/Release/inklecate.exe --deps --static -o ./ReleaseBinary/inklecate ./ink-engine-runtime/Newtonsoft.Json.dll
    



# The development of ink

## How to contribute

We’d of course appreciate any bug fixes you might find! Also see the roadmap below for future planned features and optimisations that you might be able to help out with.

[Create a GitHub issue](https://github.com/inkle/ink/issues/new) if you want to start a discussion or request a feature. (Is this the best place for community discussion? We're pretty new to open source!) Or if you want to get in touch with us directly, [email us](mailto:info@inklestudios.com).

We also have a room on Gitter where you may find someone to help you:

[![Gitter](https://badges.gitter.im/inkle/ink.svg)](https://gitter.im/inkle/ink?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

In terms of related projects outside of the scope of this repo, we'd love to see the following since we don't have time to do it ourselves right now, and we think it would substantially help the community:

 * A more friendly install-edit-play flow for writers. For example, a downloadable GUI-based app with an editor pane on the left and a play pane on the right. Bonus points if the play pane automatically reloads the state of the story as you type!
 * Implementations of the runtime engine in other languages - for example JavaScript, so that stories can be run on the web.
 * Unity template project to demonstrate how to set up a particular style of game.

## Roadmap

Internally we've been thinking about the following. We can't guarantee we'll implement them any time soon though, or indeed at all!

### Definitely coming

 - Save state from runtime
 - Punctuation and whitespace cleaner. Although the ink engine does the best it can at fixing various issues such including the right amount of whitespace, there are certain things that are hard or impossible to deal with, due to the text being inherently interactive and unpredictable. For *Sorcery!* and *80 Days* we had a cleaning function which tidied up spacing and punctuation, and we intend to do the same with this latest version of the ink engine. A similar feature exists in HTML due to the inclusion of markup within text - for example, multiple spaces are collapsed down into one.
 - Improve succinctness of JSON representation - it’s currently much larger than it needs to be. Big problem when you have 10MB+ of source ink, as we've had on past games.
 - Bug fixes!
 
### Probably coming

 - A scheme to split up the monolithic JSON output into smaller files that can be loaded on the fly in the runtime. This was necessary on Sorcery! and 80 Days as the quantity of content increased substantially, and we were running our games on low-end iOS devices at the time.
     -  Our implementation for the previous version of ink was to have two files: one huge text file with lots of JSON snippets that never got loaded in one go, and an index file, which contained byte offsets and lengths for all the compiled knots in the game. This worked pretty well, although it meant that the compiled JSON was still in one huge file.
     -  A possible alternative we could consider for ink2 is to (optionally?) be able to have a one-to-one mapping between source `.ink` files and output `.json` files, so that the size and arrangement is predictable and controllable.
 - Some kind of enum / flag system. Design yet to be determined.
 - General refactoring and improvements to code structure and optimisation of the compiler.
 - Structured JSON-like data objects within ink format. Exact design still to be determined, but goals are for it to be a superset of JSON, so that it’s compatible, but can be simpler (a bit like YAML, though not YAML for various reasons). Would allow more complex hierarchical game state to be stored within the ink engine.

### To investigate

 - Consider changing multi-bullet weave indentation to Python-style whitespace indentation. This would be a huge syntax-breaking change, but we'd welcome a discussion and/or an experimental implementation.
 - Plugin architecture, to allow you to extract information from the ink while it's being compiled. Currently there's a basic example in the codebase, but it currently has to be built directly into the compiler, rather than via DLLs.
 - Audio and localisation. Difficult problems that need some thought.

## Architectural overview

See the [architectural overview documentation](https://github.com/inkle/ink/blob/master/Documentation/ArchitectureAndDevOverview.md) for information about the pipeline of the **ink** engine, and a birds-eye view of the project's code structure.

# License

**ink** is released under the MIT license. Although we don't require attribution, we'd love to know if you decide to use **ink** a project! Let us know on [Twitter](http://www.twitter.com/inkleStudios) or [by email](mailto:info@inklestudios.com).

### The MIT License (MIT)
Copyright (c) 2016 inkle Ltd.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.