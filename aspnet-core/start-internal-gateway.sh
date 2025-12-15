#!/bin/bash

# Thiết lập mã hóa UTF-8 (tương đương chcp 65001)
export LANG=en_US.UTF-8

# Xóa màn hình (tương đương cls)
clear

# Hiển thị thông báo (tương đương title internal-apigateway và echo. 启动内部网关)
echo "Khởi động internal gateway"

# Chuyển đến thư mục dự án API Gateway
cd ../gateways/internal/LINGYUN.MicroService.Internal.ApiGateway/src/LINGYUN.MicroService.Internal.ApiGateway/ || { echo "Không tìm thấy thư mục API Gateway"; exit 1; }

# Kiểm tra tham số $1
case "$1" in
  "--publish")
    dotnet publish -c Release -o ../../../../../aspnet-core/services/Publish/internal-apigateway --no-cache --no-restore
    cp Dockerfile ../../../../../aspnet-core/services/Publish/internal-apigateway/Dockerfile
    exit 0
    ;;
  "--run"|"")
    dotnet run
    exit 0
    ;;
  "--watchrun")
    dotnet watch run --no-restore
    exit 0
    ;;
  "--restore")
    dotnet restore
    exit 0
    ;;
  *)
    exit 1
    ;;
esac