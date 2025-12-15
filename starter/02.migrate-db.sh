#!/bin/bash

# Chuyển đến thư mục aspnet-core
cd ../aspnet-core || { echo "Không tìm thấy thư mục aspnet-core"; exit 1; }

# Gọi script migrate-database.sh
./migrate-database.sh