cd "`dirname "$0"`"
mono --profile=log:output=output.mlpd inklecate2Sharp/bin/Debug/inklecate2Sharp.exe -s > report-in-progress.txt
echo "----------------------------" >> report-in-progress.txt
mprof-report --verbose output.mlpd >> report-in-progress.txt 
rm output.mlpd
mv report-in-progress.txt report.txt