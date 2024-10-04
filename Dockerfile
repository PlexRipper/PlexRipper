ARG VERSION=0.0.0
ARG PORT=7000
ARG TARGETPLATFORM=linux/amd64
ARG BUILDPLATFORM=linux/amd64

# Stage 1 - Build the Nuxt front-end
FROM oven/bun:alpine AS client-build

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
    
# Stage 2 Build the .NET back-end
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS back-end

ARG VERSION

WORKDIR /src

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
### File System
COPY ["src/FileSystem/FileSystem.csproj", "src/FileSystem/"]
COPY ["src/FileSystem.Contracts/FileSystem.Contracts.csproj", "src/FileSystem.Contracts/"]
### Plex API
COPY ["src/PlexApi/PlexApi.csproj", "src/PlexApi/"]
COPY ["src/PlexApi.Contracts/PlexApi.Contracts.csproj", "src/PlexApi.Contracts/"]
### Settings
COPY ["src/Settings/Settings.csproj", "src/Settings/"]
COPY ["src/Settings.Contracts/Settings.Contracts.csproj", "src/Settings.Contracts/"]

## Presentation projects
### WebAPI
COPY ["src/WebAPI/WebAPI.csproj", "src/WebAPI/"]
COPY ["src/WebAPI.Contracts/WebAPI.Contracts.csproj", "src/WebAPI.Contracts/"]

## Restore Projects
RUN dotnet restore "src/WebAPI/WebAPI.csproj" --locked-mode
COPY . .
WORKDIR "/src/src/WebAPI"
RUN dotnet build "WebAPI.csproj" -c Release -o /app/build /p:AssemblyVersion=$VERSION --no-restore

RUN dotnet publish "WebAPI.csproj" -c Release -o /app/publish /p:AssemblyVersion=$VERSION

# Stage 3 - Build runtime image
FROM ghcr.io/linuxserver/baseimage-alpine:3.20 AS final

ARG VERSION
ARG PORT
ARG TARGETPLATFORM
ARG BUILDPLATFORM

## set environment variables
ENV PORT=${PORT}
ENV VERSION=${VERSION}
ENV TARGETPLATFORM=${TARGETPLATFORM}
ENV BUILDPLATFORM=${BUILDPLATFORM}
ENV DOTNET_ENVIRONMENT=Production
ENV DOTNET_URLS=http://+:${PORT}
ENV ASPNETCORE_URLS=http://+:${PORT}
ENV S6_SERVICES_GRACETIME=15000

## Install dotnet runtime
RUN \
   echo "**** Updating package information ****" && \
   apk update && \
   echo "**** Install pre-reqs ****" && \
   apk add --no-cache bash icu-libs krb5-libs libgcc libintl libssl3 libstdc++ zlib && \
   echo "**** Installing dotnet ****" && \
   mkdir -p /usr/share/dotnet && \
   wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh && \
   chmod +x /tmp/dotnet-install.sh && \
   if  [ "$TARGETPLATFORM" = "linux/arm/v7" ] ; then \
   /tmp/dotnet-install.sh --version 8.0.8 --runtime aspnetcore --install-dir /usr/share/dotnet --architecture arm ; \
   elif [ "$TARGETPLATFORM" = "linux/arm64" ] ; then \
   /tmp/dotnet-install.sh --version 8.0.8 --runtime aspnetcore --install-dir /usr/share/dotnet --architecture arm64 ; \
   else \
   /tmp/dotnet-install.sh --version 8.0.8 --runtime aspnetcore --install-dir /usr/share/dotnet --architecture x64 ; \
   fi

# Make dotnet command available
ENV PATH "$PATH:/usr/share/dotnet"

## Copy to final image
WORKDIR /app
COPY --from=back-end /app/publish .
COPY --from=client-build /tmp/ClientApp/.output/public /app/wwwroot

## Copy the s6-overlay config files
COPY docker/ /

## set version label
LABEL company="PlexRipper"
LABEL maintainer="plexripper@protonmail.com"

EXPOSE ${PORT}
VOLUME /Config /Downloads /Movies /TvShows

# Set the entrypoint to use s6-overlay
ENTRYPOINT ["/init"]
