#!/bin/bash

# Clear the screen
clear

# Change to the aspnet-core directory
cd ../aspnet-core/ || { echo "Directory ../aspnet-core/ not found"; exit 1; }

# Run the start-http-api-host.sh script with parameters
./start-http-api-host.sh LY.MicroService.LocalizationManagement.HttpApi.Host localization --watchrun