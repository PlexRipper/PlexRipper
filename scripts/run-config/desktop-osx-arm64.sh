#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
DATA_ROOT="${REAPARR_DATA_PATH:-$HOME/.local/share/Reaparr/}"
CONFIG_ROOT="${REAPARR_CONFIG_PATH:-$DATA_ROOT/Config}"

mkdir -p "$DATA_ROOT"
mkdir -p "$CONFIG_ROOT"

export REAPARR_DATA_PATH="$DATA_ROOT"
export REAPARR_CONFIG_PATH="$CONFIG_ROOT"
export DOTNET_ENVIRONMENT="${DOTNET_ENVIRONMENT:-Development}"
export DOTNET_HTTP_PORTS="${DOTNET_HTTP_PORTS:-5000}"
export REAPARR_PLATFORM="desktop"
export PUID="${PUID:-$(id -u)}"
export PGID="${PGID:-$(id -g)}"

(cd "$ROOT_DIR/src/AppHost/ClientApp" && bun run generate)

dotnet publish "$ROOT_DIR/src/AppHost/AppHost.csproj" \
  /p:PublishProfile=Desktop-osx-arm64 \
  /p:Version=0.0.0-local \
  /p:InformationalVersion=0.0.0-local

mkdir -p "$ROOT_DIR/src/AppHost/bin/Publish/Desktop/osx-arm64/wwwroot"
cp -r "$ROOT_DIR/src/AppHost/ClientApp/.output/public/." "$ROOT_DIR/src/AppHost/bin/Publish/Desktop/osx-arm64/wwwroot/"

exec "$ROOT_DIR/src/AppHost/bin/Publish/Desktop/osx-arm64/Reaparr.AppHost"
