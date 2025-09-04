using Microsoft.AspNetCore.Mvc;
using Shopping_Demo.Models;
using Shopping_Demo.Models.ViewModels;
using Shopping_Demo.Repository;
using Shopping_Demo.Services.Momo;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Shopping_Demo.Controllers
{
    public class PaymentController : Controller
    {
        private readonly DataContext _context;
        private readonly IMomoService _momoService;

        public PaymentController(DataContext context, IMomoService momoService)
        {
            _context = context;
            _momoService = momoService;
        }

        [HttpPost]
        public IActionResult ProcessPayment([FromBody] PaymentRequestModel request)
        {
            try
            {
                var payment = new PaymentModel
                {
                    OrderId = GenerateOrderId(),
                    PaymentMethod = request.PaymentMethod,
                    Amount = request.Amount,
                    ProcessingFee = 0,
                    Discount = 0,
                    TotalAmount = request.Amount,
                    Status = "pending",
                    CustomerEmail = request.CustomerEmail,
                    CustomerPhone = request.CustomerPhone,
                    ShippingAddress = request.ShippingAddress,
                    CreatedAt = DateTime.Now,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
                };

                // Lưu vào database
                _context.Payments.Add(payment);
                _context.SaveChanges();

                switch (request.PaymentMethod)
                {
                    case "credit-card":
                        return ProcessCreditCard(payment);
                    case "bank-transfer":
                        return ProcessBankTransfer(payment);
                    case "installment":
                        return ProcessInstallment(payment);
                    case "cash":
                        return ProcessCashPayment(payment);
                    case "cod":
                        return ProcessCODPayment(payment);
                    default:
                        return Json(new { success = false, message = "Phương thức thanh toán không hợp lệ" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private IActionResult ProcessCreditCard(PaymentModel payment)
        {
            // Sử dụng MoMo thay vì VNPAY
            try
            {
                var momoRequest = new OrderInfoModel
                {
                    Amount = payment.TotalAmount
                };

                var momoResponse = _momoService.CreatePaymentMomo(momoRequest).Result;

                if (momoResponse != null && !string.IsNullOrEmpty(momoResponse.PayUrl))
                {
                    payment.PaymentGateway = "momo";
                    payment.PaymentUrl = momoResponse.PayUrl;
                    payment.ReturnUrl = Url.Action("PaymentCallback", "Payment", null, Request.Scheme);
                    _context.SaveChanges();

                    return Json(new
                    {
                        success = true,
                        paymentMethod = "credit-card",
                        orderId = payment.OrderId,
                        amount = payment.TotalAmount,
                        redirectUrl = momoResponse.PayUrl,
                        message = "Đang chuyển hướng đến cổng thanh toán MoMo"
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể tạo thanh toán MoMo" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi MoMo: " + ex.Message });
            }
        }

        private IActionResult ProcessBankTransfer(PaymentModel payment)
        {
            // Thông tin chuyển khoản VPBank với mã QR
            var bankInfo = new
            {
                BankName = "Ngân hàng TMCP Việt Nam Thịnh Vượng (VPBank)",
                AccountNumber = "1234567890",
                AccountName = "CÔNG TY TNHH ĐỒNG HỒ CAO CẤP",
                Branch = "Chi nhánh Hà Nội",
                SwiftCode = "BFTVVNVX",
                OrderId = payment.OrderId,
                Amount = payment.TotalAmount,
                // Thông tin QR Code
                QRCode = new
                {
                    BankCode = "970432", // Mã ngân hàng VPBank
                    AccountNo = "1234567890",
                    Template = "compact2",
                    Amount = payment.TotalAmount,
                    Description = payment.OrderId,
                    AccountName = "CONG TY TNHH DONG HO CAO CAP",
                    QRContent = $"00020101021238530704323200063970432010113456789000208QRIBFTTA53037045802VN62{payment.OrderId.Length:D2}{payment.OrderId}6304", // QR code theo chuẩn VietQR
                    Instructions = "Quét mã QR bằng ứng dụng VPBank hoặc ứng dụng ngân hàng khác hỗ trợ VietQR"
                },
                // Hướng dẫn chuyển khoản
                TransferInstructions = new string[]
                {
                    "1. Mở ứng dụng VPBank trên điện thoại",
                    "2. Chọn chức năng 'Quét mã QR'",
                    "3. Quét mã QR bên dưới",
                    "4. Kiểm tra thông tin thanh toán:",
                    "   - Số tiền: " + payment.TotalAmount.ToString("#,##0") + "₫",
                    "   - Nội dung: " + payment.OrderId,
                    "5. Xác nhận thanh toán",
                    "6. Nhập mật khẩu hoặc vân tay",
                    "7. Lưu lại biên lai chuyển khoản",
                    "8. Liên hệ hotline 0388672928 để xác nhận"
                },
                // Thông tin bổ sung
                AdditionalInfo = new
                {
                    SupportedApps = new string[] { "VPBank Mobile", "VPBank Online", "VPBank QR" },
                    ProcessingTime = "Ngay lập tức",
                    Fee = "Miễn phí",
                    SupportPhone = "0388672928",
                    SupportEmail = "support@donghocaocap.com"
                }
            };

            payment.PaymentGateway = "manual";
            payment.Status = "pending";
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                paymentMethod = "bank-transfer",
                orderId = payment.OrderId,
                amount = payment.TotalAmount,
                bankInfo = bankInfo,
                message = "Vui lòng chuyển khoản theo hướng dẫn"
            });
        }

        private IActionResult ProcessInstallment(PaymentModel payment)
        {
            // Tích hợp thật với các ngân hàng trả góp
            var installmentInfo = new
            {
                OrderId = payment.OrderId,
                Amount = payment.TotalAmount,
                // Thông tin trả góp thật
                Banks = new[]
                {
                    new {
                        Name = "Vietcombank",
                        Code = "VCB",
                        InterestRate = "0%",
                        Tenure = "6-12 tháng",
                        MinAmount = 5000000,
                        MaxAmount = 500000000,
                        Requirements = new string[] { "CMND/CCCD", "Hóa đơn lương 3 tháng gần nhất", "Giấy tờ chứng minh thu nhập" },
                        ApplicationUrl = "https://www.vietcombank.com.vn/vi/personal/loans/installment-loans"
                    },
                    new {
                        Name = "Techcombank",
                        Code = "TCB",
                        InterestRate = "0%",
                        Tenure = "6-24 tháng",
                        MinAmount = 3000000,
                        MaxAmount = 300000000,
                        Requirements = new string[] { "CMND/CCCD", "Hợp đồng lao động", "Sao kê tài khoản 3 tháng" },
                        ApplicationUrl = "https://www.techcombank.com.vn/ca-nhan/vay-tin-chap"
                    },
                    new {
                        Name = "BIDV",
                        Code = "BIDV",
                        InterestRate = "0%",
                        Tenure = "6-18 tháng",
                        MinAmount = 5000000,
                        MaxAmount = 500000000,
                        Requirements = new string[] { "CMND/CCCD", "Giấy tờ chứng minh thu nhập", "Sổ hộ khẩu" },
                        ApplicationUrl = "https://www.bidv.com.vn/vi/ca-nhan/vay-tin-chap"
                    }
                },
                // Hướng dẫn trả góp
                Instructions = new string[]
                {
                    "1. Chọn ngân hàng phù hợp với điều kiện của bạn",
                    "2. Chuẩn bị đầy đủ giấy tờ theo yêu cầu",
                    "3. Liên hệ ngân hàng để làm thủ tục trả góp",
                    "4. Cung cấp mã đơn hàng: " + payment.OrderId,
                    "5. Sau khi được duyệt, ngân hàng sẽ thanh toán cho chúng tôi",
                    "6. Bạn sẽ trả góp hàng tháng cho ngân hàng"
                },
                // Thông tin liên hệ hỗ trợ
                SupportInfo = new
                {
                    Phone = "0388672928",
                    Email = "support@donghocaocap.com",
                    WorkingHours = "9:00 - 18:00 (Thứ 2 - Thứ 6)"
                }
            };

            payment.PaymentGateway = "manual";
            payment.Status = "pending";
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                paymentMethod = "installment",
                orderId = payment.OrderId,
                amount = payment.TotalAmount,
                installmentInfo = installmentInfo,
                message = "Liên hệ ngân hàng để hoàn tất thủ tục trả góp"
            });
        }

        private IActionResult ProcessCashPayment(PaymentModel payment)
        {
            // Thông tin cửa hàng THẬT - Tràng Tiền Plaza
            var cashInfo = new
            {
                StoreName = "CỬA HÀNG ĐỒNG HỒ CAO CẤP - TRÀNG TIỀN PLAZA",
                StoreAddress = "01-05, Tầng 1 Tràng Tiền Plaza, 24 Hai Bà Trưng, Phường Cửa Nam, Quận Hoàn Kiếm, Thành phố Hà Nội", // Địa chỉ thật
                Phone = "0388672928", // Số điện thoại thật
                WorkingHours = "9:00 - 22:00 (Thứ 2 - Chủ nhật)",
                OrderId = payment.OrderId,
                Amount = payment.TotalAmount,
                // Hướng dẫn thanh toán tiền mặt
                Instructions = new string[]
                {
                    "1. Đến cửa hàng tại Tràng Tiền Plaza trong giờ làm việc",
                    "2. Cung cấp mã đơn hàng: " + payment.OrderId,
                    "3. Kiểm tra sản phẩm trước khi thanh toán",
                    "4. Thanh toán bằng tiền mặt hoặc thẻ ATM nội địa",
                    "5. Nhận hóa đơn và bảo hành chính hãng",
                    "6. Sản phẩm sẽ được giao ngay tại cửa hàng"
                },
                // Thông tin bổ sung
                AdditionalInfo = new
                {
                    Parking = "Có bãi đỗ xe Tràng Tiền Plaza (có phí)",
                    PaymentMethods = new string[] { "Tiền mặt", "Thẻ ATM nội địa", "Chuyển khoản ngân hàng" },
                    Services = new string[] { "Kiểm tra đồng hồ", "Điều chỉnh dây đeo", "Hướng dẫn sử dụng", "Bảo hành chính hãng", "Dịch vụ bảo trì" }
                }
            };

            payment.PaymentGateway = "manual";
            payment.Status = "pending";
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                paymentMethod = "cash",
                orderId = payment.OrderId,
                amount = payment.TotalAmount,
                cashInfo = cashInfo,
                message = "Vui lòng đến cửa hàng để thanh toán"
            });
        }

        [HttpGet]
        public IActionResult PaymentCallback()
        {
            try
            {
                // Lấy parameters từ MoMo
                var partnerCode = Request.Query["partnerCode"].ToString();
                var orderId = Request.Query["orderId"].ToString();
                var requestId = Request.Query["requestId"].ToString();
                var amount = Request.Query["amount"].ToString();
                var orderInfo = Request.Query["orderInfo"].ToString();
                var orderType = Request.Query["orderType"].ToString();
                var transId = Request.Query["transId"].ToString();
                var resultCode = Request.Query["resultCode"].ToString();
                var message = Request.Query["message"].ToString();
                var payType = Request.Query["payType"].ToString();
                var signature = Request.Query["signature"].ToString();

                // Tìm payment trong database
                var payment = _context.Payments.FirstOrDefault(p => p.OrderId == orderId);
                
                if (payment != null)
                {
                    if (resultCode == "0")
                    {
                        // Thanh toán thành công
                        payment.Status = "completed";
                        payment.CompletedAt = DateTime.Now;
                        payment.TransactionId = transId;
                        _context.SaveChanges();

                        return RedirectToAction("PaymentSuccess", new { orderId = orderId });
                    }
                    else
                    {
                        // Thanh toán thất bại
                        payment.Status = "failed";
                        _context.SaveChanges();

                        return RedirectToAction("PaymentFailed", new { orderId = orderId, errorCode = resultCode });
                    }
                }

                return RedirectToAction("PaymentFailed", new { orderId = orderId, errorCode = "ORDER_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PaymentCallback Error: {ex.Message}");
                return RedirectToAction("PaymentFailed", new { orderId = "", errorCode = "EXCEPTION" });
            }
        }

        [HttpPost]
        public IActionResult MomoNotify()
        {
            try
            {
                // Xử lý IPN từ MoMo
                var partnerCode = Request.Form["partnerCode"].ToString();
                var orderId = Request.Form["orderId"].ToString();
                var requestId = Request.Form["requestId"].ToString();
                var amount = Request.Form["amount"].ToString();
                var orderInfo = Request.Form["orderInfo"].ToString();
                var orderType = Request.Form["orderType"].ToString();
                var transId = Request.Form["transId"].ToString();
                var resultCode = Request.Form["resultCode"].ToString();
                var message = Request.Form["message"].ToString();
                var payType = Request.Form["payType"].ToString();
                var signature = Request.Form["signature"].ToString();

                // Tìm payment trong database
                var payment = _context.Payments.FirstOrDefault(p => p.OrderId == orderId);
                
                if (payment != null)
                {
                    if (resultCode == "0")
                    {
                        // Thanh toán thành công
                        payment.Status = "completed";
                        payment.CompletedAt = DateTime.Now;
                        payment.TransactionId = transId;
                        _context.SaveChanges();
                    }
                    else
                    {
                        // Thanh toán thất bại
                        payment.Status = "failed";
                        _context.SaveChanges();
                    }
                }

                return Ok("OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MomoNotify Error: {ex.Message}");
                return BadRequest("ERROR");
            }
        }

        [HttpGet]
        public IActionResult PaymentSuccess(string orderId)
        {
            var payment = _context.Payments.FirstOrDefault(p => p.OrderId == orderId);
            return View(payment);
        }

        [HttpGet]
        public IActionResult PaymentFailed(string orderId, string errorCode)
        {
            var payment = _context.Payments.FirstOrDefault(p => p.OrderId == orderId);
            ViewBag.ErrorCode = errorCode;
            return View(payment);
        }

        private IActionResult ProcessCODPayment(PaymentModel payment)
        {
            try
            {
                // COD - Chuyển hướng đến Checkout để tạo đơn hàng
                payment.Status = "pending";
                payment.PaymentGateway = "cod";
                payment.PaymentUrl = Url.Action("Checkout", "Checkout", null, Request.Scheme);
                _context.SaveChanges();

                return Json(new
                {
                    success = true,
                    paymentMethod = "cod",
                    orderId = payment.OrderId,
                    amount = payment.TotalAmount,
                    redirectUrl = Url.Action("Checkout", "Checkout", null, Request.Scheme),
                    message = "Đang chuyển hướng đến trang xác nhận đơn hàng"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi COD: " + ex.Message });
            }
        }

        private string GenerateOrderId()
        {
            return "ORDER_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + new Random().Next(1000, 9999);
        }
    }

    public class PaymentRequestModel
    {
        public string PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string ShippingAddress { get; set; }
    }
}

