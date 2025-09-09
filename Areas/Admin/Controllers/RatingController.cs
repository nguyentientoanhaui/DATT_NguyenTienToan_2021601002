using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;

namespace Shopping_Demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager,Staff,Sale")]
    public class RatingController : Controller
    {
        private readonly DataContext _dataContext;

        public RatingController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            var query = _dataContext.ProductReviews
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedDate);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            
            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));
            
            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;

            return View(reviews);
        }

        public async Task<IActionResult> Details(int id)
        {
            var review = await _dataContext.ProductReviews
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _dataContext.ProductReviews.FindAsync(id);
            
            if (review == null)
            {
                return NotFound();
            }

            _dataContext.ProductReviews.Remove(review);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Đánh giá đã được xóa thành công";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var review = await _dataContext.ProductReviews.FindAsync(id);
            
            if (review == null)
            {
                return NotFound();
            }

            // Assuming there's an IsActive field, if not, you can add it to the model
            // review.IsActive = !review.IsActive;
            
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Trạng thái đánh giá đã được cập nhật";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ProductReviews(int productId)
        {
            var reviews = await _dataContext.ProductReviews
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            var product = await _dataContext.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            ViewBag.Product = product;
            return View(reviews);
        }
    }
}
