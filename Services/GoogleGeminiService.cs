using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;
using System.Text;

namespace Shopping_Demo.Services
{
    public class GoogleGeminiService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleGeminiService> _logger;
        private readonly DataContext _context;
        private readonly string _apiKey;
        private readonly string _modelName;

        public GoogleGeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GoogleGeminiService> logger, DataContext context)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _apiKey = _configuration["GoogleGemini:ApiKey"] ?? "";
            _modelName = _configuration["GoogleGemini:ModelName"] ?? "gemini-pro";
        }

        public async Task<string> GenerateResponseAsync(string userMessage, string context = "")
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogWarning("Google Gemini API key not configured");
                    return "Xin lỗi, tôi chưa được cấu hình để sử dụng AI. Vui lòng liên hệ admin.";
                }

                // Lấy thông tin từ database (giới hạn độ dài)
                var databaseInfo = await GetDatabaseInfoAsync(userMessage);
                
                // Giới hạn độ dài database info để tránh prompt quá dài
                if (databaseInfo.Length > 1000)
                {
                    databaseInfo = databaseInfo.Substring(0, 1000) + "...";
                }

                // Tạo system prompt ngắn gọn hơn
                var systemPrompt = @"Bạn là trợ lý AI của cửa hàng đồng hồ Aurum Watches tại Hà Nội.
Trả lời bằng tiếng Việt, thân thiện và chuyên nghiệp.
Sử dụng thông tin sản phẩm dưới đây để trả lời chính xác:

THÔNG TIN SẢN PHẨM:
" + databaseInfo + @"

THÔNG TIN CỬA HÀNG:
- Địa chỉ: 24 Hai Bà Trưng, Hà Nội
- Hotline: 0388 672 928
- Email: AurumWatches@gmail.com";

                var fullPrompt = $"{systemPrompt}\n\n";
                
                // Thêm ngữ cảnh nếu có
                if (!string.IsNullOrEmpty(context))
                {
                    fullPrompt += $"{context}\n";
                }
                
                fullPrompt += $"Khách hàng hỏi: {userMessage}\nTrợ lý AI trả lời:";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = fullPrompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        topK = 40,
                        topP = 0.95,
                        maxOutputTokens = 300,
                        stopSequences = new string[0]
                    },
                    safetySettings = new[]
                    {
                        new
                        {
                            category = "HARM_CATEGORY_HARASSMENT",
                            threshold = "BLOCK_MEDIUM_AND_ABOVE"
                        },
                        new
                        {
                            category = "HARM_CATEGORY_HATE_SPEECH",
                            threshold = "BLOCK_MEDIUM_AND_ABOVE"
                        },
                        new
                        {
                            category = "HARM_CATEGORY_SEXUALLY_EXPLICIT",
                            threshold = "BLOCK_MEDIUM_AND_ABOVE"
                        },
                        new
                        {
                            category = "HARM_CATEGORY_DANGEROUS_CONTENT",
                            threshold = "BLOCK_MEDIUM_AND_ABOVE"
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending request to Google Gemini API: {Model}", _modelName);

                var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent?key={_apiKey}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Google Gemini API response: {Response}", responseContent);
                    
                    var result = JsonConvert.DeserializeObject<GeminiResponse>(responseContent);
                    
                    if (result?.Candidates != null && result.Candidates.Count > 0 && 
                        result.Candidates[0].Content?.Parts != null && result.Candidates[0].Content.Parts.Count > 0)
                    {
                        var generatedText = result.Candidates[0].Content.Parts[0].Text;
                        var cleanedResponse = CleanResponse(generatedText);
                        
                        _logger.LogInformation("AI generated response: {Response}", cleanedResponse);
                        return cleanedResponse;
                    }
                    else
                    {
                        _logger.LogWarning("No valid response from Gemini API");
                        return "Xin lỗi, tôi không thể tạo phản hồi phù hợp. Vui lòng thử lại.";
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Google Gemini API error: {StatusCode} - {Content}", 
                        response.StatusCode, errorContent);
                    
                    // Trả về thông báo lỗi cụ thể hơn
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return "Xin lỗi, API key không hợp lệ. Vui lòng liên hệ admin.";
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        return "Xin lỗi, đã vượt quá giới hạn API. Vui lòng thử lại sau.";
                    }
                }

                return "Xin lỗi, tôi gặp sự cố kỹ thuật. Vui lòng thử lại sau hoặc liên hệ nhân viên tư vấn.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Google Gemini API");
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

                var response = await _httpClient.GetAsync($"https://generativelanguage.googleapis.com/v1beta/models?key={_apiKey}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private bool ShouldUseAI(string message)
        {
            // Luôn sử dụng AI cho tất cả câu hỏi
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

        private async Task<string> GetDatabaseInfoAsync(string userMessage)
        {
            try
            {
                var lowerMessage = userMessage.ToLower();
                var info = new StringBuilder();

                // Lấy thông tin sản phẩm (giới hạn 10 sản phẩm)
                var products = await _context.Products
                    .Include(p => p.Brand)
                    .Take(10)
                    .ToListAsync();

                if (products.Any())
                {
                    info.AppendLine("SẢN PHẨM:");
                    foreach (var product in products)
                    {
                        var brandName = product.Brand?.Name ?? "Không rõ";
                        info.AppendLine($"- {product.Name} ({brandName}) - {product.Price:N0} VNĐ");
                    }
                }

                // Lấy thông tin thương hiệu (giới hạn 5)
                var brands = await _context.Brands.Take(5).ToListAsync();
                if (brands.Any())
                {
                    info.AppendLine("\nTHƯƠNG HIỆU:");
                    foreach (var brand in brands)
                    {
                        info.AppendLine($"- {brand.Name}");
                    }
                }

                // Nếu câu hỏi về giá cả cụ thể
                if (lowerMessage.Contains("giá") || lowerMessage.Contains("price"))
                {
                    var priceRanges = products.GroupBy(p => p.Price switch
                    {
                        < 10000000 => "Dưới 10 triệu",
                        < 50000000 => "10-50 triệu",
                        < 100000000 => "50-100 triệu",
                        < 500000000 => "100-500 triệu",
                        _ => "Trên 500 triệu"
                    }).ToList();

                    info.AppendLine("\nGIÁ:");
                    foreach (var range in priceRanges)
                    {
                        info.AppendLine($"- {range.Key}: {range.Count()} sản phẩm");
                    }
                }

                return info.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database info");
                return "Có sản phẩm đồng hồ cao cấp từ các thương hiệu nổi tiếng.";
            }
        }
    }

    // Model classes for Google Gemini API response
    public class GeminiResponse
    {
        [JsonProperty("candidates")]
        public List<GeminiCandidate> Candidates { get; set; } = new();
    }

    public class GeminiCandidate
    {
        [JsonProperty("content")]
        public GeminiContent Content { get; set; } = new();
    }

    public class GeminiContent
    {
        [JsonProperty("parts")]
        public List<GeminiPart> Parts { get; set; } = new();
    }

    public class GeminiPart
    {
        [JsonProperty("text")]
        public string Text { get; set; } = "";
    }
}
