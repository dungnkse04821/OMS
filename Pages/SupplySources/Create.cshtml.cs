using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Data;
using OMS.Models;

namespace OMS.Pages.SupplySources
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _ctx;
        public CreateModel(ApplicationDbContext ctx) => _ctx = ctx;

        [BindProperty]
        public SupplySource Source { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            Source.CreatedAt = DateTime.UtcNow;
            _ctx.SupplySources.Add(Source);
            await _ctx.SaveChangesAsync();

            TempData["SuccessMsg"] = $"✅ Đã thêm nguồn hàng \"{Source.Name}\".";
            return RedirectToPage("./Index");
        }
    }
}
