#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app

## Setup Nuxt front-end
FROM node:12.20.0-alpine AS client-build
WORKDIR /tmp/build/ClientApp

ARG PORT=7000
ENV PORT=$PORT

# Essential config files
COPY ./src/WebAPI/ClientApp/package*.json ./
COPY ./src/WebAPI/ClientApp/tsconfig.json ./
COPY ./src/WebAPI/ClientApp/nuxt.config.ts ./
RUN npm install
## Copy the rest of the project files
COPY ./src/WebAPI/ClientApp/ ./
RUN npm run generate

## Setup .NET Core back-end
FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src

## Core Projects
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Application/Application.csproj", "src/Application/"]

## Presentation projects
COPY ["src/WebAPI/WebAPI.csproj", "src/WebAPI/"]

## Infrastructure Projects
COPY ["src/Data/Data.csproj", "src/Data/"]
COPY ["src/DownloadManager/DownloadManager.csproj", "src/DownloadManager/"]
COPY ["src/FileSystem/FileSystem.csproj", "src/FileSystem/"]
COPY ["src/HttpClient/HttpClient.csproj", "src/HttpClient/"]
COPY ["src/PlexApi/PlexApi.csproj", "src/PlexApi/"]
COPY ["src/Settings/Settings.csproj", "src/Settings/"]


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
ENV ASPNETCORE_URLS=http://+:7000
ENV DOTNET_URLS=http://+:7000
WORKDIR /app

COPY --from=publish /app/publish .
COPY --from=client-build /tmp/build/ClientApp/dist /app/wwwroot

LABEL company="PlexRipper"
LABEL maintainer="plexripper@protonmail.com"

EXPOSE 7000
VOLUME /config /downloads /movies /tvshows

ENTRYPOINT ["dotnet", "PlexRipper.WebAPI.dll"]