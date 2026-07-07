using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models;

namespace OMS.Pages.SupplySources
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _ctx;
        public IndexModel(ApplicationDbContext ctx) => _ctx = ctx;

        public List<SupplySource> Sources { get; set; } = new();
        public string? SuccessMsg { get; set; }

        public async Task OnGetAsync()
        {
            Sources = await _ctx.SupplySources
                .OrderBy(s => s.SortOrder).ThenBy(s => s.Name)
                .ToListAsync();

            SuccessMsg = TempData["SuccessMsg"]?.ToString();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var source = await _ctx.SupplySources.FindAsync(id);
            if (source == null) return NotFound();

            _ctx.SupplySources.Remove(source);
            await _ctx.SaveChangesAsync();
            TempData["SuccessMsg"] = $"✅ Đã xóa nguồn hàng \"{source.Name}\".";
            return RedirectToPage();
        }
    }
}
