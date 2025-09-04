using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopping_Demo.Services;
using System.Threading.Tasks;

namespace Shopping_Demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class VietnamizationController : Controller
    {
        private readonly IDatabaseVietnamizationService _vietnamizationService;

        public VietnamizationController(IDatabaseVietnamizationService vietnamizationService)
        {
            _vietnamizationService = vietnamizationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VietnamizeDatabase()
        {
            try
            {
                var result = await _vietnamizationService.VietnamizeDatabaseAsync();
                
                if (result)
                {
                    TempData["Success"] = "Đã việt hóa database thành công!";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi việt hóa database.";
                }
                
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConvertCurrency()
        {
            try
            {
                var result = await _vietnamizationService.ConvertCurrencyToVNDAsync();
                
                if (result)
                {
                    TempData["Success"] = "Đã chuyển đổi tiền tệ từ USD sang VND thành công!";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi chuyển đổi tiền tệ.";
                }
                
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> VietnamizeFields()
        {
            try
            {
                var result = await _vietnamizationService.VietnamizeProductFieldsAsync();
                
                if (result)
                {
                    TempData["Success"] = "Đã việt hóa các trường sản phẩm thành công!";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi việt hóa các trường.";
                }
                
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
