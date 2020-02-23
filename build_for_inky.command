cd "`dirname "$0"`"

# Build the debug code
#dotnet build inlkecate/inklecate.csproj
dotnet publish -c Release -r win-x86 /p:PublishTrimmed=true;PublishSingleFile=true -o BuildForInky inklecate/inklecate.csproj
dotnet publish -c Release -r osx-x64 /p:PublishTrimmed=true;PublishSingleFile=true -o BuildForInky inklecate/inklecate.csproj

mv BuildForInky/inklecate.exe BuildForInky/inklecate_win.exe
mv BuildForInky/inklecate BuildForInky/inklecate_mac
