using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using OMS.Models;
using OMS.Repositories;
using System.Text.Json;

namespace OMS.Pages.Orders
{
    public class CreateModel : PageModel
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;

        public CreateModel(
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
        }

        [BindProperty]
        public Order Order { get; set; } = new();

        // Dropdowns data
        public List<Customer> Customers { get; set; } = new();
        public List<Product> Products { get; set; } = new();
        
        // Serialized JSON data for auto-population in Javascript
        public string CustomersJson { get; set; } = "[]";
        public string ProductsJson { get; set; } = "[]";

        public async Task OnGetAsync()
        {
            Order.OrderDate = DateTime.Today;
            Order.Status = "Chờ đặt";
            Order.Quantity = 1;

            await LoadDropdownDataAsync();
        }

        private async Task LoadDropdownDataAsync()
        {
            Customers = await _customerRepository.GetAllAsync();
            Products = await _productRepository.GetAllAsync();

            CustomersJson = JsonSerializer.Serialize(Customers);
            ProductsJson = JsonSerializer.Serialize(Products);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Re-calculate math fields securely on server side
            Order.TotalAmount = Order.SellingPrice * Order.Quantity;
            Order.RemainingAmount = Order.TotalAmount - Order.Deposit - Order.Discount;
            Order.TotalImportCost = Order.ImportPrice * Order.Quantity;
            Order.Profit = Order.TotalAmount - Order.TotalImportCost;

            // Generate unique Order ID like DH0001
            var existing = await _orderRepository.GetAllAsync();
            int nextNumber = existing.Count + 1;
            string newId = $"DH{nextNumber:D4}";
            while (existing.Any(o => o.Id == newId))
            {
                nextNumber++;
                newId = $"DH{nextNumber:D4}";
            }
            Order.Id = newId;

            // Simple validation override (we recalculate fields above so they don't block validation if not present in binding)
            ModelState.Remove("Order.Id");
            ModelState.Remove("Order.TotalAmount");
            ModelState.Remove("Order.RemainingAmount");
            ModelState.Remove("Order.TotalImportCost");
            ModelState.Remove("Order.Profit");

            if (!ModelState.IsValid)
            {
                await LoadDropdownDataAsync();
                return Page();
            }

            await _orderRepository.AddAsync(Order);
            return RedirectToPage("./Index");
        }
    }
}
