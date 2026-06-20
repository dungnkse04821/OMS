using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Models;
using OMS.Repositories;

namespace OMS.Pages.Orders
{
    public class PrintModel : PageModel
    {
        private readonly IOrderRepository _orderRepository;

        public PrintModel(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public Order? Order { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var all = await _orderRepository.GetAllAsync();
            Order = all.FirstOrDefault(o => o.Id == id);

            if (Order == null)
                return NotFound();

            return Page();
        }
    }
}
