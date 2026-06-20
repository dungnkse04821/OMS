using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OMS.Models;
using OMS.Repositories;
using System.Text;

namespace OMS.Pages.Orders
{
    public class ExportModel : PageModel
    {
        private readonly IOrderRepository _orderRepository;

        public ExportModel(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IActionResult> OnGetAsync(string? statusFilter, string? searchQuery, string? dateFrom, string? dateTo)
        {
            var list = await _orderRepository.GetAllAsync();

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(statusFilter))
                list = list.Where(o => o.Status == statusFilter).ToList();

            // Apply search query
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var q = searchQuery.Trim().ToLower();
                list = list.Where(o =>
                    o.Id.ToLower().Contains(q) ||
                    o.CustomerName.ToLower().Contains(q) ||
                    o.PhoneNumber.Contains(q) ||
                    o.ProductName.ToLower().Contains(q)
                ).ToList();
            }

            // Apply date range filter
            if (DateTime.TryParse(dateFrom, out var from))
                list = list.Where(o => o.OrderDate >= from).ToList();
            if (DateTime.TryParse(dateTo, out var to))
                list = list.Where(o => o.OrderDate <= to.AddDays(1)).ToList();

            // Sort
            list = list.OrderByDescending(o => o.OrderDate ?? DateTime.MinValue).ToList();

            // Build CSV with UTF-8 BOM (for Excel compatibility with Vietnamese)
            var sb = new StringBuilder();
            sb.AppendLine("Mã ĐH,Ngày đặt,Khách hàng,Số điện thoại,Địa chỉ giao hàng,Mã SP,Tên sản phẩm,Màu,Size,SL,Giá bán,Tổng tiền,Đã cọc,Chiết khấu,Còn lại,Giá nhập,Tổng vốn,Lợi nhuận,Ngày về,Ngày TT,Trạng thái");

            foreach (var o in list)
            {
                sb.AppendLine(string.Join(",", new[]
                {
                    Escape(o.Id),
                    o.OrderDate?.ToString("dd/MM/yyyy") ?? "",
                    Escape(o.CustomerName),
                    Escape(o.PhoneNumber),
                    Escape(o.ShippingAddress),
                    Escape(o.Code),
                    Escape(o.ProductName),
                    Escape(o.Color ?? ""),
                    Escape(o.Size ?? ""),
                    o.Quantity.ToString(),
                    o.SellingPrice.ToString("N0"),
                    o.TotalAmount.ToString("N0"),
                    o.Deposit.ToString("N0"),
                    o.Discount.ToString("N0"),
                    o.RemainingAmount.ToString("N0"),
                    o.ImportPrice.ToString("N0"),
                    o.TotalImportCost.ToString("N0"),
                    o.Profit.ToString("N0"),
                    o.ArrivalDate?.ToString("dd/MM/yyyy") ?? "",
                    o.PaymentDate?.ToString("dd/MM/yyyy") ?? "",
                    Escape(o.Status)
                }));
            }

            // UTF-8 BOM bytes + content
            var bom = Encoding.UTF8.GetPreamble();
            var content = Encoding.UTF8.GetBytes(sb.ToString());
            var fileBytes = bom.Concat(content).ToArray();

            var fileName = $"DonHang_{DateTime.Now:yyyyMMdd_HHmm}.csv";
            return File(fileBytes, "text/csv", fileName);
        }

        // Escape CSV field: wrap in quotes if contains comma, quote, or newline
        private static string Escape(string? value)
        {
            if (value == null) return "";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }
}
