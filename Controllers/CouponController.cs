using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping_Demo.Controllers
{
    public class CouponController : Controller
    {
        private readonly DataContext _dataContext;

        public CouponController(DataContext context)
        {
            _dataContext = context;
        }

        // GET: /Coupon/Index
        public async Task<IActionResult> Index()
        {
            var availableCoupons = await _dataContext.Coupons
                .Where(c => c.DateExpired >= DateTime.Now && c.DateStart <= DateTime.Now && c.Status == 1 && c.Quantity > 0)
                .ToListAsync();

            return View(availableCoupons);
        }

        // GET: /Coupon/Available (API endpoint for AJAX calls)
        [HttpGet]
        public async Task<IActionResult> Available()
        {
            var availableCoupons = await _dataContext.Coupons
                .Where(c => c.DateExpired >= DateTime.Now && c.DateStart <= DateTime.Now && c.Status == 1 && c.Quantity > 0)
                .Select(c => new {
                    code = c.Name,
                    description = c.Description,
                    discountValue = c.DiscountValue,
                    discountType = c.DiscountType,
                    expiryDate = c.DateExpired.ToString("dd/MM/yyyy"),
                    used = c.Quantity,
                    isPercentage = c.DiscountType == DiscountType.Percentage
                })
                .ToListAsync();

            return Json(availableCoupons);
        }

        // POST: /Coupon/ValidateCoupon
        [HttpPost]
        public async Task<IActionResult> ValidateCoupon(string couponCode)
        {
            var coupon = await _dataContext.Coupons
                .FirstOrDefaultAsync(c => c.Name == couponCode && c.Status == 1);

            if (coupon == null)
            {
                return Json(new { isValid = false, message = "Mã giảm giá không tồn tại" });
            }

            // Check if coupon is expired or not yet active
            if (coupon.DateExpired < DateTime.Now)
            {
                return Json(new { isValid = false, message = "Mã giảm giá đã hết hạn" });
            }

            if (coupon.DateStart > DateTime.Now)
            {
                return Json(new { isValid = false, message = "Mã giảm giá chưa có hiệu lực" });
            }

            // Check if coupon is out of stock
            if (coupon.Quantity <= 0)
            {
                return Json(new { isValid = false, message = "Mã giảm giá đã hết lượt sử dụng" });
            }

            // Format discount value
            string discountText = coupon.DiscountType == DiscountType.Percentage
                ? $"{coupon.DiscountValue}%"
                : $"{coupon.DiscountValue.ToString("#,##0")} VNĐ";

            return Json(new
            {
                isValid = true,
                discountValue = coupon.DiscountValue,
                discountType = coupon.DiscountType.ToString(),
                discountText = discountText,
                message = "Mã giảm giá hợp lệ"
            });
        }
    }
}