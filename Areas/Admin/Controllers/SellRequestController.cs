using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;
using X.PagedList;
using X.PagedList.Mvc.Core;
using X.PagedList.Extensions;

namespace Shopping_Demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SellRequestController : Controller
    {
        private readonly DataContext _context;

        public SellRequestController(DataContext context)
        {
            _context = context;
        }

        // GET: Admin/SellRequest
        public async Task<IActionResult> Index(int? page, string status, string search)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var query = _context.SellRequests
                .Include(sr => sr.Product)
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<SellRequestStatus>(status, out var statusEnum))
            {
                query = query.Where(sr => sr.Status == statusEnum);
            }

            // Search functionality
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(sr => 
                    sr.FullName.Contains(search) ||
                    sr.Email.Contains(search) ||
                    sr.PhoneNumber.Contains(search) ||
                    sr.Product.Name.Contains(search));
            }

            var sellRequests = query
                .OrderByDescending(sr => sr.CreatedAt)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.Status = status;
            ViewBag.Search = search;
            ViewBag.StatusOptions = Enum.GetValues<SellRequestStatus>()
                .Select(s => new { Value = s.ToString(), Text = GetStatusDisplayName(s) })
                .ToList();

            return View(sellRequests);
        }

        // GET: Admin/SellRequest/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sellRequest = await _context.SellRequests
                .Include(sr => sr.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sellRequest == null)
            {
                return NotFound();
            }

            return View(sellRequest);
        }

        // GET: Admin/SellRequest/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sellRequest = await _context.SellRequests
                .Include(sr => sr.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sellRequest == null)
            {
                return NotFound();
            }

            ViewBag.StatusOptions = Enum.GetValues<SellRequestStatus>()
                .Select(s => new { Value = (int)s, Text = GetStatusDisplayName(s) })
                .ToList();

            return View(sellRequest);
        }

        // POST: Admin/SellRequest/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SellRequestModel sellRequest)
        {
            if (id != sellRequest.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRequest = await _context.SellRequests.FindAsync(id);
                    if (existingRequest == null)
                    {
                        return NotFound();
                    }

                    // Update only the fields that admin can modify
                    existingRequest.Status = sellRequest.Status;
                    existingRequest.AdminResponse = sellRequest.AdminResponse;
                    existingRequest.SuggestedPrice = sellRequest.SuggestedPrice;
                    existingRequest.UpdatedAt = DateTime.Now;

                    _context.Update(existingRequest);
                    await _context.SaveChangesAsync();

                    TempData["success"] = "Cập nhật đơn thu mua thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SellRequestExists(sellRequest.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.StatusOptions = Enum.GetValues<SellRequestStatus>()
                .Select(s => new { Value = (int)s, Text = GetStatusDisplayName(s) })
                .ToList();

            return View(sellRequest);
        }

        // POST: Admin/SellRequest/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, SellRequestStatus status, string adminResponse = null, decimal? suggestedPrice = null)
        {
            try
            {
                var sellRequest = await _context.SellRequests.FindAsync(id);
                if (sellRequest == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn thu mua" });
                }

                sellRequest.Status = status;
                sellRequest.UpdatedAt = DateTime.Now;

                if (!string.IsNullOrEmpty(adminResponse))
                {
                    sellRequest.AdminResponse = adminResponse;
                }

                if (suggestedPrice.HasValue)
                {
                    sellRequest.SuggestedPrice = suggestedPrice;
                }

                _context.Update(sellRequest);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Admin/SellRequest/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sellRequest = await _context.SellRequests
                .Include(sr => sr.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sellRequest == null)
            {
                return NotFound();
            }

            return View(sellRequest);
        }

        // POST: Admin/SellRequest/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sellRequest = await _context.SellRequests.FindAsync(id);
            if (sellRequest != null)
            {
                _context.SellRequests.Remove(sellRequest);
                await _context.SaveChangesAsync();
                TempData["success"] = "Xóa đơn thu mua thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/SellRequest/Statistics
        public async Task<IActionResult> Statistics()
        {
            var statistics = new
            {
                Total = await _context.SellRequests.CountAsync(),
                Pending = await _context.SellRequests.CountAsync(sr => sr.Status == SellRequestStatus.Pending),
                Reviewed = await _context.SellRequests.CountAsync(sr => sr.Status == SellRequestStatus.Reviewed),
                Contacted = await _context.SellRequests.CountAsync(sr => sr.Status == SellRequestStatus.Contacted),
                Agreed = await _context.SellRequests.CountAsync(sr => sr.Status == SellRequestStatus.Agreed),
                Completed = await _context.SellRequests.CountAsync(sr => sr.Status == SellRequestStatus.Completed),
                Rejected = await _context.SellRequests.CountAsync(sr => sr.Status == SellRequestStatus.Rejected),
                Cancelled = await _context.SellRequests.CountAsync(sr => sr.Status == SellRequestStatus.Cancelled),
                ThisMonth = await _context.SellRequests.CountAsync(sr => sr.CreatedAt.Month == DateTime.Now.Month && sr.CreatedAt.Year == DateTime.Now.Year),
                ThisWeek = await _context.SellRequests.CountAsync(sr => sr.CreatedAt >= DateTime.Now.AddDays(-7))
            };

            return Json(statistics);
        }


        private bool SellRequestExists(int id)
        {
            return _context.SellRequests.Any(e => e.Id == id);
        }

        private string GetStatusDisplayName(SellRequestStatus status)
        {
            return status switch
            {
                SellRequestStatus.Pending => "Chờ xử lý",
                SellRequestStatus.Reviewed => "Đã xem",
                SellRequestStatus.Contacted => "Đã liên hệ",
                SellRequestStatus.Agreed => "Đã thỏa thuận",
                SellRequestStatus.Completed => "Đã hoàn thành",
                SellRequestStatus.Rejected => "Từ chối",
                SellRequestStatus.Cancelled => "Hủy",
                _ => status.ToString()
            };
        }
    }
}
