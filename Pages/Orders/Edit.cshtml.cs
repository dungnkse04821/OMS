using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Models;
using OMS.Repositories;
using System.Text.Json;

namespace OMS.Pages.Orders
{
    public class EditModel : PageModel
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;

        public EditModel(
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

        // Dropdown lists
        public List<Customer> Customers { get; set; } = new();
        public List<Product> Products { get; set; } = new();

        // Serialized JSON for auto-completion
        public string CustomersJson { get; set; } = "[]";
        public string ProductsJson { get; set; } = "[]";

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToPage("./Index");
            }

            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return RedirectToPage("./Index");
            }

            Order = order;
            await LoadDropdownDataAsync();
            return Page();
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
            // Re-calculate financial fields securely on the server
            Order.TotalAmount = Order.SellingPrice * Order.Quantity;
            Order.RemainingAmount = Order.TotalAmount - Order.Deposit - Order.Discount;
            Order.TotalImportCost = Order.ImportPrice * Order.Quantity;
            Order.Profit = Order.TotalAmount - Order.TotalImportCost;

            // Remove generated values from ModelState
            ModelState.Remove("Order.TotalAmount");
            ModelState.Remove("Order.RemainingAmount");
            ModelState.Remove("Order.TotalImportCost");
            ModelState.Remove("Order.Profit");

            if (!ModelState.IsValid)
            {
                await LoadDropdownDataAsync();
                return Page();
            }

            try
            {
                await _orderRepository.UpdateAsync(Order);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật thông tin đơn hàng.");
                await LoadDropdownDataAsync();
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
