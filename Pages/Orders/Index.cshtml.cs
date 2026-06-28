using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models;
using OMS.Repositories;

namespace OMS.Pages.Orders
{
    public class IndexModel : PageModel
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ApplicationDbContext _ctx;

        public IndexModel(IOrderRepository orderRepository, ApplicationDbContext ctx)
        {
            _orderRepository = orderRepository;
            _ctx = ctx;
        }

        public List<Order> Orders { get; set; } = new();
        public List<Carrier> Carriers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<string> StatusFilters { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public List<string> CarrierFilters { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        // ── Pagination ──────────────────────────────────────────────────
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 20;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

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
            // Get carriers for the filter UI
            Carriers = await _ctx.Carriers.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync();

            if (CurrentPage < 1) CurrentPage = 1;

            var result = await _orderRepository.SearchOrdersAsync(
                SearchQuery, 
                StatusFilters, 
                CarrierFilters, 
                FromDate, 
                ToDate,
                CurrentPage,
                PageSize);

            Orders = result.Data;
            TotalItems = result.TotalCount;
            var counts = result.StatusCounts;

            TotalCount = counts.Values.Sum();
            PendingCount = counts.GetValueOrDefault("Chờ đặt", 0);
            OrderedCount = counts.GetValueOrDefault("Đã đặt", 0);
            ShippingCount = counts.GetValueOrDefault("Đang về", 0);
            ArrivedCount = counts.GetValueOrDefault("Đã về", 0);
            DeliveredCount = counts.GetValueOrDefault("Đã giao", 0);
            CancelledCount = counts.GetValueOrDefault("Hủy", 0);
        }

        // Bulk update handler — nhận danh sách ID và trạng thái mới
        public async Task<IActionResult> OnPostBulkUpdateAsync(
            [FromForm] string[] selectedIds,
            [FromForm] string bulkStatus)
        {
            if (selectedIds == null || selectedIds.Length == 0 || string.IsNullOrWhiteSpace(bulkStatus))
            {
                return RedirectToPage();
            }

            var allOrders = await _orderRepository.GetAllAsync();
            foreach (var id in selectedIds)
            {
                var order = allOrders.FirstOrDefault(o => o.Id == id);
                if (order != null)
                {
                    order.Status = bulkStatus;
                    await _orderRepository.UpdateAsync(order);
                }
            }

            return RedirectToPage(new { SearchQuery, StatusFilters, CarrierFilters, FromDate, ToDate, CurrentPage });
        }

        /// <summary>
        /// Builds a query string preserving all current filters + overriding the page number.
        /// </summary>
        public string BuildPageUrl(int page)
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(SearchQuery))
                queryParams.Add($"SearchQuery={Uri.EscapeDataString(SearchQuery)}");
            
            foreach (var s in StatusFilters)
                queryParams.Add($"StatusFilters={Uri.EscapeDataString(s)}");
            
            foreach (var c in CarrierFilters)
                queryParams.Add($"CarrierFilters={Uri.EscapeDataString(c)}");
            
            if (FromDate.HasValue)
                queryParams.Add($"FromDate={FromDate.Value:yyyy-MM-dd}");
            
            if (ToDate.HasValue)
                queryParams.Add($"ToDate={ToDate.Value:yyyy-MM-dd}");
            
            queryParams.Add($"CurrentPage={page}");
            
            return "/Orders?" + string.Join("&", queryParams);
        }
    }
}
