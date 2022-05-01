#!/bin/bash

git pull
git submodule update --init --recursive

dotnet restore
screen -dmS statserver bash ./start.sh
