using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;
using System.Text;

namespace Shopping_Demo.Services
{
    public class LocalAIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LocalAIService> _logger;
        private readonly DataContext _context;
        private readonly string _ollamaUrl;
        private readonly string _modelName;

        public LocalAIService(HttpClient httpClient, IConfiguration configuration, ILogger<LocalAIService> logger, DataContext context)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(120); // Tăng timeout lên 2 phút
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _ollamaUrl = _configuration["LocalAI:OllamaUrl"] ?? "http://localhost:11434";
            _modelName = _configuration["LocalAI:ModelName"] ?? "llama3.2:3b";
        }

        public async Task<string> GenerateResponseAsync(string userMessage, string context = "")
        {
            try
            {
                _logger.LogInformation("Generating AI response for: {Message}", userMessage);

                // Lấy thông tin sản phẩm từ database
                var products = await GetRelevantProductsAsync(userMessage);
                var productInfo = FormatProductInfo(products);

                // Tạo prompt đơn giản và hiệu quả
                var systemPrompt = $@"Bạn là trợ lý AI của cửa hàng đồng hồ Aurum Watches tại Hà Nội.
Trả lời bằng tiếng Việt, thân thiện và chuyên nghiệp.

THÔNG TIN SẢN PHẨM:
{productInfo}

THÔNG TIN CỬA HÀNG:
- Địa chỉ: 24 Hai Bà Trưng, Hà Nội
- Hotline: 0388 672 928
- Email: AurumWatches@gmail.com

HƯỚNG DẪN:
1. Khi khách hàng hỏi về sản phẩm, hãy đưa ra link sản phẩm
2. Format link: /Product/Details/{{ID}}
3. Luôn đưa ra thông tin chi tiết: tên, thương hiệu, giá
4. Trả lời ngắn gọn và hữu ích

Trả lời câu hỏi của khách hàng:";

                var fullPrompt = $"{systemPrompt}\n\nKhách hàng: {userMessage}\nTrợ lý AI:";

                var requestBody = new
                {
                    model = _modelName,
                    prompt = fullPrompt,
                    stream = false,
                    options = new
                    {
                        temperature = 0.3,
                        top_p = 0.7,
                        max_tokens = 300,
                        num_predict = 300,
                        num_ctx = 4096,
                        num_gpu = 0,
                        num_thread = 8
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending request to Ollama API");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                var response = await _httpClient.PostAsync($"{_ollamaUrl}/api/generate", content, cts.Token);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<OllamaResponse>(responseContent);
                    
                    if (!string.IsNullOrEmpty(result?.Response))
                    {
                        var cleanedResponse = CleanResponse(result.Response);
                        _logger.LogInformation("AI response generated successfully");
                        return cleanedResponse;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Ollama API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                }

                return "Xin lỗi, tôi đang gặp sự cố kỹ thuật. Vui lòng thử lại sau.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI response");
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
                    QuickReplies = GetQuickReplies(message)
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
                var response = await _httpClient.GetAsync($"{_ollamaUrl}/api/tags");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private async Task<List<ProductModel>> GetRelevantProductsAsync(string userMessage)
        {
            try
            {
                var lowerMessage = userMessage.ToLower();
                var products = new List<ProductModel>();

                // Tìm kiếm sản phẩm dựa trên từ khóa
                if (lowerMessage.Contains("vàng") || lowerMessage.Contains("gold"))
                {
                    products = await _context.Products
                        .Include(p => p.Brand)
                        .Where(p => p.CaseMaterial.Contains("Gold") || p.CaseMaterial.Contains("Vàng"))
                        .Take(5)
                        .ToListAsync();
                }
                else if (lowerMessage.Contains("rolex"))
                {
                    products = await _context.Products
                        .Include(p => p.Brand)
                        .Where(p => p.Brand != null && p.Brand.Name.Contains("Rolex"))
                        .Take(5)
                        .ToListAsync();
                }
                else if (lowerMessage.Contains("omega"))
                {
                    products = await _context.Products
                        .Include(p => p.Brand)
                        .Where(p => p.Brand != null && p.Brand.Name.Contains("Omega"))
                        .Take(5)
                        .ToListAsync();
                }
                else if (lowerMessage.Contains("giá") || lowerMessage.Contains("price"))
                {
                    if (lowerMessage.Contains("rẻ") || lowerMessage.Contains("dưới"))
                    {
                        products = await _context.Products
                            .Include(p => p.Brand)
                            .Where(p => p.Price < 10000000)
                            .OrderBy(p => p.Price)
                            .Take(5)
                            .ToListAsync();
                    }
                    else
                    {
                        products = await _context.Products
                            .Include(p => p.Brand)
                            .OrderBy(p => p.Price)
                            .Take(5)
                            .ToListAsync();
                    }
                }
                else
                {
                    // Mặc định: lấy sản phẩm phổ biến
                    products = await _context.Products
                        .Include(p => p.Brand)
                        .Where(p => p.IsActive)
                        .OrderByDescending(p => p.Sold)
                        .Take(5)
                        .ToListAsync();
                }

                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return new List<ProductModel>();
            }
        }

        private string FormatProductInfo(List<ProductModel> products)
        {
            if (!products.Any())
                return "Hiện tại chưa có sản phẩm phù hợp.";

            var info = new StringBuilder();
            info.AppendLine("SẢN PHẨM CÓ SẴN:");
            
            foreach (var product in products)
            {
                var brandName = product.Brand?.Name ?? "Không rõ";
                var productUrl = $"/Product/Details/{product.Id}";
                info.AppendLine($"- {product.Name} ({brandName}) - {product.Price:N0} VNĐ - Link: {productUrl}");
            }

            return info.ToString();
        }

        private string CleanResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
                return "Xin lỗi, tôi không hiểu câu hỏi của bạn. Vui lòng thử lại.";

            response = response.Trim();
            
            // Chuyển đổi link thành HTML link có thể click được
            response = ConvertLinksToHtml(response);
            
            if (response.Length > 2000)
                response = response.Substring(0, 2000) + "...";

            return response;
        }

        private string ConvertLinksToHtml(string response)
        {
            var linkPattern = @"(/Product/Details/\d+)";
            var matches = System.Text.RegularExpressions.Regex.Matches(response, linkPattern);
            
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var link = match.Value;
                var productId = System.Text.RegularExpressions.Regex.Match(link, @"\d+").Value;
                var htmlLink = $"<a href=\"{link}\" target=\"_blank\" style=\"color: #007bff; text-decoration: underline; font-weight: bold;\">Xem sản phẩm #{productId}</a>";
                response = response.Replace(link, htmlLink);
            }
            
            return response;
        }

        private List<string> GetQuickReplies(string userMessage)
        {
            var lowerMessage = userMessage.ToLower();
            
            if (lowerMessage.Contains("giá") || lowerMessage.Contains("price"))
            {
                return new List<string> { "Xem giá cụ thể", "So sánh giá", "Khuyến mãi" };
            }
            else if (lowerMessage.Contains("rolex") || lowerMessage.Contains("omega"))
            {
                return new List<string> { "Xem thêm Rolex", "Xem thêm Omega", "Tư vấn thương hiệu" };
            }
            else if (lowerMessage.Contains("vàng") || lowerMessage.Contains("gold"))
            {
                return new List<string> { "Xem đồng hồ vàng", "Tư vấn chất liệu", "So sánh giá" };
            }
            else
            {
                return new List<string> { "Tư vấn thêm", "Xem sản phẩm", "Liên hệ hỗ trợ" };
            }
        }
    }

    public class OllamaResponse
    {
        [JsonProperty("response")]
        public string Response { get; set; } = "";
        
        [JsonProperty("done")]
        public bool Done { get; set; }
    }
}