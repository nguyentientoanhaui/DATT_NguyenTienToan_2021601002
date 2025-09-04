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

            ViewBag.CountCoupon = _dataContext.Coupons.Count();
            ViewBag.CountBrand = _dataContext.Brands.Count();
            ViewBag.CountSlider = _dataContext.Sliders.Count();

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
    }

    // View model for order status
    public class OrderStatusViewModel
    {
        public string Status { get; set; }
        public int Count { get; set; }
    }
}