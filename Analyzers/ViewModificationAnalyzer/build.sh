#!/bin/bash

# Navigate to the directory of this script
cd "$(dirname "$0")"

echo "Building ViewDataRestrictionAnalyzer..."

# Run dotnet build
# The .csproj is already configured to output to ../../Assets/scripts/rules
dotnet build ViewModificationAnalyzer.csproj -c Release

if [ $? -eq 0 ]; then
    echo "Build successful! DLL updated in Assets/scripts/rules"
else
    echo "Build failed!"
    exit 1
fi
