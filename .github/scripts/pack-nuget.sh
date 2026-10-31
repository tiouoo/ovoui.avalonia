#!/usr/bin/env bash
set -euo pipefail

: "${VERSION:?VERSION is required}"
mkdir -p artifacts

# Pack analyzers first so their exact release versions can be restored locally.
dotnet pack src/OvoUi.AvaloniaEdit.Showcase.Generator/OvoUi.AvaloniaEdit.Showcase.Generator.csproj \
  -c Release -p:Version="$VERSION" -o artifacts
dotnet pack src/OvoUi.Lang.Generator/OvoUi.Lang.Generator.csproj \
  -c Release -p:Version="$VERSION" -o artifacts

projects=(
  src/OvoUi/OvoUi.csproj
  src/OvoUi.DataGrid/OvoUi.DataGrid.csproj
  src/OvoUi.Fonts/OvoUi.Fonts.csproj
  src/OvoUi.Platform/OvoUi.Platform.csproj
  src/OvoUi.Platform.NoAOT/OvoUi.Platform.NoAOT.csproj
  src/OvoUi.Tab/OvoUi.Tab.csproj
  src/OvoUi.SmoothScroll/OvoUi.SmoothScroll.csproj
  src/OvoUi.Lang/OvoUi.Lang.csproj
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
