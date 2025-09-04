using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;

namespace Shopping_Demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager,Staff,Sale")]
    public class ColorController : Controller
    {
        private readonly DataContext _dataContext;

        public ColorController(DataContext context)
        {
            _dataContext = context;
        }

        // GET: Admin/Color
        public async Task<IActionResult> Index()
        {
            var colors = await _dataContext.Colors.OrderBy(c => c.Name).ToListAsync();
            return View(colors);
        }

        // GET: Admin/Color/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var color = await _dataContext.Colors.FirstOrDefaultAsync(c => c.Id == id);

            if (color == null)
            {
                return NotFound();
            }

            return View(color);
        }

        // GET: Admin/Color/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Color/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ColorModel color)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra tên màu đã tồn tại chưa
                var existingColor = await _dataContext.Colors.FirstOrDefaultAsync(c => c.Name == color.Name);
                if (existingColor != null)
                {
                    ModelState.AddModelError("Name", "Tên màu sắc này đã tồn tại");
                    return View(color);
                }

                _dataContext.Add(color);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm màu sắc thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(color);
        }

        // GET: Admin/Color/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var color = await _dataContext.Colors.FindAsync(id);

            if (color == null)
            {
                return NotFound();
            }

            return View(color);
        }

        // POST: Admin/Color/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ColorModel color)
        {
            if (id != color.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra tên màu đã tồn tại chưa (ngoại trừ màu hiện tại)
                    var existingColor = await _dataContext.Colors
                        .FirstOrDefaultAsync(c => c.Name == color.Name && c.Id != id);

                    if (existingColor != null)
                    {
                        ModelState.AddModelError("Name", "Tên màu sắc này đã tồn tại");
                        return View(color);
                    }

                    _dataContext.Update(color);
                    await _dataContext.SaveChangesAsync();
                    TempData["success"] = "Cập nhật màu sắc thành công";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ColorExists(color.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(color);
        }

        // GET: Admin/Color/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var color = await _dataContext.Colors.FindAsync(id);

            if (color == null)
            {
                return NotFound();
            }

            // Kiểm tra xem màu này có đang được sử dụng không
            var productColors = await _dataContext.ProductColors.CountAsync(pc => pc.ColorId == id);

            if (productColors > 0)
            {
                TempData["error"] = "Không thể xóa màu sắc này vì đang được sử dụng cho " + productColors + " sản phẩm";
                return RedirectToAction(nameof(Index));
            }

            _dataContext.Colors.Remove(color);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Xóa màu sắc thành công";

            return RedirectToAction(nameof(Index));
        }

        private bool ColorExists(int id)
        {
            return _dataContext.Colors.Any(e => e.Id == id);
        }
    }
}