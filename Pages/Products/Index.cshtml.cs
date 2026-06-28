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

        // ── Pagination ──────────────────────────────────────────────────
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 20;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public async Task OnGetAsync()
        {
            if (CurrentPage < 1) CurrentPage = 1;

            var result = await _productRepository.SearchAsync(SearchQuery, CurrentPage, PageSize);
            Products = result.Data;
            TotalItems = result.TotalCount;
        }

        public string BuildPageUrl(int page)
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(SearchQuery))
                queryParams.Add($"SearchQuery={Uri.EscapeDataString(SearchQuery)}");
            
            queryParams.Add($"CurrentPage={page}");
            
            return "/Products?" + string.Join("&", queryParams);
        }
    }
}
