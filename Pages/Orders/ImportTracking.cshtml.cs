using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models;

namespace OMS.Pages.Orders
{
    public class ImportTrackingModel : PageModel
    {
        private readonly ApplicationDbContext _ctx;
        public ImportTrackingModel(ApplicationDbContext ctx) => _ctx = ctx;

        public List<Carrier> Carriers { get; set; } = new();

        // Results after import
        public int UpdatedCount { get; set; }
        public int NotFoundCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<ImportRow> PreviewRows { get; set; } = new();
        public bool ImportDone { get; set; }

        public async Task OnGetAsync()
        {
            Carriers = await _ctx.Carriers.OrderBy(c => c.SortOrder).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile? csvFile)
        {
            Carriers = await _ctx.Carriers.OrderBy(c => c.SortOrder).ToListAsync();

            if (csvFile == null || csvFile.Length == 0)
            {
                Errors.Add("Vui lòng chọn file CSV.");
                return Page();
            }

            if (!csvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                Errors.Add("Chỉ chấp nhận file .csv");
                return Page();
            }

            // Parse CSV
            var rows = new List<ImportRow>();
            using var reader = new System.IO.StreamReader(csvFile.OpenReadStream());
            var lineNum = 0;
            while (!reader.EndOfStream)
            {
                var line = (await reader.ReadLineAsync())?.Trim();
                lineNum++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Skip header row
                if (lineNum == 1 && line.StartsWith("OrderId", StringComparison.OrdinalIgnoreCase))
                    continue;

                var parts = line.Split(',');
                if (parts.Length < 2)
                {
                    Errors.Add($"Dòng {lineNum}: Thiếu cột (cần ít nhất OrderId, TrackingCode).");
                    continue;
                }

                rows.Add(new ImportRow
                {
                    OrderId = parts[0].Trim().Trim('"'),
                    TrackingCode = parts[1].Trim().Trim('"'),
                    ShippingCarrier = parts.Length >= 3 ? parts[2].Trim().Trim('"') : null,
                    LineNumber = lineNum
                });
            }

            if (rows.Count == 0)
            {
                Errors.Add("File CSV không có dòng dữ liệu nào hợp lệ.");
                return Page();
            }

            // Match and update orders
            foreach (var row in rows)
            {
                var order = await _ctx.Orders.FirstOrDefaultAsync(o => o.Id == row.OrderId);
                if (order == null)
                {
                    row.Status = "❌ Không tìm thấy";
                    NotFoundCount++;
                }
                else
                {
                    order.TrackingCode = row.TrackingCode;
                    if (!string.IsNullOrWhiteSpace(row.ShippingCarrier))
                        order.ShippingCarrier = row.ShippingCarrier;
                    row.Status = "✅ Cập nhật";
                    UpdatedCount++;
                }
            }

            await _ctx.SaveChangesAsync();
            PreviewRows = rows;
            ImportDone = true;
            return Page();
        }

        public class ImportRow
        {
            public string OrderId { get; set; } = "";
            public string TrackingCode { get; set; } = "";
            public string? ShippingCarrier { get; set; }
            public int LineNumber { get; set; }
            public string Status { get; set; } = "";
        }
    }
}
