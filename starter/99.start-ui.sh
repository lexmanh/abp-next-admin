#!/bin/bash

# Xóa màn hình (tương đương cls)
clear

# Chuyển đến thư mục ../apps/vue/
cd ../apps/vue/ || { echo "Không tìm thấy thư mục ../apps/vue/"; exit 1; }

# Hiển thị thông báo (tương đương title abp-next-admin-ui)
echo "Khởi động abp-next-admin-ui"

# Chạy ứng dụng Vue.js ở chế độ phát triển (tương đương pnpm run dev)
pnpm run dev