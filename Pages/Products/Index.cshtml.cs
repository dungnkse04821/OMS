using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Models;
using OMS.Repositories;

namespace OMS.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly IProductRepository _productRepository;

        public IndexModel(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public List<Product> Products { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        public async Task OnGetAsync()
        {
            var list = await _productRepository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.Trim().ToLower();
                list = list.Where(p => 
                    p.Sku.ToLower().Contains(query) || 
                    p.Name.ToLower().Contains(query) || 
                    p.Category.ToLower().Contains(query) || 
                    p.Source.ToLower().Contains(query)
                ).ToList();
            }

            Products = list;
        }
    }
}
