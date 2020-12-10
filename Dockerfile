#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["src/WebAPI/WebAPI.csproj", "src/WebAPI/"]
COPY ["src/PlexApi/PlexApi.csproj", "src/PlexApi/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/FileSystem/FileSystem.csproj", "src/FileSystem/"]
COPY ["src/Data/Data.csproj", "src/Data/"]
COPY ["src/Settings/Settings.csproj", "src/Settings/"]
COPY ["src/SignalR/SignalR.csproj", "src/SignalR/"]
COPY ["src/DownloadManager/DownloadManager.csproj", "src/DownloadManager/"]
RUN dotnet restore "src/WebAPI/WebAPI.csproj"
COPY . .
WORKDIR "/src/src/WebAPI"
RUN dotnet build "WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlexRipper.WebAPI.dll"]

VOLUME /config /downloads

# Setup Nuxt
#build stage for a Node.js application
#FROM node:12.20.0-alpine AS build-stage
#LABEL company="PlexRipper"
#LABEL maintainer="plexripper@protonmail.com"
#LABEL version="0.1.0"
#
#WORKDIR /app/webui
## Essential config files
#COPY ./src/WebUI/package*.json ./
#COPY ./src/WebUI/tsconfig.json ./
#COPY ./src/WebUI/nuxt.config.ts ./
#RUN npm install
## Copy the rest of the project files
#COPY ./src/WebUI/ ./
#RUN npm run generate
#EXPOSE 3000

##production stage
#FROM nginx:stable-alpine AS production-stage
#COPY --from=build-stage /app/webui/dist /usr/share/nginx/html
#RUN npm run start
#EXPOSE 80
#CMD ["nginx", "-g", "daemon off;"]