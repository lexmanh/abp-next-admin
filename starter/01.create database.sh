#!/bin/bash

# Chuyển đến thư mục cha aspnet-core
cd ../aspnet-core || { echo "Không tìm thấy thư mục aspnet-core"; exit 1; }

# Gọi script create-database.sh
./create-database.sh