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
        public List<OrderLog> Logs { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return RedirectToPage("/Orders/Index");

            Order = await _context.Orders.FindAsync(id);
            if (Order == null)
                return NotFound();

            Logs = await _context.OrderLogs
                .Where(l => l.OrderId == id)
                .OrderByDescending(l => l.ChangedAt)
                .ToListAsync();

            return Page();
        }
    }
}
