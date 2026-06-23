using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OMS.Models
{
    /// <summary>
    /// User-defined shipping carrier with tracking URL pattern.
    /// System carriers are seeded and cannot be deleted; user carriers can be fully managed.
    /// </summary>
    public class Carrier
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đơn vị vận chuyển")]
        [DisplayName("Tên đơn vị")]
        [MaxLength(100)]
        public string Name { get; set; } = "";

        /// <summary>
        /// Tracking URL pattern. Use {code} as placeholder for the tracking code.
        /// Example: https://i.ghtk.vn/{code}
        /// Leave empty to fallback to Google search.
        /// </summary>
        [DisplayName("URL tra cứu (dùng {code} cho mã vận đơn)")]
        [MaxLength(500)]
        public string? TrackingUrlPattern { get; set; }

        /// <summary>True for built-in system carriers that cannot be deleted.</summary>
        public bool IsSystem { get; set; } = false;

        [DisplayName("Thứ tự hiển thị")]
        public int SortOrder { get; set; } = 99;

        public bool IsDeleted { get; set; } = false;
    }
}
