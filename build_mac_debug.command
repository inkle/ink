cd "`dirname "$0"`"

# Build the debug code
xbuild /p:Configuration=Debug ink.sln

# Create folders
mkdir -p DebugMacBinary

# Mac: Make a native binary that includes the mono runtime
# Prepare to bundle up compiled binary
export PATH=/Library/Frameworks/Mono.framework/Commands:$PATH
export AS="as -arch i386"
export CC="cc -arch i386 -framework CoreFoundation -lobjc -liconv"

# "Bundles in addition support a –static flag. The –static flag causes mkbundle to generate a static executable that statically links the Mono runtime. Be advised that this option will trigger the LGPL requirement that you still distribute the independent pieces to your user so he can manually upgrade his Mono runtime if he chooses to do so. Alternatively, you can obtain a proprietary license of Mono by contacting Xamarin."
# http://www.mono-project.com/archived/guiderunning_mono_applications/
cd ./inklecate/bin/Debug/
mkbundle --static inklecate.exe Newtonsoft.Json.dll -o inklecate

cp inklecate ../../../DebugMacBinary/
cp inklecate.exe.mdb ../../../DebugMacBinary/
