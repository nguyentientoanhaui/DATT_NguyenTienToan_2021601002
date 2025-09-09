using Microsoft.AspNetCore.Mvc;
using Shopping_Demo.Models;
using Shopping_Demo.Models.ViewModels;
using Shopping_Demo.Repository;
using Shopping_Demo.Services;
using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;

namespace Shopping_Demo.Controllers
{
    public class PaymentController : Controller
    {
        private readonly DataContext _context;
        private readonly IMoMoService _momoService;
        private readonly ILargePaymentService _largePaymentService;

        public PaymentController(DataContext context, IMoMoService momoService, ILargePaymentService largePaymentService)
        {
            _context = context;
            _momoService = momoService;
            _largePaymentService = largePaymentService;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestModel request)
        {
            try
            {
                Console.WriteLine($"Processing payment: {JsonConvert.SerializeObject(request)}");
                
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

                Console.WriteLine($"Created payment model: {JsonConvert.SerializeObject(payment)}");

                // Lưu vào database
                _context.Payments.Add(payment);
                _context.SaveChanges();
                
                Console.WriteLine($"Payment saved to database with ID: {payment.OrderId}");

                switch (request.PaymentMethod)
                {
                    case "credit-card":
                        return await ProcessCreditCard(payment);
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

        private async Task<IActionResult> ProcessCreditCard(PaymentModel payment)
        {
            // Sử dụng MoMo
            try
            {
                Console.WriteLine($"Processing MoMo payment for amount: {payment.TotalAmount}");
                
                var momoRequest = new OrderInfoModel
                {
                    OrderId = payment.OrderId,
                    Amount = (long)payment.TotalAmount,
                    OrderInfo = $"Thanh toan don hang {payment.OrderId}",
                    FullName = payment.CustomerEmail
                };

                Console.WriteLine($"Created MoMo request: {JsonConvert.SerializeObject(momoRequest)}");
                
                var momoResponse = await _momoService.CreatePaymentAsync(momoRequest);
                
                Console.WriteLine($"MoMo response: {JsonConvert.SerializeObject(momoResponse)}");

                if (momoResponse.IsSuccess && !string.IsNullOrEmpty(momoResponse.PayUrl))
                {
                    payment.PaymentGateway = "momo";
                    payment.PaymentUrl = momoResponse.PayUrl;
                    payment.TransactionId = momoResponse.TransactionId;
                    _context.SaveChanges();

                    return Json(new
                    {
                        success = true,
                        paymentMethod = "credit-card",
                        orderId = payment.OrderId,
                        amount = payment.TotalAmount,
                        redirectUrl = momoResponse.PayUrl,
                        qrCodeUrl = momoResponse.QrCodeUrl,
                        message = "Đang chuyển hướng đến cổng thanh toán MoMo",
                        momoInfo = new
                        {
                            partnerCode = "MOMO",
                            merchantName = "Shopping Demo",
                            supportPhone = "1900 55 55 77",
                            supportEmail = "support@momo.vn"
                        }
                    });
                }
                else
                {
                    Console.WriteLine($"MoMo payment failed: {momoResponse.ResponseMessage}");
                    return Json(new { 
                        success = false,
                        message = $"Không thể tạo thanh toán MoMo: {momoResponse.ResponseMessage}"
                    });
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
        public IActionResult MomoCallback()
        {
            try
            {
                // Xử lý callback từ MoMo
                var momoResponse = _momoService.ProcessCallback(Request.Query);
                
                Console.WriteLine($"MoMo Callback Response: {JsonConvert.SerializeObject(momoResponse)}");

                // Tìm payment trong database
                var payment = _context.Payments.FirstOrDefault(p => p.OrderId == momoResponse.OrderId);
                
                if (payment != null)
                {
                    if (momoResponse.IsSuccess)
                    {
                        // Thanh toán thành công
                        payment.Status = "completed";
                        payment.CompletedAt = DateTime.Now;
                        payment.TransactionId = momoResponse.TransactionId;
                        _context.SaveChanges();

                        return RedirectToAction("PaymentSuccess", new { orderId = momoResponse.OrderId });
                    }
                    else
                    {
                        // Thanh toán thất bại
                        payment.Status = "failed";
                        _context.SaveChanges();

                        return RedirectToAction("PaymentFailed", new { orderId = momoResponse.OrderId, errorCode = momoResponse.ResponseCode });
                    }
                }

                return RedirectToAction("PaymentFailed", new { orderId = momoResponse.OrderId, errorCode = "ORDER_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MomoCallback Error: {ex.Message}");
                return RedirectToAction("PaymentFailed", new { orderId = "", errorCode = "EXCEPTION" });
            }
        }

        [HttpPost]
        public IActionResult MomoNotify()
        {
            try
            {
                // Xử lý notify từ MoMo
                var momoResponse = _momoService.ProcessCallback(Request.Query);
                
                Console.WriteLine($"MoMo Notify Response: {JsonConvert.SerializeObject(momoResponse)}");

                // Tìm payment trong database
                var payment = _context.Payments.FirstOrDefault(p => p.OrderId == momoResponse.OrderId);
                
                if (payment != null)
                {
                    if (momoResponse.IsSuccess)
                    {
                        // Thanh toán thành công
                        payment.Status = "completed";
                        payment.CompletedAt = DateTime.Now;
                        payment.TransactionId = momoResponse.TransactionId;
                        _context.SaveChanges();
                    }
                    else
                    {
                        // Thanh toán thất bại
                        payment.Status = "failed";
                        _context.SaveChanges();
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MomoNotify Error: {ex.Message}");
                return BadRequest();
            }
        }


        [HttpGet]
        public IActionResult DebugPayment()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> TestMomo()
        {
            try
            {
                var testRequest = new OrderInfoModel
                {
                    OrderId = "TEST_" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    Amount = 50000, // 50,000 VND (hợp lệ)
                    OrderInfo = "Test MoMo payment",
                    FullName = "Test Customer"
                };

                Console.WriteLine("=== Testing MoMo API ===");
                var momoResponse = await _momoService.CreatePaymentAsync(testRequest);
                
                if (momoResponse.IsSuccess && !string.IsNullOrEmpty(momoResponse.PayUrl))
                {
                    Console.WriteLine($"MoMo Test Success: {momoResponse.PayUrl}");
                    return Json(new { 
                        success = true, 
                        message = "MoMo API test thành công",
                        paymentUrl = momoResponse.PayUrl,
                        qrCodeUrl = momoResponse.QrCodeUrl
                    });
                }
                else
                {
                    Console.WriteLine($"MoMo Test Failed: {momoResponse.ResponseMessage}");
                    return Json(new { 
                        success = false, 
                        message = $"MoMo API test thất bại: {momoResponse.ResponseMessage}" 
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MoMo Test Exception: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = "MoMo API test exception: " + ex.Message 
                });
            }
        }

        [HttpPost]
        [Route("CreatePaymentUrl")]
        public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
        {
            var response = await _momoService.CreatePaymentAsync(model);
            return Redirect(response.PayUrl);
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

        [HttpPost]
        public async Task<IActionResult> ProcessLargePayment([FromBody] LargePaymentRequestModel request)
        {
            try
            {
                Console.WriteLine($"Processing large payment: {request.Amount:N0} VND");
                
                var result = await _largePaymentService.ProcessLargePayment(
                    request.Amount,
                    request.OrderId ?? GenerateOrderId(),
                    request.CustomerEmail,
                    request.CustomerPhone,
                    request.ShippingAddress
                );

                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Large payment error: {ex.Message}");
                return Json(new LargePaymentResult
                {
                    Success = false,
                    Message = $"Lỗi xử lý giao dịch lớn: {ex.Message}",
                    OrderId = request.OrderId ?? "UNKNOWN",
                    TotalAmount = request.Amount
                });
            }
        }

        [HttpGet]
        public IActionResult LargePaymentInfo()
        {
            var info = new
            {
                success = true,
                message = "Thông tin xử lý giao dịch lớn",
                momoLimit = new
                {
                    maxAmount = 50000000,
                    minAmount = 5000,
                    currency = "VND"
                },
                alternatives = new[]
                {
                    new
                    {
                        method = "bank-transfer",
                        name = "Chuyển khoản ngân hàng",
                        description = "Không giới hạn số tiền",
                        processingTime = "Ngay lập tức"
                    },
                    new
                    {
                        method = "cash",
                        name = "Thanh toán tiền mặt",
                        description = "Tại cửa hàng Tràng Tiền Plaza",
                        processingTime = "Ngay lập tức"
                    },
                    new
                    {
                        method = "installment",
                        name = "Trả góp ngân hàng",
                        description = "Liên hệ ngân hàng để làm thủ tục",
                        processingTime = "1-3 ngày làm việc"
                    }
                },
                splitPayment = new
                {
                    maxParts = 5,
                    description = "Chia giao dịch lớn thành nhiều phần nhỏ để thanh toán qua MoMo"
                }
            };

            return Json(info);
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

    public class LargePaymentRequestModel
    {
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string ShippingAddress { get; set; }
    }
}

