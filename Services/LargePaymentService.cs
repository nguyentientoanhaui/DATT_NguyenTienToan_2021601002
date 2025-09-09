using Shopping_Demo.Models;
using Shopping_Demo.Repository;
using Shopping_Demo.Services;
using System.Text.Json;

namespace Shopping_Demo.Services
{
    public interface ILargePaymentService
    {
        Task<LargePaymentResult> ProcessLargePayment(decimal totalAmount, string orderId, string customerEmail, string customerPhone, string shippingAddress);
    }

    public class LargePaymentService : ILargePaymentService
    {
        private readonly DataContext _context;
        private readonly IMoMoService _momoService;
        private const decimal MAX_MOMO_AMOUNT = 50000000; // 50 triệu đồng
        private const decimal MIN_PAYMENT_AMOUNT = 1000; // 1,000 VND

        public LargePaymentService(DataContext context, IMoMoService momoService)
        {
            _context = context;
            _momoService = momoService;
        }

        public async Task<LargePaymentResult> ProcessLargePayment(decimal totalAmount, string orderId, string customerEmail, string customerPhone, string shippingAddress)
        {
            try
            {
                Console.WriteLine($"Processing large payment: {totalAmount:N0} VND for order {orderId}");

                var result = new LargePaymentResult
                {
                    OrderId = orderId,
                    TotalAmount = totalAmount,
                    PaymentMethod = "large-payment",
                    Status = "processing"
                };

                if (totalAmount <= MAX_MOMO_AMOUNT)
                {
                    // Giao dịch nhỏ, sử dụng MoMo bình thường
                    result.PaymentMethod = "momo";
                    result.Message = "Giao dịch có thể thanh toán qua MoMo";
                    result.CanUseMomo = true;
                }
                else
                {
                    // Giao dịch lớn, chia nhỏ hoặc đề xuất phương thức khác
                    result.CanUseMomo = false;
                    result.Message = "Giao dịch quá lớn cho MoMo, đề xuất phương thức thanh toán khác";
                    
                    // Tính số lần thanh toán cần thiết
                    var paymentCount = (int)Math.Ceiling(totalAmount / MAX_MOMO_AMOUNT);
                    result.SuggestedPayments = new List<PaymentSuggestion>();

                    // Đề xuất chia nhỏ
                    if (paymentCount <= 5) // Chỉ chia tối đa 5 lần
                    {
                        var amountPerPayment = totalAmount / paymentCount;
                        for (int i = 0; i < paymentCount; i++)
                        {
                            var paymentOrderId = $"{orderId}_PART_{i + 1}";
                            var amount = i == paymentCount - 1 ? 
                                totalAmount - (amountPerPayment * (paymentCount - 1)) : 
                                amountPerPayment;

                            result.SuggestedPayments.Add(new PaymentSuggestion
                            {
                                OrderId = paymentOrderId,
                                Amount = Math.Round(amount, 0),
                                PaymentMethod = "momo",
                                Description = $"Thanh toán phần {i + 1}/{paymentCount} cho đơn hàng {orderId}"
                            });
                        }
                    }

                    // Đề xuất phương thức thanh toán khác
                    result.AlternativePayments = new List<AlternativePayment>
                    {
                        new AlternativePayment
                        {
                            Method = "bank-transfer",
                            Name = "Chuyển khoản ngân hàng",
                            Description = "Chuyển khoản trực tiếp đến tài khoản ngân hàng",
                            BankInfo = new BankTransferInfo
                            {
                                BankName = "Ngân hàng TMCP Việt Nam Thịnh Vượng (VPBank)",
                                AccountNumber = "1234567890",
                                AccountName = "CÔNG TY TNHH ĐỒNG HỒ CAO CẤP",
                                Branch = "Chi nhánh Hà Nội",
                                SwiftCode = "BFTVVNVX",
                                Amount = totalAmount,
                                Content = orderId
                            }
                        },
                        new AlternativePayment
                        {
                            Method = "cash",
                            Name = "Thanh toán tiền mặt tại cửa hàng",
                            Description = "Đến cửa hàng để thanh toán trực tiếp",
                            StoreInfo = new StoreInfo
                            {
                                Name = "CỬA HÀNG ĐỒNG HỒ CAO CẤP - TRÀNG TIỀN PLAZA",
                                Address = "01-05, Tầng 1 Tràng Tiền Plaza, 24 Hai Bà Trưng, Phường Cửa Nam, Quận Hoàn Kiếm, Thành phố Hà Nội",
                                Phone = "0388672928",
                                WorkingHours = "9:00 - 22:00 (Thứ 2 - Chủ nhật)"
                            }
                        },
                        new AlternativePayment
                        {
                            Method = "installment",
                            Name = "Trả góp qua ngân hàng",
                            Description = "Liên hệ ngân hàng để làm thủ tục trả góp",
                            Banks = new List<BankInfo>
                            {
                                new BankInfo { Name = "Vietcombank", MaxAmount = 500000000, Tenure = "6-12 tháng" },
                                new BankInfo { Name = "Techcombank", MaxAmount = 300000000, Tenure = "6-24 tháng" },
                                new BankInfo { Name = "BIDV", MaxAmount = 500000000, Tenure = "6-18 tháng" }
                            }
                        }
                    };
                }

                // Lưu thông tin giao dịch lớn
                var largePayment = new LargePaymentModel
                {
                    OrderId = orderId,
                    TotalAmount = totalAmount,
                    CustomerEmail = customerEmail,
                    CustomerPhone = customerPhone,
                    ShippingAddress = shippingAddress,
                    Status = result.Status,
                    CreatedAt = DateTime.Now,
                    PaymentMethod = result.PaymentMethod
                };

                _context.LargePayments.Add(largePayment);
                await _context.SaveChangesAsync();

                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing large payment: {ex.Message}");
                return new LargePaymentResult
                {
                    Success = false,
                    Message = $"Lỗi xử lý giao dịch lớn: {ex.Message}",
                    OrderId = orderId,
                    TotalAmount = totalAmount
                };
            }
        }
    }

    public class LargePaymentResult
    {
        public bool Success { get; set; }
        public string OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public bool CanUseMomo { get; set; }
        public List<PaymentSuggestion> SuggestedPayments { get; set; } = new();
        public List<AlternativePayment> AlternativePayments { get; set; } = new();
    }

    public class PaymentSuggestion
    {
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Description { get; set; }
    }

    public class AlternativePayment
    {
        public string Method { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public BankTransferInfo BankInfo { get; set; }
        public StoreInfo StoreInfo { get; set; }
        public List<BankInfo> Banks { get; set; }
    }

    public class BankTransferInfo
    {
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string Branch { get; set; }
        public string SwiftCode { get; set; }
        public decimal Amount { get; set; }
        public string Content { get; set; }
    }

    public class StoreInfo
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string WorkingHours { get; set; }
    }

    public class BankInfo
    {
        public string Name { get; set; }
        public decimal MaxAmount { get; set; }
        public string Tenure { get; set; }
    }
}
