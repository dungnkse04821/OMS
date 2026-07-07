using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Data;
using OMS.Models;

namespace OMS.Pages.SupplySources
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _ctx;
        public EditModel(ApplicationDbContext ctx) => _ctx = ctx;

        [BindProperty]
        public SupplySource Source { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var source = await _ctx.SupplySources.FindAsync(id);
            if (source == null) return RedirectToPage("./Index");
            Source = source;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var existing = await _ctx.SupplySources.FindAsync(Source.Id);
            if (existing == null) return NotFound();

            existing.Name        = Source.Name;
            existing.Description = Source.Description;
            existing.IsActive    = Source.IsActive;
            existing.SortOrder   = Source.SortOrder;

            await _ctx.SaveChangesAsync();
            TempData["SuccessMsg"] = $"✅ Đã cập nhật nguồn hàng \"{existing.Name}\".";
            return RedirectToPage("./Index");
        }
    }
}
