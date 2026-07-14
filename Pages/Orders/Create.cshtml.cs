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
        public List<Warehouse> Warehouses { get; set; } = new();
        public List<SupplySource> SupplySources { get; set; } = new();

        public string CustomersJson { get; set; } = "[]";
        public string ProductsJson  { get; set; } = "[]";

        // ── AJAX: Quick-add a new Warehouse ───────────────────────────────
        public async Task<IActionResult> OnPostQuickAddWarehouseAsync([FromBody] QuickAddRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.Name))
                return BadRequest(new { error = "Tên kho không được để trống." });

            var name = req.Name.Trim();
            var exists = await _ctx.Warehouses.AnyAsync(w => w.Name == name);
            if (exists)
                return StatusCode(409, new { error = $"Kho \"{name}\" đã tồn tại." });

            var wh = new Warehouse { Name = name, IsActive = true, SortOrder = 99, CreatedAt = DateTime.UtcNow };
            _ctx.Warehouses.Add(wh);
            await _ctx.SaveChangesAsync();
            return new JsonResult(new { name = wh.Name, id = wh.Id });
        }

        // ── AJAX: Quick-add a new SupplySource ────────────────────────────
        public async Task<IActionResult> OnPostQuickAddSourceAsync([FromBody] QuickAddRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.Name))
                return BadRequest(new { error = "Tên nguồn hàng không được để trống." });

            var name = req.Name.Trim();
            var exists = await _ctx.SupplySources.AnyAsync(s => s.Name == name);
            if (exists)
                return StatusCode(409, new { error = $"Nguồn \"{name}\" đã tồn tại." });

            var src = new SupplySource { Name = name, IsActive = true, SortOrder = 99, CreatedAt = DateTime.UtcNow };
            _ctx.SupplySources.Add(src);
            await _ctx.SaveChangesAsync();
            return new JsonResult(new { name = src.Name, id = src.Id });
        }

        public record QuickAddRequest(string Name);

        public async Task OnGetAsync()
        {
            Order.OrderDate = DateTime.Today;
            Order.Status = "Chờ đặt";
            Order.Quantity = 1;
            await LoadDropdownDataAsync();
        }

        private async Task LoadDropdownDataAsync()
        {
            Customers     = await _customerRepository.GetAllAsync();
            Products      = await _productRepository.GetAllAsync();
            Carriers      = await _ctx.Carriers.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync();
            Warehouses    = await _ctx.Warehouses.Where(w => w.IsActive).OrderBy(w => w.SortOrder).ThenBy(w => w.Name).ToListAsync();
            SupplySources = await _ctx.SupplySources.Where(s => s.IsActive).OrderBy(s => s.SortOrder).ThenBy(s => s.Name).ToListAsync();

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

            // ── Auto-create Customer if phone not found ────────────────────
            if (!string.IsNullOrWhiteSpace(Order.PhoneNumber))
            {
                var allCustomers = await _customerRepository.GetAllAsync();
                var existingCustomer = allCustomers.FirstOrDefault(c =>
                    c.PhoneNumber == Order.PhoneNumber.Trim());

                if (existingCustomer == null)
                {
                    // Generate customer ID
                    int nextCustNo = allCustomers.Count + 1;
                    string custId  = $"KH{nextCustNo:D4}";
                    while (allCustomers.Any(c => c.Id == custId))
                    {
                        nextCustNo++;
                        custId = $"KH{nextCustNo:D4}";
                    }

                    await _customerRepository.AddAsync(new Customer
                    {
                        Id          = custId,
                        FullName    = Order.CustomerName ?? Order.PhoneNumber,
                        PhoneNumber = Order.PhoneNumber.Trim(),
                        Address     = Order.ShippingAddress ?? "",
                        CreatedAt   = DateTime.UtcNow,
                    });
                }
            }

            // ── Auto-create Product if SKU not found ───────────────────────
            if (!string.IsNullOrWhiteSpace(Order.Code))
            {
                var existingProduct = await _productRepository.GetByIdAsync(Order.Code.Trim());
                if (existingProduct == null)
                {
                    await _productRepository.AddAsync(new Product
                    {
                        Sku          = Order.Code.Trim(),
                        Name         = Order.ProductName ?? Order.Code,
                        Category     = Order.Category ?? "",
                        ImportPrice  = Order.ImportPrice,
                        SellingPrice = Order.SellingPrice,
                        Source       = Order.Source ?? "",
                        Warehouse    = Order.Warehouse ?? "",
                        CreatedAt    = DateTime.UtcNow,
                    });
                }
            }

            await _orderRepository.AddAsync(Order);
            return RedirectToPage("./Index");
        }
    }
}
