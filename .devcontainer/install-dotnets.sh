#!/bin/bash

# downloads installer script https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script
curl -SL --output dotnet-install.sh https://dot.net/v1/dotnet-install.sh

# Attempt to install via global.json first
FILE=global.json
if test -f "$FILE"; then
    echo "installing dotnet via $FILE"
    /bin/bash dotnet-install.sh --verbose --jsonfile $FILE
fi

# Add additional versions if required
DOTNET_VERSIONS=(
    'latest'
    'LTS'
    'STS'
)
for version in ${DOTNET_VERSIONS[@]}; do
    echo "installing dotnet $version"
    /bin/bash dotnet-install.sh --verbose --channel $version
done
