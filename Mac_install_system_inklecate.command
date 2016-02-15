#!/bin/sh
# Fail if any individual command fails
# http://stackoverflow.com/questions/5195607/checking-bash-exit-status-of-several-commands-efficiently
set -e

cd "`dirname "$0"`"

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

# Install
sudo cp ./ReleaseBinary/inklecate /usr/local/bin/inklecate
