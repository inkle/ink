#!/bin/sh
# Fail if any individual command fails
# http://stackoverflow.com/questions/5195607/checking-bash-exit-status-of-several-commands-efficiently
set -e

cd "`dirname "$0"`"

# Build the release code
xbuild /p:Configuration=Release inklewriter.sln

# Prepare to bundle up compiled binary
export PATH=/Library/Frameworks/Mono.framework/Commands:$PATH
export AS="as -arch i386"
export CC="cc -arch i386 -framework CoreFoundation -lobjc -liconv"
mkdir -p ReleaseBinary

# "Bundles in addition support a –static flag. The –static flag causes mkbundle to generate a static executable that statically links the Mono runtime. Be advised that this option will trigger the LGPL requirement that you still distribute the independent pieces to your user so he can manually upgrade his Mono runtime if he chooses to do so. Alternatively, you can obtain a proprietary license of Mono by contacting Xamarin."
# http://www.mono-project.com/archived/guiderunning_mono_applications/
mkbundle ./inklecate/bin/Release/inklecate.exe --deps --static -o ./ReleaseBinary/inklecate ./inklewriter-engine-runtime/Newtonsoft.Json.dll

# Install
# Rename to inklecate2 to avoid possible collision with old inklecate
sudo cp ./ReleaseBinary/inklecate /usr/local/bin/inklecate2

# Copy latest syntax highlighting grammar into place for Sublime Text 2 and 3
sublime2Folder="$HOME/Library/Application Support/Sublime Text 2"
if [ -d "$sublime2Folder" ]; then
    sublime2Packages="$sublime2Folder/Packages/User"
    mkdir -p "$sublime2Packages"
    cp ./Sublime3Syntax/ink2.tmLanguage "$sublime2Packages"
fi

sublime3Folder="$HOME/Library/Application Support/Sublime Text 3"
if [ -d "$sublime3Folder" ]; then
    sublime3Packages="$sublime3Folder/Packages/User"
    mkdir -p "$sublime3Packages"
    cp ./Sublime3Syntax/ink2.tmLanguage "$sublime3Packages"
fi
