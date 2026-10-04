## Yêu cầu hệ thống
- .NET 8 SDK
- Docker Desktop
- Visual Studio
## Hướng dẫn cài đặt và chạy dự án
- Bước 1: Khởi động Database (MongoDB & Elasticsearch)
Mở Terminal tại thư mục chứa file `docker-compose.yml` và chạy lệnh sau: docker-compose up -d
- Bước 2: Chạy ứng dụng
Mở file Solution (.sln) bằng Visual Studio.
Đặt project WebApi làm Startup Project.
Ấn F5 để chạy.

## API Import
Có file .csv mẫu để ở trong thư mục gốc
