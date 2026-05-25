using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Models;
using OMS.Repositories;

namespace OMS.Pages.Products
{
    public class CreateModel : PageModel
    {
        private readonly IProductRepository _productRepository;

        public CreateModel(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check SKU uniqueness
            var existing = await _productRepository.GetByIdAsync(Product.Sku.Trim());
            if (existing != null)
            {
                ModelState.AddModelError("Product.Sku", "Mã SKU này đã tồn tại trong hệ thống.");
                return Page();
            }

            Product.Sku = Product.Sku.Trim().ToUpper();
            await _productRepository.AddAsync(Product);

            return RedirectToPage("./Index");
        }
    }
}
