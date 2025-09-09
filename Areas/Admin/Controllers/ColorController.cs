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

        // DISABLED: This controller is disabled because the Colors table was removed from the database
        // All methods return empty results or NotFound to prevent errors

        // GET: Admin/Color
        public async Task<IActionResult> Index()
        {
            // var colors = await _dataContext.Colors.OrderBy(c => c.Name).ToListAsync();
            var colors = new List<ColorModel>(); // Empty list since table was removed
            return View(colors);
        }

        // GET: Admin/Color/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // var color = await _dataContext.Colors.FirstOrDefaultAsync(c => c.Id == id);
            // if (color == null)
            // {
            //     return NotFound();
            // }
            // return View(color);
            return NotFound(); // Always return not found since table was removed
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
            // if (ModelState.IsValid)
            // {
            //     _dataContext.Add(color);
            //     await _dataContext.SaveChangesAsync();
            //     return RedirectToAction(nameof(Index));
            // }
            // return View(color);
            return RedirectToAction(nameof(Index)); // Always redirect since table was removed
        }

        // GET: Admin/Color/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // var color = await _dataContext.Colors.FindAsync(id);
            // if (color == null)
            // {
            //     return NotFound();
            // }
            // return View(color);
            return NotFound(); // Always return not found since table was removed
        }

        // POST: Admin/Color/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ColorModel color)
        {
            // if (id != color.Id)
            // {
            //     return NotFound();
            // }

            // if (ModelState.IsValid)
            // {
            //     try
            //     {
            //         _dataContext.Update(color);
            //         await _dataContext.SaveChangesAsync();
            //     }
            //     catch (DbUpdateConcurrencyException)
            //     {
            //         if (!ColorExists(color.Id))
            //         {
            //             return NotFound();
            //         }
            //         else
            //         {
            //             throw;
            //         }
            //     }
            //     return RedirectToAction(nameof(Index));
            // }
            // return View(color);
            return RedirectToAction(nameof(Index)); // Always redirect since table was removed
        }

        // GET: Admin/Color/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            // var color = await _dataContext.Colors
            //     .FirstOrDefaultAsync(m => m.Id == id);
            // if (color == null)
            // {
            //     return NotFound();
            // }

            // return View(color);
            return NotFound(); // Always return not found since table was removed
        }

        // POST: Admin/Color/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // var color = await _dataContext.Colors.FindAsync(id);
            // if (color != null)
            // {
            //     _dataContext.Colors.Remove(color);
            // }

            // await _dataContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); // Always redirect since table was removed
        }

        private bool ColorExists(int id)
        {
            // return _dataContext.Colors.Any(e => e.Id == id);
            return false; // Always return false since table was removed
        }
    }
}