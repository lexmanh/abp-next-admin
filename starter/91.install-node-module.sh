#!/bin/bash

# Xóa màn hình (tương đương cls)
clear

# Chuyển đến thư mục ../apps/vue/
cd ../apps/vue/ || { echo "Không tìm thấy thư mục ../apps/vue/"; exit 1; }

# Hiển thị thông báo (tương đương title install-module)
echo "Cài đặt module"

# Cài đặt dependencies (tương đương pnpm install)
pnpm install

# Tạm dừng và chờ người dùng nhấn Enter (tương đương pause)
read -p "Nhấn Enter để tiếp tục..."