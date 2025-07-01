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

# create dbmigrator docker image  
echo "Đang build và push Docker image cho DbMigrator..."
# copy DbMigrator all files from $rootFolder/deploy/dbmigrator/Dockerfile 

# Đảm bảo thư mục publish tồn tại
mkdir -p "$rootFolder/aspnet-core/services/Publish/DbMigrator"

# Sao chép tất cả các tệp trong thư mục deploy/dbmigrator vào thư mục publish
cp -r "$deployPath/dbmigrator/"* "$rootFolder/aspnet-core/services/Publish/DbMigrator/"

# clean up old images
echo "Đang dọn dẹp các Docker image cũ cho dịch vụ 'abp-next-dbmigrator'..."
docker image prune -f --filter "label=service=abp-next-dbmigrator"

# Build Docker image
cd "$rootFolder/aspnet-core/services/Publish/DbMigrator" || exit 1
echo "Đang build Docker image cho dịch vụ 'abp-next-dbmigrator' với tag '${CTAG}'..."
docker build -t "abp-next-dbmigrator:${CTAG}" .
if [ $? -ne 0 ]; then
   echo "Lỗi: Không thể build Docker image cho dịch vụ 'abp-next-dbmigrator'."
   exit 1
fi
 
# Correct tag if needed
echo "Đang gán tags cho Docker image 'abp-next-dbmigrator' với tag '${CTAG}', '${TAG}' và 'latest'..."
docker image tag "abp-next-dbmigrator:${CTAG}" "${REGISTRY}/abp-next-dbmigrator:${CTAG}"
docker image tag "abp-next-dbmigrator:${CTAG}" "${REGISTRY}/abp-next-dbmigrator:${TAG}"
docker image tag "abp-next-dbmigrator:${CTAG}" "${REGISTRY}/abp-next-dbmigrator:latest"

echo "Đang push tất cả các Docker image cho dịch vụ 'abp-next-dbmigrator'..."
docker push -a "${REGISTRY}/abp-next-dbmigrator"

if [ $? -ne 0 ]; then
   echo "Lỗi: Không thể push Docker image cho dịch vụ 'abp-next-dbmigrator'."
   exit 1
fi

echo "Docker image cho dịch vụ 'abp-next-dbmigrator' đã được build và push thành công."
# Clean up temporary images
echo "Đang dọn dẹp các Docker image tạm thời cho dịch vụ 'abp-next-dbmigrator'..."
docker image prune -f --filter "label=service=abp-next-dbmigrator"
echo "--- Hoàn thành build và push cho 'abp-next-dbmigrator' ---"