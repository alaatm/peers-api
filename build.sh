#!/usr/bin/env bash

dotnet run --project ./build/Build.csproj --configuration Release -- "$@"
