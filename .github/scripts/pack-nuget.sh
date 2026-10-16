#!/usr/bin/env bash
set -euo pipefail

: "${VERSION:?VERSION is required}"
mkdir -p artifacts

# The generator is packed first because the showcase package consumes it.
dotnet pack src/OvoUi.AvaloniaEdit.Showcase.Generator/OvoUi.AvaloniaEdit.Showcase.Generator.csproj \
  -c Release -p:Version="$VERSION" -o artifacts

projects=(
  src/OvoUi/OvoUi.csproj
  src/OvoUi.Fonts/OvoUi.Fonts.csproj
  src/OvoUi.Platform/OvoUi.Platform.csproj
  src/OvoUi.AvaloniaEdit/OvoUi.AvaloniaEdit.csproj
  src/OvoUi.AvaloniaEdit.Showcase/OvoUi.AvaloniaEdit.Showcase.csproj
)

for project in "${projects[@]}"; do
  dotnet restore "$project" \
    --source "$(pwd)/artifacts" \
    --source https://api.nuget.org/v3/index.json \
    -p:Version="$VERSION" \
    -p:NuGetRelease=true
done

for project in "${projects[@]}"; do
  dotnet pack "$project" \
    -c Release \
    -p:Version="$VERSION" \
    -p:NuGetRelease=true \
    --no-restore \
    -o artifacts
done

ls -lh artifacts
