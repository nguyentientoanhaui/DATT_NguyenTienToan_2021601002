using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;
using System.Text;

namespace Shopping_Demo.Services
{
    public class ONNXAIService : IAIService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ONNXAIService> _logger;
        private readonly DataContext _context;
        private readonly Dictionary<string, string> _responses;

        public ONNXAIService(IConfiguration configuration, ILogger<ONNXAIService> logger, DataContext context)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _responses = InitializeResponses();
        }

        public async Task<string> GenerateResponseAsync(string userMessage, string context = "")
        {
            try
            {
                // Lấy thông tin từ database
                var databaseInfo = await GetDatabaseInfoAsync(userMessage);
                
                // Phân tích câu hỏi và tạo phản hồi thông minh
                var response = await AnalyzeAndRespondAsync(userMessage, databaseInfo);
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ONNX AI processing");
                return "Xin lỗi, tôi đang gặp sự cố kỹ thuật. Vui lòng thử lại sau.";
            }
        }

        public async Task<ChatResponse> ProcessWithAIAsync(string message, string sessionId, string? userId = null)
        {
            try
            {
                var aiResponse = await GenerateResponseAsync(message);
                
                return new ChatResponse
                {
                    Message = aiResponse,
                    SessionId = sessionId,
                    QuickReplies = GetSmartQuickReplies(message, aiResponse)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI processing");
                return new ChatResponse
                {
                    Message = "Xin lỗi, tôi đang gặp sự cố kỹ thuật. Vui lòng thử lại sau.",
                    SessionId = sessionId
                };
            }
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                // Test với một câu hỏi đơn giản
                var testResponse = await GenerateResponseAsync("xin chào");
                return !string.IsNullOrEmpty(testResponse) && !testResponse.Contains("sự cố kỹ thuật");
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> AnalyzeAndRespondAsync(string userMessage, string databaseInfo)
        {
            var lowerMessage = userMessage.ToLower();
            
            // Phân tích intent và tạo phản hồi thông minh
            if (lowerMessage.Contains("xin chào") || lowerMessage.Contains("hello") || lowerMessage.Contains("hi"))
            {
                return "Xin chào! Tôi là trợ lý AI của cửa hàng đồng hồ Aurum Watches. Tôi có thể giúp bạn tư vấn về đồng hồ, kiểm tra đơn hàng và hỗ trợ mua sắm. Bạn cần hỗ trợ gì?";
            }
            
            if (lowerMessage.Contains("giá") || lowerMessage.Contains("price"))
            {
                return await GeneratePriceResponseAsync(databaseInfo);
            }
            
            if (lowerMessage.Contains("sản phẩm") || lowerMessage.Contains("đồng hồ"))
            {
                return await GenerateProductResponseAsync(databaseInfo);
            }
            
            if (lowerMessage.Contains("tư vấn") || lowerMessage.Contains("gợi ý"))
            {
                return await GenerateAdviceResponseAsync(userMessage, databaseInfo);
            }
            
            if (lowerMessage.Contains("so sánh"))
            {
                return await GenerateComparisonResponseAsync(userMessage, databaseInfo);
            }
            
            if (lowerMessage.Contains("đơn hàng") || lowerMessage.Contains("order"))
            {
                return "Để kiểm tra đơn hàng, bạn có thể đăng nhập vào tài khoản tại trang web hoặc liên hệ hotline: 0388 672 928 để được hỗ trợ.";
            }
            
            // Phản hồi mặc định thông minh
            return await GenerateDefaultResponseAsync(userMessage, databaseInfo);
        }

        private async Task<string> GeneratePriceResponseAsync(string databaseInfo)
        {
            var products = await _context.Products
                .Include(p => p.Brand)
                .Take(5)
                .ToListAsync();

            if (products.Any())
            {
                var response = new StringBuilder();
                response.AppendLine("**Giá đồng hồ tại cửa hàng:**\n");
                
                foreach (var product in products)
                {
                    var brandName = product.Brand?.Name ?? "Không rõ";
                    response.AppendLine($"• {product.Name} ({brandName}): {product.Price:N0} VNĐ");
                }
                
                response.AppendLine("\nChúng tôi có nhiều phân khúc giá từ vài triệu đến hàng tỷ đồng. Bạn quan tâm đến ngân sách nào?");
                return response.ToString();
            }
            
            return "Chúng tôi có đồng hồ với nhiều phân khúc giá khác nhau. Bạn có thể đến showroom tại 24 Hai Bà Trưng, Hà Nội để xem và trải nghiệm trực tiếp.";
        }

        private async Task<string> GenerateProductResponseAsync(string databaseInfo)
        {
            var products = await _context.Products
                .Include(p => p.Brand)
                .Take(6)
                .ToListAsync();

            if (products.Any())
            {
                var response = new StringBuilder();
                response.AppendLine("**Sản phẩm nổi bật:**\n");
                
                foreach (var product in products)
                {
                    var brandName = product.Brand?.Name ?? "Không rõ";
                    response.AppendLine($"• {product.Name} ({brandName})");
                }
                
                response.AppendLine("\nBạn quan tâm đến dòng đồng hồ nào? Tôi có thể tư vấn chi tiết hơn.");
                return response.ToString();
            }
            
            return "Chúng tôi có nhiều dòng đồng hồ cao cấp từ các thương hiệu nổi tiếng. Bạn có thể xem chi tiết tại trang sản phẩm hoặc đến showroom để trải nghiệm.";
        }

        private async Task<string> GenerateAdviceResponseAsync(string userMessage, string databaseInfo)
        {
            var lowerMessage = userMessage.ToLower();
            
            if (lowerMessage.Contains("nam") || lowerMessage.Contains("đàn ông"))
            {
                return "**Gợi ý đồng hồ nam:**\n\n" +
                       "• **Công việc văn phòng:** Rolex Datejust, Omega De Ville\n" +
                       "• **Thể thao:** Rolex Submariner, Omega Seamaster\n" +
                       "• **Sang trọng:** Patek Philippe Calatrava\n\n" +
                       "Bạn có ngân sách dự kiến bao nhiêu để tôi tư vấn cụ thể hơn?";
            }
            
            if (lowerMessage.Contains("nữ") || lowerMessage.Contains("phụ nữ"))
            {
                return "**Gợi ý đồng hồ nữ:**\n\n" +
                       "• **Thanh lịch:** Cartier Tank, Omega Constellation\n" +
                       "• **Thể thao:** Omega Speedmaster, TAG Heuer Formula 1\n" +
                       "• **Sang trọng:** Rolex Lady-Datejust\n\n" +
                       "Bạn thích phong cách nào? Tôi có thể gợi ý cụ thể hơn.";
            }
            
            return "Tôi rất vui được tư vấn cho bạn! Để đưa ra gợi ý phù hợp nhất, bạn có thể chia sẻ:\n\n" +
                   "• **Giới tính và độ tuổi**\n" +
                   "• **Ngân sách dự kiến**\n" +
                   "• **Phong cách yêu thích**\n" +
                   "• **Mục đích sử dụng**\n\n" +
                   "Hoặc bạn có thể đến showroom tại 24 Hai Bà Trưng, Hà Nội để được tư vấn trực tiếp!";
        }

        private async Task<string> GenerateComparisonResponseAsync(string userMessage, string databaseInfo)
        {
            var lowerMessage = userMessage.ToLower();
            
            if (lowerMessage.Contains("rolex") && lowerMessage.Contains("omega"))
            {
                return "**So sánh Rolex vs Omega:**\n\n" +
                       "**Rolex:**\n" +
                       "• Thương hiệu cao cấp nhất thế giới\n" +
                       "• Giá từ 200-500 triệu\n" +
                       "• Độ chính xác cao, bền bỉ\n" +
                       "• Phù hợp: Doanh nhân, sưu tập\n\n" +
                       "**Omega:**\n" +
                       "• Thương hiệu Thụy Sĩ danh tiếng\n" +
                       "• Giá từ 50-200 triệu\n" +
                       "• Công nghệ Co-Axial tiên tiến\n" +
                       "• Phù hợp: Thể thao, hàng ngày\n\n" +
                       "Bạn muốn xem chi tiết dòng nào?";
            }
            
            return "Tôi có thể so sánh các thương hiệu đồng hồ cho bạn:\n\n" +
                   "• Rolex vs Omega\n" +
                   "• Patek Philippe vs Vacheron Constantin\n" +
                   "• Cartier vs Bulgari\n\n" +
                   "Bạn muốn so sánh thương hiệu nào cụ thể?";
        }

        private async Task<string> GenerateDefaultResponseAsync(string userMessage, string databaseInfo)
        {
            return "Tôi hiểu bạn đang quan tâm đến: \"" + userMessage + "\". \n\n" +
                   "Để hỗ trợ bạn tốt nhất, bạn có thể:\n" +
                   "• **Mô tả cụ thể hơn** về nhu cầu\n" +
                   "• **Chia sẻ ngân sách** dự kiến\n" +
                   "• **Đến showroom** để trải nghiệm trực tiếp\n" +
                   "• **Liên hệ hotline:** 0388 672 928\n\n" +
                   "Tôi luôn sẵn sàng hỗ trợ bạn!";
        }

        private List<string> GetSmartQuickReplies(string userMessage, string aiResponse)
        {
            var lowerMessage = userMessage.ToLower();
            var replies = new List<string>();

            if (lowerMessage.Contains("giá") || lowerMessage.Contains("price"))
            {
                replies.AddRange(new[] { "Xem giá cụ thể", "So sánh giá", "Khuyến mãi" });
            }
            else if (lowerMessage.Contains("so sánh"))
            {
                replies.AddRange(new[] { "So sánh chi tiết", "Xem thêm thương hiệu", "Tư vấn cá nhân" });
            }
            else if (lowerMessage.Contains("tư vấn"))
            {
                replies.AddRange(new[] { "Tư vấn chi tiết", "Xem sản phẩm", "Đặt lịch tư vấn" });
            }
            else
            {
                replies.AddRange(new[] { "Tư vấn thêm", "Xem sản phẩm", "Liên hệ hỗ trợ" });
            }

            return replies.Take(3).ToList();
        }

        private async Task<string> GetDatabaseInfoAsync(string userMessage)
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Brand)
                    .Take(5)
                    .ToListAsync();

                var info = new StringBuilder();
                if (products.Any())
                {
                    info.AppendLine("SẢN PHẨM:");
                    foreach (var product in products)
                    {
                        var brandName = product.Brand?.Name ?? "Không rõ";
                        info.AppendLine($"- {product.Name} ({brandName}) - {product.Price:N0} VNĐ");
                    }
                }

                return info.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database info");
                return "";
            }
        }

        private Dictionary<string, string> InitializeResponses()
        {
            return new Dictionary<string, string>
            {
                { "chào", "Xin chào! Tôi là trợ lý AI của cửa hàng đồng hồ Aurum Watches. Tôi có thể giúp bạn tư vấn về đồng hồ, kiểm tra đơn hàng và hỗ trợ mua sắm." },
                { "giá", "Chúng tôi có đồng hồ với nhiều phân khúc giá khác nhau. Bạn có thể đến showroom để xem và trải nghiệm trực tiếp." },
                { "sản phẩm", "Chúng tôi có nhiều dòng đồng hồ cao cấp từ các thương hiệu nổi tiếng như Rolex, Omega, Patek Philippe, Cartier." }
            };
        }
    }
}
