using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OMS.Models
{
    /// <summary>
    /// Kho hàng - nơi lưu trữ và xuất hàng.
    /// </summary>
    public class Warehouse
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên kho hàng")]
        [DisplayName("Tên kho")]
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
