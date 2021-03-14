FROM debian:buster-slim


RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        ca-certificates \
        \
        # .NET Core dependencies
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libicu63 \
        libssl1.1 \
        libstdc++6 \
        zlib1g \
        curl \
        git \
        procps \
        wget \
    && rm -rf /var/lib/apt/lists/*


ENV \
    # Enable detection of running in a container
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_INSTALL_DIR=/usr/share/dotnet/ \
    DOTNET_ROOT=/usr/share/dotnet/

COPY ./.devcontainer/install-dotnets.sh global.json* .

RUN /bin/bash install-dotnets.sh

ENV PATH="$DOTNET_ROOT:${PATH}"
RUN ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

RUN  dotnet --info

# Copy endpoint specific user settings into container to specify
# .NET Core should be used as the runtime.
COPY ./.devcontainer/settings.vscode.json /root/.vscode-remote/data/Machine/settings.json
