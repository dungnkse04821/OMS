using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models;

namespace OMS.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders.ToListAsync();
        }

        public async Task<Order> GetByIdAsync(string id)
        {
            return await _context.Orders.FindAsync(id);
        }

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            // Load existing order to detect status change for audit log and stock deduction
            var existing = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == order.Id);

            _context.Orders.Update(order);

            if (existing != null && existing.Status != order.Status)
            {
                // --- Audit Trail: record status change ---
                var log = new OrderLog
                {
                    OrderId = order.Id,
                    OldStatus = existing.Status,
                    NewStatus = order.Status,
                    ChangedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                };
                await _context.OrderLogs.AddAsync(log);

                // --- Inventory: deduct stock when order is delivered ---
                if (order.Status == "Đã giao" && !string.IsNullOrWhiteSpace(order.Code))
                {
                    var product = await _context.Products.FindAsync(order.Code);
                    if (product != null && product.StockQuantity > 0)
                    {
                        product.StockQuantity = Math.Max(0, product.StockQuantity - order.Quantity);
                        _context.Products.Update(product);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _context.Orders.FindAsync(id);
            if (entity != null)
            {
                _context.Orders.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task BulkUpdateAsync(string id, string newStatus, System.DateTime? arrivalDate, decimal? importPrice, decimal? paidAmount)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return;
            if (!string.IsNullOrEmpty(newStatus)) order.Status = newStatus;
            if (arrivalDate.HasValue) order.ArrivalDate = arrivalDate;
            if (importPrice.HasValue) order.ImportPrice = importPrice.Value;
            // paidAmount could be stored in Deposit field as an example
            if (paidAmount.HasValue) order.Deposit = paidAmount.Value;
            await _context.SaveChangesAsync();
        }

        public async Task<(List<Order> Data, Dictionary<string, int> StatusCounts)> SearchOrdersAsync(
            string? query, 
            List<string>? statuses, 
            List<string>? carriers, 
            DateTime? fromDate, 
            DateTime? toDate)
        {
            IQueryable<Order> baseQuery = _context.Orders;

            // 1. Date Filters
            if (fromDate.HasValue)
                baseQuery = baseQuery.Where(o => o.OrderDate >= fromDate.Value);
            
            if (toDate.HasValue)
                baseQuery = baseQuery.Where(o => o.OrderDate <= toDate.Value);

            // 2. Carrier Filters
            if (carriers != null && carriers.Any())
                baseQuery = baseQuery.Where(o => carriers.Contains(o.ShippingCarrier));

            // 3. Text Search (using GIN tsvector index)
            if (!string.IsNullOrWhiteSpace(query))
            {
                var formattedQuery = query.Trim();
                // WebSearchToTsQuery handles basic search logic gracefully (like quotes and operators)
                baseQuery = baseQuery.Where(o => o.SearchVector.Matches(EF.Functions.WebSearchToTsQuery("simple", formattedQuery)));
            }

            // At this point, we calculate the status counts (KPIs) based on the date/carrier/search filters,
            // BUT BEFORE applying the Status filter itself.
            var statusCountsQuery = await baseQuery
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var statusCounts = statusCountsQuery.ToDictionary(k => k.Status, v => v.Count);

            // 4. Status Filter (applied after counting)
            if (statuses != null && statuses.Any())
            {
                baseQuery = baseQuery.Where(o => statuses.Contains(o.Status));
            }

            // Default sorting
            baseQuery = baseQuery.OrderByDescending(o => o.OrderDate ?? DateTime.MinValue);

            var data = await baseQuery.ToListAsync();

            return (data, statusCounts);
        }
    }
}
