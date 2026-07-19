using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models;
using OMS.Repositories;

namespace OMS.Pages.Customers
{
    /// <summary>Per-customer financial summary row.</summary>
    public class CustomerFinancial
    {
        public Customer Customer           { get; init; } = null!;
        public int     TotalOrders        { get; init; }
        public int     ActiveOrders       { get; init; }   // not Hủy, not Đã giao
        public decimal TotalRevenue       { get; init; }   // sum TotalAmount (excl Hủy)
        public decimal TotalPaid          { get; init; }   // sum Deposit (excl Hủy)
        public decimal TotalDiscount      { get; init; }
        public decimal TotalDebt          { get; init; }   // sum RemainingAmount > 0
        public DateTime? LastOrderDate    { get; init; }
    }

    public class IndexModel : PageModel
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ApplicationDbContext _ctx;

        public IndexModel(ICustomerRepository customerRepository, ApplicationDbContext ctx)
        {
            _customerRepository = customerRepository;
            _ctx = ctx;
        }

        // ── Filter ────────────────────────────────────────────────────────
        public List<Customer> Customers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "debt";   // debt | revenue | orders | name

        // ── Financial dashboard data ──────────────────────────────────────
        public List<CustomerFinancial> Financials     { get; set; } = new();
        public bool ShowDashboard                     { get; set; } = true;

        // ── Global KPIs ───────────────────────────────────────────────────
        public decimal GlobalRevenue   { get; set; }
        public decimal GlobalPaid      { get; set; }
        public decimal GlobalDebt      { get; set; }
        public int     CustomersInDebt { get; set; }
        public int     TotalOrderCount { get; set; }

        public async Task OnGetAsync()
        {
            var allCustomers = await _customerRepository.GetAllAsync();

            // ── Filter for CRUD table ──────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var q = SearchQuery.Trim().ToLower();
                Customers = allCustomers.Where(c =>
                    c.FullName.ToLower().Contains(q) ||
                    c.PhoneNumber.Contains(q) ||
                    (c.Reference != null && c.Reference.ToLower().Contains(q))
                ).ToList();
            }
            else
            {
                Customers = allCustomers;
            }

            // ── Load all non-deleted orders once ──────────────────────────
            var allOrders = await _ctx.Orders
                .Where(o => !o.IsDeleted)
                .ToListAsync();

            TotalOrderCount = allOrders.Count;

            // ── Build per-customer financial ───────────────────────────────
            var financials = new List<CustomerFinancial>();

            foreach (var c in allCustomers)
            {
                var orders = allOrders
                    .Where(o => o.PhoneNumber == c.PhoneNumber ||
                                o.CustomerName == c.FullName)
                    .ToList();

                if (!orders.Any()) continue;   // skip customers with no orders

                var valid = orders.Where(o => o.Status != "Hủy").ToList();

                financials.Add(new CustomerFinancial
                {
                    Customer     = c,
                    TotalOrders  = orders.Count,
                    ActiveOrders = orders.Count(o => o.Status != "Đã giao" && o.Status != "Hủy"),
                    TotalRevenue = valid.Sum(o => o.TotalAmount),
                    TotalPaid    = valid.Sum(o => o.Deposit),
                    TotalDiscount= valid.Sum(o => o.Discount),
                    TotalDebt    = valid.Where(o => o.RemainingAmount > 0).Sum(o => o.RemainingAmount),
                    LastOrderDate= orders.Max(o => o.OrderDate ?? o.CreatedAt),
                });
            }

            // ── Sort ──────────────────────────────────────────────────────
            Financials = SortBy switch
            {
                "revenue" => financials.OrderByDescending(f => f.TotalRevenue).ToList(),
                "orders"  => financials.OrderByDescending(f => f.TotalOrders).ToList(),
                "name"    => financials.OrderBy(f => f.Customer.FullName).ToList(),
                "paid"    => financials.OrderByDescending(f => f.TotalPaid).ToList(),
                _         => financials.OrderByDescending(f => f.TotalDebt).ToList(), // "debt" default
            };

            // ── Global KPIs ───────────────────────────────────────────────
            GlobalRevenue   = financials.Sum(f => f.TotalRevenue);
            GlobalPaid      = financials.Sum(f => f.TotalPaid);
            GlobalDebt      = financials.Sum(f => f.TotalDebt);
            CustomersInDebt = financials.Count(f => f.TotalDebt > 0);
        }
    }
}
