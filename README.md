# inklecate2

## Installing the compiler

Pre-requisites:

 * Install Xamarin
 * Install Xcode, and run, make sure it installs command line tools
 * Unfortunately you seem to open up the `.sln` and build the Release configuration before the install script works, initially? TODO: Fix this!

Run the `build_and_install_inklecate2.command` script. This will do two things:

 - Build the latest version of the compiler and install it in `/usr/local/bin/` with the name `inklecate2` so that you can access it from the command line.
 - Install syntax highlighting for Sublime Text 2 and 3.

 
## Building the runtime DLL (for Unity)

Run the `build-runtime-dll.command` script. This will build a DLL file and place it in `RuntimeDLL/ink-engine.dll`.

