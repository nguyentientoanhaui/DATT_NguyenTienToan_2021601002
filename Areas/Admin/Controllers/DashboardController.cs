using Microsoft.AspNetCore.Mvc;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shopping_Demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin,Manager,Staff,Sale,Shipper")] // Tạm thời comment để test
    public class DashboardController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DashboardController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            StatisticalModel model = new StatisticalModel();

            ViewBag.CountProduct = _dataContext.Products.Count();
            ViewBag.CountOrder = _dataContext.Orders.Count();
            ViewBag.CountCategory = _dataContext.Categories.Count();
            ViewBag.CountUser = _dataContext.Users.Count();

            ViewBag.CountBrand = _dataContext.Brands.Count();

            var orderStatusData = _dataContext.Orders
                .GroupBy(o => o.Status)
                .Select(g => new OrderStatusViewModel
                {
                    Status = GetStatusNameStatic(g.Key),
                    Count = g.Count()
                })
                .ToList();

            ViewBag.OrderStatusData = orderStatusData;

            return View(model);
        }

        public IActionResult SalesStats()
        {
            StatisticalModel model = new StatisticalModel();
            return View(model);
        }

        public IActionResult TestData()
        {
            return View();
        }




        public IActionResult ProductStats()
        {
            return RedirectToAction("Index", "ProductStats");
        }

        private static string GetStatusNameStatic(int status)
        {
            switch (status)
            {
                case 0: return "Đã giao hàng";
                case 1: return "Đơn hàng mới";
                case 2: return "Chờ xác nhận";
                case 3: return "Đã hủy";
                case 4: return "Đang giao hàng";
                case 5: return "Yêu cầu hoàn hàng";
                case 6: return "Hoàn hàng thành công";
                default: return "Không xác định";
            }
        }

        private string GetStatusName(int status)
        {
            return GetStatusNameStatic(status);
        }

        [HttpPost]
        public IActionResult GetChartData()
        {
            var data = _dataContext.Statisticals
                .OrderBy(x => x.DateCreated)
                .Select(x => new
                {
                    date = x.DateCreated.ToString("yyyy-MM-dd"),
                    sold = x.Sold,
                    quantity = x.Quantity,
                    revenue = x.Revenue,
                    profit = x.Profit
                }).ToList();
            return Json(data);
        }

        [HttpPost]
        public IActionResult GetChartDataBySelect(DateTime startDate, DateTime endDate)
        {
            var data = _dataContext.Statisticals
                .Where(x => x.DateCreated >= startDate && x.DateCreated <= endDate)
                .OrderBy(x => x.DateCreated)
                .Select(x => new
                {
                    date = x.DateCreated.ToString("yyyy-MM-dd"),
                    sold = x.Sold,
                    quantity = x.Quantity,
                    revenue = x.Revenue,
                    profit = x.Profit
                }).ToList();
            return Json(data);
        }

        // Temporary method to check user roles and permissions
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CheckUserRoles()
        {
            try
            {
                var results = new List<string>();
                
                // Check if user is authenticated
                if (User.Identity.IsAuthenticated)
                {
                    results.Add($"User is authenticated: {User.Identity.Name}");
                    results.Add($"User ID: {User.FindFirstValue(ClaimTypes.NameIdentifier)}");
                    results.Add($"User Email: {User.FindFirstValue(ClaimTypes.Email)}");
                    
                    // Check user roles
                    var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
                    results.Add($"User Roles: {string.Join(", ", userRoles)}");
                    
                    // Check if user has admin role
                    var hasAdminRole = User.IsInRole("Admin");
                    results.Add($"Has Admin Role: {hasAdminRole}");
                    
                    // Check if user has any admin-related role
                    var adminRoles = new[] { "Admin", "Manager", "Staff", "Sale", "Shipper" };
                    var hasAnyAdminRole = adminRoles.Any(role => User.IsInRole(role));
                    results.Add($"Has Any Admin Role: {hasAnyAdminRole}");
                }
                else
                {
                    results.Add("User is NOT authenticated");
                }
                
                // Check all roles in database
                var allRoles = _dataContext.Roles.ToList();
                results.Add($"All Roles in Database: {string.Join(", ", allRoles.Select(r => r.Name))}");
                
                // Check all users and their roles
                var allUsers = _dataContext.Users.ToList();
                results.Add($"Total Users: {allUsers.Count}");
                
                foreach (var user in allUsers.Take(5)) // Show first 5 users
                {
                    var userRoles = _dataContext.UserRoles
                        .Where(ur => ur.UserId == user.Id)
                        .Join(_dataContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                        .ToList();
                    
                    results.Add($"User {user.Email}: {string.Join(", ", userRoles)}");
                }
                
                ViewBag.Results = results;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        // Test action to check admin layout
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Test()
        {
            return View();
        }

        // Simple test action
        [HttpGet]
        [AllowAnonymous]
        public IActionResult SimpleTest()
        {
            return View();
        }

        // Basic test action with simple HTML
        [HttpGet]
        [AllowAnonymous]
        public IActionResult BasicTest()
        {
            return View();
        }

        // Complete admin dashboard
        [HttpGet]
        [AllowAnonymous]
        public IActionResult CompleteAdmin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult FilterData(DateTime? fromDate, DateTime? toDate)
        {
            var query = _dataContext.Statisticals.AsQueryable();
            if (fromDate.HasValue)
            {
                query = query.Where(s => s.DateCreated >= fromDate);
            }
            if (toDate.HasValue)
            {
                query = query.Where(s => s.DateCreated <= toDate);
            }
            var data = query
                .OrderBy(x => x.DateCreated)
                .Select(x => new
                {
                    date = x.DateCreated.ToString("yyyy-MM-dd"),
                    sold = x.Sold,
                    quantity = x.Quantity,
                    revenue = x.Revenue,
                    profit = x.Profit
                }).ToList();
            return Json(data);
        }

        [HttpPost]
        public IActionResult GetOrderStatusData()
        {
            var orderStatusData = _dataContext.Orders
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    status = GetStatusNameStatic(g.Key), 
                    count = g.Count()
                })
                .ToList();

            return Json(orderStatusData);
        }

        // Thống kê đồng hồ cao cấp - VIẾT LẠI HOÀN TOÀN
        [HttpPost]
        public IActionResult GetLuxuryWatchSalesStats(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // 1. LẤY TẤT CẢ ĐƠN HÀNG ĐÃ GIAO THÀNH CÔNG (Status = 0)
                var allDeliveredOrders = _dataContext.Orders
                    .Where(o => o.Status == 0)
                    .ToList();

                // 2. ÁP DỤNG FILTER THỜI GIAN
                var filteredOrders = allDeliveredOrders.AsQueryable();
                
                if (startDate.HasValue && endDate.HasValue)
                {
                    // Sử dụng DeliveredDate nếu có, nếu không thì dùng CreatedDate
                    filteredOrders = filteredOrders.Where(o => 
                        (o.DeliveredDate.HasValue && 
                         o.DeliveredDate.Value.Date >= startDate.Value.Date && 
                         o.DeliveredDate.Value.Date <= endDate.Value.Date) ||
                        (!o.DeliveredDate.HasValue && 
                         o.CreatedDate.Date >= startDate.Value.Date && 
                         o.CreatedDate.Date <= endDate.Value.Date));
                }

                var filteredOrdersList = filteredOrders.ToList();
                var totalOrders = filteredOrdersList.Count;

                // 3. TÍNH DOANH THU TỪ ORDERDETAILS
                var orderCodes = filteredOrdersList.Select(o => o.OrderCode).ToList();
                var orderDetails = _dataContext.OrderDetails
                    .Where(od => orderCodes.Contains(od.OrderCode))
                    .ToList();

                var totalRevenue = orderDetails.Sum(od => od.Price * od.Quantity);
                var totalWatchesSold = orderDetails.Sum(od => od.Quantity);
                var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

                // 4. DEBUG INFORMATION
                var debugInfo = new
                {
                    totalOrdersInDB = _dataContext.Orders.Count(),
                    totalDeliveredOrdersInDB = allDeliveredOrders.Count,
                    filteredOrdersCount = totalOrders,
                    startDate = startDate?.ToString("yyyy-MM-dd"),
                    endDate = endDate?.ToString("yyyy-MM-dd"),
                    sampleOrders = filteredOrdersList.Take(5).Select(o => new
                    {
                        o.OrderCode,
                        o.CreatedDate,
                        o.DeliveredDate,
                        DateUsed = o.DeliveredDate?.Date ?? o.CreatedDate.Date,
                        DateType = o.DeliveredDate.HasValue ? "DeliveredDate" : "CreatedDate"
                    }).ToList(),
                    orderDetailsCount = orderDetails.Count,
                    totalRevenue = totalRevenue,
                    message = $"Tìm thấy {totalOrders} đơn hàng đã giao trong khoảng thời gian"
                };

                return Json(new
                {
                    success = true,
                    debug = debugInfo,
                    metrics = new
                    {
                        totalRevenue = totalRevenue,
                        totalOrders = totalOrders,
                        totalWatchesSold = totalWatchesSold,
                        avgOrderValue = avgOrderValue,
                        deliveredOrders = totalOrders, // Tất cả đều là đơn hàng đã giao
                        cancelledOrders = 0,
                        returnedOrders = 0,
                        returnRate = 0
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }



        // Thống kê theo giới tính - SỬ DỤNG USERNAME THAY VÌ USERID
        [HttpPost]
        public IActionResult GetGenderStats(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Lấy các đơn hàng đã giao hàng (Status = 0) trong khoảng thời gian
                var deliveredOrderCodes = _dataContext.Orders
                    .Where(o => o.Status == 0) // Đã giao hàng
                    .Where(o => startDate == null || o.CreatedDate >= startDate)
                    .Where(o => endDate == null || o.CreatedDate <= endDate)
                    .Select(o => o.OrderCode)
                    .ToList();

                // Lấy dữ liệu chi tiết đơn hàng với thông tin sản phẩm và user - SỬ DỤNG EMAIL
                var orderData = from od in _dataContext.OrderDetails
                               where deliveredOrderCodes.Contains(od.OrderCode)
                               join o in _dataContext.Orders on od.OrderCode equals o.OrderCode
                               join u in _dataContext.Users on o.UserName equals u.Email // SỬ DỤNG EMAIL
                               join p in _dataContext.Products on od.ProductId equals p.Id
                               join b in _dataContext.Brands on p.BrandId equals b.Id
                               join c in _dataContext.Categories on p.CategoryId equals c.Id
                               select new
                               {
                                   OrderCode = od.OrderCode,
                                   ProductId = p.Id,
                                   ProductName = p.Name,
                                   BrandName = b.Name,
                                   CategoryName = c.Name,
                                   Quantity = od.Quantity,
                                   Price = od.Price,
                                   TotalAmount = od.Quantity * od.Price,
                                   UserId = u.Id,
                                   UserName = u.UserName,
                                   Email = u.Email,
                                   Gender = u.Gender,
                                   FullName = u.FullName
                               };

                // Thống kê theo giới tính
                var genderStats = orderData
                    .GroupBy(x => x.Gender ?? "Không xác định")
                    .Select(g => new
                    {
                        Gender = g.Key,
                        TotalRevenue = g.Sum(x => x.TotalAmount),
                        TotalQuantity = g.Sum(x => x.Quantity),
                        OrderCount = g.Select(x => x.OrderCode).Distinct().Count()
                    })
                    .OrderByDescending(x => x.TotalRevenue)
                    .ToList();

                // Top sản phẩm nam mua (theo số lượng)
                var maleProducts = orderData
                    .Where(x => x.Gender == "Nam")
                    .GroupBy(x => new { x.ProductId, x.ProductName, x.BrandName, x.CategoryName })
                    .Select(g => new
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.ProductName,
                        BrandName = g.Key.BrandName,
                        CategoryName = g.Key.CategoryName,
                        TotalQuantitySold = g.Sum(x => x.Quantity),
                        TotalRevenue = g.Sum(x => x.TotalAmount),
                        OrderCount = g.Select(x => x.OrderCode).Distinct().Count()
                    })
                    .OrderByDescending(x => x.TotalQuantitySold)
                    .Take(5)
                    .ToList();

                // Top sản phẩm nữ mua (theo số lượng)
                var femaleProducts = orderData
                    .Where(x => x.Gender == "Nữ")
                    .GroupBy(x => new { x.ProductId, x.ProductName, x.BrandName, x.CategoryName })
                    .Select(g => new
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.ProductName,
                        BrandName = g.Key.BrandName,
                        CategoryName = g.Key.CategoryName,
                        TotalQuantitySold = g.Sum(x => x.Quantity),
                        TotalRevenue = g.Sum(x => x.TotalAmount),
                        OrderCount = g.Select(x => x.OrderCode).Distinct().Count()
                    })
                    .OrderByDescending(x => x.TotalQuantitySold)
                    .Take(5)
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        genderStats = genderStats,
                        maleProducts = maleProducts,
                        femaleProducts = femaleProducts,
                        debug = new
                        {
                            deliveredOrdersCount = deliveredOrderCodes.Count,
                            totalOrderData = orderData.Count(),
                            genderStatsCount = genderStats.Count,
                            maleProductsCount = maleProducts.Count,
                            femaleProductsCount = femaleProducts.Count,
                            startDate = startDate?.ToString("yyyy-MM-dd"),
                            endDate = endDate?.ToString("yyyy-MM-dd"),
                            sampleOrderData = orderData.Take(3).ToList(),
                            sampleGenderStats = genderStats.Take(3).ToList()
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // Thống kê WishList
        [HttpPost]
        public IActionResult GetWishlistStats()
        {
            try
            {
                var wishlistData = from w in _dataContext.WishLists
                                   join p in _dataContext.Products on w.ProductId equals p.Id
                                   join b in _dataContext.Brands on p.BrandId equals b.Id
                                   join c in _dataContext.Categories on p.CategoryId equals c.Id
                                   join u in _dataContext.Users on w.UserId equals u.Id
                                   select new
                                   {
                                       ProductId = p.Id,
                                       ProductName = p.Name,
                                       BrandName = b.Name,
                                       CategoryName = c.Name,
                                       Price = p.Price,
                                       UserId = w.UserId,
                                       Gender = u.Gender,
                                       Age = u.Age
                                   };

                // Top sản phẩm được yêu thích nhất
                var topWishlistProducts = wishlistData
                    .GroupBy(x => new { x.ProductId, x.ProductName, x.BrandName, x.CategoryName, x.Price })
                    .Select(g => new WishListProductModel
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
                    .Select(g => new WishListGenderModel
                    {
                        Gender = g.Key ?? "Không xác định",
                        Count = g.Count()
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    topWishlistProducts = topWishlistProducts,
                    wishlistByGender = wishlistByGender
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Thống kê mua hàng theo giới tính
        [HttpPost]
        public IActionResult GetPurchaseStatsByGender()
        {
            try
            {
                var purchaseData = from od in _dataContext.OrderDetails
                                 join o in _dataContext.Orders on od.OrderCode equals o.OrderCode
                                 join p in _dataContext.Products on od.ProductId equals p.Id
                                 join b in _dataContext.Brands on p.BrandId equals b.Id
                                 join u in _dataContext.Users on o.UserId equals u.Id
                                 // Tính tất cả orders, không chỉ Status = 0
                                 select new
                                 {
                                     ProductId = p.Id,
                                     ProductName = p.Name,
                                     BrandName = b.Name,
                                     Price = od.Price,
                                     Quantity = od.Quantity,
                                     TotalAmount = od.Price * od.Quantity,
                                     Gender = u.Gender,
                                     Age = u.Age,
                                     PurchaseDate = o.CreatedDate
                                 };

                // Thống kê theo giới tính
                var statsByGender = purchaseData
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

                return Json(new
                {
                    success = true,
                    data = statsByGender
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        // Thống kê mua hàng theo độ tuổi
        [HttpPost]
        public IActionResult GetPurchaseStatsByAge(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var purchaseData = from od in _dataContext.OrderDetails
                                 join o in _dataContext.Orders on od.OrderCode equals o.OrderCode
                                 join p in _dataContext.Products on od.ProductId equals p.Id
                                 join b in _dataContext.Brands on p.BrandId equals b.Id
                                 join u in _dataContext.Users on o.UserName equals u.Email // SỬ DỤNG EMAIL
                                 where u.Age.HasValue && o.Status == 0 // Chỉ tính users có tuổi và đơn hàng đã giao
                                 && (startDate == null || (o.DeliveredDate.HasValue ? o.DeliveredDate.Value.Date >= startDate.Value.Date : o.CreatedDate.Date >= startDate.Value.Date))
                                 && (endDate == null || (o.DeliveredDate.HasValue ? o.DeliveredDate.Value.Date <= endDate.Value.Date : o.CreatedDate.Date <= endDate.Value.Date))
                                 select new
                                 {
                                     ProductId = p.Id,
                                     ProductName = p.Name,
                                     BrandName = b.Name,
                                     Price = od.Price,
                                     Quantity = od.Quantity,
                                     TotalAmount = od.Price * od.Quantity,
                                     Age = u.Age.Value,
                                     PurchaseDate = o.CreatedDate
                                 };

                // Thống kê theo nhóm tuổi
                var statsByAgeGroup = purchaseData
                    .ToList() // Chuyển sang client evaluation
                    .Select(x => new
                    {
                        x.ProductId,
                        x.ProductName,
                        x.BrandName,
                        x.Price,
                        x.Quantity,
                        x.TotalAmount,
                        x.Age,
                        AgeGroup = GetAgeGroup(x.Age), // Tính toán AgeGroup ở client
                        x.PurchaseDate
                    })
                    .GroupBy(x => x.AgeGroup)
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

                return Json(new
                {
                    success = true,
                    data = statsByAgeGroup,
                    debug = new
                    {
                        totalRecords = purchaseData.Count(),
                        filteredRecords = statsByAgeGroup.Count(),
                        startDate = startDate,
                        endDate = endDate,
                        sampleData = statsByAgeGroup.Take(3).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        // Thống kê trạng thái đơn hàng - API MỚI
        [HttpPost]
        public IActionResult GetOrderStatusStats(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // 1. LẤY TẤT CẢ ĐƠN HÀNG
                var allOrders = _dataContext.Orders.AsQueryable();
                
                // 2. ÁP DỤNG FILTER THỜI GIAN NẾU CÓ
                if (startDate.HasValue && endDate.HasValue)
                {
                    allOrders = allOrders.Where(o => 
                        o.CreatedDate.Date >= startDate.Value.Date && 
                        o.CreatedDate.Date <= endDate.Value.Date);
                }
                
                var ordersList = allOrders.ToList();
                
                // 3. THỐNG KÊ THEO STATUS
                var statusStats = ordersList
                    .GroupBy(o => o.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        StatusName = GetStatusName(g.Key)
                    })
                    .OrderBy(s => s.Status)
                    .ToList();
                
                // 4. TẠO DỮ LIỆU CHO CHART (5 trạng thái: 0,1,2,3,4)
                var chartData = new int[5]; // [0,1,2,3,4]
                
                foreach (var stat in statusStats)
                {
                    if (stat.Status >= 0 && stat.Status <= 4)
                    {
                        chartData[stat.Status] = stat.Count;
                    }
                }
                
                // 5. DEBUG INFORMATION
                var debugInfo = new
                {
                    totalOrders = ordersList.Count,
                    startDate = startDate?.ToString("yyyy-MM-dd"),
                    endDate = endDate?.ToString("yyyy-MM-dd"),
                    statusStats = statusStats,
                    chartData = chartData,
                    message = $"Tìm thấy {ordersList.Count} đơn hàng với {statusStats.Count} trạng thái khác nhau"
                };
                
                return Json(new
                {
                    success = true,
                    data = chartData,
                    labels = new[] { "Đã giao hàng", "Đơn hàng mới", "Chờ xác nhận", "Đã hủy", "Đang giao hàng" },
                    debug = debugInfo
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }




        // Thống kê hiệu suất theo thương hiệu - DỮ LIỆU THẬT
        [HttpPost]
        public IActionResult GetBrandPerformanceStats(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Lấy đơn hàng đã giao
                var deliveredOrders = _dataContext.Orders
                    .Where(o => o.Status == 0)
                    .ToList()
                    .Where(o => 
                        (startDate == null || endDate == null) ||
                        ((o.DeliveredDate.HasValue && 
                          o.DeliveredDate.Value.Date >= startDate.Value.Date && 
                          o.DeliveredDate.Value.Date <= endDate.Value.Date) ||
                         (!o.DeliveredDate.HasValue && 
                          o.CreatedDate.Date >= startDate.Value.Date && 
                          o.CreatedDate.Date <= endDate.Value.Date)))
                    .ToList();

                var orderCodes = deliveredOrders.Select(o => o.OrderCode).ToList();

                // Thống kê theo thương hiệu
                var brandStats = _dataContext.OrderDetails
                    .Where(od => orderCodes.Contains(od.OrderCode))
                    .Join(_dataContext.Products, od => od.ProductId, p => p.Id, (od, p) => new { od, p })
                    .Join(_dataContext.Brands, x => x.p.BrandId, b => b.Id, (x, b) => new { x.od, x.p, b })
                    .GroupBy(x => new { x.b.Id, x.b.Name })
                    .Select(g => new BrandPerformanceModel
                    {
                        BrandId = g.Key.Id,
                        BrandName = g.Key.Name,
                        TotalQuantitySold = g.Sum(x => x.od.Quantity)
                    })
                    .OrderByDescending(x => x.TotalQuantitySold)
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = brandStats,
                    debug = new
                    {
                        deliveredOrdersCount = deliveredOrders.Count,
                        orderCodesCount = orderCodes.Count,
                        brandStatsCount = brandStats.Count,
                        sampleBrandStats = brandStats.Take(3).Select(b => new
                        {
                            BrandId = b.BrandId,
                            BrandName = b.BrandName,
                            TotalQuantitySold = b.TotalQuantitySold
                        }).ToList(),
                        message = $"Found {brandStats.Count} brands from {deliveredOrders.Count} delivered orders"
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Thống kê phân bố theo danh mục - API MỚI
        [HttpPost]
        public IActionResult GetCategoryDistributionStats(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // 1. LẤY ĐƠN HÀNG ĐÃ GIAO TRONG KHOẢNG THỜI GIAN
                var deliveredOrders = _dataContext.Orders
                    .Where(o => o.Status == 0)
                    .ToList()
                    .Where(o => 
                        (startDate == null || endDate == null) ||
                        ((o.DeliveredDate.HasValue && 
                          o.DeliveredDate.Value.Date >= startDate.Value.Date && 
                          o.DeliveredDate.Value.Date <= endDate.Value.Date) ||
                         (!o.DeliveredDate.HasValue && 
                          o.CreatedDate.Date >= startDate.Value.Date && 
                          o.CreatedDate.Date <= endDate.Value.Date)))
                    .ToList();

                var orderCodes = deliveredOrders.Select(o => o.OrderCode).ToList();

                // 2. THỐNG KÊ THEO DANH MỤC TỪ ORDERDETAILS
                var categoryStats = _dataContext.OrderDetails
                    .Where(od => orderCodes.Contains(od.OrderCode))
                    .Join(_dataContext.Products, od => od.ProductId, p => p.Id, (od, p) => new { od, p })
                    .Join(_dataContext.Categories, x => x.p.CategoryId, c => c.Id, (x, c) => new { x.od, x.p, c })
                    .GroupBy(x => new { x.c.Id, x.c.Name })
                    .Select(g => new CategoryDistributionModel
                    {
                        CategoryId = g.Key.Id,
                        CategoryName = g.Key.Name,
                        TotalQuantitySold = g.Sum(x => x.od.Quantity),
                        TotalSales = g.Sum(x => x.od.Price * x.od.Quantity)
                    })
                    .OrderByDescending(x => x.TotalQuantitySold)
                    .ToList();

                // 3. DEBUG INFORMATION
                var debugInfo = new
                {
                    totalDeliveredOrders = deliveredOrders.Count,
                    orderCodesCount = orderCodes.Count,
                    categoryStatsCount = categoryStats.Count,
                    startDate = startDate?.ToString("yyyy-MM-dd"),
                    endDate = endDate?.ToString("yyyy-MM-dd"),
                    message = $"Tìm thấy {categoryStats.Count} danh mục từ {deliveredOrders.Count} đơn hàng đã giao"
                };

                return Json(new
                {
                    success = true,
                    data = categoryStats,
                    debug = debugInfo
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // Thống kê phân tích khoảng giá - API MỚI
        [HttpPost]
        public IActionResult GetPriceRangeStats(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // 1. LẤY ĐƠN HÀNG ĐÃ GIAO TRONG KHOẢNG THỜI GIAN
                var deliveredOrders = _dataContext.Orders
                    .Where(o => o.Status == 0)
                    .ToList()
                    .Where(o => 
                        (startDate == null || endDate == null) ||
                        ((o.DeliveredDate.HasValue && 
                          o.DeliveredDate.Value.Date >= startDate.Value.Date && 
                          o.DeliveredDate.Value.Date <= endDate.Value.Date) ||
                         (!o.DeliveredDate.HasValue && 
                          o.CreatedDate.Date >= startDate.Value.Date && 
                          o.CreatedDate.Date <= endDate.Value.Date)))
                    .ToList();

                var orderCodes = deliveredOrders.Select(o => o.OrderCode).ToList();

                // 2. THỐNG KÊ THEO KHOẢNG GIÁ TỪ ORDERDETAILS
                var orderDetails = _dataContext.OrderDetails
                    .Where(od => orderCodes.Contains(od.OrderCode))
                    .ToList();

                // 3. PHÂN LOẠI THEO KHOẢNG GIÁ
                var priceRanges = new[]
                {
                    new { Range = "< 50M", Min = 0m, Max = 50000000m },
                    new { Range = "50M-100M", Min = 50000000m, Max = 100000000m },
                    new { Range = "100M-200M", Min = 100000000m, Max = 200000000m },
                    new { Range = "200M-500M", Min = 200000000m, Max = 500000000m },
                    new { Range = "> 500M", Min = 500000000m, Max = decimal.MaxValue }
                };

                var priceStats = priceRanges.Select(pr => new PriceRangeModel
                {
                    Range = pr.Range,
                    Count = orderDetails.Count(od => od.Price >= pr.Min && od.Price < pr.Max),
                    TotalQuantitySold = orderDetails.Where(od => od.Price >= pr.Min && od.Price < pr.Max)
                                                    .Sum(od => od.Quantity),
                    TotalRevenue = orderDetails.Where(od => od.Price >= pr.Min && od.Price < pr.Max)
                                              .Sum(od => od.Price * od.Quantity)
                }).ToList();

                // 4. DEBUG INFORMATION
                var debugInfo = new
                {
                    totalDeliveredOrders = deliveredOrders.Count,
                    orderCodesCount = orderCodes.Count,
                    orderDetailsCount = orderDetails.Count,
                    priceStatsCount = priceStats.Count,
                    startDate = startDate?.ToString("yyyy-MM-dd"),
                    endDate = endDate?.ToString("yyyy-MM-dd"),
                    message = $"Tìm thấy {orderDetails.Count} sản phẩm từ {deliveredOrders.Count} đơn hàng đã giao"
                };

                return Json(new
                {
                    success = true,
                    data = priceStats,
                    debug = debugInfo
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // Thống kê xu hướng bán hàng - VIẾT LẠI HOÀN TOÀN
        [HttpPost]
        public IActionResult GetSalesTrendData(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                // 1. SETUP THỜI GIAN
                var start = startDate ?? DateTime.Now.AddDays(-30);
                var end = endDate ?? DateTime.Now;
                
                // 2. LẤY ĐƠN HÀNG ĐÃ GIAO TRONG KHOẢNG THỜI GIAN
                var deliveredOrders = _dataContext.Orders
                    .Where(o => o.Status == 0)
                    .ToList()
                    .Where(o => 
                        (o.DeliveredDate.HasValue && 
                         o.DeliveredDate.Value.Date >= start.Date && 
                         o.DeliveredDate.Value.Date <= end.Date) ||
                        (!o.DeliveredDate.HasValue && 
                         o.CreatedDate.Date >= start.Date && 
                         o.CreatedDate.Date <= end.Date))
                    .ToList();

                // 3. NHÓM THEO NGÀY VÀ TÍNH DOANH THU
                var dailyData = deliveredOrders
                    .GroupBy(o => o.DeliveredDate?.Date ?? o.CreatedDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        Orders = g.ToList()
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                // 4. TÍNH DOANH THU CHO TỪNG NGÀY
                var result = new List<object>();
                
                foreach (var day in dailyData)
                {
                    var orderCodes = day.Orders.Select(o => o.OrderCode).ToList();
                    var dayRevenue = _dataContext.OrderDetails
                        .Where(od => orderCodes.Contains(od.OrderCode))
                        .Sum(od => od.Price * od.Quantity);
                    
                    result.Add(new
                    {
                        Date = day.Date,
                        Revenue = dayRevenue,
                        OrderCount = day.Orders.Count
                    });
                }

                // 5. DEBUG INFORMATION
                var debugInfo = new
                {
                    startDate = start.ToString("yyyy-MM-dd"),
                    endDate = end.ToString("yyyy-MM-dd"),
                    totalDeliveredOrders = deliveredOrders.Count,
                    dailyDataCount = dailyData.Count,
                    sampleOrders = deliveredOrders.Take(3).Select(o => new
                    {
                        o.OrderCode,
                        o.CreatedDate,
                        o.DeliveredDate,
                        DateUsed = o.DeliveredDate?.Date ?? o.CreatedDate.Date,
                        DateType = o.DeliveredDate.HasValue ? "DeliveredDate" : "CreatedDate"
                    }).ToList(),
                    message = $"Tìm thấy {deliveredOrders.Count} đơn hàng đã giao trong {dailyData.Count} ngày"
                };

                return Json(new
                {
                    success = true,
                    dates = result.Select(r => ((dynamic)r).Date),
                    revenues = result.Select(r => ((dynamic)r).Revenue),
                    debug = debugInfo
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }


        // Helper methods
        private string GetPriceRange(decimal price)
        {
            if (price < 50000000) return "< 50M";
            if (price < 100000000) return "50M-100M";
            if (price < 200000000) return "100M-200M";
            if (price < 500000000) return "200M-500M";
            return "> 500M";
        }

        private string GetAgeGroup(int age)
        {
            if (age < 18) return "Dưới 18";
            if (age < 25) return "18-24";
            if (age < 35) return "25-34";
            if (age < 45) return "35-44";
            if (age < 55) return "45-54";
            if (age < 65) return "55-64";
            return "Trên 65";
        }
    }

    // View model for order status
    public class OrderStatusViewModel
    {
        public string Status { get; set; }
        public int Count { get; set;         }
    }

    // Model cho Brand Performance
    public class BrandPerformanceModel
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int TotalQuantitySold { get; set; }
    }

    // Model cho Category Distribution
    public class CategoryDistributionModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalSales { get; set; }
    }

    // Model cho Price Range Analysis
    public class PriceRangeModel
    {
        public string Range { get; set; }
        public int Count { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    // Model cho WishList Product
    public class WishListProductModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public int WishlistCount { get; set; }
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }
    }

    // Model cho WishList by Gender
    public class WishListGenderModel
    {
        public string Gender { get; set; }
        public int Count { get; set; }
    }
}