using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;
using Shopping_Demo.Services;

namespace Shopping_Demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager,Staff,Sale,Shipper")]
    public class InvoiceController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly InvoiceExportService _invoiceExportService;
        private readonly EmailService _emailService;

        public InvoiceController(DataContext dataContext, InvoiceExportService invoiceExportService, EmailService emailService)
        {
            _dataContext = dataContext;
            _invoiceExportService = invoiceExportService;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode))
            {
                TempData["error"] = "Mã đơn hàng không hợp lệ";
                return RedirectToAction("Index", "Order");
            }

            var order = await _dataContext.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

            if (order == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Index", "Order");
            }

            // Tính toán subtotal
            var subtotal = order.OrderDetails?.Sum(od => od.Price * od.Quantity) ?? 0;

            var invoiceData = new InvoiceViewModel
            {
                Order = order,
                Subtotal = subtotal,
                DiscountAmount = order.DiscountAmount,
                ShippingCost = order.ShippingCost,
                TotalAmount = subtotal - order.DiscountAmount + order.ShippingCost,
                PaymentInfo = await _dataContext.MomoInfos.FirstOrDefaultAsync(m => m.OrderCode == orderCode)
            };

            return View(invoiceData);
        }

        [HttpGet]
        public async Task<IActionResult> Download(string orderCode, string format = "pdf")
        {
            var order = await _dataContext.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

            if (order == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Index", "Order");
            }

            // Tính toán subtotal
            var subtotal = order.OrderDetails?.Sum(od => od.Price * od.Quantity) ?? 0;

            var invoiceData = new InvoiceViewModel
            {
                Order = order,
                Subtotal = subtotal,
                DiscountAmount = order.DiscountAmount,
                ShippingCost = order.ShippingCost,
                TotalAmount = subtotal - order.DiscountAmount + order.ShippingCost,
                PaymentInfo = await _dataContext.MomoInfos.FirstOrDefaultAsync(m => m.OrderCode == orderCode)
            };

            byte[] fileData;
            string fileName;
            string contentType;

            switch (format.ToLower())
            {
                case "pdf":
                    fileData = _invoiceExportService.ExportToPdf(invoiceData);
                    fileName = $"HoaDon_{orderCode}.pdf";
                    contentType = "application/pdf";
                    break;
                case "excel":
                    fileData = _invoiceExportService.ExportToExcel(invoiceData);
                    fileName = $"HoaDon_{orderCode}.xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case "csv":
                    fileData = _invoiceExportService.ExportToCsv(invoiceData);
                    fileName = $"HoaDon_{orderCode}.csv";
                    contentType = "text/csv";
                    break;
                default:
                    fileData = _invoiceExportService.ExportToPdf(invoiceData);
                    fileName = $"HoaDon_{orderCode}.pdf";
                    contentType = "application/pdf";
                    break;
            }

            return File(fileData, contentType, fileName);
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(string orderCode, string email, string format = "pdf")
        {
            var order = await _dataContext.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            // Tính toán subtotal
            var subtotal = order.OrderDetails?.Sum(od => od.Price * od.Quantity) ?? 0;

            var invoiceData = new InvoiceViewModel
            {
                Order = order,
                Subtotal = subtotal,
                DiscountAmount = order.DiscountAmount,
                ShippingCost = order.ShippingCost,
                TotalAmount = subtotal - order.DiscountAmount + order.ShippingCost,
                PaymentInfo = await _dataContext.MomoInfos.FirstOrDefaultAsync(m => m.OrderCode == orderCode)
            };

            var success = await _emailService.SendInvoiceEmailAsync(invoiceData, email, format);
            
            if (success)
            {
                return Json(new { success = true, message = "Gửi email thành công!" });
            }
            else
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi gửi email" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> BulkExport(int page = 1, int pageSize = 10, string status = "", string dateFrom = "", string dateTo = "")
        {
            var query = _dataContext.Orders.AsQueryable();

            // Filter by status
            if (!string.IsNullOrEmpty(status) && int.TryParse(status, out int statusInt))
            {
                query = query.Where(o => o.Status == statusInt);
            }

            // Filter by date range
            if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out DateTime fromDate))
            {
                query = query.Where(o => o.CreatedDate >= fromDate);
            }
            if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out DateTime toDate))
            {
                query = query.Where(o => o.CreatedDate <= toDate.AddDays(1));
            }

            var totalCount = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    o.OrderCode,
                    o.CreatedDate,
                    o.Status,
                    o.UserName,
                    TotalAmount = o.OrderDetails != null ? o.OrderDetails.Sum(od => od.Price * od.Quantity) - o.DiscountAmount + o.ShippingCost : 0,
                })
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.Status = status;
            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> ExportMultiple(string[] orderCodes, string format = "excel")
        {
            if (orderCodes == null || orderCodes.Length == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn ít nhất một đơn hàng" });
            }

            var orders = await _dataContext.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => orderCodes.Contains(o.OrderCode))
                .ToListAsync();

            if (!orders.Any())
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng nào" });
            }

            var invoiceDataList = new List<InvoiceViewModel>();
            foreach (var order in orders)
            {
                var subtotal = order.OrderDetails?.Sum(od => od.Price * od.Quantity) ?? 0;
                invoiceDataList.Add(new InvoiceViewModel
                {
                    Order = order,
                    Subtotal = subtotal,
                    DiscountAmount = order.DiscountAmount,
                    ShippingCost = order.ShippingCost,
                    TotalAmount = subtotal - order.DiscountAmount + order.ShippingCost,
                    PaymentInfo = await _dataContext.MomoInfos.FirstOrDefaultAsync(m => m.OrderCode == order.OrderCode)
                });
            }

            byte[] fileData;
            string fileName;
            string contentType;

            if (format.ToLower() == "excel")
            {
                fileData = _invoiceExportService.ExportMultipleToExcel(invoiceDataList);
                fileName = $"HoaDon_HangLoat_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }
            else
            {
                return Json(new { success = false, message = "Chỉ hỗ trợ định dạng Excel cho xuất hàng loạt" });
            }

            return File(fileData, contentType, fileName);
        }
    }
}
