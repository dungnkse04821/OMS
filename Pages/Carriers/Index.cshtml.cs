using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models;

namespace OMS.Pages.Carriers
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _ctx;
        public IndexModel(ApplicationDbContext ctx) => _ctx = ctx;

        public List<Carrier> Carriers { get; set; } = new();
        public string? SuccessMsg { get; set; }

        public async Task OnGetAsync()
        {
            Carriers = await _ctx.Carriers
                .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
                .ToListAsync();

            SuccessMsg = TempData["SuccessMsg"]?.ToString();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var carrier = await _ctx.Carriers.FindAsync(id);
            if (carrier == null) return NotFound();
            if (carrier.IsSystem)
            {
                TempData["SuccessMsg"] = "❌ Không thể xóa đơn vị vận chuyển mặc định của hệ thống.";
                return RedirectToPage();
            }
            // Soft-delete handled by DbContext interceptor
            _ctx.Carriers.Remove(carrier);
            await _ctx.SaveChangesAsync();
            TempData["SuccessMsg"] = $"✅ Đã xóa đơn vị vận chuyển \"{carrier.Name}\".";
            return RedirectToPage();
        }
    }
}
