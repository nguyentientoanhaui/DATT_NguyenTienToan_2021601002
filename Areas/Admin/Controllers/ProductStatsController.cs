using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shopping_Demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager,Staff,Sale,Shipper")]
    public class ProductStatsController : Controller
    {
        private readonly DataContext _dataContext;

        public ProductStatsController(DataContext context)
        {
            _dataContext = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // SalesStats - Real data metrics
        [HttpPost]
        public IActionResult GetLuxuryWatchSalesStats()
        {
            try
            {
                // Only count delivered orders (Status = 0)
                var deliveredOrders = _dataContext.Orders.Where(o => o.Status == 0);

                // Revenue and sold items from delivered order details
                var deliveredLines = from od in _dataContext.OrderDetails
                                     join o in deliveredOrders on od.OrderCode equals o.OrderCode
                                     select new { od.Price, od.Quantity, o.CreatedDate, od.ProductId };

                var totalRevenue = deliveredLines.Any()
                    ? deliveredLines.Sum(x => x.Price * x.Quantity)
                    : 0m;

                var totalOrders = deliveredOrders.Count();
                var totalWatchesSold = deliveredLines.Any() ? deliveredLines.Sum(x => x.Quantity) : 0;
                var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0m;

                // Brand performance
                var brandPerformance = (from od in _dataContext.OrderDetails
                                        join o in _dataContext.Orders on od.OrderCode equals o.OrderCode
                                        join p in _dataContext.Products on od.ProductId equals p.Id
                                        join b in _dataContext.Brands on p.BrandId equals b.Id
                                        where o.Status == 0
                                        group new { od, p, b } by b.Name into g
                                        select new
                                        {
                                            name = g.Key,
                                            sales = g.Sum(x => x.od.Quantity),
                                            revenue = g.Sum(x => x.od.Price * x.od.Quantity)
                                        })
                                        .OrderByDescending(x => x.revenue)
                                        .Take(10)
                                        .ToList();

                // Category distribution
                var categoryDistribution = (from od in _dataContext.OrderDetails
                                            join o in _dataContext.Orders on od.OrderCode equals o.OrderCode
                                            join p in _dataContext.Products on od.ProductId equals p.Id
                                            join c in _dataContext.Categories on p.CategoryId equals c.Id
                                            where o.Status == 0
                                            group new { od, p, c } by c.Name into g
                                            select new
                                            {
                                                name = g.Key,
                                                sales = g.Sum(x => x.od.Quantity)
                                            })
                                            .OrderByDescending(x => x.sales)
                                            .Take(15)
                                            .ToList();

                // Price range distribution by sold quantity
                var priceRanges = new[]
                {
                    new { Label = "< 50M", Min = 0m, Max = (decimal?)50000000m },
                    new { Label = "50M-100M", Min = 50000000m, Max = (decimal?)100000000m },
                    new { Label = "100M-200M", Min = 100000000m, Max = (decimal?)200000000m },
                    new { Label = "200M-500M", Min = 200000000m, Max = (decimal?)500000000m },
                    new { Label = "> 500M", Min = 500000000m, Max = (decimal?)null }
                };

                var priceRangeData = priceRanges.Select(r => new
                {
                    range = r.Label,
                    count = _dataContext.OrderDetails
                        .Join(_dataContext.Orders, od => od.OrderCode, o => o.OrderCode, (od, o) => new { od, o })
                        .Where(x => x.o.Status == 0 && (r.Max == null ? x.od.Price >= r.Min : x.od.Price >= r.Min && x.od.Price < r.Max))
                        .Select(x => x.od.Quantity)
                        .DefaultIfEmpty(0)
                        .Sum()
                }).ToList();

                return Json(new
                {
                    success = true,
                    metrics = new
                    {
                        totalRevenue,
                        totalOrders,
                        totalWatchesSold,
                        avgOrderValue
                    },
                    brandPerformance,
                    categoryDistribution,
                    priceRangeData
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult GetSalesTrendData(string startDate, string endDate)
        {
            try
            {
                if (!DateTime.TryParse(startDate, out DateTime start) || !DateTime.TryParse(endDate, out DateTime end))
                {
                    return Json(new { success = false, message = "Invalid date format" });
                }

                // Normalize time range to dates (inclusive)
                start = start.Date;
                end = end.Date.AddDays(1).AddTicks(-1);

                // Revenue and watches per day from delivered orders
                var revenueWatchesByDay = (from o in _dataContext.Orders
                                           join od in _dataContext.OrderDetails on o.OrderCode equals od.OrderCode
                                           where o.Status == 0 && o.CreatedDate >= start && o.CreatedDate <= end
                                           group new { o, od } by o.CreatedDate.Date into g
                                           select new
                                           {
                                               date = g.Key,
                                               revenue = g.Sum(x => x.od.Price * x.od.Quantity),
                                               watches = g.Sum(x => x.od.Quantity)
                                           })
                                           .ToList();

                // Orders count per day (delivered)
                var ordersByDay = _dataContext.Orders
                    .Where(o => o.Status == 0 && o.CreatedDate >= start && o.CreatedDate <= end)
                    .GroupBy(o => o.CreatedDate.Date)
                    .Select(g => new { date = g.Key, count = g.Count() })
                    .ToList();

                // Merge results by date
                var allDates = new SortedSet<DateTime>(revenueWatchesByDay.Select(x => x.date).Concat(ordersByDay.Select(x => x.date)));
                var dates = new List<string>();
                var revenues = new List<decimal>();
                var orders = new List<int>();
                var watches = new List<int>();

                foreach (var d in allDates.OrderBy(x => x))
                {
                    dates.Add(d.ToString("yyyy-MM-dd"));
                    var rw = revenueWatchesByDay.FirstOrDefault(x => x.date == d);
                    var oc = ordersByDay.FirstOrDefault(x => x.date == d);
                    revenues.Add(rw?.revenue ?? 0m);
                    orders.Add(oc?.count ?? 0);
                    watches.Add(rw?.watches ?? 0);
                }

                return Json(new { success = true, dates, revenues, orders, watches });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult GetTopSellingWatches()
        {
            try
            {
                var topWatches = _dataContext.Products
                    .Where(p => p.Sold > 0)
                    .Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        brand = _dataContext.Brands
                            .Where(b => b.Id == p.BrandId)
                            .Select(b => b.Name)
                            .FirstOrDefault(),
                        sold = p.Sold,
                        revenue = p.Sold * p.Price,
                        price = p.Price
                    })
                    .OrderByDescending(p => p.sold)
                    .Take(10)
                    .ToList();

                return Json(new { success = true, data = topWatches });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult GetWatchConditionAnalysis()
        {
            try
            {
                var conditionAnalysis = _dataContext.Products
                    .GroupBy(p => p.Condition)
                    .Select(g => new
                    {
                        condition = g.Key,
                        count = g.Count(),
                        totalSold = g.Sum(p => p.Sold),
                        totalRevenue = g.Sum(p => p.Sold * p.Price)
                    })
                    .OrderByDescending(x => x.totalRevenue)
                    .ToList();

                return Json(new { success = true, data = conditionAnalysis });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult GetGenderBasedSales()
        {
            try
            {
                var genderSales = _dataContext.Products
                    .GroupBy(p => p.Gender)
                    .Select(g => new
                    {
                        gender = g.Key,
                        count = g.Count(),
                        totalSold = g.Sum(p => p.Sold),
                        totalRevenue = g.Sum(p => p.Sold * p.Price),
                        avgPrice = g.Average(p => p.Price)
                    })
                    .OrderByDescending(x => x.totalRevenue)
                    .ToList();

                return Json(new { success = true, data = genderSales });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult GetInventoryDistribution()
        {
            // Get inventory distribution by category
            var categoryData = _dataContext.Categories
                .Select(c => new
                {
                    name = c.Name,
                    quantity = _dataContext.Products
                        .Where(p => p.CategoryId == c.Id)
                        .Sum(p => p.Quantity)
                })
                .OrderByDescending(c => c.quantity)
                .ToList();

            // Calculate total inventory quantity and value
            var totalQuantity = _dataContext.Products.Sum(p => p.Quantity);
            var totalValue = _dataContext.Products.Sum(p => p.Quantity * p.CapitalPrice);

            return Json(new
            {
                categories = categoryData,
                totalQuantity = totalQuantity,
                totalValue = totalValue
            });
        }

        [HttpPost]
        public IActionResult GetDetailedInventoryByCategory()
        {
            // Get detailed inventory distribution by category
            var data = _dataContext.Categories
                .Select(c => new
                {
                    name = c.Name,
                    quantity = _dataContext.Products
                        .Where(p => p.CategoryId == c.Id)
                        .Sum(p => p.Quantity),
                    value = _dataContext.Products
                        .Where(p => p.CategoryId == c.Id)
                        .Sum(p => p.Quantity * p.CapitalPrice)
                })
                .OrderByDescending(c => c.quantity)
                .ToList();

            return Json(data);
        }

        [HttpPost]
        public IActionResult GetDetailedInventoryByBrand()
        {
            // Get detailed inventory distribution by brand
            var data = _dataContext.Brands
                .Select(b => new
                {
                    name = b.Name,
                    quantity = _dataContext.Products
                        .Where(p => p.BrandId == b.Id)
                        .Sum(p => p.Quantity),
                    value = _dataContext.Products
                        .Where(p => p.BrandId == b.Id)
                        .Sum(p => p.Quantity * p.CapitalPrice)
                })
                .OrderByDescending(b => b.quantity)
                .ToList();

            return Json(data);
        }

        [HttpPost]
        public IActionResult GetLowStockProducts()
        {
            // Get products with low stock (less than 10 units)
            var lowStockProducts = _dataContext.Products
                .Where(p => p.Quantity < 10)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    quantity = p.Quantity,
                    sold = p.Sold,
                    categoryId = p.CategoryId,
                    categoryName = _dataContext.Categories
                        .Where(c => c.Id == p.CategoryId)
                        .Select(c => c.Name)
                        .FirstOrDefault()
                })
                .OrderBy(p => p.quantity)
                .ToList();

            return Json(lowStockProducts);
        }

        [HttpPost]
        public IActionResult GetTopSellingProducts()
        {
            // Get top 5 selling products by quantity
            var topProducts = _dataContext.Products
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    sold = p.Sold,
                    revenue = p.Sold * p.Price,
                    profit = p.Sold * (p.Price - p.CapitalPrice)
                })
                .Where(p => p.sold > 0)
                .OrderByDescending(p => p.sold)
                .Take(5)
                .ToList();

            return Json(topProducts);
        }

        [HttpPost]
        public IActionResult GetWorstSellingProducts()
        {
            // Get 5 worst selling products (with stock) by quantity
            var worstProducts = _dataContext.Products
                .Where(p => p.Quantity > 0)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    sold = p.Sold,
                    inventory = p.Quantity
                    // Note: Removed lastUpdate as it's not in your model
                })
                .OrderBy(p => p.sold)
                .Take(5)
                .ToList();

            return Json(worstProducts);
        }

        [HttpPost]
        public IActionResult GetHighProfitMarginProducts()
        {
            // Get top 5 products with highest profit margin
            var highProfitProducts = _dataContext.Products
                .Where(p => p.Price > 0 && p.CapitalPrice > 0)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price,
                    cost = p.CapitalPrice,
                    profitMargin = (p.Price - p.CapitalPrice) / p.Price, // Calculate profit margin as percentage
                    sold = p.Sold
                })
                .OrderByDescending(p => p.profitMargin)
                .Take(5)
                .ToList();

            return Json(highProfitProducts);
        }

        [HttpPost]
        public IActionResult GetAllProfitMarginProducts()
        {
            // Get all products with profit margin data
            var allProfitProducts = _dataContext.Products
                .Where(p => p.Price > 0 && p.CapitalPrice > 0)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    category = _dataContext.Categories
                        .Where(c => c.Id == p.CategoryId)
                        .Select(c => c.Name)
                        .FirstOrDefault(),
                    price = p.Price,
                    cost = p.CapitalPrice,
                    profitMargin = (p.Price - p.CapitalPrice) / p.Price,
                    sold = p.Sold,
                    totalProfit = p.Sold * (p.Price - p.CapitalPrice)
                })
                .OrderByDescending(p => p.profitMargin)
                .ToList();

            return Json(allProfitProducts);
        }

        [HttpPost]
        public IActionResult GetProductPerformanceOverview()
        {
            // Get overall product performance metrics
            var totalProducts = _dataContext.Products.Count();
            var activeProducts = _dataContext.Products.Count(); // Assuming all products are active as IsActive is not in your model
            var totalInventory = _dataContext.Products.Sum(p => p.Quantity);
            var inventoryValue = _dataContext.Products.Sum(p => p.Quantity * p.CapitalPrice);
            var soldItems = _dataContext.Products.Sum(p => p.Sold);
            var totalRevenue = _dataContext.Products.Sum(p => p.Sold * p.Price);

            // Calculate total profit
            var totalProfit = _dataContext.Products.Sum(p => p.Sold * (p.Price - p.CapitalPrice));

            // Calculate average profit margin across all products
            var avgProfitMargin = _dataContext.Products
                .Where(p => p.Price > 0 && p.CapitalPrice > 0)
                .Select(p => (p.Price - p.CapitalPrice) / p.Price)
                .Average();

            return Json(new
            {
                totalProducts = totalProducts,
                activeProducts = activeProducts,
                totalInventory = totalInventory,
                inventoryValue = inventoryValue,
                soldItems = soldItems,
                totalRevenue = totalRevenue,
                totalProfit = totalProfit,
                avgProfitMargin = avgProfitMargin
            });
        }

        [HttpPost]
        public IActionResult GetCategoryPerformance()
        {
            // Get performance metrics by category
            var categoryPerformance = _dataContext.Categories
                .Select(c => new
                {
                    name = c.Name,
                    productCount = _dataContext.Products.Count(p => p.CategoryId == c.Id),
                    inventory = _dataContext.Products
                        .Where(p => p.CategoryId == c.Id)
                        .Sum(p => p.Quantity),
                    sold = _dataContext.Products
                        .Where(p => p.CategoryId == c.Id)
                        .Sum(p => p.Sold),
                    revenue = _dataContext.Products
                        .Where(p => p.CategoryId == c.Id)
                        .Sum(p => p.Sold * p.Price)
                })
                .OrderByDescending(c => c.revenue)
                .ToList();

            return Json(categoryPerformance);
        }

        // Method to get all low stock products for the modal view
        [HttpPost]
        public IActionResult GetAllLowStockProducts()
        {
            // Get all products with low stock (less than 10 units)
            var lowStockProducts = _dataContext.Products
                .Where(p => p.Quantity < 10)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    quantity = p.Quantity,
                    sold = p.Sold,
                    categoryId = p.CategoryId,
                    categoryName = _dataContext.Categories
                        .Where(c => c.Id == p.CategoryId)
                        .Select(c => c.Name)
                        .FirstOrDefault(),
                    price = p.Price,
                    reorderLevel = 5 // Fixed value as it's not in your model
                })
                .OrderBy(p => p.quantity)
                .ToList();

            return Json(lowStockProducts);
        }
    }
}