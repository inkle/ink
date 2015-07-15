#!/bin/bash
cd "`dirname "$0"`"

# Build the dll into the following directory
mkdir -p RuntimeDLL

# Unity requires SDK 2.x
mcs -t:library -r:inklewriter-engine-runtime/Newtonsoft.Json.dll -out:RuntimeDLL/inklewriter-engine.dll -sdk:2 inklewriter-engine-runtime/*.cs