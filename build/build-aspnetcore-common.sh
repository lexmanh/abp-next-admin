#!/bin/bash

# COMMON PATHS
rootFolder=$(cd "$(dirname "$0")" && pwd)

# List of solutions used only in development mode
declare -a serviceArray=()

serviceArray+=(["Path"]="$rootFolder/../aspnet-core/services/LY.MicroService.BackendAdmin.HttpApi.Host/" ["Service"]="admin")
serviceArray+=(["Path"]="$rootFolder/../aspnet-core/services/LY.MicroService.AuthServer/" ["Service"]="authserver")
serviceArray+=(["Path"]="$rootFolder/../aspnet-core/services/LY.MicroService.AuthServer.HttpApi.Host/" ["Service"]="authserver-api")
serviceArray+=(["Path"]="$rootFolder/../aspnet-core/services/LY.MicroService.identityServer/" ["Service"]="identityserver")
serviceArray+=(["Path"]="$rootFolder/../aspnet-core/services/LY.MicroService.identityServer.HttpApi.Host/" ["Service"]="identityserver4-admin")
serviceArray+=(["Path"]="$rootFolder/../aspnet-core/services/LY.MicroService.LocalizationManagement.HttpApi.Host/" ["Service"]="localization")
serviceArray+=(["Path"]="$rootFolder/../aspnet-core/services/LY.MicroService.PlatformManagement.HttpApi.Host/" ["Service"]="platform")
serviceArray+=(["Path"]="$rootFolder/../aspnet-core/services/LY.MicroService.RealtimeMessage.HttpApi.Host/" ["Service"]="messages")
serviceArray+=(["Path"]="$rootFolder/../aspnet-core/services/LY.MicroService.TaskManagement.HttpApi.Host/" ["Service"]="task-management")
serviceArray+=(["Path"]="$rootFolder/../aspnet-core/services/LY.MicroService.WebhooksManagement.HttpApi.Host/" ["Service"]="webhooks")
serviceArray+=(["Path"]="$rootFolder/../aspnet-core/services/LY.MicroService.WorkflowManagement.HttpApi.Host/" ["Service"]="workflow")
serviceArray+=(["Path"]="$rootFolder/../aspnet-core/services/LY.MicroService.WechatManagement.HttpApi.Host/" ["Service"]="wechat")
serviceArray+=(["Path"]="$rootFolder/../gateways/internal/LINGYUN.MicroService.Internal.ApiGateway/src/LINGYUN.MicroService.Internal.Gateway/" ["Service"]="internal-apigateway")

declare -a solutionArray=()
solutionArray+=(["File"]="$rootFolder/../aspnet-core/LINGYUN.MicroService.All.sln")
solutionArray+=(["File"]="$rootFolder/../aspnet-core/LINGYUN.MicroService.Common.sln")
solutionArray+=(["File"]="$rootFolder/../aspnet-core/LINGYUN.MicroService.TaskManagement.sln")
solutionArray+=(["File"]="$rootFolder/../aspnet-core/LINGYUN.MicroService.WebhooksManagement.sln")
solutionArray+=(["File"]="$rootFolder/../aspnet-core/LINGYUN.MicroService.Workflow.sln")
solutionArray+=(["File"]="$rootFolder/../aspnet-core/LINGYUN.MicroService.SingleProject.sln")
solutionArray+=(["File"]="$rootFolder/../aspnet-core/LINGYUN.MicroService.WechatManagement.sln")
solutionArray+=(["File"]="$rootFolder/../gateways/internal/LINGYUN.MicroService.Internal.ApiGateway/LINGYUN.MicroService.Internal.ApiGateway.sln")

declare -a migrationArray=()
migrationArray+=(["Path"]="$rootFolder/../aspnet-core/migrations/LY.MicroService.Platform.DbMigrator")
migrationArray+=(["Path"]="$rootFolder/../aspnet-core/migrations/LY.MicroService.LocalizationManagement.DbMigrator")
migrationArray+=(["Path"]="$rootFolder/../aspnet-core/migrations/LY.MicroService.RealtimeMessage.DbMigrator")
migrationArray+=(["Path"]="$rootFolder/../aspnet-core/migrations/LY.MicroService.IdentityServer.DbMigrator")
migrationArray+=(["Path"]="$rootFolder/../aspnet-core/migrations/LY.MicroService.TaskManagement.DbMigrator")
migrationArray+=(["Path"]="$rootFolder/../aspnet-core/migrations/LY.MicroService.AuthServer.DbMigrator")
migrationArray+=(["Path"]="$rootFolder/../aspnet-core/migrations/LY.MicroService.WebhooksManagement.DbMigrator")
migrationArray+=(["Path"]="$rootFolder/../aspnet-core/migrations/LY.MicroService.BackendAdmin.DbMigrator")
#migrationArray+=(["Path"]="$rootFolder/../aspnet-core/migrations/LY.MicroService.Applications.Single.DbMigrator")

echo ""
echo -e "\033[1;31m:::::::::::::: !!! You are in development mode !!! ::::::::::::::\033[0m \033[43m\033[30m\033[0m"
echo ""