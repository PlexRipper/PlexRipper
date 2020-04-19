#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster deI:\Projects\PlexRipper\src\WebAPI\Dockerfile.originalbugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/WebAPI/WebAPI.csproj", "src/WebAPI/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
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

# Setup Nuxt
#build stage for a Node.js application
FROM node:lts-alpine as build-stage
WORKDIR /app/webui
# Essential config files
COPY ../WebUI/package*.json ./
COPY ../WebUI/tsconfig.json ./
COPY ../WebUI/nuxt.config.ts ./
RUN npm install
# Copy the rest of the project files
COPY ../WebUI/ ./
RUN npm run build
EXPOSE 3000

#production stage
#FROM nginx:stable-alpine as production-stage
#COPY --from=build-stage /app/webui/dist /usr/share/nginx/html
#EXPOSE 80
#CMD ["nginx", "-g", "daemon off;"]