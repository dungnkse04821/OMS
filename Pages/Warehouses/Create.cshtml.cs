using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Data;
using OMS.Models;

namespace OMS.Pages.Warehouses
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _ctx;
        public CreateModel(ApplicationDbContext ctx) => _ctx = ctx;

        [BindProperty]
        public Warehouse Warehouse { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            Warehouse.CreatedAt = DateTime.UtcNow;
            _ctx.Warehouses.Add(Warehouse);
            await _ctx.SaveChangesAsync();

            TempData["SuccessMsg"] = $"✅ Đã thêm kho hàng \"{Warehouse.Name}\".";
            return RedirectToPage("./Index");
        }
    }
}
