# 📦 OMS (Order Management System)

Chào mừng bạn đến với **VibeCode OMS** - Hệ thống Quản lý Đơn hàng hiện đại được xây dựng trên nền tảng .NET 8 và PostgreSQL.

## 🌟 Tính năng Nổi bật

- **Quản lý Đơn hàng (Order Management)**: Tạo mới, cập nhật trạng thái, theo dõi đơn hàng với hiệu năng cao nhờ Server-side Pagination & Lazy Loading.
- **Quản lý Vận chuyển (Shipping & Tracking)**: Tính năng nhập (import) mã vận đơn và đơn vị vận chuyển hàng loạt qua file CSV.
- **Quản lý Sản phẩm & Khách hàng**: Lưu trữ và quản lý danh mục sản phẩm, mức tồn kho và thông tin khách hàng.
- **Phân quyền người dùng (Role-based Access Control)**: Tích hợp ASP.NET Core Identity với 3 phân quyền chính (Admin, Staff, Viewer).
- **Lưu vết Hệ thống (Audit Log)**: Tự động ghi lại mọi thao tác thay đổi dữ liệu (tạo mới, sửa, xóa) trong hệ thống để dễ dàng đối soát.
- **Thiết kế Giao diện Hiện đại**: Giao diện mang phong cách Glassmorphism với chế độ Sáng/Tối (Dark/Light mode) mượt mà.
- **REST APIs**: Cung cấp các Minimal APIs gọn nhẹ đi kèm tài liệu Swagger (OpenAPI) cho các tích hợp hệ thống bên thứ ba hoặc Mobile App.

## 🛠️ Công nghệ Sử dụng (Tech Stack)

- **Back-end**: C# 12, ASP.NET Core 8, Razor Pages, Minimal APIs.
- **Cơ sở dữ liệu**: PostgreSQL, Entity Framework Core 7/8.
- **Xác thực**: ASP.NET Core Identity (Cookie-based auth).
- **Front-end**: HTML5, Vanilla CSS, Vanilla JavaScript (Không sử dụng Framework nặng).

## 📚 Tài liệu Hệ thống

Để hiểu rõ hơn về cách cài đặt và các chức năng của hệ thống, vui lòng tham khảo các tài liệu sau trong thư mục `docs/`:

1. [Hướng dẫn Cài đặt cho Developer](docs/DEVELOPER_SETUP.md) - Cách cài đặt .NET, Database, và khởi chạy dự án tại local.
2. [Hướng dẫn Tính năng Vận chuyển](docs/SHIPPING_TRACKING.md) - Hướng dẫn chi tiết cách Import file CSV mã vận đơn.

## 🚀 Bắt đầu Nhanh

1. Đảm bảo đã cài đặt **.NET 8 SDK** và **PostgreSQL**.
2. Thiết lập chuỗi kết nối trong `appsettings.Development.json`.
3. Chạy lệnh:
```bash
cd OMS
dotnet restore
dotnet run
```
Hệ thống sẽ tự động khởi tạo database và tạo các tài khoản mẫu để bạn trải nghiệm.

---
*Được phát triển bởi VibeCode.*
