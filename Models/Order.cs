using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NpgsqlTypes;

namespace OMS.Models
{
    public class Order
    {
        [Key]
        public string Id { get; set; }

        [DisplayName("Ngày đặt")]
        [DataType(DataType.Date)]
        public DateTime? OrderDate { get; set; }

        [DisplayName("Nguồn hàng")]
        public string Source { get; set; }

        [DisplayName("Kho")]
        public string Warehouse { get; set; }

        [DisplayName("Mã sản phẩm/Code")]
        public string Code { get; set; } = string.Empty;

        [DisplayName("Loại sản phẩm")]
        public string Category { get; set; }

        [DisplayName("Tên sản phẩm")]
        public string ProductName { get; set; }

        [DisplayName("Màu sắc")]
        public string? Color { get; set; }

        [DisplayName("Size")]
        public string? Size { get; set; }

        [DisplayName("Giá bán")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal SellingPrice { get; set; }

        [DisplayName("Số lượng")]
        public int Quantity { get; set; }

        [DisplayName("Tổng tiền")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalAmount { get; set; }

        [DisplayName("Khách hàng")]
        public string CustomerName { get; set; }

        [DisplayName("Đặt cọc")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Deposit { get; set; }

        [DisplayName("Chiết khấu")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Discount { get; set; }

        [DisplayName("Còn lại")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal RemainingAmount { get; set; }

        [DisplayName("Ngày về")]
        [DataType(DataType.Date)]
        public DateTime? ArrivalDate { get; set; }

        [DisplayName("Ngày TT")]
        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }

        [DisplayName("Giá nhập")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal ImportPrice { get; set; }

        [DisplayName("Tổng vốn")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalImportCost { get; set; }

        [DisplayName("Lãi")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Profit { get; set; }

        [DisplayName("Trạng thái")]
        public string Status { get; set; } = "Chờ đặt";

        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại")]
        [DisplayName("Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Địa chỉ")]
        [DisplayName("Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }

        [DisplayName("Đơn vị vận chuyển")]
        public string? ShippingCarrier { get; set; }

        [DisplayName("Mã vận đơn")]
        public string? TrackingCode { get; set; }

        // ── PostgreSQL GIN Full-Text Search Vector ───────────────────────
        public NpgsqlTsVector? SearchVector { get; set; }

        // ── Soft-delete & audit metadata ─────────────────────────────────
        public bool IsDeleted { get; set; } = false;

        [DisplayName("Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DisplayName("Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }
    }

    public static class OrderStatusList
    {
        public static List<string> All = new List<string>
        {
            "Chờ đặt",
            "Đã đặt",
            "Đang về",
            "Đã về",
            "Đã giao",
            "Hủy"
        };
    }
}
