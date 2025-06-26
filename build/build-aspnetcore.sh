#!/bin/bash

# shellcheck source=../build/build-aspnetcore-common.sh
# Giả sử build-aspnetcore-common.sh nằm ở thư mục ../build/ so với script này
# Các biến mảng servicePaths, serviceNames, solutionFiles, migrationProjectPaths sẽ được nạp vào đây
source "./build-aspnetcore-common.sh"

currentScriptDir=$(cd "$(dirname "$0")" && pwd)
rootFolder=$(cd "$currentScriptDir/../" && pwd)

deployPath="$rootFolder/deploy"
buildPath="$rootFolder/build"
aspnetcorePath="$rootFolder/aspnet-core"
vuePath="$rootFolder/apps/vue"

echo "Thư mục gốc dự án (root): $rootFolder"

## Build and publish .NET projects
echo "Release các dự án .NET..."
if [ ! -d "$buildPath" ]; then
    echo "Lỗi: Thư mục build '$buildPath' không tồn tại."
    exit 1
fi
cd "$buildPath" || exit 1

# In ra tất cả các dịch vụ và đường dẫn của chúng từ các mảng mới
echo "Các dịch vụ sẽ được xử lý:"
for i in "${!servicePaths[@]}"; do # Lặp qua các chỉ mục của mảng servicePaths
    echo "  Dịch vụ: ${serviceNames[i]}, Đường dẫn: ${servicePaths[i]}"
done

echo "" # Thêm dòng trống cho dễ đọc

for i in "${!servicePaths[@]}"; do # Lặp qua các chỉ mục
    current_service_name="${serviceNames[i]}"
    current_service_path="${servicePaths[i]}" # Đường dẫn đến thư mục chứa file .csproj hoặc file build được
    
    # Kiểm tra xem current_service_path có trỏ đến file project hay thư mục project
    # Thông thường, dotnet publish cần đường dẫn đến file project hoặc thư mục chứa file project.
    # Giả sử current_service_path là thư mục chứa file project.
    
    publish_target_path="$rootFolder/aspnet-core/services/Publish/${current_service_name}/"
    
    echo "Đang publish dịch vụ '${current_service_name}' từ '${current_service_path}' đến '${publish_target_path}'"
    
    # Đảm bảo thư mục publish tồn tại
    mkdir -p "$publish_target_path"
    
    # Thực hiện publish
    # Giả định current_service_path là thư mục chứa project file (.csproj, .vbproj)
    # Nếu current_service_path trỏ đến một file cụ thể, bạn có thể không cần thay đổi.
    # Nếu current_service_path là một thư mục, dotnet publish sẽ tự tìm project file.
    dotnet publish "$current_service_path" -c Release -o "$publish_target_path" --no-cache
    
    # Tìm và sao chép Dockerfile
    # Tìm Dockerfile trong thư mục gốc của service project (current_service_path)
    dockerFile=$(find "$current_service_path" -maxdepth 1 -type f -name "Dockerfile")
    if [ -f "$dockerFile" ]; then
        echo "  Tìm thấy Dockerfile: $dockerFile, đang sao chép đến $publish_target_path"
        cp "$dockerFile" "$publish_target_path"
    else
        echo "  Cảnh báo: Không tìm thấy Dockerfile trong $current_service_path"
    fi
    echo "--- Hoàn thành publish cho ${current_service_name} ---"
    echo "" # Thêm dòng trống
done
echo "Hoàn thành quá trình publish các dịch vụ .NET."