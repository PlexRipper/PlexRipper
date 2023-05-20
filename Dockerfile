#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS base
WORKDIR /app

## Setup Nuxt front-end
FROM node:18.11.0-alpine AS client-build
WORKDIR /tmp/build/ClientApp

ARG port=7000

ENV NUXT_HOST=0.0.0.0
ENV NUXT_PORT=$port
ENV API_PORT=$port
# Essential config files
COPY ./src/WebAPI/ClientApp/package*.json ./
COPY ./src/WebAPI/ClientApp/tsconfig.json ./
COPY ./src/WebAPI/ClientApp/nuxt.config.ts ./
RUN npm install
## Copy the rest of the project files
COPY ./src/WebAPI/ClientApp/ ./
RUN npm run generate --fail-on-error

## Setup .NET Core back-end
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
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
### Background Services
COPY ["src/BackgroundServices/BackgroundServices.csproj", "src/BackgroundServices/"]
COPY ["src/BackgroundServices.Contracts/BackgroundServices.Contracts.csproj", "src/BackgroundServices.Contracts/"]
### Data Access
COPY ["src/Data/Data.csproj", "src/Data/"]
COPY ["src/Data.Contracts/Data.Contracts.csproj", "src/Data.Contracts/"]
### Download Manager
COPY ["src/DownloadManager/DownloadManager.csproj", "src/DownloadManager/"]
COPY ["src/DownloadManager.Contracts/DownloadManager.Contracts.csproj", "src/DownloadManager.Contracts/"]
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
RUN dotnet restore "src/WebAPI/WebAPI.csproj"
COPY . .
WORKDIR "/src/src/WebAPI"
RUN dotnet build "WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebAPI.csproj" -c Release -o /app/publish

## Merge into one container
FROM base AS final
ENV ASPNETCORE_ENVIRONMENT Production
ENV DOTNET_ENVIRONMENT Production
ENV ASPNETCORE_URLS=http://+:$port
ENV DOTNET_URLS=http://+:$port
WORKDIR /app

COPY --from=publish /app/publish .
COPY --from=client-build /tmp/build/ClientApp/dist /app/wwwroot

LABEL company="PlexRipper"
LABEL maintainer="plexripper@protonmail.com"

EXPOSE $port
VOLUME /Config /Downloads /Movies /TvShows

ENTRYPOINT ["dotnet", "PlexRipper.WebAPI.dll"]