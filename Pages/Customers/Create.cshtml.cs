using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Models;
using OMS.Repositories;

namespace OMS.Pages.Customers
{
    public class CreateModel : PageModel
    {
        private readonly ICustomerRepository _customerRepository;

        public CreateModel(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        [BindProperty]
        public Customer Customer { get; set; } = new();

        public void OnGet()
        {
            // Initial load
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Generate unique customer ID like KH0001, KH0002...
            var existing = await _customerRepository.GetAllAsync();
            int nextNumber = existing.Count + 1;
            string newId = $"KH{nextNumber:D4}";

            // Guarantee uniqueness
            while (existing.Any(c => c.Id == newId))
            {
                nextNumber++;
                newId = $"KH{nextNumber:D4}";
            }

            Customer.Id = newId;

            await _customerRepository.AddAsync(Customer);
            return RedirectToPage("./Index");
        }
    }
}
