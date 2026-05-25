using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Models;
using OMS.Repositories;

namespace OMS.Pages.Products
{
    public class DeleteModel : PageModel
    {
        private readonly IProductRepository _productRepository;

        public DeleteModel(IProductRepository productRepository)
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
            if (string.IsNullOrEmpty(Product.Sku))
            {
                return RedirectToPage("./Index");
            }

            await _productRepository.DeleteAsync(Product.Sku);
            return RedirectToPage("./Index");
        }
    }
}
