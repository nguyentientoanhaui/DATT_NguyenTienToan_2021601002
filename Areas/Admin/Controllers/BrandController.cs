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
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        public BrandController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index()
        {
            var brands = await _dataContext.Brands
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(brands);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "-");
                var slug = await _dataContext.Brands.FirstOrDefaultAsync(p => p.Slug == brand.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Thương hiệu đã tồn tại");
                    return View(brand);
                }

                _dataContext.Add(brand);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Modelu có 1 vài phần bị lỗi";
                List<string> errors = new List<string>();
                foreach (var val in ModelState.Values)
                {
                    foreach (var error in val.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
        }
        public async Task<IActionResult> Edit(int Id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(Id);

            return View(brand);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BrandModel brand)
        {
            var existed_brand = _dataContext.Brands.Find(brand.Id);

            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "-");

                existed_brand.Name = brand.Name;
                existed_brand.Description = brand.Description;
                existed_brand.Status = brand.Status;


                _dataContext.Update(existed_brand);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Modelu có 1 vài phần bị lỗi";
                List<string> errors = new List<string>();
                foreach (var val in ModelState.Values)
                {
                    foreach (var error in val.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
        }
        public async Task<IActionResult> Delete(int Id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(Id);

            // Kiểm tra xem thương hiệu có sản phẩm không
            bool hasProducts = await _dataContext.Products.AnyAsync(p => p.BrandId == Id);

            if (hasProducts)
            {
                // Nếu có sản phẩm, chỉ cập nhật trạng thái thành ẩn
                brand.Status = 0; // 0: Ẩn
                _dataContext.Update(brand);
                await _dataContext.SaveChangesAsync();

                TempData["error"] = "Thương hiệu có sản phẩm, đã chuyển trạng thái thành ẩn";
            }
            else
            {
                // Nếu không có sản phẩm, xóa thương hiệu
                _dataContext.Brands.Remove(brand);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thương hiệu đã xóa";
            }

            return RedirectToAction("Index");
        }


    }
}
