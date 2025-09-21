using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;
using System.Text;
using System.Globalization;

namespace Shopping_Demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class ExportController : Controller
    {
        private readonly DataContext _dataContext;

        public ExportController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        private async Task<bool> CheckTableExists(string tableName)
        {
            try
            {
                var connection = _dataContext.Database.GetDbConnection();
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";
                var result = await command.ExecuteScalarAsync();
                await connection.CloseAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Orders Export
        [HttpGet]
        public async Task<IActionResult> ExportOrders(string format = "excel", DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Kiểm tra xem có đơn hàng nào không
                var hasOrders = await _dataContext.Orders.AnyAsync();
                if (!hasOrders)
                {
                    TempData["error"] = "Không có dữ liệu đơn hàng để xuất";
                    return RedirectToAction("Index");
                }

                var query = _dataContext.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .AsQueryable();

                // Apply date filter if provided
                if (startDate.HasValue)
                {
                    query = query.Where(o => o.CreatedDate >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    query = query.Where(o => o.CreatedDate <= endDate.Value.AddDays(1));
                }

                var orders = await query.OrderByDescending(o => o.CreatedDate).ToListAsync();

                if (format.ToLower() == "csv")
                {
                    return await ExportOrdersToCSV(orders);
                }
                else
                {
                    return await ExportOrdersToExcel(orders);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Lỗi xuất dữ liệu đơn hàng: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private async Task<IActionResult> ExportOrdersToCSV(List<OrderModel> orders)
        {
            var csv = new StringBuilder();
            
            // CSV Header
            csv.AppendLine("Mã đơn hàng,Tên khách hàng,Ngày tạo,Trạng thái,Phương thức thanh toán,Phí vận chuyển,Giảm giá,Tổng tiền,Địa chỉ giao hàng");

            foreach (var order in orders)
            {
                // Calculate total amount
                decimal subtotal = order.OrderDetails?.Sum(od => od.Price * od.Quantity) ?? 0;
                decimal totalAmount = subtotal - order.DiscountAmount + order.ShippingCost;

                csv.AppendLine($"{order.OrderCode},{EscapeCsvField(order.UserName)},{order.CreatedDate:dd/MM/yyyy HH:mm},{GetStatusText(order.Status)},{order.PaymentMethod ?? "COD"},{order.ShippingCost},{order.DiscountAmount},{totalAmount},{EscapeCsvField($"{order.ShippingAddress}, {order.ShippingWard}, {order.ShippingDistrict}, {order.ShippingCity}")}");
            }

            var fileName = $"Orders_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "text/csv", fileName);
        }

        private async Task<IActionResult> ExportOrdersToExcel(List<OrderModel> orders)
        {
            var csv = new StringBuilder();
            
            // Excel-compatible CSV with BOM
            csv.AppendLine("\uFEFF"); // UTF-8 BOM for Excel
            
            // Header
            csv.AppendLine("Mã đơn hàng,Tên khách hàng,Ngày tạo,Trạng thái,Phương thức thanh toán,Phí vận chuyển,Giảm giá,Tổng tiền,Địa chỉ giao hàng,Sản phẩm");

            foreach (var order in orders)
            {
                // Calculate total amount
                decimal subtotal = order.OrderDetails?.Sum(od => od.Price * od.Quantity) ?? 0;
                decimal totalAmount = subtotal - order.DiscountAmount + order.ShippingCost;

                // Get products info
                var productsInfo = string.Join("; ", order.OrderDetails?.Select(od => $"{od.Product?.Name} (SL:{od.Quantity}, Giá:{od.Price:N0}₫)") ?? new List<string>());

                csv.AppendLine($"{order.OrderCode},{EscapeCsvField(order.UserName)},{order.CreatedDate:dd/MM/yyyy HH:mm},{GetStatusText(order.Status)},{order.PaymentMethod ?? "COD"},{order.ShippingCost},{order.DiscountAmount},{totalAmount},{EscapeCsvField($"{order.ShippingAddress}, {order.ShippingWard}, {order.ShippingDistrict}, {order.ShippingCity}")},{EscapeCsvField(productsInfo)}");
            }

            var fileName = $"Orders_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "application/vnd.ms-excel", fileName);
        }
        #endregion

        #region Products Export
        [HttpGet]
        public async Task<IActionResult> ExportProducts(string format = "excel")
        {
            try
            {
                var products = await _dataContext.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.ProductImages)
                    .OrderByDescending(p => p.Id)
                    .ToListAsync();

                if (format.ToLower() == "csv")
                {
                    return await ExportProductsToCSV(products);
                }
                else
                {
                    return await ExportProductsToExcel(products);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Lỗi xuất dữ liệu sản phẩm: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private async Task<IActionResult> ExportProductsToCSV(List<ProductModel> products)
        {
            var csv = new StringBuilder();
            
            // CSV Header
            csv.AppendLine("ID,Tên sản phẩm,Model,Năm,Giới tính,Tình trạng,Giá,Gía vốn,Số lượng,Đã bán,Thương hiệu,Danh mục,Kích thước vỏ,Vật liệu vỏ,Mặt số,Màu mặt số,Bộ máy,Calibre,Vật liệu dây đeo,Trạng thái");

            foreach (var product in products)
            {
                csv.AppendLine($"{product.Id},{EscapeCsvField(product.Name)},{EscapeCsvField(product.Model)},{product.Year},{EscapeCsvField(product.Gender)},{EscapeCsvField(product.Condition)},{product.Price},{product.CapitalPrice},{product.Quantity},{product.Sold},{EscapeCsvField(product.Brand?.Name)},{EscapeCsvField(product.Category?.Name)},{EscapeCsvField(product.CaseSize)},{EscapeCsvField(product.CaseMaterial)},{EscapeCsvField(product.Crystal)},{EscapeCsvField(product.DialColor)},{EscapeCsvField(product.MovementType)},{EscapeCsvField(product.Calibre)},{EscapeCsvField(product.BraceletMaterial)},{(product.IsActive ? "Hoạt động" : "Ẩn")}");
            }

            var fileName = $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "text/csv", fileName);
        }

        private async Task<IActionResult> ExportProductsToExcel(List<ProductModel> products)
        {
            var csv = new StringBuilder();
            
            // Excel-compatible CSV with BOM
            csv.AppendLine("\uFEFF"); // UTF-8 BOM for Excel
            
            // Header
            csv.AppendLine("ID,Tên sản phẩm,Model,Năm,Giới tính,Tình trạng,Giá,Gía vốn,Số lượng,Đã bán,Thương hiệu,Danh mục,Kích thước vỏ,Vật liệu vỏ,Mặt số,Màu mặt số,Bộ máy,Calibre,Vật liệu dây đeo,Trạng thái,Mô tả");

            foreach (var product in products)
            {
                csv.AppendLine($"{product.Id},{EscapeCsvField(product.Name)},{EscapeCsvField(product.Model)},{product.Year},{EscapeCsvField(product.Gender)},{EscapeCsvField(product.Condition)},{product.Price},{product.CapitalPrice},{product.Quantity},{product.Sold},{EscapeCsvField(product.Brand?.Name)},{EscapeCsvField(product.Category?.Name)},{EscapeCsvField(product.CaseSize)},{EscapeCsvField(product.CaseMaterial)},{EscapeCsvField(product.Crystal)},{EscapeCsvField(product.DialColor)},{EscapeCsvField(product.MovementType)},{EscapeCsvField(product.Calibre)},{EscapeCsvField(product.BraceletMaterial)},{(product.IsActive ? "Hoạt động" : "Ẩn")},{EscapeCsvField(product.Description)}");
            }

            var fileName = $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "application/vnd.ms-excel", fileName);
        }
        #endregion

        #region Users Export
        [HttpGet]
        public async Task<IActionResult> ExportUsers(string format = "excel")
        {
            try
            {
                var users = await _dataContext.Users
                    .OrderByDescending(u => u.Id)
                    .ToListAsync();

                if (format.ToLower() == "csv")
                {
                    return await ExportUsersToCSV(users);
                }
                else
                {
                    return await ExportUsersToExcel(users);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Lỗi xuất dữ liệu người dùng: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private async Task<IActionResult> ExportUsersToCSV(List<AppUserModel> users)
        {
            var csv = new StringBuilder();
            
            // CSV Header
            csv.AppendLine("ID,Tên đầy đủ,Email,Số điện thoại,Địa chỉ,Ngày tạo");

            foreach (var user in users)
            {
                csv.AppendLine($"{user.Id},{EscapeCsvField(user.FullName)},{EscapeCsvField(user.Email)},{EscapeCsvField(user.PhoneNumber)},{EscapeCsvField(user.Address)},N/A");
            }

            var fileName = $"Users_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "text/csv", fileName);
        }

        private async Task<IActionResult> ExportUsersToExcel(List<AppUserModel> users)
        {
            var csv = new StringBuilder();
            
            // Excel-compatible CSV with BOM
            csv.AppendLine("\uFEFF"); // UTF-8 BOM for Excel
            
            // Header
            csv.AppendLine("ID,Tên đầy đủ,Email,Số điện thoại,Địa chỉ,Ngày tạo,Email đã xác nhận");

            foreach (var user in users)
            {
                csv.AppendLine($"{user.Id},{EscapeCsvField(user.FullName)},{EscapeCsvField(user.Email)},{EscapeCsvField(user.PhoneNumber)},{EscapeCsvField(user.Address)},N/A,{(user.EmailConfirmed ? "Có" : "Không")}");
            }

            var fileName = $"Users_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "application/vnd.ms-excel", fileName);
        }
        #endregion

        #region Statistics Export
        [HttpGet]
        public async Task<IActionResult> ExportStatistics(string format = "excel", DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Sử dụng cùng logic như các biểu đồ trong dashboard
                var start = startDate ?? DateTime.Now.AddDays(-30);
                var end = endDate ?? DateTime.Now;
                
                // Lấy đơn hàng đã giao trong khoảng thời gian (giống GetSalesTrendData)
                var deliveredOrders = await _dataContext.Orders
                    .Where(o => o.Status == 0) // Đã giao hàng
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .ToListAsync();

                // Lọc theo thời gian
                var filteredOrders = deliveredOrders
                    .Where(o => 
                        (o.DeliveredDate.HasValue && 
                         o.DeliveredDate.Value.Date >= start.Date && 
                         o.DeliveredDate.Value.Date <= end.Date) ||
                        (!o.DeliveredDate.HasValue && 
                         o.CreatedDate.Date >= start.Date && 
                         o.CreatedDate.Date <= end.Date))
                    .ToList();

                // Nhóm theo ngày và tính toán thống kê (giống GetSalesTrendData)
                var dailyStats = filteredOrders
                    .GroupBy(o => o.DeliveredDate?.Date ?? o.CreatedDate.Date)
                    .Select(g => new
                    {
                        DateCreated = g.Key,
                        Quantity = g.Count(), // Số đơn hàng
                        Sold = g.SelectMany(o => o.OrderDetails).Sum(od => od.Quantity), // Số sản phẩm bán
                        Revenue = g.SelectMany(o => o.OrderDetails).Sum(od => od.Price * od.Quantity), // Doanh thu
                        Profit = g.SelectMany(o => o.OrderDetails).Sum(od => (od.Price - (od.Product?.CapitalPrice ?? 0)) * od.Quantity) // Lợi nhuận
                    })
                    .OrderByDescending(s => s.DateCreated)
                    .ToList();

                if (format.ToLower() == "csv")
                {
                    return await ExportStatisticsToCSV(dailyStats);
                }
                else
                {
                    return await ExportStatisticsToExcel(dailyStats);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Lỗi xuất dữ liệu thống kê: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private async Task<IActionResult> ExportStatisticsToCSV(dynamic statistics)
        {
            var csv = new StringBuilder();
            
            // CSV Header
            csv.AppendLine("Ngày,Số đơn hàng,Số sản phẩm bán,Doanh thu,Lợi nhuận");

            foreach (var stat in statistics)
            {
                csv.AppendLine($"{stat.DateCreated:dd/MM/yyyy},{stat.Quantity},{stat.Sold},{stat.Revenue},{stat.Profit}");
            }

            var fileName = $"Statistics_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "text/csv", fileName);
        }

        private async Task<IActionResult> ExportStatisticsToExcel(dynamic statistics)
        {
            var csv = new StringBuilder();
            
            // Excel-compatible CSV with BOM
            csv.AppendLine("\uFEFF"); // UTF-8 BOM for Excel
            
            // Header
            csv.AppendLine("Ngày,Số đơn hàng,Số sản phẩm bán,Doanh thu,Lợi nhuận,Tỷ lệ lợi nhuận (%)");

            foreach (var stat in statistics)
            {
                decimal profitMargin = stat.Revenue > 0 ? (stat.Profit / stat.Revenue) * 100 : 0;
                csv.AppendLine($"{stat.DateCreated:dd/MM/yyyy},{stat.Quantity},{stat.Sold},{stat.Revenue},{stat.Profit},{profitMargin:F2}");
            }

            var fileName = $"Statistics_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "application/vnd.ms-excel", fileName);
        }

        // Xuất thống kê chi tiết theo các loại khác nhau
        [HttpGet]
        public async Task<IActionResult> ExportDetailedStatistics(string format = "excel", DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.Now.AddDays(-30);
                var end = endDate ?? DateTime.Now;

                // 1. Thống kê trạng thái đơn hàng
                var orderStatusStats = await GetOrderStatusStatsData(start, end);
                
                // 2. Thống kê hiệu suất thương hiệu
                var brandStats = await GetBrandPerformanceStatsData(start, end);
                
                // 3. Thống kê phân bố danh mục
                var categoryStats = await GetCategoryDistributionStatsData(start, end);
                
                // 4. Thống kê xu hướng bán hàng
                var salesTrendStats = await GetSalesTrendStatsData(start, end);
                
                // 5. Thống kê WishList
                var wishlistStats = await GetWishlistStatsData();
                
                // 6. Thống kê theo giới tính
                var genderStats = await GetGenderStatsData(start, end);
                
                // 7. Thống kê theo độ tuổi
                var ageStats = await GetAgeStatsData(start, end);
                
                // 8. Thống kê theo khoảng giá
                var priceRangeStats = await GetPriceRangeStatsData(start, end);

                if (format.ToLower() == "csv")
                {
                    return await ExportDetailedStatisticsToCSV(orderStatusStats, brandStats, categoryStats, salesTrendStats, wishlistStats, genderStats, ageStats, priceRangeStats);
                }
                else
                {
                    return await ExportDetailedStatisticsToExcel(orderStatusStats, brandStats, categoryStats, salesTrendStats, wishlistStats, genderStats, ageStats, priceRangeStats);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Lỗi xuất thống kê chi tiết: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private async Task<dynamic> GetOrderStatusStatsData(DateTime start, DateTime end)
        {
            var allOrders = await _dataContext.Orders
                .Where(o => o.CreatedDate.Date >= start.Date && o.CreatedDate.Date <= end.Date)
                .ToListAsync();

            return allOrders
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    StatusName = GetStatusText(g.Key)
                })
                .OrderBy(s => s.Status)
                .ToList();
        }

        private async Task<dynamic> GetBrandPerformanceStatsData(DateTime start, DateTime end)
        {
            var deliveredOrders = await _dataContext.Orders
                .Where(o => o.Status == 0)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Brand)
                .ToListAsync();

            var filteredOrders = deliveredOrders
                .Where(o => 
                    (o.DeliveredDate.HasValue && 
                     o.DeliveredDate.Value.Date >= start.Date && 
                     o.DeliveredDate.Value.Date <= end.Date) ||
                    (!o.DeliveredDate.HasValue && 
                     o.CreatedDate.Date >= start.Date && 
                     o.CreatedDate.Date <= end.Date))
                .ToList();

            return filteredOrders
                .SelectMany(o => o.OrderDetails)
                .GroupBy(od => new { od.Product.Brand.Id, od.Product.Brand.Name })
                .Select(g => new
                {
                    BrandId = g.Key.Id,
                    BrandName = g.Key.Name,
                    TotalQuantitySold = g.Sum(od => od.Quantity),
                    TotalSales = g.Sum(od => od.Price * od.Quantity),
                    TotalProfit = g.Sum(od => (od.Price - (od.Product?.CapitalPrice ?? 0)) * od.Quantity)
                })
                .OrderByDescending(x => x.TotalSales)
                .ToList();
        }

        private async Task<dynamic> GetCategoryDistributionStatsData(DateTime start, DateTime end)
        {
            var deliveredOrders = await _dataContext.Orders
                .Where(o => o.Status == 0)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Category)
                .ToListAsync();

            var filteredOrders = deliveredOrders
                .Where(o => 
                    (o.DeliveredDate.HasValue && 
                     o.DeliveredDate.Value.Date >= start.Date && 
                     o.DeliveredDate.Value.Date <= end.Date) ||
                    (!o.DeliveredDate.HasValue && 
                     o.CreatedDate.Date >= start.Date && 
                     o.CreatedDate.Date <= end.Date))
                .ToList();

            return filteredOrders
                .SelectMany(o => o.OrderDetails)
                .GroupBy(od => new { od.Product.Category.Id, od.Product.Category.Name })
                .Select(g => new
                {
                    CategoryId = g.Key.Id,
                    CategoryName = g.Key.Name,
                    TotalQuantitySold = g.Sum(od => od.Quantity),
                    TotalSales = g.Sum(od => od.Price * od.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .ToList();
        }

        private async Task<dynamic> GetSalesTrendStatsData(DateTime start, DateTime end)
        {
            var deliveredOrders = await _dataContext.Orders
                .Where(o => o.Status == 0)
                .Include(o => o.OrderDetails)
                .ToListAsync();

            var filteredOrders = deliveredOrders
                .Where(o => 
                    (o.DeliveredDate.HasValue && 
                     o.DeliveredDate.Value.Date >= start.Date && 
                     o.DeliveredDate.Value.Date <= end.Date) ||
                    (!o.DeliveredDate.HasValue && 
                     o.CreatedDate.Date >= start.Date && 
                     o.CreatedDate.Date <= end.Date))
                .ToList();

            return filteredOrders
                .GroupBy(o => o.DeliveredDate?.Date ?? o.CreatedDate.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    OrdersCount = g.Count(),
                    TotalRevenue = g.SelectMany(o => o.OrderDetails).Sum(od => od.Price * od.Quantity),
                    TotalQuantity = g.SelectMany(o => o.OrderDetails).Sum(od => od.Quantity)
                })
                .OrderBy(x => x.Date)
                .ToList();
        }

        private async Task<dynamic> GetWishlistStatsData()
        {
            var wishlistData = await _dataContext.WishLists
                .Join(_dataContext.Products, w => w.ProductId, p => p.Id, (w, p) => new { w, p })
                .Join(_dataContext.Brands, x => x.p.BrandId, b => b.Id, (x, b) => new { x.w, x.p, b })
                .Join(_dataContext.Categories, x => x.p.CategoryId, c => c.Id, (x, c) => new { x.w, x.p, x.b, c })
                .Join(_dataContext.Users, x => x.w.UserId, u => u.Id, (x, u) => new { x.w, x.p, x.b, x.c, u })
                .Select(x => new
                {
                    ProductId = x.p.Id,
                    ProductName = x.p.Name,
                    BrandName = x.b.Name,
                    CategoryName = x.c.Name,
                    Price = x.p.Price,
                    UserId = x.w.UserId,
                    Gender = x.u.Gender,
                    Age = x.u.Age
                })
                .ToListAsync();

            // Top sản phẩm được yêu thích nhất
            var topWishlistProducts = wishlistData
                .GroupBy(x => new { x.ProductId, x.ProductName, x.BrandName, x.CategoryName, x.Price })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    BrandName = g.Key.BrandName,
                    CategoryName = g.Key.CategoryName,
                    Price = g.Key.Price,
                    WishlistCount = g.Count(),
                    MaleCount = g.Count(x => x.Gender == "Nam"),
                    FemaleCount = g.Count(x => x.Gender == "Nữ")
                })
                .OrderByDescending(x => x.WishlistCount)
                .Take(15)
                .ToList();

            // Thống kê theo giới tính
            var wishlistByGender = wishlistData
                .GroupBy(x => x.Gender)
                .Select(g => new
                {
                    Gender = g.Key ?? "Không xác định",
                    Count = g.Count()
                })
                .ToList();

            return new { topWishlistProducts, wishlistByGender };
        }

        private async Task<dynamic> GetGenderStatsData(DateTime start, DateTime end)
        {
            var deliveredOrders = await _dataContext.Orders
                .Where(o => o.Status == 0)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .ToListAsync();

            var filteredOrders = deliveredOrders
                .Where(o => 
                    (o.DeliveredDate.HasValue && 
                     o.DeliveredDate.Value.Date >= start.Date && 
                     o.DeliveredDate.Value.Date <= end.Date) ||
                    (!o.DeliveredDate.HasValue && 
                     o.CreatedDate.Date >= start.Date && 
                     o.CreatedDate.Date <= end.Date))
                .ToList();

            var orderCodes = filteredOrders.Select(o => o.OrderCode).ToList();

            var purchaseData = await _dataContext.OrderDetails
                .Where(od => orderCodes.Contains(od.OrderCode))
                .Join(_dataContext.Products, od => od.ProductId, p => p.Id, (od, p) => new { od, p })
                .Join(_dataContext.Orders, x => x.od.OrderCode, o => o.OrderCode, (x, o) => new { x.od, x.p, o })
                .Join(_dataContext.Users, x => x.o.UserId, u => u.Id, (x, u) => new { x.od, x.p, x.o, u })
                .Select(x => new
                {
                    ProductId = x.p.Id,
                    ProductName = x.p.Name,
                    BrandName = x.p.Brand.Name,
                    Gender = x.u.Gender,
                    TotalAmount = x.od.Price * x.od.Quantity,
                    Quantity = x.od.Quantity,
                    PurchaseDate = x.o.CreatedDate
                })
                .ToListAsync();

            return purchaseData
                .GroupBy(x => x.Gender)
                .Select(g => new
                {
                    Gender = g.Key ?? "Không xác định",
                    TotalRevenue = g.Sum(x => x.TotalAmount),
                    TotalQuantity = g.Sum(x => x.Quantity),
                    OrderCount = g.Select(x => x.PurchaseDate).Distinct().Count(),
                    TopProducts = g.GroupBy(x => new { x.ProductId, x.ProductName, x.BrandName })
                                 .Select(pg => new
                                 {
                                     ProductName = pg.Key.ProductName,
                                     BrandName = pg.Key.BrandName,
                                     TotalSold = pg.Sum(x => x.Quantity),
                                     TotalRevenue = pg.Sum(x => x.TotalAmount)
                                 })
                                 .OrderByDescending(pg => pg.TotalSold)
                                 .Take(10)
                                 .ToList()
                })
                .ToList();
        }

        private async Task<dynamic> GetAgeStatsData(DateTime start, DateTime end)
        {
            var deliveredOrders = await _dataContext.Orders
                .Where(o => o.Status == 0)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .ToListAsync();

            var filteredOrders = deliveredOrders
                .Where(o => 
                    (o.DeliveredDate.HasValue && 
                     o.DeliveredDate.Value.Date >= start.Date && 
                     o.DeliveredDate.Value.Date <= end.Date) ||
                    (!o.DeliveredDate.HasValue && 
                     o.CreatedDate.Date >= start.Date && 
                     o.CreatedDate.Date <= end.Date))
                .ToList();

            var orderCodes = filteredOrders.Select(o => o.OrderCode).ToList();

            var purchaseData = await _dataContext.OrderDetails
                .Where(od => orderCodes.Contains(od.OrderCode))
                .Join(_dataContext.Products, od => od.ProductId, p => p.Id, (od, p) => new { od, p })
                .Join(_dataContext.Orders, x => x.od.OrderCode, o => o.OrderCode, (x, o) => new { x.od, x.p, o })
                .Join(_dataContext.Users, x => x.o.UserId, u => u.Id, (x, u) => new { x.od, x.p, x.o, u })
                .Select(x => new
                {
                    ProductId = x.p.Id,
                    ProductName = x.p.Name,
                    BrandName = x.p.Brand.Name,
                    Age = x.u.Age,
                    TotalAmount = x.od.Price * x.od.Quantity,
                    Quantity = x.od.Quantity,
                    PurchaseDate = x.o.CreatedDate
                })
                .ToListAsync();

            // Phân nhóm theo độ tuổi
            var ageGroups = purchaseData
                .GroupBy(x => x.Age.HasValue ? 
                    (x.Age.Value < 18 ? "Dưới 18" :
                     x.Age.Value < 25 ? "18-24" :
                     x.Age.Value < 35 ? "25-34" :
                     x.Age.Value < 45 ? "35-44" :
                     x.Age.Value < 55 ? "45-54" :
                     "Trên 55") : "Không xác định")
                .Select(g => new
                {
                    AgeGroup = g.Key,
                    TotalRevenue = g.Sum(x => x.TotalAmount),
                    TotalQuantity = g.Sum(x => x.Quantity),
                    OrderCount = g.Select(x => x.PurchaseDate).Distinct().Count(),
                    TopProducts = g.GroupBy(x => new { x.ProductId, x.ProductName, x.BrandName })
                                 .Select(pg => new
                                 {
                                     ProductName = pg.Key.ProductName,
                                     BrandName = pg.Key.BrandName,
                                     TotalSold = pg.Sum(x => x.Quantity),
                                     TotalRevenue = pg.Sum(x => x.TotalAmount)
                                 })
                                 .OrderByDescending(pg => pg.TotalSold)
                                 .Take(10)
                                 .ToList()
                })
                .OrderBy(x => x.AgeGroup)
                .ToList();

            return ageGroups;
        }

        private async Task<dynamic> GetPriceRangeStatsData(DateTime start, DateTime end)
        {
            var deliveredOrders = await _dataContext.Orders
                .Where(o => o.Status == 0)
                .ToListAsync();

            var filteredOrders = deliveredOrders
                .Where(o => 
                    (o.DeliveredDate.HasValue && 
                     o.DeliveredDate.Value.Date >= start.Date && 
                     o.DeliveredDate.Value.Date <= end.Date) ||
                    (!o.DeliveredDate.HasValue && 
                     o.CreatedDate.Date >= start.Date && 
                     o.CreatedDate.Date <= end.Date))
                .ToList();

            var orderCodes = filteredOrders.Select(o => o.OrderCode).ToList();

            var orderDetails = await _dataContext.OrderDetails
                .Where(od => orderCodes.Contains(od.OrderCode))
                .ToListAsync();

            // Phân loại theo khoảng giá
            var priceRanges = new[]
            {
                new { Range = "< 50M", Min = 0m, Max = 50000000m },
                new { Range = "50M-100M", Min = 50000000m, Max = 100000000m },
                new { Range = "100M-200M", Min = 100000000m, Max = 200000000m },
                new { Range = "200M-500M", Min = 200000000m, Max = 500000000m },
                new { Range = "> 500M", Min = 500000000m, Max = decimal.MaxValue }
            };

            var priceStats = priceRanges.Select(pr => new
            {
                Range = pr.Range,
                Count = orderDetails.Count(od => od.Price >= pr.Min && od.Price < pr.Max),
                TotalRevenue = orderDetails.Where(od => od.Price >= pr.Min && od.Price < pr.Max)
                                          .Sum(od => od.Price * od.Quantity),
                TotalQuantity = orderDetails.Where(od => od.Price >= pr.Min && od.Price < pr.Max)
                                          .Sum(od => od.Quantity)
            })
            .Where(ps => ps.Count > 0)
            .ToList();

            return priceStats;
        }

        private async Task<IActionResult> ExportDetailedStatisticsToCSV(dynamic orderStatus, dynamic brandStats, dynamic categoryStats, dynamic salesTrend, dynamic wishlistStats, dynamic genderStats, dynamic ageStats, dynamic priceRangeStats)
        {
            var csv = new StringBuilder();
            
            // Header
            csv.AppendLine("=== THỐNG KÊ CHI TIẾT HỆ THỐNG ===");
            csv.AppendLine($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}");
            csv.AppendLine();

            // 1. Thống kê trạng thái đơn hàng
            csv.AppendLine("1. THỐNG KÊ TRẠNG THÁI ĐƠN HÀNG");
            csv.AppendLine("Trạng thái,Số lượng");
            foreach (var stat in orderStatus)
            {
                csv.AppendLine($"{stat.StatusName},{stat.Count}");
            }
            csv.AppendLine();

            // 2. Thống kê hiệu suất thương hiệu
            csv.AppendLine("2. THỐNG KÊ HIỆU SUẤT THƯƠNG HIỆU");
            csv.AppendLine("Thương hiệu,Số lượng bán,Doanh thu,Lợi nhuận");
            foreach (var stat in brandStats)
            {
                csv.AppendLine($"{EscapeCsvField(stat.BrandName)},{stat.TotalQuantitySold},{stat.TotalSales},{stat.TotalProfit}");
            }
            csv.AppendLine();

            // 3. Thống kê phân bố danh mục
            csv.AppendLine("3. THỐNG KÊ PHÂN BỐ DANH MỤC");
            csv.AppendLine("Danh mục,Số lượng bán,Doanh thu");
            foreach (var stat in categoryStats)
            {
                csv.AppendLine($"{EscapeCsvField(stat.CategoryName)},{stat.TotalQuantitySold},{stat.TotalSales}");
            }
            csv.AppendLine();

            // 4. Thống kê xu hướng bán hàng
            csv.AppendLine("4. THỐNG KÊ XU HƯỚNG BÁN HÀNG");
            csv.AppendLine("Ngày,Số đơn hàng,Doanh thu,Số lượng sản phẩm");
            foreach (var stat in salesTrend)
            {
                csv.AppendLine($"{stat.Date},{stat.OrdersCount},{stat.TotalRevenue},{stat.TotalQuantity}");
            }
            csv.AppendLine();

            // 5. Thống kê WishList
            csv.AppendLine("5. THỐNG KÊ WISHLIST");
            csv.AppendLine("5.1. Top sản phẩm được yêu thích nhất");
            csv.AppendLine("Sản phẩm,Thương hiệu,Danh mục,Giá,Số lượt yêu thích,Nam,Nữ");
            foreach (var product in wishlistStats.topWishlistProducts)
            {
                csv.AppendLine($"{EscapeCsvField(product.ProductName)},{EscapeCsvField(product.BrandName)},{EscapeCsvField(product.CategoryName)},{product.Price},{product.WishlistCount},{product.MaleCount},{product.FemaleCount}");
            }
            csv.AppendLine();
            csv.AppendLine("5.2. Thống kê WishList theo giới tính");
            csv.AppendLine("Giới tính,Số lượng");
            foreach (var gender in wishlistStats.wishlistByGender)
            {
                csv.AppendLine($"{gender.Gender},{gender.Count}");
            }
            csv.AppendLine();

            // 6. Thống kê theo giới tính
            csv.AppendLine("6. THỐNG KÊ MUA HÀNG THEO GIỚI TÍNH");
            csv.AppendLine("Giới tính,Tổng doanh thu,Số lượng sản phẩm,Số đơn hàng");
            foreach (var stat in genderStats)
            {
                csv.AppendLine($"{stat.Gender},{stat.TotalRevenue},{stat.TotalQuantity},{stat.OrderCount}");
            }
            csv.AppendLine();

            // 7. Thống kê theo độ tuổi
            csv.AppendLine("7. THỐNG KÊ MUA HÀNG THEO ĐỘ TUỔI");
            csv.AppendLine("Nhóm tuổi,Tổng doanh thu,Số lượng sản phẩm,Số đơn hàng");
            foreach (var stat in ageStats)
            {
                csv.AppendLine($"{stat.AgeGroup},{stat.TotalRevenue},{stat.TotalQuantity},{stat.OrderCount}");
            }
            csv.AppendLine();

            // 8. Thống kê theo khoảng giá
            csv.AppendLine("8. THỐNG KÊ THEO KHOẢNG GIÁ");
            csv.AppendLine("Khoảng giá,Số sản phẩm,Tổng doanh thu,Số lượng bán");
            foreach (var stat in priceRangeStats)
            {
                csv.AppendLine($"{stat.Range},{stat.Count},{stat.TotalRevenue},{stat.TotalQuantity}");
            }

            var fileName = $"DetailedStatistics_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "text/csv", fileName);
        }

        private async Task<IActionResult> ExportDetailedStatisticsToExcel(dynamic orderStatus, dynamic brandStats, dynamic categoryStats, dynamic salesTrend, dynamic wishlistStats, dynamic genderStats, dynamic ageStats, dynamic priceRangeStats)
        {
            var csv = new StringBuilder();
            
            // Excel-compatible CSV with BOM
            csv.AppendLine("\uFEFF"); // UTF-8 BOM for Excel
            
            // Header
            csv.AppendLine("=== THỐNG KÊ CHI TIẾT HỆ THỐNG ===");
            csv.AppendLine($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}");
            csv.AppendLine();

            // 1. Thống kê trạng thái đơn hàng
            csv.AppendLine("1. THỐNG KÊ TRẠNG THÁI ĐƠN HÀNG");
            csv.AppendLine("Trạng thái,Số lượng,Tỷ lệ (%)");
            var totalOrders = 0;
            foreach (var stat in orderStatus)
            {
                totalOrders += stat.Count;
            }
            foreach (var stat in orderStatus)
            {
                decimal percentage = totalOrders > 0 ? (stat.Count * 100.0m / totalOrders) : 0;
                csv.AppendLine($"{stat.StatusName},{stat.Count},{percentage:F2}");
            }
            csv.AppendLine();

            // 2. Thống kê hiệu suất thương hiệu
            csv.AppendLine("2. THỐNG KÊ HIỆU SUẤT THƯƠNG HIỆU");
            csv.AppendLine("Thương hiệu,Số lượng bán,Doanh thu,Lợi nhuận,Tỷ lệ lợi nhuận (%)");
            foreach (var stat in brandStats)
            {
                decimal profitMargin = stat.TotalSales > 0 ? (stat.TotalProfit / stat.TotalSales) * 100 : 0;
                csv.AppendLine($"{EscapeCsvField(stat.BrandName)},{stat.TotalQuantitySold},{stat.TotalSales},{stat.TotalProfit},{profitMargin:F2}");
            }
            csv.AppendLine();

            // 3. Thống kê phân bố danh mục
            csv.AppendLine("3. THỐNG KÊ PHÂN BỐ DANH MỤC");
            csv.AppendLine("Danh mục,Số lượng bán,Doanh thu,Tỷ lệ doanh thu (%)");
            var totalSales = 0m;
            foreach (var stat in categoryStats)
            {
                totalSales += stat.TotalSales;
            }
            foreach (var stat in categoryStats)
            {
                decimal salesPercentage = totalSales > 0 ? (stat.TotalSales * 100.0m / totalSales) : 0;
                csv.AppendLine($"{EscapeCsvField(stat.CategoryName)},{stat.TotalQuantitySold},{stat.TotalSales},{salesPercentage:F2}");
            }
            csv.AppendLine();

            // 4. Thống kê xu hướng bán hàng
            csv.AppendLine("4. THỐNG KÊ XU HƯỚNG BÁN HÀNG");
            csv.AppendLine("Ngày,Số đơn hàng,Doanh thu,Số lượng sản phẩm,Doanh thu trung bình/đơn");
            foreach (var stat in salesTrend)
            {
                decimal avgRevenuePerOrder = stat.OrdersCount > 0 ? stat.TotalRevenue / stat.OrdersCount : 0;
                csv.AppendLine($"{stat.Date},{stat.OrdersCount},{stat.TotalRevenue},{stat.TotalQuantity},{avgRevenuePerOrder:F2}");
            }
            csv.AppendLine();

            // 5. Thống kê WishList
            csv.AppendLine("5. THỐNG KÊ WISHLIST");
            csv.AppendLine("5.1. Top sản phẩm được yêu thích nhất");
            csv.AppendLine("Sản phẩm,Thương hiệu,Danh mục,Giá,Số lượt yêu thích,Nam,Nữ,Tỷ lệ Nam (%)");
            foreach (var product in wishlistStats.topWishlistProducts)
            {
                decimal malePercentage = product.WishlistCount > 0 ? (product.MaleCount * 100.0m / product.WishlistCount) : 0;
                csv.AppendLine($"{EscapeCsvField(product.ProductName)},{EscapeCsvField(product.BrandName)},{EscapeCsvField(product.CategoryName)},{product.Price},{product.WishlistCount},{product.MaleCount},{product.FemaleCount},{malePercentage:F2}");
            }
            csv.AppendLine();
            csv.AppendLine("5.2. Thống kê WishList theo giới tính");
            csv.AppendLine("Giới tính,Số lượng,Tỷ lệ (%)");
            var totalWishlist = 0;
            foreach (var gender in wishlistStats.wishlistByGender)
            {
                totalWishlist += gender.Count;
            }
            foreach (var gender in wishlistStats.wishlistByGender)
            {
                decimal percentage = totalWishlist > 0 ? (gender.Count * 100.0m / totalWishlist) : 0;
                csv.AppendLine($"{gender.Gender},{gender.Count},{percentage:F2}");
            }
            csv.AppendLine();

            // 6. Thống kê theo giới tính
            csv.AppendLine("6. THỐNG KÊ MUA HÀNG THEO GIỚI TÍNH");
            csv.AppendLine("Giới tính,Tổng doanh thu,Số lượng sản phẩm,Số đơn hàng,Doanh thu trung bình/đơn");
            foreach (var stat in genderStats)
            {
                decimal avgRevenuePerOrder = stat.OrderCount > 0 ? stat.TotalRevenue / stat.OrderCount : 0;
                csv.AppendLine($"{stat.Gender},{stat.TotalRevenue},{stat.TotalQuantity},{stat.OrderCount},{avgRevenuePerOrder:F2}");
            }
            csv.AppendLine();

            // 7. Thống kê theo độ tuổi
            csv.AppendLine("7. THỐNG KÊ MUA HÀNG THEO ĐỘ TUỔI");
            csv.AppendLine("Nhóm tuổi,Tổng doanh thu,Số lượng sản phẩm,Số đơn hàng,Doanh thu trung bình/đơn");
            foreach (var stat in ageStats)
            {
                decimal avgRevenuePerOrder = stat.OrderCount > 0 ? stat.TotalRevenue / stat.OrderCount : 0;
                csv.AppendLine($"{stat.AgeGroup},{stat.TotalRevenue},{stat.TotalQuantity},{stat.OrderCount},{avgRevenuePerOrder:F2}");
            }
            csv.AppendLine();

            // 8. Thống kê theo khoảng giá
            csv.AppendLine("8. THỐNG KÊ THEO KHOẢNG GIÁ");
            csv.AppendLine("Khoảng giá,Số sản phẩm,Tổng doanh thu,Số lượng bán,Giá trung bình");
            foreach (var stat in priceRangeStats)
            {
                decimal avgPrice = stat.TotalQuantity > 0 ? stat.TotalRevenue / stat.TotalQuantity : 0;
                csv.AppendLine($"{stat.Range},{stat.Count},{stat.TotalRevenue},{stat.TotalQuantity},{avgPrice:F2}");
            }

            var fileName = $"DetailedStatistics_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "application/vnd.ms-excel", fileName);
        }
        #endregion

        #region Categories Export
        [HttpGet]
        public async Task<IActionResult> ExportCategories(string format = "excel")
        {
            try
            {
                var categories = await _dataContext.Categories
                    .OrderBy(c => c.Level)
                    .ThenBy(c => c.Name)
                    .ToListAsync();

                if (format.ToLower() == "csv")
                {
                    return await ExportCategoriesToCSV(categories);
                }
                else
                {
                    return await ExportCategoriesToExcel(categories);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Lỗi xuất dữ liệu danh mục: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private async Task<IActionResult> ExportCategoriesToCSV(List<CategoryModel> categories)
        {
            var csv = new StringBuilder();
            
            // CSV Header
            csv.AppendLine("ID,Tên danh mục,Cấp độ,Danh mục cha,Slug");

            foreach (var category in categories)
            {
                var parentName = category.ParentId.HasValue ? 
                    categories.FirstOrDefault(c => c.Id == category.ParentId)?.Name ?? "N/A" : "Danh mục gốc";
                
                csv.AppendLine($"{category.Id},{EscapeCsvField(category.Name)},{category.Level},{EscapeCsvField(parentName)},{EscapeCsvField(category.Slug)}");
            }

            var fileName = $"Categories_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "text/csv", fileName);
        }

        private async Task<IActionResult> ExportCategoriesToExcel(List<CategoryModel> categories)
        {
            var csv = new StringBuilder();
            
            // Excel-compatible CSV with BOM
            csv.AppendLine("\uFEFF"); // UTF-8 BOM for Excel
            
            // Header
            csv.AppendLine("ID,Tên danh mục,Cấp độ,Danh mục cha,Slug,Mô tả");

            foreach (var category in categories)
            {
                var parentName = category.ParentId.HasValue ? 
                    categories.FirstOrDefault(c => c.Id == category.ParentId)?.Name ?? "N/A" : "Danh mục gốc";
                
                csv.AppendLine($"{category.Id},{EscapeCsvField(category.Name)},{category.Level},{EscapeCsvField(parentName)},{EscapeCsvField(category.Slug)},{EscapeCsvField(category.Description)}");
            }

            var fileName = $"Categories_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "application/vnd.ms-excel", fileName);
        }
        #endregion

        #region Brands Export
        [HttpGet]
        public async Task<IActionResult> ExportBrands(string format = "excel")
        {
            try
            {
                var brands = await _dataContext.Brands
                    .OrderBy(b => b.Name)
                    .ToListAsync();

                if (format.ToLower() == "csv")
                {
                    return await ExportBrandsToCSV(brands);
                }
                else
                {
                    return await ExportBrandsToExcel(brands);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Lỗi xuất dữ liệu thương hiệu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private async Task<IActionResult> ExportBrandsToCSV(List<BrandModel> brands)
        {
            var csv = new StringBuilder();
            
            // CSV Header
            csv.AppendLine("ID,Tên thương hiệu,Slug");

            foreach (var brand in brands)
            {
                csv.AppendLine($"{brand.Id},{EscapeCsvField(brand.Name)},{EscapeCsvField(brand.Slug)}");
            }

            var fileName = $"Brands_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "text/csv", fileName);
        }

        private async Task<IActionResult> ExportBrandsToExcel(List<BrandModel> brands)
        {
            var csv = new StringBuilder();
            
            // Excel-compatible CSV with BOM
            csv.AppendLine("\uFEFF"); // UTF-8 BOM for Excel
            
            // Header
            csv.AppendLine("ID,Tên thương hiệu,Slug,Mô tả");

            foreach (var brand in brands)
            {
                csv.AppendLine($"{brand.Id},{EscapeCsvField(brand.Name)},{EscapeCsvField(brand.Slug)},{EscapeCsvField(brand.Description)}");
            }

            var fileName = $"Brands_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "application/vnd.ms-excel", fileName);
        }
        #endregion

        #region Helper Methods
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // Escape quotes and wrap in quotes if contains comma, quote, or newline
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            return field;
        }

        private string GetStatusText(int status)
        {
            return status switch
            {
                0 => "Đã giao hàng",
                1 => "Đơn hàng mới",
                2 => "Chờ xác nhận",
                3 => "Đã hủy",
                4 => "Đang giao hàng",
                5 => "Đang hoàn",
                6 => "Hoàn thành",
                _ => "Không xác định"
            };
        }
        #endregion
    }
}
