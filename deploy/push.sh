#!/bin/bash

# shellcheck source=../build/build-aspnetcore-common.sh
# Giả sử build-aspnetcore-common.sh nằm ở thư mục ../build/ so với script này
# Các biến mảng servicePaths, serviceNames, solutionFiles, migrationProjectPaths sẽ được nạp vào đây
source "$(dirname "$0")/../build/build-aspnetcore-common.sh"

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

# set env: REGISTRY, TAG = dev
export REGISTRY="cr.uef.edu.vn/uef" # Thiết lập biến môi trường REGISTRY cho Docker Compose
# Bạn có thể thay đổi 'your-registry' thành tên registry thực tế của bạn
# Nếu bạn có một registry cụ thể, hãy thay thế 'your-registry' bằng tên registry của bạn
# Ví dụ: REGISTRY="docker.io/yourusername" hoặc REGISTRY="registry.example.com/yourproject"
export TAG="dev" # Thiết lập biến môi trường TAG cho Docker Compose

# create temporary tag based on current time
current_time=$(date +%Y%m%d)
CTAG="${TAG}-${current_time}"
    
# Ensue docker registry is logged in
if ! docker info &> /dev/null; then
    echo "Lỗi: Docker không chạy hoặc không thể kết nối đến Docker daemon."
    exit 1
fi
if ! docker login "$REGISTRY"; then
    echo "Lỗi: Không thể đăng nhập vào Docker registry '$REGISTRY'."
    exit 1
fi
echo "Đang đăng nhập vào Docker registry: $REGISTRY với tag: $TAG"

## Build and publish .NET projects
echo "Release các dự án .NET..."
#if [ ! -d "$buildPath" ]; then
#    echo "Lỗi: Thư mục build '$buildPath' không tồn tại."
#    exit 1
#fi
cd "$buildPath" || exit 1

for i in "${!servicePaths[@]}"; do # Lặp qua các chỉ mục
    current_service_name="${serviceNames[i]}"
    current_service_path="${servicePaths[i]}" # Đường dẫn đến thư mục chứa file .csproj hoặc file build được
    
    # Kiểm tra xem current_service_path có trỏ đến file project hay thư mục project
    # Thông thường, dotnet publish cần đường dẫn đến file project hoặc thư mục chứa file project.
    # Giả sử current_service_path là thư mục chứa file project.
    
    publish_target_path="$rootFolder/aspnet-core/services/Publish/${current_service_name}/"
    
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
    
    # build docker image and push to registry
    echo "Đang build và push Docker image cho dịch vụ '${current_service_name}'..."
    cd "$publish_target_path" || exit 1
    
    
    # clean up old images
    echo "Đang dọn dẹp các Docker image cũ cho dịch vụ '${current_service_name}'..."
    docker image prune -f --filter "label=service=${current_service_name}"
    
    # Build Docker image
    echo "Đang build Docker image cho dịch vụ '${current_service_name}' với tag '${CTAG}'..."
    docker build -t "${current_service_name}:${CTAG}" .
    if [ $? -ne 0 ]; then
        echo "Lỗi: Không thể build Docker image cho dịch vụ '${current_service_name}'."
        exit 1
    fi
    
    # Correct tag if needed
    echo "Đang gán tags cho Docker image '${current_service_name}' với tag '${CTAG}', '${TAG}' và 'latest'..."
    docker image tag "${current_service_name}:${CTAG}" "${REGISTRY}/abp-next-${current_service_name}:${CTAG}"
    docker image tag "${current_service_name}:${CTAG}" "${REGISTRY}/abp-next-${current_service_name}:${TAG}"
    docker image tag "${current_service_name}:${CTAG}" "${REGISTRY}/abp-next-${current_service_name}:latest"
    
    echo "Đang push tất cả các Docker image cho dịch vụ '${current_service_name}'..."
    docker push -a "${REGISTRY}/abp-next-${current_service_name}"
    
    if [ $? -ne 0 ]; then
        echo "Lỗi: Không thể push Docker image cho dịch vụ '${current_service_name}'."
        exit 1
    fi
    
    echo "Docker image cho dịch vụ '${current_service_name}' đã được build và push thành công."
    # Clean up temporary images
    echo "Đang dọn dẹp các Docker image tạm thời cho dịch vụ '${current_service_name}'..."
    echo "--- Hoàn thành build và push cho ${current_service_name} ---"
done


## Running application
echo "Buid and push Docker images..."
cd "$rootFolder" || exit 1


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

echo "Dự án Vue đã được build thành công."

# Clean up old images
echo "Đang dọn dẹp các Docker image cũ cho abp-next-admin-ui..."
docker image prune -f --filter "label=service=abp-next-admin-ui"

# Build Docker image cho Vue app
echo "Đang build Docker image cho abp-next-admin-ui..."
docker build -t "${REGISTRY}/abp-next-admin-ui:${TAG}" -f "$vuePath/Dockerfile" "$vuePath"
if [ $? -ne 0 ]; then
    echo "Lỗi: Không thể build Docker image cho abp-next-admin-ui."
    exit 1
fi

# Correct tag if needed
echo "Đang gán tags cho Docker image abp-next-admin-ui với tag '${CTAG}', '${TAG}' và 'latest'..."
docker image tag "${REGISTRY}/abp-next-admin-ui:${TAG}" "${REGISTRY}/abp-next-admin-ui:${CTAG}"
docker image tag "${REGISTRY}/abp-next-admin-ui:${TAG}" "${REGISTRY}/abp-next-admin-ui:latest"

echo "Đang push Docker image abp-next-admin-ui với tag '${TAG}'..."
docker push "${REGISTRY}/abp-next-admin-ui:${TAG}"

if [ $? -ne 0 ]; then
    echo "Lỗi: Không thể push Docker image abp-next-admin-ui."
    exit 1
fi

echo "Docker image cho abp-next-admin-ui đã được build và push thành công."
# Clean up temporary images
echo "Đang dọn dẹp các Docker image tạm thời cho abp-next-admin-ui..."
docker image prune -f --filter "label=service=abp-next-admin-ui"

# Done
echo "Tất cả các dịch vụ đã được build và push thành công."