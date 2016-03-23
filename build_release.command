cd "`dirname "$0"`"

# Build the release code
xbuild /p:Configuration=Release ink.sln

# Create folders
mkdir -p ReleaseBinary

# Windows: Simply zip up inklecate.exe, Newtonsoft.Json.dll and the runtime together
# We rely on a compatible version of .NET being installed on Windows
zip --junk-paths ReleaseBinary/inklecate_windows_and_linux.zip inklecate/bin/Release/Newtonsoft.Json.dll inklecate/bin/Release/inklecate.exe ink-engine-dll/bin/Release/ink-engine.dll

# Mac: Make a native binary that includes the mono runtime
# Prepare to bundle up compiled binary
export PATH=/Library/Frameworks/Mono.framework/Commands:$PATH
export AS="as -arch i386"
export CC="cc -arch i386 -framework CoreFoundation -lobjc -liconv"

# "Bundles in addition support a –static flag. The –static flag causes mkbundle to generate a static executable that statically links the Mono runtime. Be advised that this option will trigger the LGPL requirement that you still distribute the independent pieces to your user so he can manually upgrade his Mono runtime if he chooses to do so. Alternatively, you can obtain a proprietary license of Mono by contacting Xamarin."
# http://www.mono-project.com/archived/guiderunning_mono_applications/
mkbundle ./inklecate/bin/Release/inklecate.exe --deps --static -o ./ReleaseBinary/inklecate ./ink-engine-runtime/Newtonsoft.Json.dll
zip --junk-paths ReleaseBinary/inklecate_mac.zip ReleaseBinary/inklecate ink-engine-dll/bin/Release/ink-engine.dll inklecate/bin/Release/Newtonsoft.Json.dll
rm ReleaseBinary/inklecate