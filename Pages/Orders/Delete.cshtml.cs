using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Models;
using OMS.Repositories;

namespace OMS.Pages.Orders
{
    public class DeleteModel : PageModel
    {
        private readonly IOrderRepository _orderRepository;

        public DeleteModel(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [BindProperty]
        public Order Order { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToPage("./Index");
            }

            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return RedirectToPage("./Index");
            }

            Order = order;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Order.Id))
            {
                return RedirectToPage("./Index");
            }

            await _orderRepository.DeleteAsync(Order.Id);
            return RedirectToPage("./Index");
        }
    }
}
