using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Data;
using OMS.Models;

namespace OMS.Pages.Carriers
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _ctx;
        public EditModel(ApplicationDbContext ctx) => _ctx = ctx;

        [BindProperty]
        public Carrier Carrier { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Carrier = await _ctx.Carriers.FindAsync(id) ?? default!;
            if (Carrier == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var existing = await _ctx.Carriers.FindAsync(Carrier.Id);
            if (existing == null) return NotFound();

            existing.Name = Carrier.Name;
            existing.TrackingUrlPattern = Carrier.TrackingUrlPattern;
            existing.SortOrder = Carrier.SortOrder;
            // Don't allow changing IsSystem via form
            await _ctx.SaveChangesAsync();
            TempData["SuccessMsg"] = $"✅ Đã cập nhật đơn vị vận chuyển \"{Carrier.Name}\".";
            return RedirectToPage("./Index");
        }
    }
}
