#!/bin/bash

declare -a migrationServices=()
migrationServices+=("platform/LY.MicroService.Platform.DbMigrator.dll")
migrationServices+=("localization/LY.MicroService.LocalizationManagement.DbMigrator.dll")
migrationServices+=("messages/LY.MicroService.RealtimeMessage.DbMigrator.dll")
migrationServices+=("identityserver/LY.MicroService.IdentityServer.DbMigrator.dll")
migrationServices+=("task-management/LY.MicroService.TaskManagement.DbMigrator.dll")
migrationServices+=("authserver/LY.MicroService.AuthServer.DbMigrator.dll")
migrationServices+=("webhooks/LY.MicroService.WebhooksManagement.DbMigrator.dll")
migrationServices+=("admin/LY.MicroService.BackendAdmin.DbMigrator.dll")
#migrationServices+=("workflow/LY.MicroService.WorkflowManagement.DbMigrator.dll")

for i in "${!migrationServices[@]}"; do # Lặp qua các chỉ mục
    current_service_name="${migrationServices[i]}"
    
    # run dotnet $current_service_name
    echo "Đang chạy DbMigrator cho dịch vụ '${current_service_name}'..."
    dotnet "${current_service_name}"
    if [ $? -ne 0 ]; then
        echo "Lỗi: Không thể chạy DbMigrator cho dịch vụ '${current_service_name}'."
        exit 1
    fi
    echo "DbMigrator cho dịch vụ '${current_service_name}' đã chạy thành công." 
done

echo "--- Hoàn thành chạy DbMigrator cho tất cả các dịch vụ ---"
