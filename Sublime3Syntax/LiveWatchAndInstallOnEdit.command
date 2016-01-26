#!/bin/sh
cd "`dirname "$0"`"

# Check for existence of fswatch
command -v fswatch >/dev/null 2>&1 || { echo >&2 "ERROR: 'fswatch' is required to run this script! Please see LiveUpdateReadme.md"; exit 1; }

# Run an initial make, then watch for ink files changing
./install_for_sublime2and3.command
fswatch -0 . | xargs -0 -n 1 -I{} ./install_for_sublime2and3.command