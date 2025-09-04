using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;

namespace Shopping_Demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager,Staff,Sale")]
    public class SizeController : Controller
    {
        private readonly DataContext _dataContext;

        public SizeController(DataContext context)
        {
            _dataContext = context;
        }

        // GET: Admin/Size
        public async Task<IActionResult> Index()
        {
            var sizes = await _dataContext.Sizes.OrderBy(s => s.Name).ToListAsync();
            return View(sizes);
        }

        // GET: Admin/Size/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var size = await _dataContext.Sizes.FirstOrDefaultAsync(s => s.Id == id);

            if (size == null)
            {
                return NotFound();
            }

            return View(size);
        }

        // GET: Admin/Size/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Size/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SizeModel size)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra tên kích cỡ đã tồn tại chưa
                var existingSize = await _dataContext.Sizes.FirstOrDefaultAsync(s => s.Name == size.Name);
                if (existingSize != null)
                {
                    ModelState.AddModelError("Name", "Tên kích cỡ này đã tồn tại");
                    return View(size);
                }

                _dataContext.Add(size);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm kích cỡ thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(size);
        }

        // GET: Admin/Size/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var size = await _dataContext.Sizes.FindAsync(id);

            if (size == null)
            {
                return NotFound();
            }

            return View(size);
        }

        // POST: Admin/Size/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SizeModel size)
        {
            if (id != size.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra tên kích cỡ đã tồn tại chưa (ngoại trừ kích cỡ hiện tại)
                    var existingSize = await _dataContext.Sizes
                        .FirstOrDefaultAsync(s => s.Name == size.Name && s.Id != id);

                    if (existingSize != null)
                    {
                        ModelState.AddModelError("Name", "Tên kích cỡ này đã tồn tại");
                        return View(size);
                    }

                    _dataContext.Update(size);
                    await _dataContext.SaveChangesAsync();
                    TempData["success"] = "Cập nhật kích cỡ thành công";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SizeExists(size.Id))
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
            return View(size);
        }

        // GET: Admin/Size/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var size = await _dataContext.Sizes.FindAsync(id);

            if (size == null)
            {
                return NotFound();
            }

            // Kiểm tra xem kích cỡ này có đang được sử dụng không
            var productSizes = await _dataContext.ProductSizes.CountAsync(ps => ps.SizeId == id);

            if (productSizes > 0)
            {
                TempData["error"] = "Không thể xóa kích cỡ này vì đang được sử dụng cho " + productSizes + " sản phẩm";
                return RedirectToAction(nameof(Index));
            }

            _dataContext.Sizes.Remove(size);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Xóa kích cỡ thành công";

            return RedirectToAction(nameof(Index));
        }

        private bool SizeExists(int id)
        {
            return _dataContext.Sizes.Any(e => e.Id == id);
        }
    }
}