#!/bin/bash

# Thiết lập mã hóa UTF-8 (tương đương chcp 65001)
export LANG=en_US.UTF-8

# Xóa màn hình (tương đương cls)
clear

# Hiển thị thông báo (tương đương title %2-host và @echo %2-host)
echo "$2-host"

# Chuyển đến thư mục services/$1
cd ./services/$1 || { echo "Không tìm thấy thư mục services/$1"; exit 1; }

# Kiểm tra tham số $3
case "$3" in
  "--publish")
    dotnet publish -c Release -o ./services/Publish/$2 --no-cache --no-restore
    cp Dockerfile ./services/Publish/$2/Dockerfile
    exit 0
    ;;
  "--watchrun")
    dotnet watch run --no-restore
    exit 0
    ;;
  "--run"|"")
    dotnet run
    exit 0
    ;;
  "--restore")
    dotnet restore
    exit 0
    ;;
  "--ef-u")
    dotnet ef database update
    exit 0
    ;;
  *)
    exit 1
    ;;
esac