# ink

Ink is [inkle](http://www.inklestudios.com/)'s scripting language for writing interactive narrative, both for text-centric games as well as more graphical games that contain highly branching stories. It's designed to be easy to learn, but with powerful enough features to allow an advanced level of structuring.

 * **inklecate** is the command-line compiler for ink. It takes one or more text files with an `.ink` extension, and produces a `.json` file. It can also be used in *play* mode, for testing a story on the command line.
 * The **ink runtime engine** is a C# library that can be used within Unity or other C# environment.


# Getting started

## Writing

Note that you will require basic knowledge of the Mac command line.

 * Install **inklecate** by following the instructions below
 * Create a text file called `myStory.ink`, containing the text `Hello, world!`.
 * On the command line, run: `inklecate2 -p myStory.ink`. The `-p` option uses play mode so that you can see the result immediately.
 * Follow the tutorial in `Documentation/Tutorial.md`.

## Installing *inklecate* - the ink compiler

Pre-requisites:

 * Install [Xamarin](https://xamarin.com/download)
 * Install [Xcode](https://itunes.apple.com/app/xcode/id497799835), and run, make sure it [installs command line tools](http://stackoverflow.com/questions/9329243/xcode-4-4-and-later-install-command-line-tools).

Run the `build_and_install_inklecate2.command` script by double clicking it. This will:

 - Build the latest version of the compiler and install it in `/usr/local/bin/` with the name `inklecate2` so that you can access it from the command line.

 
## Building the runtime DLL (for Unity)

Run the `build-runtime-dll.command` script. This will build a DLL file and place it in `RuntimeDLL/ink-engine.dll`.

# inklecate's source

TODO: Overview of inklecate, including:

 * Parser and runtime export pipeline
 * Runtime engine