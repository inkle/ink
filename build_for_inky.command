cd "`dirname "$0"`"

# Build for each platform
dotnet publish -c Release -r win-x86 /p:PublishTrimmed=true /p:PublishSingleFile=true -o BuildForInky inklecate/inklecate.csproj
mv BuildForInky/inklecate.exe BuildForInky/inklecate_win.exe

dotnet publish -c Release -r osx-x64 /p:PublishTrimmed=true /p:PublishSingleFile=true -o BuildForInky inklecate/inklecate.csproj
mv BuildForInky/inklecate BuildForInky/inklecate_mac

dotnet publish -c Release -r linux-x64 /p:PublishTrimmed=true /p:PublishSingleFile=true -o BuildForInky inklecate/inklecate.csproj
mv BuildForInky/inklecate BuildForInky/inklecate_linux


# Copy the runtime and compiler debug symbols in
cp ink-engine-runtime/bin/Release/netstandard2.0/ink-engine-runtime.pdb BuildForInky/
cp compiler/bin/Release/netstandard2.0/ink_compiler.pdb BuildForInky/