using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OMS.Models;

namespace OMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<OrderLog> OrderLogs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Carrier> Carriers { get; set; }

        // ── Global Query Filters: automatically exclude soft-deleted records ──────
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>().HasQueryFilter(e => !e.IsDeleted);

            // Configure GIN Full-Text Index for Orders
            modelBuilder.Entity<Order>()
                .HasGeneratedTsVectorColumn(
                    p => p.SearchVector,
                    "simple", // using 'simple' to allow prefix matching more easily, or 'english' for stemming
                    p => new { p.Id, p.Code, p.ProductName, p.CustomerName, p.PhoneNumber, p.TrackingCode })
                .HasIndex(p => p.SearchVector)
                .HasMethod("GIN");

            // B-tree indexes for pagination & filtering performance
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.CreatedAt)
                .HasDatabaseName("IX_Orders_CreatedAt");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.ShippingCarrier)
                .HasDatabaseName("IX_Orders_ShippingCarrier");

            modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Carrier>().HasQueryFilter(e => !e.IsDeleted);

            // ── Seed default system carriers ──────────────────────────────
            modelBuilder.Entity<Carrier>().HasData(
                new Carrier { Id = 1,  Name = "GHTK",             TrackingUrlPattern = "https://i.ghtk.vn/{code}",                                              IsSystem = true, SortOrder = 1 },
                new Carrier { Id = 2,  Name = "GHN",              TrackingUrlPattern = "https://donhang.ghn.vn/?order_code={code}",                             IsSystem = true, SortOrder = 2 },
                new Carrier { Id = 3,  Name = "Viettel Post",     TrackingUrlPattern = "https://viettelpost.com.vn/tra-cuu-hanh-trinh-don/?billCode={code}",    IsSystem = true, SortOrder = 3 },
                new Carrier { Id = 4,  Name = "Vietnam Post",     TrackingUrlPattern = "https://www.vnpost.vn/vi-vn/dinh-vi/buu-pham?key={code}",              IsSystem = true, SortOrder = 4 },
                new Carrier { Id = 5,  Name = "J&T Express",      TrackingUrlPattern = "https://jtexpress.vn/tracking?bills={code}",                           IsSystem = true, SortOrder = 5 },
                new Carrier { Id = 6,  Name = "Best Express",     TrackingUrlPattern = "https://best-inc-vn.com/track?bills={code}",                           IsSystem = true, SortOrder = 6 },
                new Carrier { Id = 7,  Name = "Ninja Van",        TrackingUrlPattern = "https://www.ninjavan.co/vi-vn/tracking?id={code}",                     IsSystem = true, SortOrder = 7 },
                new Carrier { Id = 8,  Name = "SPX Express",      TrackingUrlPattern = "https://spx.vn/tracking/?trackingNumber={code}",                       IsSystem = true, SortOrder = 8 },
                new Carrier { Id = 9,  Name = "Lazada Logistics", TrackingUrlPattern = "https://www.lazada.vn/order/{code}/",                                  IsSystem = true, SortOrder = 9 },
                new Carrier { Id = 10, Name = "Tiki Now",         TrackingUrlPattern = "https://tiki.vn/tracking?orderCode={code}",                            IsSystem = true, SortOrder = 10 },
                new Carrier { Id = 11, Name = "Khác",             TrackingUrlPattern = null,                                                                   IsSystem = true, SortOrder = 99 }
            );
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
