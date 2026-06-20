using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Models;
using OMS.Repositories;
using System.Text.Json;

namespace OMS.Pages.Reports
{
    public class IndexModel : PageModel
    {
        private readonly IOrderRepository _orderRepository;

        public IndexModel(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // KPI
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfit { get; set; }
        public int OrderCount { get; set; }
        public decimal AverageOrderValue { get; set; }

        // Status breakdown
        public int DeliveredCount { get; set; }
        public int PendingCount { get; set; }
        public int CancelledCount { get; set; }

        // Chart data (JSON for Chart.js)
        public string DailyLabelsJson { get; set; } = "[]";
        public string DailyRevenueJson { get; set; } = "[]";
        public string DailyProfitJson { get; set; } = "[]";
        public string TopProductLabelsJson { get; set; } = "[]";
        public string TopProductQtyJson { get; set; } = "[]";
        public string TopProductRevenueJson { get; set; } = "[]";

        // Filter
        [BindProperty(SupportsGet = true)]
        public string? Month { get; set; }

        public string DisplayMonth { get; set; } = "";

        public async Task OnGetAsync()
        {
            var allOrders = await _orderRepository.GetAllAsync();

            // Determine filter month
            DateTime filterMonth;
            if (!DateTime.TryParseExact(Month, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out filterMonth))
                filterMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            DisplayMonth = filterMonth.ToString("MM/yyyy");
            var nextMonth = filterMonth.AddMonths(1);

            // Filter orders for the selected month (delivered orders only for revenue/profit)
            var monthOrders = allOrders
                .Where(o => o.OrderDate >= filterMonth && o.OrderDate < nextMonth)
                .ToList();

            OrderCount = monthOrders.Count;
            TotalRevenue = monthOrders.Sum(o => o.TotalAmount);
            TotalCost = monthOrders.Sum(o => o.TotalImportCost);
            TotalProfit = monthOrders.Sum(o => o.Profit);
            AverageOrderValue = OrderCount > 0 ? TotalRevenue / OrderCount : 0;

            DeliveredCount = monthOrders.Count(o => o.Status == "Đã giao");
            CancelledCount = monthOrders.Count(o => o.Status == "Hủy");
            PendingCount = monthOrders.Count(o => o.Status != "Đã giao" && o.Status != "Hủy");

            // Daily revenue & profit chart
            var days = Enumerable.Range(1, DateTime.DaysInMonth(filterMonth.Year, filterMonth.Month))
                .Select(d => new DateTime(filterMonth.Year, filterMonth.Month, d))
                .ToList();

            var dailyGroups = monthOrders
                .GroupBy(o => o.OrderDate?.Date ?? DateTime.MinValue)
                .ToDictionary(g => g.Key, g => g.ToList());

            var dailyLabels = days.Select(d => d.ToString("dd/MM")).ToList();
            var dailyRevenue = days.Select(d => dailyGroups.TryGetValue(d, out var g) ? g.Sum(o => o.TotalAmount) : 0).ToList();
            var dailyProfit = days.Select(d => dailyGroups.TryGetValue(d, out var g) ? g.Sum(o => o.Profit) : 0).ToList();

            DailyLabelsJson = JsonSerializer.Serialize(dailyLabels);
            DailyRevenueJson = JsonSerializer.Serialize(dailyRevenue);
            DailyProfitJson = JsonSerializer.Serialize(dailyProfit);

            // Top 7 products by quantity (all time, not just this month)
            var topProducts = allOrders
                .Where(o => !string.IsNullOrWhiteSpace(o.ProductName))
                .GroupBy(o => o.ProductName)
                .Select(g => new {
                    Name = g.Key,
                    Qty = g.Sum(o => o.Quantity),
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderByDescending(x => x.Qty)
                .Take(7)
                .ToList();

            TopProductLabelsJson = JsonSerializer.Serialize(topProducts.Select(p => p.Name).ToList());
            TopProductQtyJson = JsonSerializer.Serialize(topProducts.Select(p => p.Qty).ToList());
            TopProductRevenueJson = JsonSerializer.Serialize(topProducts.Select(p => p.Revenue).ToList());
        }
    }
}
