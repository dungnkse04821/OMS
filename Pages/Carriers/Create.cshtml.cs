using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Data;
using OMS.Models;

namespace OMS.Pages.Carriers
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _ctx;
        public CreateModel(ApplicationDbContext ctx) => _ctx = ctx;

        [BindProperty]
        public Carrier Carrier { get; set; } = new() { SortOrder = 99 };

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            Carrier.IsSystem = false;
            _ctx.Carriers.Add(Carrier);
            await _ctx.SaveChangesAsync();
            TempData["SuccessMsg"] = $"✅ Đã thêm đơn vị vận chuyển \"{Carrier.Name}\".";
            return RedirectToPage("./Index");
        }
    }
}
