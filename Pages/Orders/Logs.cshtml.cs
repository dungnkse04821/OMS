using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models;

namespace OMS.Pages.Orders
{
    public class LogsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LogsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Order? Order { get; set; }

        /// <summary>Legacy status-change logs (backward compat)</summary>
        public List<OrderLog> StatusLogs { get; set; } = new();

        /// <summary>New universal audit entries for this order</summary>
        public List<OMS.Models.AuditLog> AuditLogs { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return RedirectToPage("/Orders/Index");

            // IgnoreQueryFilters so soft-deleted orders can still show their logs
            Order = await _context.Orders
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(o => o.Id == id);

            if (Order == null)
                return NotFound();

            StatusLogs = await _context.OrderLogs
                .Where(l => l.OrderId == id)
                .OrderByDescending(l => l.ChangedAt)
                .ToListAsync();

            AuditLogs = await _context.AuditLogs
                .Where(l => l.EntityType == "Order" && l.EntityId == id)
                .OrderByDescending(l => l.ChangedAt)
                .ToListAsync();

            return Page();
        }
    }
}
