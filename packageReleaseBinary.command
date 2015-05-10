cd "`dirname "$0"`"
export PATH=/Library/Frameworks/Mono.framework/Commands:$PATH
export AS="as -arch i386"
export CC="cc -arch i386 -framework CoreFoundation -lobjc -liconv"
mkdir -p ReleaseBinary
mkbundle ./inklecate2Sharp/bin/Debug/inklecate2Sharp.exe -o ./ReleaseBinary/inklecate2