using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Data;
using OMS.Models;

namespace OMS.Pages.Warehouses
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _ctx;
        public EditModel(ApplicationDbContext ctx) => _ctx = ctx;

        [BindProperty]
        public Warehouse Warehouse { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var warehouse = await _ctx.Warehouses.FindAsync(id);
            if (warehouse == null) return RedirectToPage("./Index");
            Warehouse = warehouse;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var existing = await _ctx.Warehouses.FindAsync(Warehouse.Id);
            if (existing == null) return NotFound();

            existing.Name        = Warehouse.Name;
            existing.Description = Warehouse.Description;
            existing.IsActive    = Warehouse.IsActive;
            existing.SortOrder   = Warehouse.SortOrder;

            await _ctx.SaveChangesAsync();
            TempData["SuccessMsg"] = $"✅ Đã cập nhật kho hàng \"{existing.Name}\".";
            return RedirectToPage("./Index");
        }
    }
}
