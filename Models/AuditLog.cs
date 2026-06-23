using System.ComponentModel.DataAnnotations;

namespace OMS.Models
{
    /// <summary>
    /// Universal audit log capturing every CRUD operation on Order, Customer, Product.
    /// Records are written automatically by ApplicationDbContext.SaveChangesAsync().
    /// </summary>
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Entity type: "Order", "Customer", "Product"</summary>
        public string EntityType { get; set; } = "";

        /// <summary>Primary key of the affected entity (as string)</summary>
        public string EntityId { get; set; } = "";

        /// <summary>Action performed: "Create", "Update", "Delete" (soft)</summary>
        public string Action { get; set; } = "";

        /// <summary>JSON snapshot of property values BEFORE the change (null for Create)</summary>
        public string? OldValues { get; set; }

        /// <summary>JSON snapshot of property values AFTER the change (null for Delete)</summary>
        public string? NewValues { get; set; }

        /// <summary>UTC timestamp when the change happened</summary>
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Optional human-readable note (e.g. reason for deletion)</summary>
        public string? Note { get; set; }
    }
}
