using System.ComponentModel.DataAnnotations;

namespace OMS.Models
{
    public class OrderLog
    {
        [Key]
        public int Id { get; set; }

        public string OrderId { get; set; } = "";

        public string OldStatus { get; set; } = "";

        public string NewStatus { get; set; } = "";

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public string? Note { get; set; }
    }
}
