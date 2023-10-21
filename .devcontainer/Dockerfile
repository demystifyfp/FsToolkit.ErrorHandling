# [Choice] Debian version (use bullseye on local arm64/Apple Silicon): bookworm, bullseye, buster
ARG VARIANT="bookworm"
FROM buildpack-deps:${VARIANT}-curl


ENV \
    # Enable detection of running in a container
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_ROOT=/usr/share/dotnet/ \
    DOTNET_NOLOGO=true \
    DOTNET_CLI_TELEMETRY_OPTOUT=false


# [Optional] Uncomment this section to install additional OS packages.
# RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
#     && apt-get -y install --no-install-recommends <your-package-list-here>
