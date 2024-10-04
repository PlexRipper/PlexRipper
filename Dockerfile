#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /app

## Setup Nuxt front-end
FROM oven/bun:alpine AS client-build
WORKDIR /tmp/build/ClientApp

ARG VERSION=0.0.0

ENV NUXT_HOST=0.0.0.0
ENV NUXT_PORT=7000
ENV API_PORT=7000
ENV NUXT_PUBLIC_IS_DOCKER=true

## Copy the project files
COPY ./src/WebAPI/ClientApp/ ./
RUN bun install --frozen-lockfile
RUN bun run generate --fail-on-error

## Setup .NET Core back-end
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
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

FROM build AS publish
RUN dotnet publish "WebAPI.csproj" -c Release -o /app/publish /p:AssemblyVersion=$VERSION

## Merge into one container
FROM base AS final
ENV DOTNET_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:7000
ENV DOTNET_URLS=http://+:7000


# Stage 3 - Build runtime image
FROM ghcr.io/linuxserver/baseimage-alpine:3.20
ARG TARGETPLATFORM
ENV TARGETPLATFORM=${TARGETPLATFORM:-linux/amd64}
ARG BUILDPLATFORM
ENV BUILDPLATFORM=${BUILDPLATFORM:-linux/amd64}

# set version label
ARG BUILD_DATE
ARG VERSION
LABEL build_version="Linuxserver.io extended version:- ${VERSION} Build-date:- ${BUILD_DATE}"
LABEL maintainer="ravensorb"

# set environment variables
ARG DEBIAN_FRONTEND="noninteractive"
ENV XDG_CONFIG_HOME="/config/xdg"
ENV RDTCLIENT_BRANCH="main"

RUN \
   mkdir -p /data/downloads /data/db || true && \
   echo "**** Updating package information ****" && \
   apk update && \
   echo "**** Install pre-reqs ****" && \
   apk add bash icu-libs krb5-libs libgcc libintl libssl3 libstdc++ zlib && \
   echo "**** Installing dotnet ****" && \
   mkdir -p /usr/share/dotnet

RUN \
   if [ "$TARGETPLATFORM" = "linux/arm/v7" ] ; then \
   wget https://download.visualstudio.microsoft.com/download/pr/c3bf3103-efdb-42e0-af55-bbf861a4215b/dc22eda8877933b8c6569e3823f18d21/aspnetcore-runtime-8.0.0-linux-musl-arm64.tar.gz && \
   tar zxf aspnetcore-runtime-8.0.0-linux-musl-arm64.tar.gz -C /usr/share/dotnet ; \
   elif [ "$TARGETPLATFORM" = "linux/arm64" ] ; then \
   wget https://download.visualstudio.microsoft.com/download/pr/c3bf3103-efdb-42e0-af55-bbf861a4215b/dc22eda8877933b8c6569e3823f18d21/aspnetcore-runtime-8.0.0-linux-musl-arm64.tar.gz && \
   tar zxf aspnetcore-runtime-8.0.0-linux-musl-arm64.tar.gz -C /usr/share/dotnet ; \
   else \
   wget https://download.visualstudio.microsoft.com/download/pr/7aa33fc7-07fe-48c2-8e44-a4bfb4928535/3b96ec50970eee414895ef3a5b188bcd/aspnetcore-runtime-8.0.0-linux-musl-x64.tar.gz && \
   tar zxf aspnetcore-runtime-8.0.0-linux-musl-x64.tar.gz -C /usr/share/dotnet ; \
   fi

RUN \
   echo "**** Setting permissions ****" && \
   chown -R abc:abc /data && \
   rm -rf \
   /tmp/* \
   /var/cache/apk/* \
   /var/tmp/* || true

ENV PATH "$PATH:/usr/share/dotnet"

# Define the s6 service for the dotnet app
RUN mkdir -p /etc/services.d/dotnet-webapi

# Create the run script that will start the dotnet app
RUN echo '#!/bin/sh' > /etc/services.d/dotnet-webapi/run && \
    echo 'exec dotnet /app/PlexRipper.WebAPI.dll' >> /etc/services.d/dotnet-webapi/run

# Make the script executable
RUN chmod +x /etc/services.d/dotnet-webapi/run

WORKDIR /app

COPY --from=publish /app/publish .
COPY --from=client-build /tmp/build/ClientApp/dist /app/wwwroot

LABEL company="PlexRipper"
LABEL maintainer="plexripper@protonmail.com"

EXPOSE 7000
VOLUME /Config /Downloads /Movies /TvShows

# Set the entrypoint to use s6-overlay
ENTRYPOINT ["/init"]
