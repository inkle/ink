cd "`dirname "$0"`"

# Ensure we have latest packages
mono NuGet.exe restore ink.sln

# Build the release code
xbuild /p:Configuration=Release ink.sln

# Create folders
mkdir -p ReleaseBinary

# Windows: Simply zip up inklecate.exe and the runtime together
# We rely on a compatible version of .NET being installed on Windows
zip --junk-paths ReleaseBinary/inklecate_windows_and_linux.zip inklecate/bin/Release/inklecate.exe ink-engine-runtime/bin/Release/ink-engine-runtime.dll ink-engine-runtime/bin/Release/ink-engine-runtime.xml

# Mac: Make a native binary that includes the mono runtime
# Prepare to bundle up compiled binary
export PATH=/Library/Frameworks/Mono.framework/Commands:$PATH
export AS="as -arch i386"
export CC="cc -arch i386 -framework CoreFoundation -lobjc -liconv"

# "Bundles in addition support a –static flag. The –static flag causes mkbundle to generate a static executable that statically links the Mono runtime. Be advised that this option will trigger the LGPL requirement that you still distribute the independent pieces to your user so he can manually upgrade his Mono runtime if he chooses to do so. Alternatively, you can obtain a proprietary license of Mono by contacting Xamarin."
# http://www.mono-project.com/archived/guiderunning_mono_applications/
mkbundle ./inklecate/bin/Release/inklecate.exe ./inklecate/bin/Release/ink-engine-runtime.dll --deps --static -o ./ReleaseBinary/inklecate
zip --junk-paths ReleaseBinary/inklecate_mac.zip ReleaseBinary/inklecate ink-engine-runtime/bin/Release/ink-engine-runtime.dll

rm ReleaseBinary/inklecate
