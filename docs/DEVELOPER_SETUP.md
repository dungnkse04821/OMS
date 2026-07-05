# Hướng dẫn Cài đặt cho Lập trình viên (Developer Setup)

Tài liệu này hướng dẫn cách cài đặt môi trường và chạy dự án OMS (Hệ thống quản lý đơn hàng) trên máy tính cá nhân.

## 1. Yêu cầu hệ thống (Prerequisites)

Để phát triển và chạy dự án, máy tính của bạn cần cài đặt:
- **[.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)**: Nền tảng cốt lõi của dự án.
- **[PostgreSQL](https://www.postgresql.org/download/)**: Hệ quản trị cơ sở dữ liệu. (Bạn có thể dùng PostgreSQL cài trên máy hoặc sử dụng dịch vụ đám mây như Neon.tech, Supabase).
- **IDE**: Visual Studio 2022, JetBrains Rider hoặc Visual Studio Code (kèm C# Dev Kit).

## 2. Thiết lập cơ sở dữ liệu (Database Setup)

Dự án sử dụng Entity Framework Core (EF Core) để giao tiếp với PostgreSQL.

### 2.1 Cấu hình chuỗi kết nối (Connection String)
Chuỗi kết nối cơ sở dữ liệu được định nghĩa trong file `appsettings.Development.json` (tạo mới nếu chưa có) hoặc sử dụng tính năng **User Secrets**.

Ví dụ chuỗi kết nối trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=oms_db;Username=postgres;Password=yourpassword"
  }
}
```

### 2.2 Áp dụng Migrations
Ứng dụng đã được cấu hình tự động chạy Migration và Seed dữ liệu mẫu (Seeder) khi khởi động (xem trong `Program.cs`). Tuy nhiên, trong quá trình phát triển, nếu bạn thay đổi cấu trúc bảng, bạn cần sử dụng EF Core CLI.

1. Cài đặt EF Core CLI (nếu chưa có):
   ```bash
   dotnet tool install --global dotnet-ef
   ```
2. Thêm Migration mới (nếu có thay đổi Model):
   ```bash
   dotnet ef migrations add <TênMigration>
   ```
3. Cập nhật Database thủ công (tùy chọn):
   ```bash
   dotnet ef database update
   ```

## 3. Khởi chạy dự án

1. Mở Terminal/Command Prompt, di chuyển vào thư mục chứa file `OMS.csproj` (`cd OMS`).
2. Khôi phục các gói phụ thuộc (NuGet packages):
   ```bash
   dotnet restore
   ```
3. Chạy ứng dụng:
   ```bash
   dotnet run
   ```
4. Truy cập trình duyệt ở địa chỉ được hiển thị trong Terminal (thường là `http://localhost:5000` hoặc `https://localhost:5001`).

## 4. Tài khoản mẫu (Demo Accounts)

Hệ thống sẽ tự động tạo các tài khoản mẫu khi khởi chạy lần đầu:
- **Admin**: `admin@oms.local` (Mật khẩu: `Admin@123`)
- **Staff**: `staff@oms.local` (Mật khẩu: `Staff@123`)
- **Viewer**: `viewer@oms.local` (Mật khẩu: `Viewer@123`)

*Lưu ý: Bạn có thể đổi lại logic seed tài khoản trong `IdentitySeeder.cs` để phục vụ cho các bài test cụ thể.*
