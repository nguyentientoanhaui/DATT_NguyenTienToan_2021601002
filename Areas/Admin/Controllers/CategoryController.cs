using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;


namespace Shopping_Demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager,Staff,Sale")]
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;
        public CategoryController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index()
        {
            var categories = await _dataContext.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildCategories)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(categories);
        }

        public IActionResult Create()
        {
            ViewBag.ParentCategories = new SelectList(_dataContext.Categories.Where(c => c.Level == 1), "Id", "Name");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.Replace(" ", "-").ToLower();
                var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Danh mục đã tồn tại");
                    ViewBag.ParentCategories = new SelectList(_dataContext.Categories.Where(c => c.Level == 1), "Id", "Name");
                    return View(category);
                }

                // Xác định level
                if (category.ParentId.HasValue)
                {
                    category.Level = 2; // Danh mục phụ
                }
                else
                {
                    category.Level = 1; // Danh mục chính
                }

                _dataContext.Add(category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model có một vài phần bị lỗi";
                List<string> errors = new List<string>();
                foreach (var val in ModelState.Values)
                {
                    foreach (var error in val.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                ViewBag.ParentCategories = new SelectList(_dataContext.Categories.Where(c => c.Level == 1), "Id", "Name");
                return BadRequest(errorMessage);
            }
        }

        public async Task<IActionResult> Edit(int Id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(Id);
            ViewBag.ParentCategories = new SelectList(_dataContext.Categories.Where(c => c.Level == 1 && c.Id != Id), "Id", "Name");
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryModel category)
        {
            var existed_category = await _dataContext.Categories.FindAsync(category.Id);

            if (ModelState.IsValid)
            {
                category.Slug = category.Name.Replace(" ", "-").ToLower();

                existed_category.Name = category.Name;
                existed_category.Description = category.Description;
                existed_category.Status = category.Status;
                existed_category.ParentId = category.ParentId;

                // Xác định level
                if (category.ParentId.HasValue)
                {
                    existed_category.Level = 2; // Danh mục phụ
                }
                else
                {
                    existed_category.Level = 1; // Danh mục chính
                }

                _dataContext.Update(existed_category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model có một vài phần bị lỗi";
                List<string> errors = new List<string>();
                foreach (var val in ModelState.Values)
                {
                    foreach (var error in val.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                ViewBag.ParentCategories = new SelectList(_dataContext.Categories.Where(c => c.Level == 1 && c.Id != category.Id), "Id", "Name");
                return BadRequest(errorMessage);
            }
        }

        public async Task<IActionResult> Delete(int Id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(Id);

            // Kiểm tra xem danh mục có sản phẩm không
            bool hasProducts = await _dataContext.Products.AnyAsync(p => p.CategoryId == Id);

            // Kiểm tra xem đây có phải là danh mục chính và có danh mục phụ không
            bool hasChildCategories = await _dataContext.Categories.AnyAsync(c => c.ParentId == Id);

            if (hasProducts || hasChildCategories)
            {
                // Nếu có sản phẩm hoặc danh mục phụ, chỉ cập nhật trạng thái thành ẩn
                category.Status = 0; // 0: Ẩn
                _dataContext.Update(category);
                await _dataContext.SaveChangesAsync();

                string message = hasProducts ? "Danh mục có sản phẩm, đã chuyển trạng thái thành ẩn" :
                                "Danh mục có danh mục phụ, đã chuyển trạng thái thành ẩn";
                TempData["error"] = message;
            }
            else
            {
                // Nếu không có sản phẩm và danh mục phụ, xóa danh mục
                _dataContext.Categories.Remove(category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Danh mục đã xóa";
            }

            return RedirectToAction("Index");
        }
    }
}