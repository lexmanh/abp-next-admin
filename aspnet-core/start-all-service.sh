#!/bin/bash

# Xóa màn hình (tương đương cls)
clear

# Đặt thời gian chờ (tương đương set stime=8)
stime=8

# Khởi động các API host với tham số --run
./start-http-api-host.sh LY.MicroService.IdentityServer identityserver --run &
sleep "$stime"
./start-http-api-host.sh LY.MicroService.IdentityServer.HttpApi.Host identityserver4-admin --run &
sleep "$stime"
./start-http-api-host.sh LY.MicroService.LocalizationManagement.HttpApi.Host localization --run &
sleep "$stime"
./start-http-api-host.sh LY.MicroService.PlatformManagement.HttpApi.Host platform --run &
sleep "$stime"
./start-http-api-host.sh LY.MicroService.RealtimeMessage.HttpApi.Host messages --run &
sleep "$stime"
./start-http-api-host.sh LY.MicroService.TaskManagement.HttpApi.Host task-management --run &
sleep "$stime"
./start-http-api-host.sh LY.MicroService.WebhooksManagement.HttpApi.Host webhooks-management --run &
sleep "$stime"
./start-http-api-host.sh LY.MicroService.WorkflowManagement.HttpApi.Host workflow-management --run &
sleep "$stime"
./start-http-api-host.sh LY.MicroService.BackendAdmin.HttpApi.Host admin --run &
sleep "$stime"
./start-internal-gateway.sh --run &