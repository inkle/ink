cd "`dirname "$0"`"
set -e

# Build the release code; this will create self-sufficient single binary
#dotnet build -c Release inklecate/inklecate.csproj
dotnet publish -c Release -r win-x86 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true -o ReleaseBinary/inklecate/win32 inklecate/inklecate.csproj
dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true -o ReleaseBinary/inklecate/lin64 inklecate/inklecate.csproj
dotnet publish -c Release -r osx-x64 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true -o ReleaseBinary/inklecate/osx64 inklecate/inklecate.csproj
dotnet publish -c Release -r osx-arm64 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true -o ReleaseBinary/inklecate/osxarm64 inklecate/inklecate.csproj

# We have both Intel and Apple Silicon binaries for Mac, we now join them together into a universal Mac build using lipo
mkdir -p ReleaseBinary/inklecate/osxuniversal
lipo -create ReleaseBinary/inklecate/osx64/inklecate ReleaseBinary/inklecate/osxarm64/inklecate -output ReleaseBinary/inklecate/osxuniversal/inklecate

# Simply zip up inklecate executable and the DLLs together for each platform
runtimeAndCompilerDLLs="ink-engine-runtime/bin/Release/netstandard2.0/ink-engine-runtime.dll compiler/bin/Release/netstandard2.0/ink_compiler.dll"
rm -f ReleaseBinary/inklecate_windows.zip ReleaseBinary/inklecate_linux.zip ReleaseBinary/inklecate_mac.zip
zip --junk-paths ReleaseBinary/inklecate_windows.zip ReleaseBinary/inklecate/win32/inklecate.exe $runtimeAndCompilerDLLs
zip --junk-paths ReleaseBinary/inklecate_linux.zip  ReleaseBinary/inklecate/lin64/inklecate $runtimeAndCompilerDLLs
zip --junk-paths ReleaseBinary/inklecate_mac.zip  ReleaseBinary/inklecate/osxuniversal/inklecate $runtimeAndCompilerDLLs

# Clean up
rm -rf ReleaseBinary/inklecate
