#!/bin/bash

# Get the directory where this script is located
scriptDir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Remove the Generated directory
rm -rf "${scriptDir}/Generated"

# Run the ivy-docs-cli command to regenerate files
dotnet tool restore
dotnet ivy-docs-cli convert "${scriptDir}/Docs" "${scriptDir}/Generated"
