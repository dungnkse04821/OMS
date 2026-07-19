using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models;
using OMS.Repositories;

namespace OMS.Pages.Customers
{
    public class DetailModel : PageModel
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ApplicationDbContext _ctx;

        public DetailModel(ICustomerRepository customerRepository, ApplicationDbContext ctx)
        {
            _customerRepository = customerRepository;
            _ctx = ctx;
        }

        public Customer Customer { get; set; } = null!;

        // ── Order list ───────────────────────────────────────────────────
        public List<Order> Orders { get; set; } = new();

        // ── Financial KPIs ────────────────────────────────────────────────
        public decimal TotalRevenue     { get; set; }   // Tổng tiền mua (TotalAmount)
        public decimal TotalPaid        { get; set; }   // Tổng đã trả   (Deposit)
        public decimal TotalDiscount    { get; set; }   // Tổng chiết khấu
        public decimal TotalDebt        { get; set; }   // Còn nợ        (RemainingAmount > 0)
        public int     TotalOrders      { get; set; }
        public int     ActiveOrders     { get; set; }   // Chưa giao
        public int     DeliveredOrders  { get; set; }
        public int     CancelledOrders  { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null) return NotFound();

            Customer = customer;

            // Load orders matched by PhoneNumber (most reliable link)
            Orders = await _ctx.Orders
                .Where(o => o.PhoneNumber == customer.PhoneNumber ||
                             o.CustomerName == customer.FullName)
                .OrderByDescending(o => o.OrderDate ?? o.CreatedAt)
                .ToListAsync();

            // Compute KPIs
            TotalOrders     = Orders.Count;
            DeliveredOrders = Orders.Count(o => o.Status == "Đã giao");
            CancelledOrders = Orders.Count(o => o.Status == "Hủy");
            ActiveOrders    = Orders.Count(o => o.Status != "Đã giao" && o.Status != "Hủy");

            // Exclude cancelled orders from financials
            var financialOrders = Orders.Where(o => o.Status != "Hủy").ToList();
            TotalRevenue  = financialOrders.Sum(o => o.TotalAmount);
            TotalPaid     = financialOrders.Sum(o => o.Deposit);
            TotalDiscount = financialOrders.Sum(o => o.Discount);
            TotalDebt     = financialOrders.Where(o => o.RemainingAmount > 0).Sum(o => o.RemainingAmount);

            return Page();
        }
    }
}
