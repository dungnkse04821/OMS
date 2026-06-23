using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OMS.Models
{
    public class Product
    {
        [Key]
        public string Sku { get; set; }

        [DisplayName("Tên sản phẩm")]
        public string Name { get; set; }

        [DisplayName("Danh mục")]
        public string Category { get; set; }

        [DisplayName("Giá nhập")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal ImportPrice { get; set; }

        [DisplayName("Giá bán")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal SellingPrice { get; set; }

        [DisplayName("Nguồn hàng")]
        public string Source { get; set; }

        [DisplayName("Kho")]
        public string Warehouse { get; set; }

        [DisplayName("Tồn kho")]
        public int StockQuantity { get; set; } = 0;

        [DisplayName("Ngưỡng cảnh báo")]
        public int LowStockThreshold { get; set; } = 5;

        // ── Soft-delete & audit metadata ─────────────────────────────────
        public bool IsDeleted { get; set; } = false;

        [DisplayName("Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DisplayName("Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }
    }
}
