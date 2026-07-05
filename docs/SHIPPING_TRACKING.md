# Hướng dẫn Tính năng Quản lý Vận chuyển & Theo dõi (Shipping & Tracking)

Tài liệu này giải thích chi tiết về tính năng Cập nhật mã vận đơn (Tracking Number) và Đơn vị vận chuyển thông qua file CSV (Bulk-assign tracking codes) trong hệ thống OMS.

## 1. Tổng quan tính năng

Tính năng **Import Tracking** cho phép người dùng (Staff, Admin) cập nhật mã vận đơn hàng loạt cho hàng trăm, hàng ngàn đơn hàng cùng lúc thông qua việc tải lên một file `.csv`. 
Điều này giúp tối ưu hóa luồng vận hành logistics thay vì phải nhập mã vận đơn cho từng đơn hàng một cách thủ công.

## 2. Cách hoạt động (Workflow)

1. Người dùng điều hướng tới trang **Quản lý Đơn hàng** và chọn tính năng **Nhập file Vận đơn (Import CSV)**.
2. Người dùng chọn file CSV từ máy tính cá nhân. Định dạng file yêu cầu có 2 cột chính: `OrderId` (Mã đơn hàng) và `TrackingNumber` (Mã vận đơn). Ngoài ra có thể có cột `Carrier` (Đơn vị vận chuyển - tùy chọn).
3. Hệ thống sẽ đọc file CSV, tiến hành đối chiếu `OrderId` với cơ sở dữ liệu:
   - Nếu tìm thấy đơn hàng: Hệ thống cập nhật `TrackingNumber` và `ShippingCarrier` cho đơn hàng đó.
   - Nếu đơn hàng đã có mã vận đơn trước đó: Dữ liệu sẽ bị ghi đè.
   - Trạng thái đơn hàng sẽ được tự động chuyển từ `Processing` sang `Shipped` (hoặc giữ nguyên nếu đã hoàn thành).
4. Hệ thống hiển thị thông báo kết quả (Số lượng thành công, số lượng lỗi/không tìm thấy).

## 3. Quản lý Đơn vị vận chuyển (Carriers)

Người dùng có thể tự định nghĩa các nhà cung cấp vận chuyển (Shipping Carriers) để dễ dàng lựa chọn hoặc map với dữ liệu CSV. 
Các Carrier được lưu trữ trong bảng `ShippingCarriers` (Lookup Table) và có thể quản lý tại đường dẫn `/Carriers`. 

- **Tên nhà vận chuyển**: Tên hiển thị (VD: VNPost, Viettel Post, GHTK).
- **Trang web theo dõi (Tracking URL Format)**: Đường dẫn để tra cứu mã vận đơn. Có thể bao gồm biến số `{tracking_number}` để tự động tạo link khi người dùng nhấp vào mã vận đơn trên trang chi tiết đơn hàng.

## 4. Quyền truy cập (Authorization)

Tính năng import mã vận đơn và quản lý đối tác vận chuyển chỉ khả dụng đối với những người dùng có một trong các vai trò (Role) sau:
- **Admin** (Quản trị viên)
- **Staff** (Nhân viên vận hành)

Tài khoản `Viewer` chỉ có quyền xem danh sách và chi tiết đơn hàng, không được phép thực hiện chức năng tải lên file CSV này.

## 5. Điểm kỹ thuật đáng chú ý

- **File Parser**: Tính năng đọc file CSV được thực thi thông qua thư viện `CsvHelper` để đảm bảo tốc độ nhanh và tính chính xác cao.
- **Bulk Update**: EF Core được cấu hình để cập nhật dữ liệu một cách hiệu quả nhất, giảm thiểu số lượng truy vấn đến Database.
- **Database Index**: Bảng `Orders` có đánh index B-Tree cho cột `ShippingCarrier` nhằm phục vụ việc lọc và tìm kiếm đơn hàng theo nhà vận chuyển nhanh chóng hơn.
