using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;
using System.Security.Claims;

namespace Shopping_Demo.Controllers
{
    public class SellController : Controller
    {
        private readonly DataContext _dataContext;

        public SellController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        // GET: Sell/Create
        public async Task<IActionResult> Create(int productId)
        {
            var product = await _dataContext.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                TempData["error"] = "Không tìm thấy sản phẩm";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Product = product;
            ViewBag.ProductId = productId;

            return View();
        }

        // POST: Sell/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SellRequestModel sellRequest)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thông tin user hiện tại
                    var userId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;
                    var sessionId = HttpContext.Session.Id;

                    sellRequest.UserId = userId;
                    sellRequest.SessionId = sessionId;
                    sellRequest.CreatedAt = DateTime.Now;
                    sellRequest.Status = SellRequestStatus.Pending;

                    _dataContext.SellRequests.Add(sellRequest);
                    await _dataContext.SaveChangesAsync();

                    TempData["success"] = "Yêu cầu thu mua đã được gửi thành công! Chúng tôi sẽ liên hệ với bạn sớm nhất.";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi gửi yêu cầu. Vui lòng thử lại.");
                }
            }

            // Nếu có lỗi, load lại product để hiển thị form
            var product = await _dataContext.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == sellRequest.ProductId);

            ViewBag.Product = product;
            ViewBag.ProductId = sellRequest.ProductId;

            return View(sellRequest);
        }

        // GET: Sell/MyRequests
        public async Task<IActionResult> MyRequests()
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["error"] = "Vui lòng đăng nhập để xem yêu cầu thu mua";
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var requests = await _dataContext.SellRequests
                .Include(sr => sr.Product)
                .ThenInclude(p => p.Brand)
                .Where(sr => sr.UserId == userId)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        // GET: Sell/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["error"] = "Vui lòng đăng nhập để xem chi tiết";
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sellRequest = await _dataContext.SellRequests
                .Include(sr => sr.Product)
                .ThenInclude(p => p.Brand)
                .FirstOrDefaultAsync(sr => sr.Id == id && sr.UserId == userId);

            if (sellRequest == null)
            {
                TempData["error"] = "Không tìm thấy yêu cầu thu mua";
                return RedirectToAction("MyRequests");
            }

            return View(sellRequest);
        }

        // POST: Sell/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sellRequest = await _dataContext.SellRequests
                .FirstOrDefaultAsync(sr => sr.Id == id && sr.UserId == userId);

            if (sellRequest == null)
            {
                return Json(new { success = false, message = "Không tìm thấy yêu cầu thu mua" });
            }

            if (sellRequest.Status != SellRequestStatus.Pending)
            {
                return Json(new { success = false, message = "Không thể hủy yêu cầu đã được xử lý" });
            }

            sellRequest.Status = SellRequestStatus.Cancelled;
            sellRequest.UpdatedAt = DateTime.Now;
            await _dataContext.SaveChangesAsync();

            return Json(new { success = true, message = "Đã hủy yêu cầu thu mua thành công" });
        }

        // API: Get quote for product
        [HttpGet]
        public async Task<IActionResult> GetQuote(int productId)
        {
            var product = await _dataContext.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
            }

            // Tính toán giá đề xuất dựa trên giá sản phẩm (có thể điều chỉnh logic này)
            var suggestedPrice = product.Price * 0.6m; // 60% giá gốc

            return Json(new { 
                success = true, 
                product = new {
                    id = product.Id,
                    name = product.Name,
                    brand = product.Brand?.Name,
                    category = product.Category?.Name,
                    price = product.Price,
                    suggestedPrice = suggestedPrice
                }
            });
        }
    }
}
