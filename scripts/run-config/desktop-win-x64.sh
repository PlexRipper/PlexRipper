#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
TARGET_EXE="$ROOT_DIR/src/AppHost/bin/Publish/Desktop/win-x64/Reaparr.AppHost.exe"
DATA_ROOT="${REAPARR_DATA_PATH:-$ROOT_DIR/.tmp}"
CONFIG_ROOT="${REAPARR_CONFIG_PATH:-$DATA_ROOT/Config}"

mkdir -p "$DATA_ROOT"
mkdir -p "$CONFIG_ROOT"

export REAPARR_DATA_PATH="$DATA_ROOT"
export REAPARR_CONFIG_PATH="$CONFIG_ROOT"

(cd "$ROOT_DIR/src/AppHost/ClientApp" && bun run generate)

dotnet publish "$ROOT_DIR/src/AppHost/AppHost.csproj" \
  /p:PublishProfile=Desktop-win-x64 \
  /p:Version=0.0.0-local \
  /p:InformationalVersion=0.0.0-local

mkdir -p "$ROOT_DIR/src/AppHost/bin/Publish/Desktop/win-x64/wwwroot"
cp -r "$ROOT_DIR/src/AppHost/ClientApp/.output/public/." "$ROOT_DIR/src/AppHost/bin/Publish/Desktop/win-x64/wwwroot/"

if command -v wine >/dev/null 2>&1; then
  exec wine "$TARGET_EXE"
fi

if [[ "${OS:-}" == "Windows_NT" ]]; then
  exec "$TARGET_EXE"
fi

echo "Published win-x64 build to: $TARGET_EXE"
echo "No Windows runtime detected on this host, so app was not launched."
