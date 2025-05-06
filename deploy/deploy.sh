#!/bin/bash

# shellcheck source=../build/build-aspnetcore-common.sh
source "../build/build-aspnetcore-common.sh"

echo "开始部署容器."

rootFolder=$(cd "../" && pwd)
deployPath="$rootFolder/deploy"
buildPath="$rootFolder/build"
aspnetcorePath="$rootFolder/aspnet-core"
vuePath="$rootFolder/apps/vue"

echo "root: $rootFolder"

## Deploy
echo "deploy middleware..."
cd "$rootFolder" || exit 1
docker compose -f ./docker-compose.middleware.yml up -d --build

## Sleep 30s for database initialization
echo "initial database..."
sleep 30
##  Create database
echo "create database..."
cd "$aspnetcorePath" || exit 1
./create-database.sh

## Migrate database
sleep 5
echo "migrate database..."
cd "$buildPath" || exit 1
for service in "${migrationArray[@]}"; do
    cd "${service["Path"]}" || continue
    dotnet run --no-build
done

## Build and publish .NET projects
echo "release .net project..."
cd "$buildPath" || exit 1
for service in "${serviceArray[@]}"; do
    publishPath="$rootFolder/aspnet-core/services/Publish/${service["Service"]}/"
    dotnet publish -c Release -o "$publishPath" "${service["Path"]}" --no-cache
    dockerFile=$(find "${service["Path"]}" -maxdepth 1 -name "Dockerfile")
    if [ -f "$dockerFile" ]; then
        cp "$dockerFile" "$publishPath"
    fi
done

## Build and publish Vue projects
echo "build front project..."
cd "$vuePath" || exit 1
pnpm install
pnpm build

## Copy Vue project to publish path
echo "running application..."
cd "$rootFolder" || exit 1
docker compose -f ./docker-compose.yml -f ./docker-compose.override.yml -f ./docker-compose.override.configuration.yml up -d --build

cd "$deployPath" || exit 1
echo "application is running..."