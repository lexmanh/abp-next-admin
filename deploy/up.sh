#!/bin/bash

# set env TAG = dev
echo "Đang thiết lập biến môi trường TAG..."
export TAG="dev" # Thiết lập biến môi trường TAG cho Docker Compose

## Running application
echo "Chạy ứng dụng với Docker Compose..."
docker compose -f ./docker-compose.yml \
  -f ./docker-compose.harbor.yml \
  -f ./docker-compose.override.yml \
  -f ./docker-compose.override.configuration.yml \
  -f ./docker-compose.override.configuration.postgres.yml \
  up -d 

echo "Ứng dụng đang chạy..."