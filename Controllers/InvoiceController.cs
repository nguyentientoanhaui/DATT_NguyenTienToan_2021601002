using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;
using Shopping_Demo.Services;
using System.Security.Claims;

namespace Shopping_Demo.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly InvoiceExportService _invoiceExportService;

        public InvoiceController(DataContext dataContext, InvoiceExportService invoiceExportService)
        {
            _dataContext = dataContext;
            _invoiceExportService = invoiceExportService;
        }

        [HttpGet]
        public async Task<IActionResult> Simple(string orderCode)
        {
            try
            {
                if (string.IsNullOrEmpty(orderCode))
                {
                    return Content("OrderCode is required");
                }

                var order = await _dataContext.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

                if (order == null)
                {
                    return Content($"Order not found: {orderCode}");
                }

                return Content($"Order found: {order.OrderCode}, User: {order.UserName}, Items: {order.OrderDetails?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult Test()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Debug(string orderCode = "")
        {
            var debugInfo = new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                UserEmail = User.FindFirstValue(ClaimTypes.Email),
                IsAdmin = User.IsInRole("Admin"),
                OrderCode = orderCode,
                Orders = await _dataContext.Orders
                    .Where(o => string.IsNullOrEmpty(orderCode) || o.OrderCode == orderCode)
                    .Select(o => new { o.OrderCode, o.UserName, o.CreatedDate })
                    .Take(10)
                    .ToListAsync()
            };
            
            return Json(debugInfo);
        }

        [HttpGet]
        public async Task<IActionResult> Public(string orderCode)
        {
            try
            {
                Console.WriteLine($"DEBUG - InvoiceController.Public called with orderCode: {orderCode}");
                
                // Kiểm tra orderCode
                if (string.IsNullOrEmpty(orderCode))
                {
                    Console.WriteLine("DEBUG - OrderCode is null or empty");
                    TempData["error"] = "Mã đơn hàng không hợp lệ";
                    return RedirectToAction("Index", "Home");
                }

                // Lấy thông tin đơn hàng với chi tiết sản phẩm
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

                Console.WriteLine($"DEBUG - Order found: {order != null}");

                // Kiểm tra đơn hàng có tồn tại không
                if (order == null)
                {
                    Console.WriteLine("DEBUG - Order not found in database");
                    TempData["error"] = "Không tìm thấy đơn hàng";
                    return RedirectToAction("Index", "Home");
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

                Console.WriteLine("DEBUG - Returning invoice view");
                return View("Index", invoiceData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG - Exception in Public: {ex.Message}");
                TempData["error"] = "Có lỗi xảy ra khi tải hóa đơn";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index(string orderCode)
        {
            try
            {
                // Debug logging
                Console.WriteLine($"DEBUG - InvoiceController.Index called with orderCode: {orderCode}");
                
                // Kiểm tra orderCode
                if (string.IsNullOrEmpty(orderCode))
                {
                    Console.WriteLine("DEBUG - OrderCode is null or empty");
                    TempData["error"] = "Mã đơn hàng không hợp lệ";
                    return RedirectToAction("Index", "Home");
                }

                // Kiểm tra authentication
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    Console.WriteLine("DEBUG - User not authenticated");
                    TempData["error"] = "Bạn cần đăng nhập để xem hóa đơn";
                    return RedirectToAction("Login", "Account");
                }

                // Lấy thông tin user
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Staff") || User.IsInRole("Sale") || User.IsInRole("Shipper");
                
                Console.WriteLine($"DEBUG - Current user email: {userEmail}");
                Console.WriteLine($"DEBUG - Is admin/staff: {isAdmin}");
                Console.WriteLine($"DEBUG - User roles: {string.Join(", ", User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}");

                // Lấy thông tin đơn hàng với chi tiết sản phẩm
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

                Console.WriteLine($"DEBUG - Order found: {order != null}");
                if (order != null)
                {
                    Console.WriteLine($"DEBUG - Order UserName: {order.UserName}");
                    Console.WriteLine($"DEBUG - Order OrderCode: {order.OrderCode}");
                    Console.WriteLine($"DEBUG - OrderDetails count: {order.OrderDetails?.Count ?? 0}");
                }

                // Kiểm tra đơn hàng có tồn tại không
                if (order == null)
                {
                    Console.WriteLine("DEBUG - Order not found in database");
                    TempData["error"] = "Không tìm thấy đơn hàng";
                    return RedirectToAction("Index", "Home");
                }

                // Kiểm tra quyền truy cập
                if (!isAdmin && order.UserName != userEmail)
                {
                    Console.WriteLine("DEBUG - Access denied - user not authorized");
                    Console.WriteLine($"DEBUG - Order UserName: {order.UserName}");
                    Console.WriteLine($"DEBUG - Current UserEmail: {userEmail}");
                    TempData["error"] = "Bạn không có quyền xem hóa đơn này";
                    return RedirectToAction("Index", "Home");
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
                    PaymentInfo = order.MomoInfos?.FirstOrDefault() // Can be null
                };

                // Additional validation
                if (invoiceData.Order == null)
                {
                    Console.WriteLine("DEBUG - Order is null in InvoiceViewModel");
                    TempData["error"] = "Không thể tải thông tin đơn hàng";
                    return RedirectToAction("Index", "Home");
                }

                Console.WriteLine("DEBUG - Returning invoice view");
                return View(invoiceData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG - Exception in Index: {ex.Message}");
                Console.WriteLine($"DEBUG - Stack trace: {ex.StackTrace}");
                TempData["error"] = $"Lỗi khi tải hóa đơn: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Print(string orderCode)
        {
            try
            {
                Console.WriteLine($"DEBUG - InvoiceController.Print called with orderCode: {orderCode}");
                
                if (string.IsNullOrEmpty(orderCode))
                {
                    TempData["error"] = "Mã đơn hàng không hợp lệ";
                    return RedirectToAction("Index", "Home");
                }

                // Kiểm tra authentication
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    TempData["error"] = "Bạn cần đăng nhập để in hóa đơn";
                    return RedirectToAction("Login", "Account");
                }

                // Lấy thông tin đơn hàng với chi tiết sản phẩm
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
                    TempData["error"] = "Không tìm thấy đơn hàng";
                    return RedirectToAction("Index", "Home");
                }

                // Kiểm tra quyền truy cập
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var isAdmin = User.IsInRole("Admin");
                
                if (!isAdmin && order.UserName != userEmail)
                {
                    TempData["error"] = "Bạn không có quyền in hóa đơn này";
                    return RedirectToAction("Index", "Home");
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

                // Sử dụng trang Print chuyên dụng
                return View("Print", invoiceData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG - Exception in Print: {ex.Message}");
                TempData["error"] = $"Lỗi khi in hóa đơn: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Download(string orderCode, string format = "pdf")
        {
            try
            {
                Console.WriteLine($"DEBUG - InvoiceController.Download called with orderCode: {orderCode}, format: {format}");
                
                if (string.IsNullOrEmpty(orderCode))
                {
                    TempData["error"] = "Mã đơn hàng không hợp lệ";
                    return RedirectToAction("Index", "Home");
                }

                // Kiểm tra authentication
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    TempData["error"] = "Bạn cần đăng nhập để tải hóa đơn";
                    return RedirectToAction("Login", "Account");
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
                    TempData["error"] = "Không tìm thấy đơn hàng";
                    return RedirectToAction("Index", "Home");
                }

                // Kiểm tra quyền truy cập
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var isAdmin = User.IsInRole("Admin");
                
                if (!isAdmin && order.UserName != userEmail)
                {
                    TempData["error"] = "Bạn không có quyền tải hóa đơn này";
                    return RedirectToAction("Index", "Home");
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

                // Xuất file theo định dạng
                byte[] fileData;
                string fileName;
                string contentType;

                Console.WriteLine($"DEBUG - Starting export for format: {format}");
                Console.WriteLine($"DEBUG - InvoiceData Order: {invoiceData.Order?.OrderCode}");
                Console.WriteLine($"DEBUG - InvoiceData OrderDetails count: {invoiceData.Order?.OrderDetails?.Count ?? 0}");

                // Validation before export
                if (invoiceData == null)
                {
                    Console.WriteLine("DEBUG - InvoiceData is null");
                    TempData["error"] = "Không thể tải dữ liệu hóa đơn";
                    return RedirectToAction("Index", "Home");
                }

                if (invoiceData.Order == null)
                {
                    Console.WriteLine("DEBUG - Order is null");
                    TempData["error"] = "Không tìm thấy thông tin đơn hàng";
                    return RedirectToAction("Index", "Home");
                }

                if (invoiceData.Order.OrderDetails == null || !invoiceData.Order.OrderDetails.Any())
                {
                    Console.WriteLine("DEBUG - OrderDetails is null or empty");
                    TempData["error"] = "Đơn hàng không có sản phẩm nào";
                    return RedirectToAction("Index", "Home");
                }

                switch (format.ToLower())
                {
                    case "pdf":
                        Console.WriteLine("DEBUG - Exporting to PDF");
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
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG - Exception in Download: {ex.Message}");
                TempData["error"] = $"Lỗi khi tải hóa đơn: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> All(int page = 1)
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
                    return new
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

                return View(ordersWithInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in All: {ex.Message}");
                TempData["error"] = $"Lỗi khi tải danh sách đơn hàng: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> FixNullOrderCodes()
        {
            try
            {
                // Tìm các đơn hàng có OrderCode null
                var ordersWithNullCode = await _dataContext.Orders
                    .Where(o => o.OrderCode == null)
                    .ToListAsync();

                Console.WriteLine($"DEBUG - Found {ordersWithNullCode.Count} orders with null OrderCode");

                foreach (var order in ordersWithNullCode)
                {
                    // Tạo OrderCode mới
                    order.OrderCode = GenerateOrderCode();
                    Console.WriteLine($"DEBUG - Generated OrderCode: {order.OrderCode} for Order ID: {order.Id}");
                }

                if (ordersWithNullCode.Any())
                {
                    await _dataContext.SaveChangesAsync();
                    Console.WriteLine($"DEBUG - Fixed {ordersWithNullCode.Count} orders");
                }

                TempData["success"] = $"Đã sửa {ordersWithNullCode.Count} đơn hàng có mã đơn hàng bị null";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG - Error fixing null order codes: {ex.Message}");
                TempData["error"] = $"Lỗi khi sửa mã đơn hàng: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugInvoice(string orderCode)
        {
            try
            {
                Console.WriteLine($"DEBUG - DebugInvoice called with orderCode: {orderCode}");
                
                if (string.IsNullOrEmpty(orderCode))
                {
                    return Json(new { error = "OrderCode is null or empty" });
                }

                // Kiểm tra user authentication
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
                var isAdmin = User.IsInRole("Admin");
                
                Console.WriteLine($"DEBUG - User authenticated: {isAuthenticated}");
                Console.WriteLine($"DEBUG - User email: {userEmail}");
                Console.WriteLine($"DEBUG - Is admin: {isAdmin}");

                // Tìm đơn hàng
                var order = await _dataContext.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

                if (order == null)
                {
                    return Json(new { error = "Order not found", orderCode = orderCode });
                }

                Console.WriteLine($"DEBUG - Order found: ID={order.Id}, UserName={order.UserName}, OrderCode={order.OrderCode}");
                Console.WriteLine($"DEBUG - OrderDetails count: {order.OrderDetails?.Count ?? 0}");

                // Kiểm tra quyền truy cập
                var hasAccess = isAdmin || order.UserName == userEmail;
                Console.WriteLine($"DEBUG - Has access: {hasAccess}");

                return Json(new
                {
                    success = true,
                    order = new
                    {
                        id = order.Id,
                        orderCode = order.OrderCode,
                        userName = order.UserName,
                        createdDate = order.CreatedDate,
                        status = order.Status,
                        orderDetailsCount = order.OrderDetails?.Count ?? 0,
                        orderDetails = order.OrderDetails?.Select(od => new
                        {
                            productId = od.ProductId,
                            productName = od.Product?.Name ?? "N/A",
                            quantity = od.Quantity,
                            price = od.Price
                        }).ToList()
                    },
                    user = new
                    {
                        email = userEmail,
                        isAuthenticated = isAuthenticated,
                        isAdmin = isAdmin
                    },
                    hasAccess = hasAccess
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG - Exception in DebugInvoice: {ex.Message}");
                return Json(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckAllOrders()
        {
            try
            {
                var allOrders = await _dataContext.Orders
                    .OrderByDescending(o => o.CreatedDate)
                    .Take(10)
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderCode,
                        o.UserName,
                        o.CreatedDate,
                        o.Status,
                        HasOrderCode = !string.IsNullOrEmpty(o.OrderCode)
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    totalOrders = allOrders.Count,
                    orders = allOrders
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG - Exception in CheckAllOrders: {ex.Message}");
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult TestPrint()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Status()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DebugOrders()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDebugOrders()
        {
            try
            {
                var orders = await _dataContext.Orders
                    .OrderByDescending(o => o.CreatedDate)
                    .Take(20)
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderCode,
                        o.UserName,
                        o.CreatedDate,
                        o.Status,
                        StatusText = o.Status == 1 ? "Đã xác nhận" : "Chờ xử lý"
                    })
                    .ToListAsync();

                return Json(new { success = true, orders = orders });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        /// <summary>
        /// Xuất nhiều hóa đơn cùng lúc
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ExportMultiple(string[] orderCodes, string format = "excel")
        {
            try
            {
                if (orderCodes == null || !orderCodes.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất một đơn hàng" });
                }

                // Kiểm tra authentication
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập để xuất hóa đơn" });
                }

                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var isAdmin = User.IsInRole("Admin");

                var invoicesData = new List<InvoiceViewModel>();

                foreach (var orderCode in orderCodes)
                {
                    var order = await _dataContext.Orders
                        .Include(o => o.OrderDetails)
                            .ThenInclude(od => od.Product)
                        // .Include(o => o.Coupon) // Removed - Coupon table not exists
                        .Include(o => o.MomoInfos)
                        .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

                    if (order != null)
                    {
                        // Kiểm tra quyền truy cập
                        if (!isAdmin && order.UserName != userEmail)
                        {
                            continue; // Bỏ qua đơn hàng không có quyền truy cập
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

                        invoicesData.Add(invoiceData);
                    }
                }

                if (!invoicesData.Any())
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng hợp lệ" });
                }

                // Xuất file
                byte[] fileData;
                string fileName;
                string contentType;

                if (format.ToLower() == "excel")
                {
                    fileData = _invoiceExportService.ExportMultipleToExcel(invoicesData);
                    fileName = $"BaoCaoHoaDon_{DateTime.Now:yyyyMMdd}.xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                }
                else
                {
                    // Nếu không phải Excel, tạo ZIP chứa các file PDF riêng lẻ
                    // Tạm thời trả về Excel
                    fileData = _invoiceExportService.ExportMultipleToExcel(invoicesData);
                    fileName = $"BaoCaoHoaDon_{DateTime.Now:yyyyMMdd}.xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                }

                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG - Exception in ExportMultiple: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi khi xuất hóa đơn: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy danh sách đơn hàng để xuất hàng loạt
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOrdersForExport(int page = 1, int pageSize = 10, string status = "", string dateFrom = "", string dateTo = "")
        {
            try
            {
                // Kiểm tra authentication
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập" });
                }

                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var isAdmin = User.IsInRole("Admin");

                var query = _dataContext.Orders.AsQueryable();

                // Lọc theo quyền truy cập
                if (!isAdmin)
                {
                    query = query.Where(o => o.UserName == userEmail);
                }

                // Lọc theo trạng thái
                if (!string.IsNullOrEmpty(status) && int.TryParse(status, out int statusInt))
                {
                    query = query.Where(o => o.Status == statusInt);
                }

                // Lọc theo ngày
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
                        o.Id,
                        o.OrderCode,
                        o.UserName,
                        o.CreatedDate,
                        o.Status,
                        TotalAmount = o.OrderDetails != null ? o.OrderDetails.Sum(od => od.Price * od.Quantity) - o.DiscountAmount + o.ShippingCost : 0,
                        StatusText = o.Status == 1 ? "Đã xác nhận" : "Chờ xử lý"
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    orders = orders,
                    totalCount = totalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG - Exception in GetOrdersForExport: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi khi lấy danh sách đơn hàng: {ex.Message}" });
            }
        }

        private string GenerateOrderCode()
        {
            // Generate a unique order code with timestamp and random number
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"ORD{timestamp}{random}";
        }
    }

}