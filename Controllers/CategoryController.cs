using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;

namespace Shopping_Demo.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;
        public CategoryController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Index(string Slug = "", string sort_by = "", string startprice = "", string endprice = "")
        {
            // Tìm danh mục theo Slug
            CategoryModel category = await _dataContext.Categories
                .Include(c => c.ChildCategories)
                .FirstOrDefaultAsync(c => c.Slug == Slug && c.Status == 1); // Chỉ lấy danh mục có status = 1 (hiển thị)

            if (category == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Slug = Slug;
            ViewBag.CategoryName = category.Name;

            IQueryable<ProductModel> products;

            // Kiểm tra xem đây là danh mục chính hay danh mục phụ
            if (category.Level == 1) // Danh mục chính
            {
                // Lấy tất cả ID của danh mục phụ của danh mục chính này mà có status = 1
                var childCategoryIds = await _dataContext.Categories
                    .Where(c => c.ParentId == category.Id && c.Status == 1)
                    .Select(c => c.Id)
                    .ToListAsync();

                // Lấy sản phẩm từ tất cả danh mục phụ
                products = _dataContext.Products
                    .Where(p => childCategoryIds.Contains(p.CategoryId));

                // Truyền danh sách danh mục phụ cho view (chỉ những danh mục có status = 1)
                ViewBag.SubCategories = await _dataContext.Categories
                    .Where(c => c.ParentId == category.Id && c.Status == 1)
                    .ToListAsync();
            }
            else // Danh mục phụ
            {
                // Chỉ lấy sản phẩm của danh mục phụ này
                products = _dataContext.Products
                    .Where(p => p.CategoryId == category.Id);

                // Truyền thông tin danh mục chính cho view
                var parentCategory = await _dataContext.Categories
                    .FirstOrDefaultAsync(c => c.Id == category.ParentId && c.Status == 1);
                ViewBag.ParentCategory = parentCategory;
            }

            var count = await products.CountAsync();
            if (count > 0)
            {
                // Áp dụng sắp xếp dựa trên tham số sort_by
                if (sort_by == "price_increase")
                {
                    products = products.OrderBy(p => p.Price);
                }
                else if (sort_by == "price_decrease")
                {
                    products = products.OrderByDescending(p => p.Price);
                }
                else if (sort_by == "price_newest")
                {
                    products = products.OrderByDescending(p => p.Id);
                }
                else if (sort_by == "price_oldest")
                {
                    products = products.OrderBy(p => p.Id);
                }
                else if (!string.IsNullOrEmpty(startprice) && !string.IsNullOrEmpty(endprice))
                {
                    if (decimal.TryParse(startprice, out decimal startPriceValue) &&
                        decimal.TryParse(endprice, out decimal endPriceValue))
                    {
                        products = products.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
                    }
                    else
                    {
                        products = products.OrderByDescending(p => p.Id);
                    }
                }
                else
                {
                    products = products.OrderByDescending(p => p.Id);
                }
            }

            // Bao gồm các bảng liên quan
            products = products.Include(p => p.Category)
                              .Include(p => p.Brand);

            if (!products.Any())
            {
                TempData["error"] = "Không tìm thấy sản phẩm thuộc danh mục đã chọn";
                return RedirectToAction("Index", "Home");
            }

            return View(await products.ToListAsync());
        }

        // Phương thức để lấy danh mục phụ dựa trên danh mục chính
        [HttpGet]
        public async Task<IActionResult> GetSubCategories(int mainCategoryId)
        {
            var subCategories = await _dataContext.Categories
                .Where(c => c.ParentId == mainCategoryId && c.Status == 1) // Chỉ lấy danh mục có status = 1
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug
                })
                .ToListAsync();

            return Json(subCategories);
        }

        // Phương thức để lấy tất cả danh mục cho menu
        [HttpGet]
        public async Task<IActionResult> GetCategoryMenu()
        {
            var categories = await _dataContext.Categories
                .Where(c => c.Status == 1) // Chỉ lấy danh mục có status = 1
                .OrderBy(c => c.Level)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return PartialView("_CategoryMenu", categories);
        }
    }
}