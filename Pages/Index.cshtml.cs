using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Models;
using OMS.Repositories;

namespace OMS.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;

        public IndexModel(
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
        }

        // Metrics properties
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }

        // Debt summary
        public decimal TotalDebtAmount { get; set; }
        public int TotalDebtCount { get; set; }

        // Lists
        public List<Order> RecentOrders { get; set; } = new();

        public async Task OnGetAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            var customers = await _customerRepository.GetAllAsync();
            var products = await _productRepository.GetAllAsync();

            // Calculate Metrics
            TotalOrders = orders.Count;
            TotalCustomers = customers.Count;
            TotalProducts = products.Count;

            // Only sum financial metrics for non-cancelled orders
            var activeOrders = orders.Where(o => o.Status != "Hủy").ToList();
            TotalRevenue = activeOrders.Sum(o => o.TotalAmount);
            TotalDeposits = activeOrders.Sum(o => o.Deposit);
            TotalProfit = activeOrders.Sum(o => o.Profit);

            // Debt summary: orders with remaining amount > 0
            var debtOrders = activeOrders.Where(o => o.RemainingAmount > 0).ToList();
            TotalDebtAmount = debtOrders.Sum(o => o.RemainingAmount);
            TotalDebtCount = debtOrders.Count;

            // Get Top 5 Recent Orders
            RecentOrders = orders
                .OrderByDescending(o => o.OrderDate ?? DateTime.MinValue)
                .Take(5)
                .ToList();
        }
    }
}
