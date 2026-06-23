using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
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
        private readonly ApplicationDbContext _ctx;

        public CreateModel(
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            ApplicationDbContext ctx)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _ctx = ctx;
        }

        [BindProperty]
        public Order Order { get; set; } = new();

        public List<Customer> Customers { get; set; } = new();
        public List<Product> Products { get; set; } = new();
        public List<Carrier> Carriers { get; set; } = new();

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
            Products  = await _productRepository.GetAllAsync();
            Carriers  = await _ctx.Carriers.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync();

            CustomersJson = JsonSerializer.Serialize(Customers);
            ProductsJson  = JsonSerializer.Serialize(Products);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Order.TotalAmount      = Order.SellingPrice * Order.Quantity;
            Order.RemainingAmount  = Order.TotalAmount - Order.Deposit - Order.Discount;
            Order.TotalImportCost  = Order.ImportPrice * Order.Quantity;
            Order.Profit           = Order.TotalAmount - Order.TotalImportCost;

            var existing   = await _orderRepository.GetAllAsync();
            int nextNumber = existing.Count + 1;
            string newId   = $"DH{nextNumber:D4}";
            while (existing.Any(o => o.Id == newId))
            {
                nextNumber++;
                newId = $"DH{nextNumber:D4}";
            }
            Order.Id = newId;

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
