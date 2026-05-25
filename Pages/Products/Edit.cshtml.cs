using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Models;
using OMS.Repositories;

namespace OMS.Pages.Products
{
    public class EditModel : PageModel
    {
        private readonly IProductRepository _productRepository;

        public EditModel(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string sku)
        {
            if (string.IsNullOrEmpty(sku))
            {
                return RedirectToPage("./Index");
            }

            var product = await _productRepository.GetByIdAsync(sku);
            if (product == null)
            {
                return RedirectToPage("./Index");
            }

            Product = product;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _productRepository.UpdateAsync(Product);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật thông tin sản phẩm.");
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
