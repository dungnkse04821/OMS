using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models;

namespace OMS.Pages.AuditLog
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Models.AuditLog> Logs { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? FilterEntity { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterAction { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterId { get; set; }

        public int TotalCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(FilterEntity))
                query = query.Where(l => l.EntityType == FilterEntity);

            if (!string.IsNullOrWhiteSpace(FilterAction))
                query = query.Where(l => l.Action == FilterAction);

            if (!string.IsNullOrWhiteSpace(FilterId))
                query = query.Where(l => l.EntityId.Contains(FilterId));

            TotalCount = await query.CountAsync();

            Logs = await query
                .OrderByDescending(l => l.ChangedAt)
                .Take(200)   // cap at 200 rows per page for performance
                .ToListAsync();

            return Page();
        }
    }
}
