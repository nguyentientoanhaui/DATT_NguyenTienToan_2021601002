using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Models.ViewModels;
using Shopping_Demo.Repository;
using Shopping_Demo.Services;
using System.Security.Claims;

namespace Shopping_Demo.Controllers
{
    public class InvoiceViewController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly InvoiceExportService _invoiceExportService;

        public InvoiceViewController(DataContext dataContext, InvoiceExportService invoiceExportService)
        {
            _dataContext = dataContext;
            _invoiceExportService = invoiceExportService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, string search = "")
        {
            try
            {
                // Kiểm tra authentication
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    TempData["error"] = "Bạn cần đăng nhập để xem đơn hàng";
                    return RedirectToAction("Login", "Account");
                }

                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var isAdmin = User.IsInRole("Admin");

                IQueryable<OrderModel> ordersQuery;

                if (isAdmin)
                {
                    // Admin xem tất cả đơn hàng
                    ordersQuery = _dataContext.Orders
                        .Include(o => o.OrderDetails)
                            .ThenInclude(od => od.Product)
                        .OrderByDescending(o => o.CreatedDate);
                }
                else
                {
                    // User chỉ xem đơn hàng của mình
                    ordersQuery = _dataContext.Orders
                        .Include(o => o.OrderDetails)
                            .ThenInclude(od => od.Product)
                        .Where(o => o.UserName == userEmail)
                        .OrderByDescending(o => o.CreatedDate);
                }

                // Tìm kiếm theo mã đơn hàng
                if (!string.IsNullOrEmpty(search))
                {
                    ordersQuery = ordersQuery.Where(o => o.OrderCode.Contains(search));
                }

                // Phân trang
                var pageSize = 10;
                var totalOrders = await ordersQuery.CountAsync();
                var orders = await ordersQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Tính toán thông tin cho mỗi đơn hàng
                var ordersWithInfo = orders.Select(order =>
                {
                    var subtotal = order.OrderDetails?.Sum(od => od.Price * od.Quantity) ?? 0;
                    return new OrderListViewModel
                    {
                        Order = order,
                        Subtotal = subtotal,
                        TotalAmount = subtotal - order.DiscountAmount + order.ShippingCost,
                        ProductCount = order.OrderDetails?.Count ?? 0
                    };
                }).ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
                ViewBag.TotalOrders = totalOrders;
                ViewBag.IsAdmin = isAdmin;
                ViewBag.Search = search;

                return View(ordersWithInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Index: {ex.Message}");
                TempData["error"] = $"Lỗi khi tải danh sách đơn hàng: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> View(string orderCode)
        {
            try
            {
                if (string.IsNullOrEmpty(orderCode))
                {
                    TempData["error"] = "Vui lòng nhập mã đơn hàng";
                    return RedirectToAction("Index");
                }

                // Lấy thông tin đơn hàng
                var order = await _dataContext.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.Brand)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.Category)
                    .Include(o => o.MomoInfos)
                    .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

                if (order == null)
                {
                    TempData["error"] = "Không tìm thấy đơn hàng với mã: " + orderCode;
                    return RedirectToAction("Index");
                }

                // Tính toán tổng tiền
                decimal subtotal = 0;
                if (order.OrderDetails != null && order.OrderDetails.Any())
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        subtotal += detail.Price * detail.Quantity;
                    }
                }

                var invoiceData = new InvoiceViewModel
                {
                    Order = order,
                    Subtotal = subtotal,
                    DiscountAmount = order.DiscountAmount,
                    ShippingCost = order.ShippingCost,
                    TotalAmount = subtotal - order.DiscountAmount + order.ShippingCost,
                    PaymentInfo = order.MomoInfos?.FirstOrDefault()
                };

                return View("Invoice", invoiceData);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Có lỗi xảy ra khi tải hóa đơn: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Search(string orderCode)
        {
            try
            {
                if (string.IsNullOrEmpty(orderCode))
                {
                    TempData["error"] = "Vui lòng nhập mã đơn hàng";
                    return RedirectToAction("Index");
                }

                // Lấy thông tin đơn hàng
                var order = await _dataContext.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.Brand)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.Category)
                    // .Include(o => o.Coupon) // Removed - Coupon table not exists
                    .Include(o => o.MomoInfos)
                    .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

                if (order == null)
                {
                    TempData["error"] = "Không tìm thấy đơn hàng với mã: " + orderCode;
                    return RedirectToAction("Index");
                }

                // Tính toán tổng tiền
                decimal subtotal = 0;
                if (order.OrderDetails != null && order.OrderDetails.Any())
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        subtotal += detail.Price * detail.Quantity;
                    }
                }

                var invoiceData = new InvoiceViewModel
                {
                    Order = order,
                    Subtotal = subtotal,
                    DiscountAmount = order.DiscountAmount,
                    ShippingCost = order.ShippingCost,
                    TotalAmount = subtotal - order.DiscountAmount + order.ShippingCost,
                    PaymentInfo = order.MomoInfos?.FirstOrDefault()
                };

                return View("Invoice", invoiceData);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Có lỗi xảy ra khi tải hóa đơn: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Download(string orderCode, string format = "pdf")
        {
            try
            {
                Console.WriteLine($"DEBUG - InvoiceViewController.Download called with orderCode: {orderCode}, format: {format}");
                
                if (string.IsNullOrEmpty(orderCode))
                {
                    TempData["error"] = "Mã đơn hàng không hợp lệ";
                    return RedirectToAction("Index");
                }

                // Kiểm tra authentication
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    TempData["error"] = "Bạn cần đăng nhập để tải hóa đơn";
                    return RedirectToAction("Login", "Account");
                }

                var order = await _dataContext.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

                if (order == null)
                {
                    Console.WriteLine($"DEBUG - Order not found: {orderCode}");
                    TempData["error"] = "Không tìm thấy đơn hàng";
                    return RedirectToAction("Index");
                }

                Console.WriteLine($"DEBUG - Order found: {order.OrderCode}, OrderDetails count: {order.OrderDetails?.Count ?? 0}");

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

                Console.WriteLine($"DEBUG - InvoiceData created, TotalAmount: {invoiceData.TotalAmount}");

                byte[] fileData;
                string fileName;
                string contentType;

                switch (format.ToLower())
                {
                    case "pdf":
                        Console.WriteLine("DEBUG - Exporting to PDF");
                        fileData = _invoiceExportService.ExportToPdf(invoiceData);
                        fileName = $"HoaDon_{orderCode}.pdf";
                        contentType = "application/pdf";
                        Console.WriteLine($"DEBUG - PDF created, size: {fileData?.Length ?? 0} bytes");
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

                if (fileData == null || fileData.Length == 0)
                {
                    Console.WriteLine("DEBUG - File data is null or empty");
                    TempData["error"] = "Không thể tạo file";
                    return RedirectToAction("Index");
                }

                Console.WriteLine($"DEBUG - Returning file: {fileName}, size: {fileData.Length} bytes");
                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG - Exception in Download: {ex.Message}");
                Console.WriteLine($"DEBUG - Stack trace: {ex.StackTrace}");
                TempData["error"] = $"Có lỗi xảy ra khi tải xuống: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

    }
}
