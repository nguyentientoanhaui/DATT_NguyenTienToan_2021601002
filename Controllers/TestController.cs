using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Repository;

namespace Shopping_Demo.Controllers
{
    public class TestController : Controller
    {
        private readonly DataContext _context;

        public TestController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("notifications")]
        public IActionResult TestNotifications()
        {
            return View();
        }

        [HttpGet("database")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                var productCount = await _context.Products.CountAsync();
                var brandCount = await _context.Brands.CountAsync();
                var categoryCount = await _context.Categories.CountAsync();

                var products = await _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Take(5)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Price,
                        Brand = p.Brand != null ? p.Brand.Name : "No Brand",
                        Category = p.Category != null ? p.Category.Name : "No Category"
                    })
                    .ToListAsync();

                return Ok(new
                {
                    ProductCount = productCount,
                    BrandCount = brandCount,
                    CategoryCount = categoryCount,
                    Products = products,
                    Message = "Database connection successful"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = ex.Message,
                    Message = "Database connection failed"
                });
            }
        }
    }
}
