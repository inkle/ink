cd "`dirname "$0"`"
mono --profile=log:output=output.mlpd inklecate/bin/Release/net6.0/inklecate.dll -s > report-in-progress.txt
echo "----------------------------" >> report-in-progress.txt
mprof-report --verbose output.mlpd >> report-in-progress.txt  # use --time=10.0-20.0 to select a particular time period
rm output.mlpd
mv report-in-progress.txt report.txt
