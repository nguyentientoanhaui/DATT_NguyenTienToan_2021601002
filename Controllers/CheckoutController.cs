using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopping_Demo.Areas.Admin.Repository;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;
using Shopping_Demo.Services;
using System.Security.Claims;

namespace Shopping_Demo.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        private IMoMoService _momoService;
        public CheckoutController(DataContext context, IEmailSender emailSender, IMoMoService momoService)
        {
            _dataContext = context;
            _emailSender = emailSender;
            _momoService = momoService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Checkout(string OrderId)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var orderItem = new OrderModel();
                orderItem.UserName = userEmail;
                orderItem.UserId = userId;
                if (OrderId != null)
                {
                    // Sử dụng OrderId từ MoMo làm OrderCode
                    orderItem.OrderCode = OrderId;
                    orderItem.PaymentMethod = "MoMo";
                    var momoTransaction = await _dataContext.MomoInfos.FirstOrDefaultAsync(m => m.OrderCode == OrderId);
                    if (momoTransaction != null)
                    {
                        decimal totalAmount = momoTransaction.Amount;
                    }
                }
                else
                {
                    orderItem.PaymentMethod = "COD";
                    // Generate OrderCode for COD payments
                    orderItem.OrderCode = GenerateOrderCode();
                }

                var shippingCity = Request.Cookies["ShippingCity"];
                var shippingDistrict = Request.Cookies["ShippingDistrict"];
                var shippingWard = Request.Cookies["ShippingWard"];
                var shippingAddress = Request.Cookies["ShippingAddress"];
                var shippingPriceCookie = Request.Cookies["ShippingPrice"];
                decimal shippingPrice = 0;

                // Debug: Log cookie values
                Console.WriteLine($"DEBUG - Reading cookies in Checkout - ShippingCity: {shippingCity}");
                Console.WriteLine($"DEBUG - Reading cookies in Checkout - ShippingDistrict: {shippingDistrict}");
                Console.WriteLine($"DEBUG - Reading cookies in Checkout - ShippingWard: {shippingWard}");
                Console.WriteLine($"DEBUG - Reading cookies in Checkout - ShippingAddress: {shippingAddress}");
                Console.WriteLine($"DEBUG - Reading cookies in Checkout - ShippingPriceCookie: {shippingPriceCookie}");

                // Debug: Log all cookies
                Console.WriteLine("DEBUG - All cookies in Checkout:");
                foreach (var cookie in Request.Cookies)
                {
                    Console.WriteLine($"  {cookie.Key}: {cookie.Value}");
                }

                if (shippingPriceCookie != null)
                {
                    var shippingPriceJson = shippingPriceCookie;
                    shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
                }
                orderItem.ShippingCost = shippingPrice;

                var coupon_code = Request.Cookies["CouponCode"]; 
                var couponTitle = Request.Cookies["CouponTitle"];
                var discountAmountCookie = Request.Cookies["DiscountAmount"];
                var couponIdCookie = Request.Cookies["CouponId"];

                orderItem.CouponCode = coupon_code;

                if (discountAmountCookie != null)
                {
                    decimal discountAmount = 0;
                    decimal.TryParse(discountAmountCookie, out discountAmount);
                    orderItem.DiscountAmount = discountAmount;
                }

                // Giảm số lượng coupon nếu có (Disabled - Coupons table not exists)
                /*
                if (!string.IsNullOrEmpty(couponIdCookie) && int.TryParse(couponIdCookie, out int couponId))
                {
                    var coupon = await _dataContext.Coupons.FindAsync(couponId);
                    if (coupon != null && coupon.Quantity > 0)
                    {
                        coupon.Quantity -= 1;
                        _dataContext.Update(coupon);
                    }
                }
                */
                // Set shipping address from cookies, with fallback to user profile
                if (!string.IsNullOrEmpty(shippingCity))
                {
                    orderItem.ShippingCity = shippingCity;
                    orderItem.ShippingWard = shippingWard;
                    orderItem.ShippingDistrict = shippingDistrict;
                    orderItem.ShippingAddress = shippingAddress;
                }
                else
                {
                    // Fallback: Get address from user profile
                    var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                    if (user != null && !string.IsNullOrEmpty(user.Address))
                    {
                        orderItem.ShippingAddress = user.Address;
                        orderItem.ShippingCity = "Chưa cập nhật";
                        orderItem.ShippingWard = "Chưa cập nhật";
                        orderItem.ShippingDistrict = "Chưa cập nhật";
                        
                        Console.WriteLine($"DEBUG - Using fallback address from user profile:");
                        Console.WriteLine($"  Address: {orderItem.ShippingAddress}");
                        Console.WriteLine($"  City: {orderItem.ShippingCity}");
                        Console.WriteLine($"  Ward: {orderItem.ShippingWard}");
                        Console.WriteLine($"  District: {orderItem.ShippingDistrict}");
                    }
                    else
                    {
                        orderItem.ShippingCity = "Chưa cập nhật";
                        orderItem.ShippingWard = "Chưa cập nhật";
                        orderItem.ShippingDistrict = "Chưa cập nhật";
                        orderItem.ShippingAddress = "Chưa cập nhật";
                        
                        Console.WriteLine("DEBUG - No user profile found, using default values");
                    }
                }
                orderItem.Status = 1;
                orderItem.CreatedDate = DateTime.Now;
                
                // Debug: Log values being saved to database
                Console.WriteLine($"DEBUG - Saving to DB - ShippingCity: {orderItem.ShippingCity}");
                Console.WriteLine($"DEBUG - Saving to DB - ShippingWard: {orderItem.ShippingWard}");
                Console.WriteLine($"DEBUG - Saving to DB - ShippingDistrict: {orderItem.ShippingDistrict}");
                Console.WriteLine($"DEBUG - Saving to DB - ShippingAddress: {orderItem.ShippingAddress}");
                
                _dataContext.Add(orderItem);
                _dataContext.SaveChanges();

                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                foreach (var cart in cartItems)
                {
                    var orderDetails = new OrderDetails();
                    orderDetails.UserName = userEmail;
                    orderDetails.OrderCode = orderItem.OrderCode;
                    orderDetails.ProductId = cart.ProductId;
                    orderDetails.Price = cart.Price;
                    orderDetails.Quantity = cart.Quantity;
                    orderDetails.ColorName = cart.ColorName;
                    orderDetails.SizeName = cart.SizeName;
                    var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();
                    product.Quantity -= cart.Quantity;
                    product.Sold += cart.Quantity;

                    _dataContext.Update(product);
                    _dataContext.Add(orderDetails);
                    _dataContext.SaveChanges();
                }

                // Xóa cookies giảm giá và vận chuyển sau khi đặt hàng
                Response.Cookies.Delete("CouponTitle");
                Response.Cookies.Delete("CouponCode");
                Response.Cookies.Delete("DiscountAmount");
                Response.Cookies.Delete("CouponId");
                Response.Cookies.Delete("ShippingPrice");
                Response.Cookies.Delete("ShippingCity");
                Response.Cookies.Delete("ShippingDistrict");
                Response.Cookies.Delete("ShippingWard");
                Response.Cookies.Delete("ShippingAddress");

                HttpContext.Session.Remove("Cart");

                var receiver = userEmail;
                var subject = "Đặt hàng thành công - Mã đơn hàng: " + orderItem.OrderCode;

                // Tạo nội dung email chi tiết
                var messageBuilder = new System.Text.StringBuilder();
                messageBuilder.AppendLine("<h2>Xác nhận đơn hàng</h2>");
                messageBuilder.AppendLine("<p>Cảm ơn bạn đã đặt hàng tại cửa hàng của chúng tôi.</p>");
                messageBuilder.AppendLine("<p><strong>Mã đơn hàng:</strong> " + orderItem.OrderCode + "</p>");
                messageBuilder.AppendLine("<p><strong>Ngày đặt hàng:</strong> " + orderItem.CreatedDate + "</p>");
                messageBuilder.AppendLine("<h3>Chi tiết đơn hàng:</h3>");
                
                decimal total = 0;
                foreach (var item in cartItems)
                {
                    var product = await _dataContext.Products.Where(p => p.Id == item.ProductId).FirstAsync();
                    decimal itemTotal = item.Price * item.Quantity;
                    total += itemTotal;

                    messageBuilder.AppendLine("<div style='border: 1px solid #dee2e6; border-radius: 8px; margin-bottom: 20px; padding: 15px; background: #f8f9fa;'>");
                    messageBuilder.AppendLine("<div style='display: flex; align-items: flex-start; gap: 20px;'>");
                    
                    // Ảnh sản phẩm
                    messageBuilder.AppendLine("<div style='flex-shrink: 0;'>");
                    if (!string.IsNullOrEmpty(product.Image))
                    {
                        messageBuilder.AppendLine($"<img src='{product.Image}' alt='{product.Name}' style='width: 150px; height: 150px; object-fit: cover; border-radius: 8px; border: 1px solid #ddd;'>");
                    }
                    else
                    {
                        messageBuilder.AppendLine("<div style='width: 150px; height: 150px; background: #e9ecef; border-radius: 8px; display: flex; align-items: center; justify-content: center; color: #6c757d; font-size: 14px;'>Không có ảnh</div>");
                    }
                    messageBuilder.AppendLine("</div>");
                    
                    // Thông tin sản phẩm
                    messageBuilder.AppendLine("<div style='flex: 1;'>");
                    messageBuilder.AppendLine($"<h4 style='margin: 0 0 15px 0; color: #2c3e50;'>{product.Name}</h4>");
                    
                    // Thông tin cơ bản trong 2 cột
                    messageBuilder.AppendLine("<div style='display: flex; gap: 30px; margin-bottom: 15px;'>");
                    messageBuilder.AppendLine("<div style='flex: 1;'>");
                    messageBuilder.AppendLine("<h5 style='color: #007bff; margin: 0 0 8px 0; font-size: 14px;'>Thông tin cơ bản</h5>");
                    messageBuilder.AppendLine("<table style='font-size: 13px; line-height: 1.4;'>");
                    messageBuilder.AppendLine($"<tr><td style='padding: 2px 0; width: 100px;'><strong>Model:</strong></td><td>{product.Model ?? "N/A"}</td></tr>");
                    messageBuilder.AppendLine($"<tr><td style='padding: 2px 0;'><strong>Năm:</strong></td><td>{product.Year?.ToString() ?? "N/A"}</td></tr>");
                    messageBuilder.AppendLine($"<tr><td style='padding: 2px 0;'><strong>Giới tính:</strong></td><td>{product.Gender ?? "N/A"}</td></tr>");
                    messageBuilder.AppendLine($"<tr><td style='padding: 2px 0;'><strong>Tình trạng:</strong></td><td>{product.Condition ?? "N/A"}</td></tr>");
                    messageBuilder.AppendLine("</table>");
                    messageBuilder.AppendLine("</div>");
                    
                    messageBuilder.AppendLine("<div style='flex: 1;'>");
                    messageBuilder.AppendLine("<h5 style='color: #007bff; margin: 0 0 8px 0; font-size: 14px;'>Thông tin kỹ thuật</h5>");
                    messageBuilder.AppendLine("<table style='font-size: 13px; line-height: 1.4;'>");
                    messageBuilder.AppendLine($"<tr><td style='padding: 2px 0; width: 100px;'><strong>Vỏ máy:</strong></td><td>{product.CaseMaterial ?? "N/A"}</td></tr>");
                    messageBuilder.AppendLine($"<tr><td style='padding: 2px 0;'><strong>Kích thước:</strong></td><td>{product.CaseSize ?? "N/A"}</td></tr>");
                    messageBuilder.AppendLine($"<tr><td style='padding: 2px 0;'><strong>Mặt kính:</strong></td><td>{product.Crystal ?? "N/A"}</td></tr>");
                    messageBuilder.AppendLine($"<tr><td style='padding: 2px 0;'><strong>Bộ máy:</strong></td><td>{product.MovementType ?? "N/A"}</td></tr>");
                    messageBuilder.AppendLine("</table>");
                    messageBuilder.AppendLine("</div>");
                    messageBuilder.AppendLine("</div>");
                    
                    // Thông tin giá và số lượng
                    messageBuilder.AppendLine("<div style='background: white; padding: 12px; border-radius: 6px; border: 1px solid #dee2e6;'>");
                    messageBuilder.AppendLine("<div style='display: flex; justify-content: space-between; align-items: center;'>");
                    messageBuilder.AppendLine($"<span><strong>Số lượng:</strong> {item.Quantity}</span>");
                    messageBuilder.AppendLine($"<span><strong>Đơn giá:</strong> {item.Price:N0}₫</span>");
                    messageBuilder.AppendLine($"<span style='color: #28a745; font-weight: bold; font-size: 16px;'>Thành tiền: {itemTotal:N0}₫</span>");
                    messageBuilder.AppendLine("</div>");
                    messageBuilder.AppendLine("</div>");
                    
                    messageBuilder.AppendLine("</div>");
                    messageBuilder.AppendLine("</div>");
                    messageBuilder.AppendLine("</div>");
                }

                // Phần tổng thanh toán
                messageBuilder.AppendLine("<div style='background: #e8f5e8; padding: 20px; border-radius: 8px; margin: 20px 0;'>");
                messageBuilder.AppendLine("<h3 style='color: #28a745; margin-top: 0;'>💰 Tổng thanh toán</h3>");
                messageBuilder.AppendLine($"<p style='margin: 8px 0;'><strong>Tạm tính:</strong> {total:N0}₫</p>");

                if (!string.IsNullOrEmpty(couponTitle))
                {
                    messageBuilder.AppendLine($"<p style='margin: 8px 0;'><strong>Mã giảm giá:</strong> {couponTitle}</p>");
                    messageBuilder.AppendLine($"<p style='margin: 8px 0; color: #e74c3c;'><strong>Giảm giá:</strong> -{orderItem.DiscountAmount:N0}₫</p>");
                }

                messageBuilder.AppendLine($"<p style='margin: 8px 0;'><strong>Phí vận chuyển:</strong> {orderItem.ShippingCost:N0}₫</p>");

                string paymentMethodText = orderItem.PaymentMethod == "COD" ?
                    "Thanh toán khi giao hàng" :
                    "Thanh toán qua MoMo";
                messageBuilder.AppendLine($"<p style='margin: 8px 0;'><strong>Phương thức thanh toán:</strong> {paymentMethodText}</p>");

                // Tính tổng tiền
                decimal grandTotal = total - orderItem.DiscountAmount + orderItem.ShippingCost;
                if (grandTotal < 0) { grandTotal = 0; }
                messageBuilder.AppendLine($"<p style='margin: 8px 0; font-size: 18px; color: #28a745; font-weight: bold;'><strong>TỔNG THANH TOÁN:</strong> {grandTotal:N0}₫</p>");
                messageBuilder.AppendLine("</div>");

                messageBuilder.AppendLine("<div style='background: #fff3cd; padding: 20px; border-radius: 8px; margin: 20px 0;'>");
                messageBuilder.AppendLine("<h3 style='color: #856404; margin-top: 0;'>📝 Hướng dẫn</h3>");
                messageBuilder.AppendLine("<p style='margin: 8px 0;'>✅ Đơn hàng của bạn đang được xử lý</p>");
                messageBuilder.AppendLine("<p style='margin: 8px 0;'>✅ Chúng tôi sẽ liên hệ xác nhận trong vòng 24h</p>");
                messageBuilder.AppendLine("<p style='margin: 8px 0;'>✅ Vui lòng giữ máy để nhận cuộc gọi từ nhân viên</p>");
                messageBuilder.AppendLine("</div>");

                messageBuilder.AppendLine("<div style='text-align: center; padding: 20px; background: #f8f9fa; border-radius: 8px;'>");
                messageBuilder.AppendLine("<p style='margin: 0; color: #6c757d;'>Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ của chúng tôi!</p>");
                messageBuilder.AppendLine("<p style='margin: 20px 0 0 0; color: #6c757d; font-style: italic;'>Trân trọng,<br/>Đội ngũ hỗ trợ</p>");
                messageBuilder.AppendLine("</div>");


                var message = messageBuilder.ToString();
                await _emailSender.SendEmailAsync(receiver, subject, message);

                HttpContext.Session.Remove("Cart");

                if (User.Identity.IsAuthenticated)
                {
                    string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var userCart = await _dataContext.Carts
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync(c => c.UserId == currentUserId);

                    if (userCart != null && userCart.CartItems.Any())
                    {
                        // Xóa từng CartItem
                        foreach (var item in userCart.CartItems.ToList())
                        {
                            _dataContext.Remove(item);  // Xóa CartItem
                        }

                        // Lưu thay đổi
                        await _dataContext.SaveChangesAsync();
                    }
                }



                // Nếu phương thức thanh toán là MoMo, không cần chuyển hướng
                if (OrderId != null)
                {
                    return Ok(new { success = true, message = "Đặt hàng thành công" });
                }

                TempData["success"] = "Đặt hàng thành công, vui lòng chờ liên hệ từ đơn vị vận chuyển";
                return RedirectToAction("History", "Account");
            }
        }
        [HttpGet]
        public async Task<IActionResult> PaymentCallBack()
        {
            try
            {
                var allParams = HttpContext.Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
                Console.WriteLine("MoMo Callback Parameters: " + JsonConvert.SerializeObject(allParams));

                // Xử lý callback từ MoMo
                var momoResponse = _momoService.ProcessCallback(HttpContext.Request.Query);
                
                Console.WriteLine($"MoMo Callback Response: {JsonConvert.SerializeObject(momoResponse)}");

                if (momoResponse != null)
                {
                    // Lưu thông tin giao dịch thành công
                    var newMomoInsert = new MomoInfoModel
                    {
                        OrderCode = momoResponse.OrderId,
                        FullName = User?.FindFirstValue(ClaimTypes.Email) ?? "Guest",
                        Amount = decimal.Parse(momoResponse.Amount),
                        OrderInfo = momoResponse.OrderInfo,
                        DatePaid = DateTime.Now,
                        TransactionStatus = "Success"
                    };

                    _dataContext.Add(newMomoInsert);
                    await _dataContext.SaveChangesAsync();

                    // Gọi Checkout để tạo đơn hàng
                    await Checkout(momoResponse.OrderId);

                    HttpContext.Session.Remove("Cart");

                    if (User.Identity.IsAuthenticated)
                    {
                        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        var userCart = await _dataContext.Carts
                            .Include(c => c.CartItems)
                            .FirstOrDefaultAsync(c => c.UserId == userId);

                        if (userCart != null && userCart.CartItems.Any())
                        {
                            foreach (var item in userCart.CartItems.ToList())
                            {
                                _dataContext.Remove(item); 
                            }

                            await _dataContext.SaveChangesAsync();
                        }
                    }

                    TempData["success"] = "Thanh toán thành công qua MoMo!";
                    return RedirectToAction("History", "Account");
                }
                else
                {
                    // Thanh toán thất bại
                    var newMomoInsert = new MomoInfoModel
                    {
                        OrderCode = momoResponse.OrderId,
                        FullName = User?.FindFirstValue(ClaimTypes.Email) ?? "Guest",
                        Amount = decimal.Parse(momoResponse.Amount),
                        OrderInfo = momoResponse.OrderInfo,
                        DatePaid = DateTime.Now,
                        TransactionStatus = "Failed"
                    };

                    _dataContext.Add(newMomoInsert);
                    await _dataContext.SaveChangesAsync();

                    TempData["error"] = "Giao dịch MoMo thất bại. Mã lỗi: " + momoResponse.ResponseCode;
                    return RedirectToAction("Index", "Cart");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MoMo Callback Error: {ex.Message}");
                TempData["error"] = "Lỗi xử lý thanh toán MoMo: " + ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }

        private string GenerateOrderCode()
        {
            // Generate a unique order code with timestamp and random number
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"ORD{timestamp}{random}";
        }
    }
}
