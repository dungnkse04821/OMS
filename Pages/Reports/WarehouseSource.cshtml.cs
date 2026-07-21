using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Models;
using OMS.Repositories;
using System.Text.Json;

namespace OMS.Pages.Reports
{
    /// <summary>Stats aggregated by a single dimension (Source or Warehouse).</summary>
    public class DimensionStat
    {
        public string Name          { get; init; } = "";
        public int    OrderCount    { get; init; }
        public int    ActiveOrders  { get; init; }   // not Hủy, not Đã giao
        public int    DeliveredCount{ get; init; }
        public int    CancelledCount{ get; init; }
        public decimal TotalRevenue { get; init; }   // TotalAmount (excl Hủy)
        public decimal TotalCost    { get; init; }   // TotalImportCost (excl Hủy)
        public decimal TotalProfit  { get; init; }   // Profit (excl Hủy)
        public decimal TotalPaid    { get; init; }   // Deposit (excl Hủy)
        public decimal TotalDebt    { get; init; }   // RemainingAmount > 0 (excl Hủy)
        public int    TotalQty      { get; init; }
        public decimal MarginPct    => TotalRevenue > 0 ? TotalProfit / TotalRevenue * 100 : 0;
        public decimal CollectRate  => TotalRevenue > 0 ? TotalPaid   / TotalRevenue * 100 : 0;
    }

    /// <summary>Monthly revenue split by source (for trend chart).</summary>
    public class MonthlyTrend
    {
        public string Month  { get; init; } = "";
        public string Source { get; init; } = "";
        public decimal Revenue { get; init; }
    }

    public class WarehouseSourceModel : PageModel
    {
        private readonly IOrderRepository _orderRepository;

        public WarehouseSourceModel(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // ── Filter ────────────────────────────────────────────────────────
        [BindProperty(SupportsGet = true)]
        public string? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Tab { get; set; } = "source";   // source | warehouse

        // ── Data ──────────────────────────────────────────────────────────
        public List<DimensionStat> SourceStats    { get; set; } = new();
        public List<DimensionStat> WarehouseStats { get; set; } = new();

        // ── Global totals ─────────────────────────────────────────────────
        public int     TotalOrders  { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost    { get; set; }
        public decimal TotalProfit  { get; set; }
        public decimal TotalDebt    { get; set; }

        // ── Chart JSON ────────────────────────────────────────────────────
        public string SourceLabelsJson   { get; set; } = "[]";
        public string SourceRevenueJson  { get; set; } = "[]";
        public string SourceProfitJson   { get; set; } = "[]";
        public string SourceOrdersJson   { get; set; } = "[]";

        public string WhLabelsJson       { get; set; } = "[]";
        public string WhRevenueJson      { get; set; } = "[]";
        public string WhProfitJson       { get; set; } = "[]";
        public string WhOrdersJson       { get; set; } = "[]";

        // Monthly trend (last 6 months by source)
        public string TrendLabelsJson    { get; set; } = "[]";
        public string TrendDatasetsJson  { get; set; } = "[]";

        public string DisplayRange       { get; set; } = "";

        public async Task OnGetAsync()
        {
            var allOrders = await _orderRepository.GetAllAsync();

            // ── Date filter ───────────────────────────────────────────────
            DateTime? from = null, to = null;
            if (DateTime.TryParse(FromDate, out var fd)) from = fd.Date;
            if (DateTime.TryParse(ToDate,   out var td)) to   = td.Date.AddDays(1).AddTicks(-1);

            if (from == null && to == null)
            {
                // Default: current month
                var now = DateTime.Now;
                from = new DateTime(now.Year, now.Month, 1);
                to   = from.Value.AddMonths(1).AddTicks(-1);
                FromDate = from.Value.ToString("yyyy-MM-dd");
                ToDate   = to.Value.Date.ToString("yyyy-MM-dd");
            }

            DisplayRange = $"{from!.Value:dd/MM/yyyy} – {to!.Value.Date:dd/MM/yyyy}";

            // ── Filter orders in range ────────────────────────────────────
            var orders = allOrders
                .Where(o =>
                {
                    var d = o.OrderDate ?? o.CreatedAt;
                    return d >= from && d <= to;
                })
                .ToList();

            TotalOrders  = orders.Count;
            var valid    = orders.Where(o => o.Status != "Hủy").ToList();
            TotalRevenue = valid.Sum(o => o.TotalAmount);
            TotalCost    = valid.Sum(o => o.TotalImportCost);
            TotalProfit  = valid.Sum(o => o.Profit);
            TotalDebt    = valid.Where(o => o.RemainingAmount > 0).Sum(o => o.RemainingAmount);

            // ── Source stats ──────────────────────────────────────────────
            SourceStats = orders
                .GroupBy(o => string.IsNullOrWhiteSpace(o.Source) ? "(Chưa xác định)" : o.Source)
                .Select(g =>
                {
                    var v = g.Where(o => o.Status != "Hủy").ToList();
                    return new DimensionStat
                    {
                        Name          = g.Key,
                        OrderCount    = g.Count(),
                        ActiveOrders  = g.Count(o => o.Status != "Đã giao" && o.Status != "Hủy"),
                        DeliveredCount= g.Count(o => o.Status == "Đã giao"),
                        CancelledCount= g.Count(o => o.Status == "Hủy"),
                        TotalRevenue  = v.Sum(o => o.TotalAmount),
                        TotalCost     = v.Sum(o => o.TotalImportCost),
                        TotalProfit   = v.Sum(o => o.Profit),
                        TotalPaid     = v.Sum(o => o.Deposit),
                        TotalDebt     = v.Where(o => o.RemainingAmount > 0).Sum(o => o.RemainingAmount),
                        TotalQty      = v.Sum(o => o.Quantity),
                    };
                })
                .OrderByDescending(s => s.TotalRevenue)
                .ToList();

            // ── Warehouse stats ───────────────────────────────────────────
            WarehouseStats = orders
                .GroupBy(o => string.IsNullOrWhiteSpace(o.Warehouse) ? "(Chưa xác định)" : o.Warehouse)
                .Select(g =>
                {
                    var v = g.Where(o => o.Status != "Hủy").ToList();
                    return new DimensionStat
                    {
                        Name          = g.Key,
                        OrderCount    = g.Count(),
                        ActiveOrders  = g.Count(o => o.Status != "Đã giao" && o.Status != "Hủy"),
                        DeliveredCount= g.Count(o => o.Status == "Đã giao"),
                        CancelledCount= g.Count(o => o.Status == "Hủy"),
                        TotalRevenue  = v.Sum(o => o.TotalAmount),
                        TotalCost     = v.Sum(o => o.TotalImportCost),
                        TotalProfit   = v.Sum(o => o.Profit),
                        TotalPaid     = v.Sum(o => o.Deposit),
                        TotalDebt     = v.Where(o => o.RemainingAmount > 0).Sum(o => o.RemainingAmount),
                        TotalQty      = v.Sum(o => o.Quantity),
                    };
                })
                .OrderByDescending(s => s.TotalRevenue)
                .ToList();

            // ── Chart: Source ─────────────────────────────────────────────
            SourceLabelsJson  = JsonSerializer.Serialize(SourceStats.Select(s => s.Name).ToList());
            SourceRevenueJson = JsonSerializer.Serialize(SourceStats.Select(s => s.TotalRevenue).ToList());
            SourceProfitJson  = JsonSerializer.Serialize(SourceStats.Select(s => s.TotalProfit).ToList());
            SourceOrdersJson  = JsonSerializer.Serialize(SourceStats.Select(s => s.OrderCount).ToList());

            // ── Chart: Warehouse ──────────────────────────────────────────
            WhLabelsJson  = JsonSerializer.Serialize(WarehouseStats.Select(s => s.Name).ToList());
            WhRevenueJson = JsonSerializer.Serialize(WarehouseStats.Select(s => s.TotalRevenue).ToList());
            WhProfitJson  = JsonSerializer.Serialize(WarehouseStats.Select(s => s.TotalProfit).ToList());
            WhOrdersJson  = JsonSerializer.Serialize(WarehouseStats.Select(s => s.OrderCount).ToList());

            // ── Monthly trend (last 6 months) — all orders, not filtered by date ──
            var trend6Start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-5);
            var trend6End   = DateTime.Now;

            var trendOrders = allOrders
                .Where(o => o.Status != "Hủy")
                .Where(o =>
                {
                    var d = o.OrderDate ?? o.CreatedAt;
                    return d >= trend6Start && d <= trend6End;
                })
                .ToList();

            var trendMonths = Enumerable.Range(0, 6)
                .Select(i => trend6Start.AddMonths(i))
                .ToList();

            var trendMonthLabels = trendMonths.Select(m => m.ToString("MM/yyyy")).ToList();
            TrendLabelsJson = JsonSerializer.Serialize(trendMonthLabels);

            var allSources = trendOrders
                .Select(o => string.IsNullOrWhiteSpace(o.Source) ? "(Chưa xác định)" : o.Source)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            // Palette for sources
            string[] palette = { "#00f2fe", "#10b981", "#a78bfa", "#f59e0b", "#ef4444", "#38bdf8", "#fb923c" };

            var datasets = allSources.Select((src, idx) => new
            {
                label = src,
                data = trendMonths.Select(m =>
                    trendOrders
                        .Where(o => (string.IsNullOrWhiteSpace(o.Source) ? "(Chưa xác định)" : o.Source) == src)
                        .Where(o =>
                        {
                            var d = o.OrderDate ?? o.CreatedAt;
                            return d.Year == m.Year && d.Month == m.Month;
                        })
                        .Sum(o => o.TotalAmount)
                ).ToList(),
                borderColor     = palette[idx % palette.Length],
                backgroundColor = palette[idx % palette.Length] + "20",
                tension = 0.35,
                fill = true,
            }).ToList();

            TrendDatasetsJson = JsonSerializer.Serialize(datasets);
        }
    }
}
