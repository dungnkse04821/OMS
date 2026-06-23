using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OMS.Models
{
    public class Customer
    {
        [Key]
        public string Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Họ và tên")]
        [DisplayName("Họ và tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại")]
        [DisplayName("Số điện thoại")]
        public string PhoneNumber { get; set; }

        [DisplayName("Reference")]
        public string? Reference { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Địa chỉ")]
        [DisplayName("Địa chỉ")]
        public string Address { get; set; }

        [DisplayName("Ghi chú")]
        public string? Note { get; set; }

        // ── Soft-delete & audit metadata ─────────────────────────────────
        public bool IsDeleted { get; set; } = false;

        [DisplayName("Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DisplayName("Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }
    }
}
