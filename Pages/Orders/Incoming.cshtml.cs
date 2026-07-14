using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models;

namespace OMS.Pages.Orders
{
    public class IncomingModel : PageModel
    {
        private readonly ApplicationDbContext _ctx;
        private readonly IConfiguration _config;

        public IncomingModel(ApplicationDbContext ctx, IConfiguration config)
        {
            _ctx = ctx;
            _config = config;
        }

        // ── Filter properties ────────────────────────────────────────────
        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<string> StatusFilters { get; set; } = new() { "Chờ đặt", "Đã đặt", "Đang về", "Đã về" };

        [BindProperty(SupportsGet = true)]
        public string? SourceFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? WarehouseFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        // ── Data ─────────────────────────────────────────────────────────
        public List<Order> Orders { get; set; } = new();
        public List<string> AllSources { get; set; } = new();
        public List<string> AllWarehouses { get; set; } = new();

        // ──  config ────────────────────────────────────────────────
        public string BankId      => _config["BankConfig:BankId"]      ?? "MB";
        public string AccountNo   => _config["BankConfig:AccountNo"]   ?? "";
        public string AccountName => _config["BankConfig:AccountName"] ?? "";

        // ── Success / error message ──────────────────────────────────────
        public string? SuccessMsg { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
            SuccessMsg = TempData["SuccessMsg"]?.ToString();
        }

        private async Task LoadDataAsync()
        {
            var query = _ctx.Orders.AsQueryable();

            // Status filter
            if (StatusFilters.Any())
                query = query.Where(o => StatusFilters.Contains(o.Status));

            // Source filter
            if (!string.IsNullOrWhiteSpace(SourceFilter))
                query = query.Where(o => o.Source == SourceFilter);

            // Warehouse filter
            if (!string.IsNullOrWhiteSpace(WarehouseFilter))
                query = query.Where(o => o.Warehouse == WarehouseFilter);

            // Date range filter — on OrderDate
            if (FromDate.HasValue)
                query = query.Where(o => o.OrderDate >= FromDate.Value);
            if (ToDate.HasValue)
                query = query.Where(o => o.OrderDate <= ToDate.Value);

            // Text search
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var q = SearchQuery.Trim().ToLower();
                query = query.Where(o =>
                    (o.CustomerName != null && o.CustomerName.ToLower().Contains(q)) ||
                    (o.PhoneNumber  != null && o.PhoneNumber.Contains(q)) ||
                    (o.Id           != null && o.Id.ToLower().Contains(q)) ||
                    (o.Code         != null && o.Code.ToLower().Contains(q)));
            }

            Orders = await query
                .OrderByDescending(o => o.OrderDate ?? o.CreatedAt)
                .ToListAsync();

            // Distinct source/warehouse lists for filter dropdowns
            AllSources    = await _ctx.Orders.Select(o => o.Source).Distinct().OrderBy(x => x).ToListAsync();
            AllWarehouses = await _ctx.Orders.Select(o => o.Warehouse).Distinct().OrderBy(x => x).ToListAsync();
        }

        // ── Bulk Action Handler ──────────────────────────────────────────
        // Called by form POST. Handles:
        //   action = "set-arriving"  → Status = "Đang về"
        //   action = "set-arrived"   → Status = "Đã về" + ArrivalDate + ImportPrice per order
        //   action = "set-delivered" → Status = "Đã giao" + CollectedAmount per order + PaymentDate
        public async Task<IActionResult> OnPostBulkActionAsync(
            [FromForm] string   bulkAction,
            [FromForm] string[] selectedIds,
            [FromForm] Dictionary<string, string> arrivalDates,
            [FromForm] Dictionary<string, decimal> importPrices,
            [FromForm] Dictionary<string, decimal> collectedAmounts)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["SuccessMsg"] = "⚠️ Chưa chọn đơn hàng nào.";
                return RedirectToPage();
            }

            var orders = await _ctx.Orders
                .Where(o => selectedIds.Contains(o.Id))
                .ToListAsync();

            int updated = 0;
            var now = DateTime.UtcNow;

            foreach (var order in orders)
            {
                bool changed = false;

                switch (bulkAction)
                {
                    case "set-arriving":
                        order.Status = "Đang về";
                        changed = true;
                        break;

                    case "set-arrived":
                        order.Status = "Đã về";
                        // Per-order ArrivalDate
                        if (arrivalDates.TryGetValue(order.Id, out var dateStr) &&
                            DateTime.TryParse(dateStr, out var arrDate))
                            order.ArrivalDate = arrDate;
                        else if (!order.ArrivalDate.HasValue)
                            order.ArrivalDate = now;
                        // Per-order ImportPrice
                        if (importPrices.TryGetValue(order.Id, out var imp) && imp > 0)
                        {
                            order.ImportPrice     = imp;
                            order.TotalImportCost = imp * order.Quantity;
                            order.Profit          = order.TotalAmount - order.TotalImportCost;
                        }
                        changed = true;
                        break;

                    case "set-delivered":
                        order.Status = "Đã giao";
                        // Per-order collected amount → update Deposit & recalc Remaining
                        if (collectedAmounts.TryGetValue(order.Id, out var collected) && collected > 0)
                        {
                            order.Deposit         = order.Deposit + collected;
                            order.RemainingAmount = order.TotalAmount - order.Deposit - order.Discount;
                        }
                        order.PaymentDate = now;
                        changed = true;
                        break;
                }

                if (changed)
                {
                    order.UpdatedAt = now;
                    updated++;
                }
            }

            await _ctx.SaveChangesAsync();

            var actionLabel = bulkAction switch
            {
                "set-arriving"  => "Đang về",
                "set-arrived"   => "Đã về",
                "set-delivered" => "Đã giao",
                _               => bulkAction
            };
            TempData["SuccessMsg"] = $"✅ Đã cập nhật {updated} đơn hàng → \"{actionLabel}\".";
            return RedirectToPage(new { StatusFilters, SourceFilter, WarehouseFilter, FromDate, ToDate, SearchQuery });
        }
    }
}
