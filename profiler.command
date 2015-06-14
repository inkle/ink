cd "`dirname "$0"`"
mono --profile=log:output=output.mlpd inklecate2Sharp/bin/Debug/inklecate2Sharp.exe -s
mprof-report --verbose output.mlpd > report.txt 
rm output.mlpd
open report.txt