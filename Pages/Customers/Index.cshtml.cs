using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Models;
using OMS.Repositories;

namespace OMS.Pages.Customers
{
    public class IndexModel : PageModel
    {
        private readonly ICustomerRepository _customerRepository;

        public IndexModel(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public List<Customer> Customers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        public async Task OnGetAsync()
        {
            var list = await _customerRepository.GetAllAsync();
            
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.Trim().ToLower();
                list = list.Where(c => 
                    c.FullName.ToLower().Contains(query) || 
                    c.PhoneNumber.Contains(query) || 
                    (c.Reference != null && c.Reference.ToLower().Contains(query))
                ).ToList();
            }

            Customers = list;
        }
    }
}
