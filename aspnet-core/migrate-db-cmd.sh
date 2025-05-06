#!/bin/bash

# Thiết lập mã hóa UTF-8 (tương đương chcp 65001)
export LANG=en_US.UTF-8

# Hiển thị tiêu đề (tương đương title %2)
echo "Tiêu đề: $2"

# Hiển thị thông báo đang cập nhật cấu trúc db
echo "$2 đang cập nhật cấu trúc db"

# Chuyển đến thư mục migrations/$1
cd ./migrations/$1 || { echo "Không tìm thấy thư mục migrations/$1"; exit 1; }

# Kiểm tra tham số $3
case "$3" in
  "--run"|"")
    # Chạy ứng dụng không build (mặc định hoặc --run)
    dotnet run --no-build &
    ;;
  "--restore")
    # Khôi phục dependencies
    dotnet restore
    ;;
  "--ef-u")
    # Cập nhật cơ sở dữ liệu EF
    dotnet ef database update
    ;;
  *)
    # Thoát nếu tham số không hợp lệ
    exit 1
    ;;
esac

# Quay lại thư mục gốc
cd ../..

# Hiển thị thông báo hoàn tất
echo "$2 đã hoàn tất cập nhật cấu trúc db"
echo "--------"