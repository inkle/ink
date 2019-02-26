cd "`dirname "$0"`"

# Build the debug code
#dotnet build inlkecate/inklecate.csproj
dotnet publish -c Release -r win-x86 --self-contained false inklecate/inklecate.csproj

# Create folders
mkdir -p BuildForInky

# Mac: Make a native binary that includes the mono runtime
# Prepare to bundle up compiled binary
export PATH=/Library/Frameworks/Mono.framework/Commands:$PATH
export AS="as -arch i386"
export CC="cc -arch i386 -framework CoreFoundation -lobjc -liconv"

# "Bundles in addition support a –static flag. The –static flag causes mkbundle to generate a static executable that statically links the Mono runtime. Be advised that this option will trigger the LGPL requirement that you still distribute the independent pieces to your user so he can manually upgrade his Mono runtime if he chooses to do so. Alternatively, you can obtain a proprietary license of Mono by contacting Xamarin."
# http://www.mono-project.com/archived/guiderunning_mono_applications/
cd ./inklecate/bin/Debug/netcoreapp2.2/win-x86/publish/
mkbundle --static --sdk /Library/Frameworks/Mono.framework/Versions/Current --deps inklecate.exe inklecate.dll ink-engine-runtime.dll ink_compiler.dll -o inklecate_mac -L ./inklecate/bin/Release/netcoreapp2.2/win-x86/publish

# TODO: See if you can whittle down the dependencies a bit instead of using the --deps option above?
# It mentions all of the dependencies below, but I'm not convinced they're all necessary?
# /Library/Frameworks/Mono.framework/Versions/4.2.4/lib/mono 
#      /4.5/mscorlib.dll
#      /gac/System.Core/4.0.0.0__b77a5c561934e089/System.Core.dll
#      /gac/System/4.0.0.0__b77a5c561934e089/System.dll
#      /gac/Mono.Security/4.0.0.0__0738eb9f132ed756/Mono.Security.dll
#      /gac/System.Configuration/4.0.0.0__b03f5f7f11d50a3a/System.Configuration.dll
#      /gac/System.Xml/4.0.0.0__b77a5c561934e089/System.Xml.dll
#      /gac/System.Security/4.0.0.0__b03f5f7f11d50a3a/System.Security.dll
#      /gac/Mono.Posix/4.0.0.0__0738eb9f132ed756/Mono.Posix.dll
#      /gac/System.Numerics/4.0.0.0__b77a5c561934e089/System.Numerics.dll
#      /gac/System.Xml.Linq/4.0.0.0__b77a5c561934e089/System.Xml.Linq.dll
#      /gac/System.Runtime.Serialization/4.0.0.0__b77a5c561934e089/System.Runtime.Serialization.dll
#      /gac/System.ServiceModel.Internals/0.0.0.0__b77a5c561934e089/System.ServiceModel.Internals.dll
#      /gac/System.Data/4.0.0.0__b77a5c561934e089/System.Data.dll
#      /gac/System.Transactions/4.0.0.0__b77a5c561934e089/System.Transactions.dll
#      /gac/Mono.Data.Tds/4.0.0.0__0738eb9f132ed756/Mono.Data.Tds.dll
#      /gac/System.EnterpriseServices/4.0.0.0__b03f5f7f11d50a3a/System.EnterpriseServices.dll

cp inklecate_mac ../../../../../../BuildForInky/
cp inklecate.exe ../../../../../../BuildForInky/inklecate_win.exe
cp *.dll ../../../../../../BuildForInky
cp *.pdb ../../../../../../BuildForInky
