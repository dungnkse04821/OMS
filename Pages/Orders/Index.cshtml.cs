using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Models;
using OMS.Repositories;

namespace OMS.Pages.Orders
{
    public class IndexModel : PageModel
    {
        private readonly IOrderRepository _orderRepository;

        public IndexModel(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public List<Order> Orders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        // Counts for KPI / quick statistics
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int OrderedCount { get; set; }
        public int ShippingCount { get; set; }
        public int ArrivedCount { get; set; }
        public int DeliveredCount { get; set; }
        public int CancelledCount { get; set; }

        public async Task OnGetAsync()
        {
            var list = await _orderRepository.GetAllAsync();

            // Calculate status counts on all orders before applying filters
            TotalCount = list.Count;
            PendingCount = list.Count(o => o.Status == "Chờ đặt");
            OrderedCount = list.Count(o => o.Status == "Đã đặt");
            ShippingCount = list.Count(o => o.Status == "Đang về");
            ArrivedCount = list.Count(o => o.Status == "Đã về");
            DeliveredCount = list.Count(o => o.Status == "Đã giao");
            CancelledCount = list.Count(o => o.Status == "Hủy");

            // Apply Status Filter
            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                list = list.Where(o => o.Status == StatusFilter).ToList();
            }

            // Apply Search Query
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.Trim().ToLower();
                list = list.Where(o => 
                    o.Id.ToLower().Contains(query) || 
                    o.CustomerName.ToLower().Contains(query) || 
                    o.PhoneNumber.Contains(query) || 
                    o.Code.ToLower().Contains(query) || 
                    o.ProductName.ToLower().Contains(query)
                ).ToList();
            }

            // Order by date descending by default (if set)
            Orders = list.OrderByDescending(o => o.OrderDate ?? DateTime.MinValue).ToList();
        }
    }
}
