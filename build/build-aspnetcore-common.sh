#!/bin/bash
# build-aspnetcore-common.sh

# COMMON PATHS
# Lấy đường dẫn tuyệt đối đến thư mục chứa script
# Cách này hoạt động tốt trên cả Linux và macOS
rootFolder=$(cd "$(dirname "$0")" && pwd)

# --- Danh sách các dịch vụ ---
# Sử dụng mảng song song: một cho đường dẫn (Paths), một cho tên định danh (Services/Names)
declare -a servicePaths=()
declare -a serviceNames=()

# Thêm từng dịch vụ
servicePaths+=("$rootFolder/../aspnet-core/services/LY.MicroService.BackendAdmin.HttpApi.Host/")
serviceNames+=("admin")

servicePaths+=("$rootFolder/../aspnet-core/services/LY.MicroService.AuthServer/")
serviceNames+=("authserver")

servicePaths+=("$rootFolder/../aspnet-core/services/LY.MicroService.AuthServer.HttpApi.Host/")
serviceNames+=("authserver-api")

servicePaths+=("$rootFolder/../aspnet-core/services/LY.MicroService.IdentityServer/")
serviceNames+=("identityserver")

servicePaths+=("$rootFolder/../aspnet-core/services/LY.MicroService.IdentityServer.HttpApi.Host/")
serviceNames+=("identityserver4-admin")

servicePaths+=("$rootFolder/../aspnet-core/services/LY.MicroService.LocalizationManagement.HttpApi.Host/")
serviceNames+=("localization")

servicePaths+=("$rootFolder/../aspnet-core/services/LY.MicroService.PlatformManagement.HttpApi.Host/")
serviceNames+=("platform")

servicePaths+=("$rootFolder/../aspnet-core/services/LY.MicroService.RealtimeMessage.HttpApi.Host/")
serviceNames+=("messages")

servicePaths+=("$rootFolder/../aspnet-core/services/LY.MicroService.TaskManagement.HttpApi.Host/")
serviceNames+=("task-management")

servicePaths+=("$rootFolder/../aspnet-core/services/LY.MicroService.WebhooksManagement.HttpApi.Host/")
serviceNames+=("webhooks")

servicePaths+=("$rootFolder/../aspnet-core/services/LY.MicroService.WorkflowManagement.HttpApi.Host/")
serviceNames+=("workflow")

servicePaths+=("$rootFolder/../aspnet-core/services/LY.MicroService.WechatManagement.HttpApi.Host/")
serviceNames+=("wechat")

servicePaths+=("$rootFolder/../gateways/internal/LINGYUN.MicroService.Internal.ApiGateway/src/LINGYUN.MicroService.Internal.Gateway/")
serviceNames+=("internal-apigateway")

# --- Danh sách các tệp solution ---
# Sử dụng mảng chỉ mục đơn giản để lưu đường dẫn tệp
declare -a solutionFiles=()

solutionFiles+=("$rootFolder/../aspnet-core/LINGYUN.MicroService.All.sln")
solutionFiles+=("$rootFolder/../aspnet-core/LINGYUN.MicroService.Common.sln")
solutionFiles+=("$rootFolder/../aspnet-core/LINGYUN.MicroService.TaskManagement.sln")
solutionFiles+=("$rootFolder/../aspnet-core/LINGYUN.MicroService.WebhooksManagement.sln")
solutionFiles+=("$rootFolder/../aspnet-core/LINGYUN.MicroService.Workflow.sln")
solutionFiles+=("$rootFolder/../aspnet-core/LINGYUN.MicroService.SingleProject.sln")
solutionFiles+=("$rootFolder/../aspnet-core/LINGYUN.MicroService.WechatManagement.sln")
solutionFiles+=("$rootFolder/../gateways/internal/LINGYUN.MicroService.Internal.ApiGateway/LINGYUN.MicroService.Internal.ApiGateway.sln")


# --- Danh sách các dự án migration ---
# Sử dụng mảng chỉ mục đơn giản để lưu đường dẫn đến các dự án DbMigrator
declare -a migrationProjectPaths=()
declare -a migrationProjectNames=()

migrationProjectPaths+=("$rootFolder/../aspnet-core/migrations/LY.MicroService.Platform.DbMigrator")
migrationProjectNames+=("platform")
migrationProjectPaths+=("$rootFolder/../aspnet-core/migrations/LY.MicroService.LocalizationManagement.DbMigrator")
migrationProjectNames+=("localization")
migrationProjectPaths+=("$rootFolder/../aspnet-core/migrations/LY.MicroService.RealtimeMessage.DbMigrator")
migrationProjectNames+=("messages")
migrationProjectPaths+=("$rootFolder/../aspnet-core/migrations/LY.MicroService.IdentityServer.DbMigrator")
migrationProjectNames+=("identityserver")
migrationProjectPaths+=("$rootFolder/../aspnet-core/migrations/LY.MicroService.TaskManagement.DbMigrator")
migrationProjectNames+=("task-management")
migrationProjectPaths+=("$rootFolder/../aspnet-core/migrations/LY.MicroService.AuthServer.DbMigrator")
migrationProjectNames+=("authserver")
migrationProjectPaths+=("$rootFolder/../aspnet-core/migrations/LY.MicroService.WebhooksManagement.DbMigrator")
migrationProjectNames+=("webhooks")
migrationProjectPaths+=("$rootFolder/../aspnet-core/migrations/LY.MicroService.BackendAdmin.DbMigrator")
migrationProjectNames+=("admin")
# Dòng comment này cũng được chuyển đổi tương ứng:
# migrationProjectPaths+=("$rootFolder/../aspnet-core/migrations/LY.MicroService.Applications.Single.DbMigrator")

# Các lệnh echo không thay đổi, chúng tương thích tốt
echo ""
echo -e "\033[1;31m:::::::::::::: !!! You are in development mode !!! ::::::::::::::\033[0m \033[43m\033[30m\033[0m"
echo ""

# Để sử dụng các mảng này sau này (ví dụ):
#
# echo "Services:"
# for i in "${!servicePaths[@]}"; do
#   path="${servicePaths[i]}"
#   name="${serviceNames[i]}"
#   echo "  Name: $name, Path: $path"
#   # Ví dụ: cd "$path" && dotnet run ...
# done
#
# echo -e "\nSolutions:"
# for sln in "${solutionFiles[@]}"; do
#   echo "  Solution: $sln"
#   # Ví dụ: dotnet build "$sln" ...
# done
#
# echo -e "\nMigration Projects:"
# for migPath in "${migrationProjectPaths[@]}"; do
#   echo "  Migration Path: $migPath"
#   # Ví dụ: cd "$migPath" && dotnet run ...
# done