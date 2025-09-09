using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;
using Shopping_Demo.Areas.Admin.Repository;

namespace Shopping_Demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager,Staff,Sale,Shipper")]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        
        public OrderController(DataContext context, IEmailSender emailSender)
        {
            _dataContext = context;
            _emailSender = emailSender;
        }
        public async Task<IActionResult> Index()
        {
            var orders = await _dataContext.Orders
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(orders);
        }
        public async Task<IActionResult> ViewOrder(string orderCode)
        {
            var DetailsOrder = await _dataContext.OrderDetails.Include(p => p.Product).Where(o => o.OrderCode == orderCode).ToListAsync();
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);

            if (order != null)
            {
                ViewBag.ShippingCost = order.ShippingCost;
                ViewBag.DiscountAmount = order.DiscountAmount; // Thêm giá trị giảm giá
                ViewBag.Status = order.Status; // Thêm trạng thái đơn hàng

                ViewBag.ShippingCity = order.ShippingCity;
                ViewBag.ShippingDistrict = order.ShippingDistrict;
                ViewBag.ShippingWard = order.ShippingWard;
                ViewBag.ShippingAddress = order.ShippingAddress;
            }

            return View(DetailsOrder);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);
            if (order == null)
            {
                return NotFound();
            }

            // Lưu trạng thái cũ trước khi cập nhật
            var oldStatus = order.Status;

            if (status == 0 && order.Status != 0)
            {
                order.DeliveredDate = DateTime.Now;
            }

            order.Status = status;
            _dataContext.Update(order);

            if (status == 0)
            {
                var DetailsOrder = await _dataContext.OrderDetails
                    .Include(o => o.Product)
                    .Where(o => o.OrderCode == order.OrderCode)
                    .Select(o => new
                    {
                        o.Quantity,
                        o.Product.Price,
                        o.Product.CapitalPrice
                    }).ToListAsync();

                var statisticalModel = await _dataContext.Statisticals
                    .FirstOrDefaultAsync(s => s.DateCreated.Date == order.CreatedDate.Date);

                if (statisticalModel != null)
                {
                    statisticalModel.Quantity += 1;
                    foreach (var orderDetail in DetailsOrder)
                    {
                        statisticalModel.Sold += orderDetail.Quantity;
                        statisticalModel.Revenue += orderDetail.Quantity * orderDetail.Price;
                        statisticalModel.Profit += (orderDetail.Price - orderDetail.CapitalPrice) * orderDetail.Quantity;
                    }
                    _dataContext.Update(statisticalModel);
                }
                else
                {
                    int new_quantity = 1;
                    int new_sold = 0;
                    decimal new_profit = 0;
                    decimal new_revenue = 0;

                    foreach (var orderDetail in DetailsOrder)
                    {
                        new_sold += orderDetail.Quantity;
                        new_profit += (orderDetail.Price - orderDetail.CapitalPrice) * orderDetail.Quantity;
                        new_revenue += orderDetail.Quantity * orderDetail.Price;
                    }

                    statisticalModel = new StatisticalModel
                    {
                        Quantity = new_quantity,
                        Sold = new_sold,
                        Profit = new_profit,
                        Revenue = new_revenue,
                        DateCreated = order.CreatedDate,
                    };

                    _dataContext.Add(statisticalModel);
                }

            }

            // Gửi email thông báo giao hàng thành công
            if (status == 0 && oldStatus != 0)
            {
                Console.WriteLine($"DEBUG: Sending delivery email for order {order.OrderCode}");
                Console.WriteLine($"DEBUG: Old status: {oldStatus}, New status: {status}");
                Console.WriteLine($"DEBUG: Customer email: {order.UserName}");
                
                try
                {
                    var orderDetails = await _dataContext.OrderDetails
                        .Include(o => o.Product)
                        .Where(o => o.OrderCode == order.OrderCode)
                        .ToListAsync();

                    var receiver = order.UserName;
                    var subject = $"🚚 Đơn hàng đã được giao thành công - {order.OrderCode}";

                    var messageBuilder = new System.Text.StringBuilder();
                    messageBuilder.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>");
                    messageBuilder.AppendLine("<div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>");
                    messageBuilder.AppendLine("<h1 style='margin: 0; font-size: 28px;'>🎉 Đơn hàng đã được giao thành công!</h1>");
                    messageBuilder.AppendLine("<p style='margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>Cảm ơn bạn đã tin tưởng chúng tôi</p>");
                    messageBuilder.AppendLine("</div>");
                    
                    messageBuilder.AppendLine("<div style='background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px;'>");
                    messageBuilder.AppendLine("<div style='background: white; padding: 20px; border-radius: 8px; margin-bottom: 20px;'>");
                    messageBuilder.AppendLine("<h3 style='color: #2c3e50; margin-top: 0;'>📋 Thông tin đơn hàng</h3>");
                    messageBuilder.AppendLine($"<p><strong>Mã đơn hàng:</strong> <span style='color: #e74c3c; font-weight: bold;'>{order.OrderCode}</span></p>");
                    messageBuilder.AppendLine($"<p><strong>Ngày giao hàng:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
                    messageBuilder.AppendLine($"<p><strong>Phương thức thanh toán:</strong> {(order.PaymentMethod == "COD" ? "Thanh toán khi nhận hàng" : "Thanh toán online")}</p>");
                    messageBuilder.AppendLine("</div>");
                    
                    messageBuilder.AppendLine("<div style='background: white; padding: 20px; border-radius: 8px; margin-bottom: 20px;'>");
                    messageBuilder.AppendLine("<h3 style='color: #2c3e50; margin-top: 0;'>📍 Địa chỉ giao hàng</h3>");
                    messageBuilder.AppendLine($"<p style='margin: 5px 0;'>{order.ShippingAddress ?? "Không có thông tin"}</p>");
                    messageBuilder.AppendLine($"<p style='margin: 5px 0;'>Phường/Xã: {order.ShippingWard ?? "Không có thông tin"}</p>");
                    messageBuilder.AppendLine($"<p style='margin: 5px 0;'>Quận/Huyện: {order.ShippingDistrict ?? "Không có thông tin"}</p>");
                    messageBuilder.AppendLine($"<p style='margin: 5px 0;'>Tỉnh/Thành: {order.ShippingCity ?? "Không có thông tin"}</p>");
                    messageBuilder.AppendLine("</div>");
                    
                    messageBuilder.AppendLine("<div style='background: white; padding: 20px; border-radius: 8px; margin-bottom: 20px;'>");
                    messageBuilder.AppendLine("<h3 style='color: #2c3e50; margin-top: 0;'>🛍️ Chi tiết sản phẩm</h3>");
                    
                    decimal total = 0;
                    foreach (var item in orderDetails)
                    {
                        decimal itemTotal = item.Quantity * item.Price;
                        total += itemTotal;
                        
                        messageBuilder.AppendLine("<div style='border: 1px solid #dee2e6; border-radius: 8px; margin-bottom: 20px; padding: 15px; background: #f8f9fa;'>");
                        messageBuilder.AppendLine("<div style='display: flex; align-items: flex-start; gap: 20px;'>");
                        
                        // Ảnh sản phẩm
                        messageBuilder.AppendLine("<div style='flex-shrink: 0;'>");
                        if (!string.IsNullOrEmpty(item.Product?.Image))
                        {
                            messageBuilder.AppendLine($"<img src='{item.Product.Image}' alt='{item.Product.Name}' style='width: 150px; height: 150px; object-fit: cover; border-radius: 8px; border: 1px solid #ddd;'>");
                        }
                        else
                        {
                            messageBuilder.AppendLine("<div style='width: 150px; height: 150px; background: #e9ecef; border-radius: 8px; display: flex; align-items: center; justify-content: center; color: #6c757d; font-size: 14px;'>Không có ảnh</div>");
                        }
                        messageBuilder.AppendLine("</div>");
                        
                        // Thông tin sản phẩm
                        messageBuilder.AppendLine("<div style='flex: 1;'>");
                        messageBuilder.AppendLine($"<h4 style='margin: 0 0 15px 0; color: #2c3e50;'>{item.Product?.Name ?? "Sản phẩm"}</h4>");
                        
                        // Thông tin cơ bản trong 2 cột
                        messageBuilder.AppendLine("<div style='display: flex; gap: 30px; margin-bottom: 15px;'>");
                        messageBuilder.AppendLine("<div style='flex: 1;'>");
                        messageBuilder.AppendLine("<h5 style='color: #007bff; margin: 0 0 8px 0; font-size: 14px;'>Thông tin cơ bản</h5>");
                        messageBuilder.AppendLine("<table style='font-size: 13px; line-height: 1.4;'>");
                        messageBuilder.AppendLine($"<tr><td style='padding: 2px 0; width: 100px;'><strong>Model:</strong></td><td>{item.Product?.Model ?? "N/A"}</td></tr>");
                        messageBuilder.AppendLine($"<tr><td style='padding: 2px 0;'><strong>Năm:</strong></td><td>{item.Product?.Year?.ToString() ?? "N/A"}</td></tr>");
                        messageBuilder.AppendLine($"<tr><td style='padding: 2px 0;'><strong>Giới tính:</strong></td><td>{item.Product?.Gender ?? "N/A"}</td></tr>");
                        messageBuilder.AppendLine($"<tr><td style='padding: 2px 0;'><strong>Tình trạng:</strong></td><td>{item.Product?.Condition ?? "N/A"}</td></tr>");
                        messageBuilder.AppendLine("</table>");
                        messageBuilder.AppendLine("</div>");
                        
                        messageBuilder.AppendLine("<div style='flex: 1;'>");
                        messageBuilder.AppendLine("<h5 style='color: #007bff; margin: 0 0 8px 0; font-size: 14px;'>Thông tin kỹ thuật</h5>");
                        messageBuilder.AppendLine("<table style='font-size: 13px; line-height: 1.4;'>");
                        messageBuilder.AppendLine($"<tr><td style='padding: 2px 0; width: 100px;'><strong>Vỏ máy:</strong></td><td>{item.Product?.CaseMaterial ?? "N/A"}</td></tr>");
                        messageBuilder.AppendLine($"<tr><td style='padding: 2px 0;'><strong>Kích thước:</strong></td><td>{item.Product?.CaseSize ?? "N/A"}</td></tr>");
                        messageBuilder.AppendLine($"<tr><td style='padding: 2px 0;'><strong>Mặt kính:</strong></td><td>{item.Product?.Crystal ?? "N/A"}</td></tr>");
                        messageBuilder.AppendLine($"<tr><td style='padding: 2px 0;'><strong>Bộ máy:</strong></td><td>{item.Product?.MovementType ?? "N/A"}</td></tr>");
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
                    
                    messageBuilder.AppendLine("</div>");
                    
                    messageBuilder.AppendLine("<div style='background: #e8f5e8; padding: 20px; border-radius: 8px; margin-bottom: 20px;'>");
                    messageBuilder.AppendLine("<h3 style='color: #28a745; margin-top: 0;'>💰 Tổng thanh toán</h3>");
                    messageBuilder.AppendLine($"<p style='margin: 8px 0;'><strong>Tạm tính:</strong> {total:N0}₫</p>");
                    
                    if (order.DiscountAmount > 0)
                    {
                        messageBuilder.AppendLine($"<p style='margin: 8px 0; color: #e74c3c;'><strong>Giảm giá:</strong> -{order.DiscountAmount:N0}₫</p>");
                    }
                    
                    messageBuilder.AppendLine($"<p style='margin: 8px 0;'><strong>Phí vận chuyển:</strong> {order.ShippingCost:N0}₫</p>");
                    var finalTotal = total - order.DiscountAmount + order.ShippingCost;
                    messageBuilder.AppendLine($"<p style='margin: 8px 0; font-size: 18px; color: #28a745; font-weight: bold;'><strong>TỔNG THANH TOÁN:</strong> {finalTotal:N0}₫</p>");
                    messageBuilder.AppendLine("</div>");
                    
                    messageBuilder.AppendLine("<div style='background: #fff3cd; padding: 20px; border-radius: 8px; margin-bottom: 20px;'>");
                    messageBuilder.AppendLine("<h3 style='color: #856404; margin-top: 0;'>📝 Hướng dẫn</h3>");
                    messageBuilder.AppendLine("<p style='margin: 8px 0;'>✅ Vui lòng kiểm tra sản phẩm trước khi ký nhận</p>");
                    messageBuilder.AppendLine("<p style='margin: 8px 0;'>✅ Nếu có vấn đề, vui lòng từ chối nhận hàng và liên hệ ngay với chúng tôi</p>");
                    messageBuilder.AppendLine("<p style='margin: 8px 0;'>✅ Sau khi nhận hàng, bạn có thể đánh giá sản phẩm trong tài khoản</p>");
                    messageBuilder.AppendLine("</div>");
                    
                    messageBuilder.AppendLine("<div style='text-align: center; padding: 20px; background: #f8f9fa; border-radius: 8px;'>");
                    messageBuilder.AppendLine("<p style='margin: 0; color: #6c757d;'>Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ của chúng tôi!</p>");
                    messageBuilder.AppendLine("<p style='margin: 10px 0 0 0; color: #6c757d;'>Nếu có bất kỳ vấn đề gì, vui lòng liên hệ với chúng tôi.</p>");
                    messageBuilder.AppendLine("<p style='margin: 20px 0 0 0; color: #6c757d; font-style: italic;'>Trân trọng,<br/>Đội ngũ hỗ trợ</p>");
                    messageBuilder.AppendLine("</div>");
                    messageBuilder.AppendLine("</div>");
                    messageBuilder.AppendLine("</div>");

                    var message = messageBuilder.ToString();
                    await _emailSender.SendEmailAsync(receiver, subject, message);
                    Console.WriteLine($"DEBUG: Delivery email sent successfully to {receiver}");
                }
                catch (Exception ex)
                {
                    // Log lỗi gửi email nhưng không ảnh hưởng đến việc cập nhật trạng thái
                    Console.WriteLine("Error sending delivery email: " + ex.Message);
                }
            }

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Order status updated successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the order status.");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string ordercode)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);
            if (order == null)
            {
                return NotFound();
            }
            try
            {
                //delete order
                _dataContext.Orders.Remove(order);
                await _dataContext.SaveChangesAsync();
                return RedirectToAction("Index", "Order");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the order.");
            }
        }
        [HttpGet]
        public async Task<IActionResult> PaymentMomoInfo(string orderId)
        {
            var momoInfo = await _dataContext.MomoInfos.FirstOrDefaultAsync(m => m.OrderCode == orderId);

            if (momoInfo == null)
            {
                return NotFound();
            }
            return View(momoInfo);
        }

    }
}

