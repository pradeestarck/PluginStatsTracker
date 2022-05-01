#!/bin/bash

git pull
git submodule update --init --recursive

dotnet restore
screen -dmS statserver ASPNETCORE_ENVIRONMENT=Production ASPNETCORE_URLS=http://*:8131 dotnet run
