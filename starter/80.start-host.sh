#!/bin/bash

# Xóa màn hình (tương đương cls)
clear

# Đặt thời gian chờ (tương đương set stime=12)
stime=12

# Lặp qua tất cả file .sh trong thư mục hiện tại
for script in *.sh; do
    # Kiểm tra nếu file tồn tại và là file thông thường
    if [[ -f "$script" ]]; then
        echo "$script"
        # Chạy script trong nền (tương đương start)
        ./"$script" &
        # Chờ stime giây (tương đương ping -n %stime% 127.1 >nul)
        sleep "$stime"
    fi
done