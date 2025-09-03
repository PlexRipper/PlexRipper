ARG VERSION=0.0.0
ARG INFORMATIONAL_VERSION=0.0.0
ARG PORT=7000
ARG TARGETPLATFORM=linux/amd64
ARG BUILDPLATFORM=linux/amd64

# Stage 1 - Build the Nuxt front-end
# TODO: bun 1.2.1 breaks build so its pinned to this for now
FROM --platform=$BUILDPLATFORM oven/bun:1.1.45-alpine AS client-build

ARG PORT

ENV NUXT_HOST=0.0.0.0
ENV NUXT_PORT=${PORT}
ENV NUXT_PUBLIC_API_PORT=${PORT}
ENV NUXT_PUBLIC_IS_DOCKER=true

WORKDIR /tmp/ClientApp

## Copy the package.json and install the dependencies
COPY ./src/WebAPI/ClientApp/package.json ./src/WebAPI/ClientApp/bun.lockb ./
RUN bun install --frozen-lockfile

## Copy the project files
COPY ./src/WebAPI/ClientApp/ ./
RUN bun run generate --fail-on-error
    
# Stage 2 - Build the .NET back-end
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS back-end

ARG VERSION
ARG INFORMATIONAL_VERSION

WORKDIR /src

## Speed up and stabilize dotnet restore/publish
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    NUGET_XMLDOC_MODE=skip

## Domain Projects
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Environment/Environment.csproj", "src/Environment/"]
COPY ["src/FluentResultExtensions/FluentResultExtensions.csproj", "src/FluentResultExtensions/"]
COPY ["src/Logging/Logging.csproj", "src/Logging/"]

## Core Projects
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Application.Contracts/Application.Contracts.csproj", "src/Application.Contracts/"]

## Infrastructure Projects
### Data Access
COPY ["src/Data/Data.csproj", "src/Data/"]
COPY ["src/Data.Contracts/Data.Contracts.csproj", "src/Data.Contracts/"]
### Data Identity Access
COPY ["src/Identity/Identity.csproj", "src/Identity/"]
COPY ["src/Identity.Contracts/Identity.Contracts.csproj", "src/Identity.Contracts/"]
### File System
COPY ["src/FileSystem/FileSystem.csproj", "src/FileSystem/"]
COPY ["src/FileSystem.Contracts/FileSystem.Contracts.csproj", "src/FileSystem.Contracts/"]
### Plex API
COPY ["src/PlexApi/PlexApi.csproj", "src/PlexApi/"]
COPY ["src/PlexApi.Contracts/PlexApi.Contracts.csproj", "src/PlexApi.Contracts/"]
### Settings
COPY ["src/Settings/Settings.csproj", "src/Settings/"]
COPY ["src/Settings.Contracts/Settings.Contracts.csproj", "src/Settings.Contracts/"]

## WebAPI Projects
COPY ["src/WebAPI/WebAPI.csproj", "src/WebAPI/"]
COPY ["src/WebAPI.Contracts/WebAPI.Contracts.csproj", "src/WebAPI.Contracts/"]


## Restore Projects
RUN dotnet restore "src/WebAPI/WebAPI.csproj" --locked-mode

COPY . .
WORKDIR "/src/src/WebAPI"

## Build and Publish
RUN dotnet publish "WebAPI.csproj" -c Release -o /app/publish \
    --no-restore \
    /p:AssemblyVersion=${VERSION} \
    /p:InformationalVersion=${INFORMATIONAL_VERSION} \
    -p:UseAppHost=false

# Stage 3 - Runtime Image
FROM ghcr.io/linuxserver/baseimage-alpine:3.20 AS final

ARG VERSION
ARG INFORMATIONAL_VERSION
ARG PORT
ARG TARGETPLATFORM
ARG S6_OVERLAY_VERSION="3.2.1.0"

## Set environment variables
ENV PORT=${PORT}
ENV DOTNET_ENVIRONMENT=Production
ENV DOTNET_URLS=http://+:${PORT}
ENV S6_SERVICES_GRACETIME=15000

# Install dependencies and s6-overlay
RUN apk add --no-cache bash icu-libs krb5-libs libgcc libintl libssl3 libstdc++ zlib curl \
 && curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh \
 && chmod +x /tmp/dotnet-install.sh \
 && /tmp/dotnet-install.sh --runtime aspnetcore --version 9.0.7 --install-dir /usr/share/dotnet --architecture x64 \
 && rm -rf /tmp/*

## Make dotnet command available
ENV PATH="$PATH:/usr/share/dotnet"

## Copy to final image
WORKDIR /app
COPY --from=back-end /app/publish .
COPY --from=client-build /tmp/ClientApp/.output/public /app/wwwroot

## Copy the s6-overlay config files
COPY docker/etc/ /etc/
COPY docker/10-config /etc/fix-attrs.d/10-config

## Set version label
LABEL company="Reaparr" \
    maintainer="reaparr@protonmail.com" \
    org.opencontainers.image.authors="Reaparr" \
    org.opencontainers.image.vendor="Reaparr" \
    org.opencontainers.image.version="${VERSION}" \
    org.opencontainers.image.revision="${INFORMATIONAL_VERSION}"
      
EXPOSE ${PORT}
VOLUME /Config /Downloads /Movies /TvShows

# Set the entrypoint to use s6-overlay
ENTRYPOINT ["/init"]
