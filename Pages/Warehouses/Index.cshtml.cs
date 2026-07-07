using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models;

namespace OMS.Pages.Warehouses
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _ctx;
        public IndexModel(ApplicationDbContext ctx) => _ctx = ctx;

        public List<Warehouse> Warehouses { get; set; } = new();
        public string? SuccessMsg { get; set; }

        public async Task OnGetAsync()
        {
            Warehouses = await _ctx.Warehouses
                .OrderBy(w => w.SortOrder).ThenBy(w => w.Name)
                .ToListAsync();

            SuccessMsg = TempData["SuccessMsg"]?.ToString();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var warehouse = await _ctx.Warehouses.FindAsync(id);
            if (warehouse == null) return NotFound();

            _ctx.Warehouses.Remove(warehouse);
            await _ctx.SaveChangesAsync();
            TempData["SuccessMsg"] = $"✅ Đã xóa kho hàng \"{warehouse.Name}\".";
            return RedirectToPage();
        }
    }
}
