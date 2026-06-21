using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Models;
using OMS.Repositories;

namespace OMS.Pages.Debts
{
    public class IndexModel : PageModel
    {
        private readonly IOrderRepository _orderRepository;

        public IndexModel(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public List<Order> DebtOrders { get; set; } = new();

        // KPI
        public decimal TotalDebtAmount { get; set; }
        public int TotalDebtCount { get; set; }
        public decimal TotalOverdueAmount { get; set; }    // Nợ quá 30 ngày
        public int TotalOverdueCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; } = "amount_desc";

        public async Task OnGetAsync()
        {
            var all = await _orderRepository.GetAllAsync();

            // Filter: orders with remaining amount > 0 and not cancelled
            var debts = all
                .Where(o => o.RemainingAmount > 0 && o.Status != "Hủy")
                .ToList();

            // KPI calculations
            TotalDebtCount = debts.Count;
            TotalDebtAmount = debts.Sum(o => o.RemainingAmount);

            var overdueThreshold = DateTime.Now.AddDays(-30);
            var overdue = debts.Where(o => o.OrderDate.HasValue && o.OrderDate.Value < overdueThreshold).ToList();
            TotalOverdueAmount = overdue.Sum(o => o.RemainingAmount);
            TotalOverdueCount = overdue.Count;

            // Search filter
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var q = SearchQuery.Trim().ToLower();
                debts = debts.Where(o =>
                    o.Id.ToLower().Contains(q) ||
                    o.CustomerName.ToLower().Contains(q) ||
                    o.PhoneNumber.Contains(q) ||
                    o.ProductName.ToLower().Contains(q)
                ).ToList();
            }

            // Sorting
            DebtOrders = SortBy switch
            {
                "amount_asc"  => debts.OrderBy(o => o.RemainingAmount).ToList(),
                "amount_desc" => debts.OrderByDescending(o => o.RemainingAmount).ToList(),
                "date_asc"    => debts.OrderBy(o => o.OrderDate ?? DateTime.MaxValue).ToList(),
                "date_desc"   => debts.OrderByDescending(o => o.OrderDate ?? DateTime.MinValue).ToList(),
                "name_asc"    => debts.OrderBy(o => o.CustomerName).ToList(),
                _             => debts.OrderByDescending(o => o.RemainingAmount).ToList()
            };
        }

        /// <summary>
        /// Marks an order as fully paid: sets Deposit = TotalAmount, RemainingAmount = 0, PaymentDate = today.
        /// </summary>
        public async Task<IActionResult> OnPostMarkPaidAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                return RedirectToPage();

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order != null)
            {
                order.Deposit = order.TotalAmount;
                order.RemainingAmount = 0;
                order.PaymentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                await _orderRepository.UpdateAsync(order);
            }

            return RedirectToPage(new { SearchQuery, SortBy });
        }

        /// <summary>
        /// Updates only the deposit amount for partial payment.
        /// </summary>
        public async Task<IActionResult> OnPostPartialPayAsync(string orderId, decimal paidAmount)
        {
            if (string.IsNullOrWhiteSpace(orderId) || paidAmount <= 0)
                return RedirectToPage();

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order != null)
            {
                order.Deposit = Math.Min(order.TotalAmount, order.Deposit + paidAmount);
                order.RemainingAmount = Math.Max(0, order.TotalAmount - order.Discount - order.Deposit);
                if (order.RemainingAmount == 0)
                    order.PaymentDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                await _orderRepository.UpdateAsync(order);
            }

            return RedirectToPage(new { SearchQuery, SortBy });
        }
    }
}
