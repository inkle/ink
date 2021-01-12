cd "`dirname "$0"`"

# Build the release code; this will create self-sufficient single binary
#dotnet build -c Release inklecate/inklecate.csproj
dotnet publish -c Release -r win-x86 /p:PublishTrimmed=true /p:PublishSingleFile=true -o ReleaseBinary/inklecate/win32 inklecate/inklecate.csproj
dotnet publish -c Release -r linux-x64 /p:PublishTrimmed=true /p:PublishSingleFile=true -o ReleaseBinary/inklecate/lin64 inklecate/inklecate.csproj
dotnet publish -c Release -r osx-x64 /p:PublishTrimmed=true /p:PublishSingleFile=true -o ReleaseBinary/inklecate/osx64 inklecate/inklecate.csproj

# Simply zip up inklecate executable and the DLLs together for each platform
runtimeAndCompilerDLLs="ink-engine-runtime/bin/Release/netstandard2.0/ink-engine-runtime.dll compiler/bin/Release/netstandard2.0/ink_compiler.dll"
zip --junk-paths ReleaseBinary/inklecate_windows.zip ReleaseBinary/inklecate/win32/inklecate.exe $runtimeAndCompilerDLLs
zip --junk-paths ReleaseBinary/inklecate_linux.zip  ReleaseBinary/inklecate/lin64/inklecate $runtimeAndCompilerDLLs
zip --junk-paths ReleaseBinary/inklecate_mac.zip  ReleaseBinary/inklecate/osx64/inklecate $runtimeAndCompilerDLLs

# Clean up
rm -rf ReleaseBinary/inklecate
