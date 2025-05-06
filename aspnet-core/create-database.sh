#!/bin/bash

# Tắt hiển thị lệnh (tương đương @echo off)
set -x

# Xóa màn hình (tương đương cls)
clear

# Gọi các script migrate-db-cmd.sh với các tham số tương ứng
./migrate-db-cmd.sh LY.MicroService.Platform.EntityFrameworkCore platform --ef-u
./migrate-db-cmd.sh LY.MicroService.BackendAdmin.EntityFrameworkCore admin --ef-u
./migrate-db-cmd.sh LY.MicroService.AuthServer.EntityFrameworkCore authserver --ef-u
#./migrate-db-cmd.sh LY.MicroService.IdentityServer.EntityFrameworkCore identityserver4-admin --ef-u
./migrate-db-cmd.sh LY.MicroService.LocalizationManagement.EntityFrameworkCore localization --ef-u
./migrate-db-cmd.sh LY.MicroService.RealtimeMessage.EntityFrameworkCore message --ef-u
./migrate-db-cmd.sh LY.MicroService.TaskManagement.EntityFrameworkCore taskmanagement --ef-u
./migrate-db-cmd.sh LY.MicroService.WebhooksManagement.EntityFrameworkCore webhookmanagement --ef-u

# Tắt tiến trình dotnet (tương đương taskkill /IM dotnet.exe /F)
pkill -f dotnet