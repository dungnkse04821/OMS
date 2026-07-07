using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OMS.Models
{
    /// <summary>
    /// Nguồn hàng - xuất xứ / nhà cung cấp của sản phẩm (VD: Trung Quốc, Hàn Quốc...).
    /// </summary>
    public class SupplySource
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên nguồn hàng")]
        [DisplayName("Tên nguồn hàng")]
        [MaxLength(100)]
        public string Name { get; set; } = "";

        [DisplayName("Mô tả")]
        [MaxLength(500)]
        public string? Description { get; set; }

        [DisplayName("Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        [DisplayName("Thứ tự hiển thị")]
        public int SortOrder { get; set; } = 99;

        [DisplayName("Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
