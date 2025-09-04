using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;

namespace Shopping_Demo.Controllers
{
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        public BrandController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Index(string Slug = "")
        {
            BrandModel brand = _dataContext.Brands
                .Where(c => c.Slug == Slug && c.Status == 1)
                .FirstOrDefault();

            if (brand == null) return RedirectToAction("Index");

            var productByBrand = _dataContext.Products.Where(c => c.BrandId == brand.Id);

            if (!productByBrand.Any())
            {
                TempData["error"] = "Không tìm thấy sản phẩm thuộc thương hiệu đã chọn";
                return RedirectToAction("Index","Home");
            }

            ViewBag.Brand = brand;

            return View(await productByBrand.OrderByDescending(c => c.Id).ToListAsync());
        }

    }
}
