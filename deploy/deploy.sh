#!/bin/bash

# shellcheck source=../build/build-aspnetcore-common.sh
# Giả sử build-aspnetcore-common.sh nằm ở thư mục ../build/ so với script này
# Các biến mảng servicePaths, serviceNames, solutionFiles, migrationProjectPaths sẽ được nạp vào đây
source "$(dirname "$0")/../build/build-aspnetcore-common.sh"

echo "Bắt đầu triển khai container." # "Start deploying containers."

# Xác định rootFolder dựa trên vị trí của script hiện tại, đi lùi một cấp
# Nếu script này nằm trong thư mục 'deploy', thì rootFolder sẽ là thư mục cha của 'deploy'
# (thường là thư mục gốc của dự án)
currentScriptDir=$(cd "$(dirname "$0")" && pwd)
rootFolder=$(cd "$currentScriptDir/../" && pwd)

deployPath="$rootFolder/deploy"
buildPath="$rootFolder/build"
aspnetcorePath="$rootFolder/aspnet-core"
vuePath="$rootFolder/apps/vue"

echo "Thư mục gốc dự án (root): $rootFolder"

# Deploy middleware (Phần này đang được comment)
echo "Triển khai middleware..."
cd "$rootFolder" || exit 1
docker compose -f ./docker-compose.middleware.yml up -d --build

## Sleep 30s for database initialization (Phần này đang được comment)
echo "Khởi tạo database..."
sleep 30
##  Create database (Phần này đang được comment)
echo "Tạo database..."
cd "$aspnetcorePath" || exit 1
./create-database.sh

## Migrate database (Phần này đang được comment)
sleep 5
echo "Migrate database..."
cd "$buildPath" || exit 1 # Chú ý: đường dẫn này có thể cần xem lại nếu buildPath không chứa các project migration

# Cập nhật vòng lặp cho migrationProjectPaths
for migProjectPath in "${migrationProjectPaths[@]}"; do
    echo "Chạy migration cho: $migProjectPath"
    # Kiểm tra xem có cần cd vào thư mục gốc của dự án migration không, 
    # hay là $migProjectPath đã là đường dẫn chính xác để chạy.
    # Thường thì các dự án DbMigrator cần được chạy từ thư mục của chính nó.
    if [ -d "$migProjectPath" ]; then # Kiểm tra xem migProjectPath có phải là thư mục không
        cd "$migProjectPath" || { echo "Không thể cd vào $migProjectPath"; continue; }
        dotnet run --project . --no-build # Chỉ định rõ project file nếu cần, hoặc chạy từ thư mục project
        # Quay lại buildPath hoặc một thư mục gốc phù hợp sau mỗi lần chạy
        cd "$buildPath" || { echo "Không thể quay lại $buildPath"; exit 1; }
    else
        echo "Đường dẫn migration không hợp lệ: $migProjectPath"
    fi
done

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

## Build and publish Vue projects
echo "Build dự án frontend Vue..."
if [ ! -d "$vuePath" ]; then
    echo "Lỗi: Thư mục Vue '$vuePath' không tồn tại."
    exit 1
fi
cd "$vuePath" || exit 1
# Kiểm tra sự tồn tại của pnpm trước khi chạy
if ! command -v pnpm &> /dev/null
then
    echo "Lỗi: Lệnh 'pnpm' không tìm thấy. Vui lòng cài đặt pnpm."
    exit 1
fi
pnpm install
pnpm build

## Copy Vue project to publish path (Phần này có thể cần nếu bạn có bước copy riêng sau build)
# echo "Sao chép dự án Vue đã build..."
# Ví dụ: cp -r "$vuePath/dist" "$rootFolder/publish/vue-app"

## Running application
echo "Chạy ứng dụng với Docker Compose..."
cd "$rootFolder" || exit 1
docker compose -f ./docker-compose.yml -f ./docker-compose.override.yml -f ./docker-compose.override.configuration.yml up -d --build 

cd "$deployPath" || exit 1 # Quay lại thư mục deploy ban đầu
echo "Ứng dụng đang chạy..."