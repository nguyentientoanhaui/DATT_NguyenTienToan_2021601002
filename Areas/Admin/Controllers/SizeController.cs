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

        // DISABLED: This controller is disabled because the Sizes table was removed from the database
        // All methods return empty results or NotFound to prevent errors

        // GET: Admin/Size
        public async Task<IActionResult> Index()
        {
            // var sizes = await _dataContext.Sizes.OrderBy(s => s.Name).ToListAsync();
            var sizes = new List<SizeModel>(); // Empty list since table was removed
            return View(sizes);
        }

        // GET: Admin/Size/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // var size = await _dataContext.Sizes.FirstOrDefaultAsync(s => s.Id == id);
            // if (size == null)
            // {
            //     return NotFound();
            // }
            // return View(size);
            return NotFound(); // Always return not found since table was removed
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
            // if (ModelState.IsValid)
            // {
            //     _dataContext.Add(size);
            //     await _dataContext.SaveChangesAsync();
            //     return RedirectToAction(nameof(Index));
            // }
            // return View(size);
            return RedirectToAction(nameof(Index)); // Always redirect since table was removed
        }

        // GET: Admin/Size/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // var size = await _dataContext.Sizes.FindAsync(id);
            // if (size == null)
            // {
            //     return NotFound();
            // }
            // return View(size);
            return NotFound(); // Always return not found since table was removed
        }

        // POST: Admin/Size/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SizeModel size)
        {
            // if (id != size.Id)
            // {
            //     return NotFound();
            // }

            // if (ModelState.IsValid)
            // {
            //     try
            //     {
            //         _dataContext.Update(size);
            //         await _dataContext.SaveChangesAsync();
            //     }
            //     catch (DbUpdateConcurrencyException)
            //     {
            //         if (!SizeExists(size.Id))
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
            // return View(size);
            return RedirectToAction(nameof(Index)); // Always redirect since table was removed
        }

        // GET: Admin/Size/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            // var size = await _dataContext.Sizes
            //     .FirstOrDefaultAsync(m => m.Id == id);
            // if (size == null)
            // {
            //     return NotFound();
            // }

            // return View(size);
            return NotFound(); // Always return not found since table was removed
        }

        // POST: Admin/Size/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // var size = await _dataContext.Sizes.FindAsync(id);
            // if (size != null)
            // {
            //     _dataContext.Sizes.Remove(size);
            // }

            // await _dataContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); // Always redirect since table was removed
        }

        private bool SizeExists(int id)
        {
            // return _dataContext.Sizes.Any(e => e.Id == id);
            return false; // Always return false since table was removed
        }
    }
}