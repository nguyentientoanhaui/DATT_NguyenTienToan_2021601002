using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shopping_Demo.Models;
using System.Text;

namespace Shopping_Demo.Services
{
    public class CohereAIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CohereAIService> _logger;
        private readonly string _apiKey;
        private readonly string _modelName;

        public CohereAIService(HttpClient httpClient, IConfiguration configuration, ILogger<CohereAIService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _apiKey = _configuration["Cohere:ApiKey"] ?? "";
            _modelName = _configuration["Cohere:ModelName"] ?? "command";
            
            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }
        }

        public async Task<string> GenerateResponseAsync(string userMessage, string context = "")
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogWarning("Cohere API key not configured");
                    return "Xin lỗi, tôi chưa được cấu hình để sử dụng AI. Vui lòng liên hệ admin.";
                }

                // Tạo system prompt chuyên nghiệp
                var systemPrompt = @"Bạn là trợ lý AI thông minh và chuyên nghiệp của cửa hàng đồng hồ cao cấp Aurum Watches tại Hà Nội.

KIẾN THỨC CHUYÊN MÔN:
- Đồng hồ cao cấp: Rolex, Patek Philippe, Audemars Piguet, Vacheron Constantin, Omega, Cartier, etc.
- Phân khúc giá: từ vài triệu đến hàng tỷ đồng
- Các dòng sản phẩm: cổ điển, thể thao, sang trọng, complications
- Chất liệu: vàng, bạch kim, thép không gỉ, ceramic, titanium
- Công nghệ: automatic, quartz, tourbillon, chronograph

PHONG CÁCH GIAO TIẾP:
- Thân thiện, chuyên nghiệp, nhiệt tình
- Trả lời bằng tiếng Việt tự nhiên
- Ngắn gọn nhưng đầy đủ thông tin
- Luôn đề xuất sản phẩm cụ thể khi có thể
- Hướng dẫn khách hàng đến showroom khi cần thiết

THÔNG TIN CỬA HÀNG:
- Địa chỉ: 24 Hai Bà Trưng, Hà Nội
- Hotline: 0388 672 928
- Email: AurumWatches@gmail.com
- Chính sách: bảo hành chính hãng, đổi trả 30 ngày, miễn phí vận chuyển

Hãy trả lời câu hỏi của khách hàng một cách thông minh và hữu ích nhất.";

                var fullPrompt = $"{systemPrompt}\n\n";
                
                // Thêm ngữ cảnh nếu có
                if (!string.IsNullOrEmpty(context))
                {
                    fullPrompt += $"{context}\n";
                }
                
                fullPrompt += $"Khách hàng hỏi: {userMessage}\nTrợ lý AI trả lời:";

                var requestBody = new
                {
                    model = _modelName,
                    message = fullPrompt,
                    max_tokens = 1000,
                    temperature = 0.7,
                    p = 0.9,
                    k = 0,
                    stop_sequences = new string[0],
                    return_likelihoods = "NONE"
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending request to Cohere API: {Model}", _modelName);

                var response = await _httpClient.PostAsync("https://api.cohere.ai/v1/generate", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Cohere API response: {Response}", responseContent);
                    
                    var result = JsonConvert.DeserializeObject<CohereResponse>(responseContent);
                    
                    if (result?.Generations != null && result.Generations.Count > 0)
                    {
                        var generatedText = result.Generations[0].Text;
                        var cleanedResponse = CleanResponse(generatedText);
                        
                        _logger.LogInformation("AI generated response: {Response}", cleanedResponse);
                        return cleanedResponse;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Cohere API error: {StatusCode} - {Content}", 
                        response.StatusCode, errorContent);
                }

                return "Xin lỗi, tôi gặp sự cố kỹ thuật. Vui lòng thử lại sau hoặc liên hệ nhân viên tư vấn.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Cohere API");
                return "Xin lỗi, tôi gặp sự cố kỹ thuật. Vui lòng thử lại sau hoặc liên hệ nhân viên tư vấn.";
            }
        }

        public async Task<ChatResponse> ProcessWithAIAsync(string message, string sessionId, string? userId = null)
        {
            try
            {
                if (ShouldUseAI(message))
                {
                    var aiResponse = await GenerateResponseAsync(message);
                    
                    return new ChatResponse
                    {
                        Message = aiResponse,
                        SessionId = sessionId,
                        QuickReplies = GetSmartQuickReplies(message, aiResponse)
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI processing");
                return new ChatResponse
                {
                    Message = "Xin lỗi, tôi gặp sự cố kỹ thuật. Vui lòng thử lại sau.",
                    SessionId = sessionId
                };
            }
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                    return false;

                var response = await _httpClient.GetAsync("https://api.cohere.ai/v1/models");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private bool ShouldUseAI(string message)
        {
            var lowerMessage = message.ToLower();
            
            // Chỉ bỏ qua AI cho những câu đơn giản nhất
            var simpleGreetings = new[]
            {
                "hi", "hello", "xin chào", "chào", "hey", "ok", "okay", "cảm ơn", "thanks"
            };

            // Nếu chỉ là chào hỏi đơn giản và ngắn, không dùng AI
            if (simpleGreetings.Any(greeting => lowerMessage == greeting || lowerMessage == greeting + "!"))
            {
                return false;
            }

            // Sử dụng AI cho hầu hết các câu hỏi khác
            return true;
        }

        private List<string> GetSmartQuickReplies(string userMessage, string aiResponse)
        {
            var lowerMessage = userMessage.ToLower();
            var lowerResponse = aiResponse.ToLower();
            
            var replies = new List<string>();

            // Dựa trên nội dung câu hỏi và phản hồi để tạo quick replies phù hợp
            if (lowerMessage.Contains("giá") || lowerMessage.Contains("price") || lowerResponse.Contains("giá"))
            {
                replies.AddRange(new[] { "Xem giá cụ thể", "So sánh giá", "Khuyến mãi" });
            }
            else if (lowerMessage.Contains("so sánh") || lowerMessage.Contains("compare") || lowerResponse.Contains("so sánh"))
            {
                replies.AddRange(new[] { "So sánh chi tiết", "Xem thêm thương hiệu", "Tư vấn cá nhân" });
            }
            else if (lowerMessage.Contains("tư vấn") || lowerMessage.Contains("gợi ý") || lowerResponse.Contains("tư vấn"))
            {
                replies.AddRange(new[] { "Tư vấn chi tiết", "Xem sản phẩm", "Đặt lịch tư vấn" });
            }
            else if (lowerMessage.Contains("đơn hàng") || lowerMessage.Contains("order") || lowerResponse.Contains("đơn hàng"))
            {
                replies.AddRange(new[] { "Kiểm tra đơn hàng", "Theo dõi vận chuyển", "Liên hệ hỗ trợ" });
            }
            else if (lowerResponse.Contains("showroom") || lowerResponse.Contains("địa chỉ"))
            {
                replies.AddRange(new[] { "Xem địa chỉ", "Đặt lịch xem", "Liên hệ hotline" });
            }
            else
            {
                // Quick replies chung
                replies.AddRange(new[] { "Tư vấn thêm", "Xem sản phẩm", "Liên hệ hỗ trợ" });
            }

            return replies.Take(3).ToList();
        }

        private string CleanResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
                return "Xin lỗi, tôi không hiểu câu hỏi của bạn. Vui lòng thử lại.";

            // Loại bỏ các ký tự không cần thiết
            response = response.Trim();
            
            // Giới hạn độ dài
            if (response.Length > 2000)
                response = response.Substring(0, 2000) + "...";

            return response;
        }
    }

    // Model classes for Cohere API response
    public class CohereResponse
    {
        [JsonProperty("generations")]
        public List<CohereGeneration> Generations { get; set; } = new();
    }

    public class CohereGeneration
    {
        [JsonProperty("text")]
        public string Text { get; set; } = "";
    }
}
