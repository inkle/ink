# ink sublime syntax

## Quickstart

**Mac**: Double-click the `install_for_sublime2and3.command` script. This will copy the right files into the right place for Sublime Text 2 and/or 3.

**Windows** (untested): Copy the "files to be installed" into Sublime's `Packages/User` directory.

(TODO: We should get this added to [Sublime Text's Package Control](https://packagecontrol.io/).)

## What's included

### Files to be installed

 * `ink.tmLanguage`: This is the file compiled using the AAAPackageDev package in *Sublime Text 3*, and is simply an uglier plist-based XML version of the YAML grammar.
 * `ink.tmTheme`: A custom colour scheme for using ink. Unfortunately, it's necessary to use this since ink requires unique semantic markup that doesn't map very nicely to standard programming and markup concepts. We'd welcome other themes (like the dark version `ink-dark.thTheme`) that use the ink symbol names.
 * `ink.sublime-settings`: Choose the above colour scheme by default and turns on word wrapping by default for ink. If you want to use the alternate dark scheme, you may change it there.
 * `ink-comments.tmPreferences`: Defines characters to insert when user uses comment shortcut in Sublime.
 * `ink-global-symbols.tmPreferences` and `ink-local-symbols.tmPreferences`: Defines which symbols appear in Sublime's *Goto Symbol...* and *Goto Symbol In Project...* options.

### Other files

 * `ink.YAML-tmLanguage`: This is the main source file for the syntax
 * `LiveWatchAndInstallOnEdit.command` - when continuously editing the above the files, you can run this script so that it installs them automatically as you save changes to them (Mac only).

(Note: Unfortunately we can't use the alternative `.sublime-syntax` ([documentation here](https://www.sublimetext.com/docs/3/syntax.html)) just yet since it's not available for non-dev builds of Sublime Text 3 yet.)


## Syntax file development

(Workflow designed for Mac.)

1. Install the AAAPackageDev file in Sublime Text
2. Run `LiveWatchAndInstallOnEdit.command`.
3. Make edits to the `ink2.YAML-tmLanguage` file (or other files listed above).
4. CMD-B to build the language file. The first time after opening it, it'll ask you which file type to compile to - choose **Propery List**. It will then generate the compiled `.tmLanguage` file.
5. The live watch script will copy the built files into the right place (or alternatively if you don't want to install `fswatch`, you can just run the manual install script or do it yourself.)

Some helpful links:

 - <http://sublimetext.info/docs/en/extensibility/syntaxdefs.html> - Despite being apparently out of date, I found this to be a helpful and clear tutorial
 - <http://manual.macromates.com/en/language_grammars#language_grammars> - Original TextMate tutorial that Sublime Text's grammars are based off
 - <http://stackoverflow.com/questions/10834765/where-to-find-a-list-of-scopes-for-sublime2-or-textmate> - Mirror of all the scope names available for colour highlighting
 - <http://sublime-text-unofficial-documentation.readthedocs.org/en/latest/reference/syntaxdefs.html> - The most up to date reference available
 - <http://ruby-doc.org/core-2.1.1/Regexp.html> - Sublime Text uses Ruby flavoured regexes
