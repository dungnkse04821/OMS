using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OMS.Models;

namespace OMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<OrderLog> OrderLogs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // ── Global Query Filters: automatically exclude soft-deleted records ──────
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
        }

        // ── Override SaveChangesAsync: soft-delete interception + audit logging ──
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = new List<AuditLog>();
            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

            // Entities we want to audit
            var trackedTypes = new[] { typeof(Order), typeof(Customer), typeof(Product) };

            foreach (var entry in ChangeTracker.Entries()
                         .Where(e => trackedTypes.Contains(e.Entity.GetType())
                                     && e.State is EntityState.Added
                                             or EntityState.Modified
                                             or EntityState.Deleted))
            {
                var entityType = entry.Entity.GetType().Name;
                var entityId = GetPrimaryKey(entry);

                switch (entry.State)
                {
                    // ── CREATE ────────────────────────────────────────────────────
                    case EntityState.Added:
                        SetTimestamp(entry, "CreatedAt", now);
                        auditEntries.Add(new AuditLog
                        {
                            EntityType = entityType,
                            EntityId   = entityId,
                            Action     = "Create",
                            OldValues  = null,
                            NewValues  = SerializeCurrentValues(entry),
                            ChangedAt  = now
                        });
                        break;

                    // ── UPDATE ────────────────────────────────────────────────────
                    case EntityState.Modified:
                        SetTimestamp(entry, "UpdatedAt", now);
                        // If this is a soft-delete (IsDeleted flipped to true), tag as Delete
                        var isDeleteAction = entry.Property("IsDeleted").IsModified
                                            && (bool)entry.CurrentValues["IsDeleted"]!;
                        auditEntries.Add(new AuditLog
                        {
                            EntityType = entityType,
                            EntityId   = entityId,
                            Action     = isDeleteAction ? "Delete" : "Update",
                            OldValues  = SerializeOriginalValues(entry),
                            NewValues  = isDeleteAction ? null : SerializeCurrentValues(entry),
                            ChangedAt  = now
                        });
                        break;

                    // ── HARD DELETE (intercepted → converted to soft delete) ──────
                    case EntityState.Deleted:
                        // Convert physical delete → soft delete
                        entry.State = EntityState.Modified;
                        entry.CurrentValues["IsDeleted"] = true;
                        SetTimestamp(entry, "UpdatedAt", now);
                        auditEntries.Add(new AuditLog
                        {
                            EntityType = entityType,
                            EntityId   = entityId,
                            Action     = "Delete",
                            OldValues  = SerializeOriginalValues(entry),
                            NewValues  = null,
                            ChangedAt  = now
                        });
                        break;
                }
            }

            // Persist the main changes first
            var result = await base.SaveChangesAsync(cancellationToken);

            // Then persist audit logs (separate save to avoid recursive intercept)
            if (auditEntries.Count > 0)
            {
                AuditLogs.AddRange(auditEntries);
                await base.SaveChangesAsync(cancellationToken);
            }

            return result;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static string GetPrimaryKey(EntityEntry entry)
        {
            var keyValues = entry.Metadata.FindPrimaryKey()
                                ?.Properties
                                .Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? "")
                                .ToList();
            return keyValues != null ? string.Join(",", keyValues) : "";
        }

        private static void SetTimestamp(EntityEntry entry, string propertyName, DateTime value)
        {
            try
            {
                var prop = entry.Properties.FirstOrDefault(p => p.Metadata.Name == propertyName);
                if (prop != null) prop.CurrentValue = value;
            }
            catch { /* property may not exist on all entities */ }
        }

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private static string? SerializeCurrentValues(EntityEntry entry)
        {
            try
            {
                var dict = entry.CurrentValues.Properties
                    .Where(p => p.Name != "IsDeleted") // skip internal
                    .ToDictionary(p => p.Name, p => entry.CurrentValues[p]?.ToString());
                return JsonSerializer.Serialize(dict, _jsonOpts);
            }
            catch { return null; }
        }

        private static string? SerializeOriginalValues(EntityEntry entry)
        {
            try
            {
                var dict = entry.OriginalValues.Properties
                    .Where(p => p.Name != "IsDeleted")
                    .ToDictionary(p => p.Name, p => entry.OriginalValues[p]?.ToString());
                return JsonSerializer.Serialize(dict, _jsonOpts);
            }
            catch { return null; }
        }
    }
}
