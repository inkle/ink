cd "`dirname "$0"`"

# Build the release code
#dotnet build -c Release inklecate/inklecate.csproj
dotnet publish -c Release -r win-x86 --self-contained false inklecate/inklecate.csproj
dotnet publish -c Release -r linux-x64 --self-contained false inklecate/inklecate.csproj
#dotnet publish -c Release -r osx-x64 --self-contained false inklecate/inklecate.csproj

# Create folders
mkdir -p ReleaseBinary

# We rely on a compatible version of .NET Core being installed on the target machine
# Simply zip up inklecate executable and the runtime together
zip --junk-paths ReleaseBinary/inklecate_windows.zip inklecate/bin/Release/netcoreapp2.2/win-x86/publish/inklecate.exe inklecate/bin/Release/netcoreapp2.2/win-x86/publish/*.dll inklecate/bin/Release/netcoreapp2.2/win-x86/publish/*.json
zip --junk-paths ReleaseBinary/inklecate_linux.zip inklecate/bin/Release/netcoreapp2.2/linux-x64/publish/inklecate inklecate/bin/Release/netcoreapp2.2/linux-x64/publish/*.dll inklecate/bin/Release/netcoreapp2.2/linux-x64/publish/*.json
#zip --junk-paths ReleaseBinary/inklecate_mac.zip inklecate/bin/Release/netcoreapp2.2/osx-x64/publish/inklecate inklecate/bin/Release/netcoreapp2.2/osx-x64/publish/*.dll inklecate/bin/Release/netcoreapp2.2/osx-x64/publish/*.json

# Mac: Make a native binary that includes the mono runtime
# Prepare to bundle up compiled binary
export PATH=/Library/Frameworks/Mono.framework/Commands:$PATH
export AS="as -arch i386"
export CC="cc -arch i386 -framework CoreFoundation -lobjc -liconv"

# "Bundles in addition support a –static flag. The –static flag causes mkbundle to generate a static executable that statically links the Mono runtime. Be advised that this option will trigger the LGPL requirement that you still distribute the independent pieces to your user so he can manually upgrade his Mono runtime if he chooses to do so. Alternatively, you can obtain a proprietary license of Mono by contacting Xamarin."
# http://www.mono-project.com/archived/guiderunning_mono_applications/
mkbundle ./inklecate/bin/Release/netcoreapp2.2/win-x86/publish/inklecate.exe ./inklecate/bin/Release/netcoreapp2.2/win-x86/publish/inklecate.dll ./inklecate/bin/Release/netcoreapp2.2/win-x86/publish/ink-engine-runtime.dll ./inklecate/bin/Release/netcoreapp2.2/win-x86/publish/ink_compiler.dll --deps --static --sdk /Library/Frameworks/Mono.framework/Versions/Current -o ./ReleaseBinary/inklecate -L ./inklecate/bin/Release/netcoreapp2.2/win-x86/publish
zip --junk-paths ReleaseBinary/inklecate_mac.zip ReleaseBinary/inklecate inklecate/bin/Release/netcoreapp2.2/win-x86/publish/inklecate.dll inklecate/bin/Release/netcoreapp2.2/win-x86/publish/ink-engine-runtime.dll inklecate/bin/Release/netcoreapp2.2/win-x86/publish/ink_compiler.dll

rm ReleaseBinary/inklecate
