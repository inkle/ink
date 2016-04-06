#!/bin/sh
# Fail if any individual command fails
# http://stackoverflow.com/questions/5195607/checking-bash-exit-status-of-several-commands-efficiently
set -e

cd "`dirname "$0"`"


# Copy latest syntax highlighting grammar into place for Sublime Text 2 and 3
sublime2Folder="$HOME/Library/Application Support/Sublime Text 2"
if [ -d "$sublime2Folder" ]; then
    sublime2Packages="$sublime2Folder/Packages/User"
    mkdir -p "$sublime2Packages"
    cp ./ink.tmLanguage "$sublime2Packages"
    cp ./ink.tmTheme "$sublime2Packages"
    cp ./ink-dark.tmTheme "$sublime2Packages"
    cp ./ink.sublime-settings "$sublime2Packages"
    cp ./ink-global-symbols.tmPreferences "$sublime2Packages"
    cp ./ink-local-symbols.tmPreferences "$sublime2Packages"
    cp ./ink-comments.tmPreferences "$sublime2Packages"
fi

sublime3Folder="$HOME/Library/Application Support/Sublime Text 3"
if [ -d "$sublime3Folder" ]; then
    sublime3Packages="$sublime3Folder/Packages/User"
    mkdir -p "$sublime3Packages"
    cp ./ink.tmLanguage "$sublime3Packages"
    cp ./ink.tmTheme "$sublime3Packages"
    cp ./ink-dark.tmTheme "$sublime3Packages"
    cp ./ink.sublime-settings "$sublime3Packages"
    cp ./ink-global-symbols.tmPreferences "$sublime3Packages"
    cp ./ink-local-symbols.tmPreferences "$sublime3Packages"
    cp ./ink-comments.tmPreferences "$sublime3Packages"
fi
