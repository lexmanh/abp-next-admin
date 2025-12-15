#!/bin/bash

# Xóa màn hình (tương đương cls)
clear

# Chuyển đến thư mục ../aspnet-core/
cd ../aspnet-core/ || { echo "Không tìm thấy thư mục ../aspnet-core/"; exit 1; }

# Chạy script start-http-api-host.sh với các tham số
./start-http-api-host.sh LY.MicroService.BackendAdmin.HttpApi.Host admin --watchrun