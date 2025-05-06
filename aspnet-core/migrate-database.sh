#!/bin/bash

# Xóa màn hình (tương đương cls)
clear

# Gọi migrate-db-cmd.sh với tùy chọn --run cho từng dịch vụ
./migrate-db-cmd.sh LY.MicroService.Platform.DbMigrator platform --run
./migrate-db-cmd.sh LY.MicroService.AuthServer.DbMigrator auth-server --run
#./migrate-db-cmd.sh LY.MicroService.IdentityServer.DbMigrator identityserver4-admin --run
./migrate-db-cmd.sh LY.MicroService.LocalizationManagement.DbMigrator localization --run
./migrate-db-cmd.sh LY.MicroService.RealtimeMessage.DbMigrator messages --run
./migrate-db-cmd.sh LY.MicroService.TaskManagement.DbMigrator task-management --run
./migrate-db-cmd.sh LY.MicroService.WebhooksManagement.DbMigrator webhooks-management --run
./migrate-db-cmd.sh LY.MicroService.BackendAdmin.DbMigrator admin --run

# Tắt các tiến trình dotnet (tương đương taskkill /IM dotnet.exe /F)
pkill -f dotnet