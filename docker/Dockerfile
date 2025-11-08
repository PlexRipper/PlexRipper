ARG VERSION=0.0.0
ARG INFORMATIONAL_VERSION=0.0.0
ARG PORT=7000
ARG TARGETPLATFORM=linux/amd64
ARG BUILDPLATFORM=linux/amd64

# Stage 1 - Build the Nuxt front-end
FROM --platform=$BUILDPLATFORM oven/bun:1.3-alpine AS client-build

ARG PORT

ENV NUXT_HOST=0.0.0.0
ENV NUXT_PORT=${PORT}
ENV NUXT_PUBLIC_API_PORT=${PORT}
ENV NUXT_PUBLIC_IS_DOCKER=true

WORKDIR /tmp/ClientApp

## Copy the package.json and install the dependencies
COPY ./src/AppHost/ClientApp/package.json ./src/AppHost/ClientApp/bun.lockb ./
RUN bun install --frozen-lockfile

## Copy the project files
COPY ./src/AppHost/ClientApp/ ./
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
### Application Projects
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Application.Contracts/Application.Contracts.csproj", "src/Application.Contracts/"]
### Public API Projects
COPY ["src/PublicAPI/PublicAPI.csproj", "src/PublicAPI/"]
COPY ["src/PublicAPI.Contracts/PublicAPI.Contracts.csproj", "src/PublicAPI.Contracts/"]

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

## AppHost Projects
COPY ["src/AppHost/AppHost.csproj", "src/AppHost/"]


## Restore Projects
RUN dotnet restore "src/AppHost/AppHost.csproj" --locked-mode

COPY . .
WORKDIR "/src/src/AppHost"

## Build and Publish
RUN dotnet publish "AppHost.csproj" -c Release -o /app/publish \
    --no-restore \
    /p:AssemblyVersion=${VERSION} \
    /p:InformationalVersion=${INFORMATIONAL_VERSION} \
    -p:UseAppHost=false

# Stage 3 - Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS dotnet-runtime

# Stage 4 - Final Image with s6-overlay
FROM ghcr.io/linuxserver/baseimage-alpine:3.20 AS final

ARG VERSION
ARG INFORMATIONAL_VERSION
ARG PORT
ARG TARGETPLATFORM
ARG S6_OVERLAY_VERSION="3.2.1.0"

## Set environment variables
ENV VERSION=${VERSION}
ENV INFORMATIONAL_VERSION=${INFORMATIONAL_VERSION}
ENV PORT=${PORT}
ENV DOTNET_ENVIRONMENT=Production
ENV DOTNET_URLS=http://+:${PORT}
ENV S6_SERVICES_GRACETIME=15000

# Install dependencies
RUN apk add --no-cache bash icu-libs krb5-libs libgcc libintl libssl3 libstdc++ zlib

## Copy .NET runtime from official image
COPY --from=dotnet-runtime /usr/share/dotnet /usr/share/dotnet

## Make dotnet command available
ENV PATH="$PATH:/usr/share/dotnet"

## Copy to final image
WORKDIR /app
COPY --from=back-end /app/publish .
COPY --from=client-build /tmp/ClientApp/.output/public /app/wwwroot

## Copy the s6-overlay config files
COPY docker/etc/ /etc/

### This fixes the permissions on the config directory and Reaparr crashes if it can't write to it
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

# Use the non-root 'abc' user provided by linuxserver baseimage for security.
# The s6 service script already execs the app as 'abc' (see s6-setuidgid abc).
USER abc

# Set the entrypoint to use s6-overlay
ENTRYPOINT ["/init"]
