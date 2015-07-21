# ink2 sublime syntax
There are two files in this directory:

 * `ink2.YAML-tmLanguage`: This is the source file
 * `ink2.tmLanguage`: This is the file compiled using the AAAPackageDev package in *Sublime Text 3*, and is simply an uglier plist-based XML version of the YAML grammar.


### To install

1. Make sure you have *Sublime Text 3* from <http://www.sublimetext.com/3>
2. Open it and navigate to:

	Sublime Text 3 > Preferences > Browse Packages...
	
3. Copy the `ink2.tmLanguage` file into the User directory


### Editing the syntax file

1. Install the AAAPackageDev file in Sublime Text
2. Make edits to the ink2.YAML-tmLanguage file
3. It should auto-detect the file type, so that when you build with CMD-B, it generates the compiled `.tmLanguage` file.

Some helpful links:

 - <http://sublimetext.info/docs/en/extensibility/syntaxdefs.html> - Despite being apparently out of date, I found this to be a helpful and clear tutorial
 - <http://manual.macromates.com/en/language_grammars#language_grammars> - Original TextMate tutorial that Sublime Text's grammars are based off
 - <http://stackoverflow.com/questions/10834765/where-to-find-a-list-of-scopes-for-sublime2-or-textmate> - Mirror of all the scope names available for colour highlighting
 - <http://sublime-text-unofficial-documentation.readthedocs.org/en/latest/reference/syntaxdefs.html> - The most up to date reference available
 - <http://ruby-doc.org/core-2.1.1/Regexp.html> - Sublime Text uses Ruby flavoured regexes