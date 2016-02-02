# ink

[![Gitter](https://badges.gitter.im/inkle/ink.svg)](https://gitter.im/inkle/ink?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

Ink is [inkle](http://www.inklestudios.com/)'s scripting language for writing interactive narrative, both for text-centric games as well as more graphical games that contain highly branching stories. It's designed to be easy to learn, but with powerful enough features to allow an advanced level of structuring.

 * **inklecate** is the command-line compiler for ink. It takes one or more text files with an `.ink` extension, and produces a `.json` file. It can also be used in *play* mode, for testing a story on the command line.
 * The **ink runtime engine** is a C# library that can be used within Unity or other C# environment.

**Warning:** **ink** is in alpha. Features may change, bugs may be encountered. We're yet to complete a project with this major rewrite of ink - it's a work in progress!


# Getting started

## Writing

Note that you will require basic knowledge of the Mac command line.

 * Install **inklecate** by following the instructions below
 * Create a text file called `myStory.ink`, containing the text `Hello, world!`.
 * On the command line, run: `inklecate2 -p myStory.ink`. The `-p` option uses play mode so that you can see the result immediately.
 * Follow the tutorial: [Writing with Ink](https://github.com/inkle/ink/blob/master/Documentation/ArchitectureAndDevOverview.md).

## Installing *inklecate* - the ink compiler

Pre-requisites:

 * Install [Xamarin](https://xamarin.com/download)
 * Install [Xcode](https://itunes.apple.com/app/xcode/id497799835), and run, make sure it [installs command line tools](http://stackoverflow.com/questions/9329243/xcode-4-4-and-later-install-command-line-tools).

Run the `build_and_install_inklecate2.command` script by double clicking it. This will:

 - Build the latest version of the compiler and install it in `/usr/local/bin/` with the name `inklecate2` so that you can access it from the command line.

 
## Building the runtime DLL (for Unity)

Run the `build-runtime-dll.command` script. This will build a DLL file and place it in `RuntimeDLL/ink-engine.dll`.

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
 - Audio and localisation. Difficult problems that need some thought.

## Architectural overview

See the [architectural overview documentation](https://github.com/inkle/ink/blob/master/Documentation/ArchitectureAndDevOverview.md) for information about the pipeline of the **ink** engine, and a birds-eye view of the project's code structure.