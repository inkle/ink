cd "`dirname "$0"`"
set -e

# Build for each platform
dotnet publish -c Release -r win-x86 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true -o BuildForInky inklecate/inklecate.csproj
mv BuildForInky/inklecate.exe BuildForInky/inklecate_win.exe

# Build both Intel and Apple Silicon binaries, then join them together into a universal Mac build using lipo
rm -rf BuildForInky/osx-x64 BuildForInky/osx-arm64
dotnet publish -c Release -r osx-x64 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true -o BuildForInky/osx-x64 inklecate/inklecate.csproj
dotnet publish -c Release -r osx-arm64 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true -o BuildForInky/osx-arm64 inklecate/inklecate.csproj
rm -f BuildForInky/inklecate_mac
lipo -create BuildForInky/osx-x64/inklecate BuildForInky/osx-arm64/inklecate -output BuildForInky/inklecate_mac
rm -rf BuildForInky/osx-x64 BuildForInky/osx-arm64

dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true -o BuildForInky inklecate/inklecate.csproj
mv BuildForInky/inklecate BuildForInky/inklecate_linux


# Copy the runtime and compiler debug symbols in
cp ink-engine-runtime/bin/Release/netstandard2.0/ink-engine-runtime.pdb BuildForInky/
cp compiler/bin/Release/netstandard2.0/ink_compiler.pdb BuildForInky/
