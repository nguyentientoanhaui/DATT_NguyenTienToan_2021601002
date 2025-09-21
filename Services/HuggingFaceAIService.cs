using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shopping_Demo.Models;
using System.Text;

namespace Shopping_Demo.Services
{
    public class HuggingFaceAIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HuggingFaceAIService> _logger;
        private readonly string _apiKey;
        private readonly string _modelName;

        public HuggingFaceAIService(HttpClient httpClient, IConfiguration configuration, ILogger<HuggingFaceAIService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _apiKey = _configuration["HuggingFace:ApiKey"] ?? "";
            _modelName = _configuration["HuggingFace:ModelName"] ?? "microsoft/DialoGPT-medium";
            
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<string> GenerateResponseAsync(string userMessage, string context = "")
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogWarning("HuggingFace API key not configured");
                    return "Xin lỗi, tôi chưa được cấu hình để sử dụng AI. Vui lòng liên hệ admin.";
                }

                // Tạo prompt phù hợp với ngữ cảnh bán đồng hồ
                var systemPrompt = @"Bạn là trợ lý AI chuyên nghiệp của cửa hàng đồng hồ cao cấp Aurum Watches tại Hà Nội. 
Bạn có kiến thức sâu về đồng hồ, thương hiệu, và tư vấn khách hàng.
Hãy trả lời một cách thân thiện, chuyên nghiệp và hữu ích bằng tiếng Việt.
Nếu không biết câu trả lời cụ thể, hãy đề xuất liên hệ với nhân viên tư vấn.
Trả lời ngắn gọn, dễ hiểu và tập trung vào đồng hồ.";

                var fullPrompt = $"{systemPrompt}\n\n";
                
                // Thêm ngữ cảnh nếu có
                if (!string.IsNullOrEmpty(context))
                {
                    fullPrompt += $"{context}\n";
                }
                else
                {
                    fullPrompt += $"Khách hàng hỏi: {userMessage}\nTrợ lý AI trả lời:";
                }

                var requestBody = new
                {
                    inputs = fullPrompt,
                    parameters = new
                    {
                        max_length = 200,
                        temperature = 0.8,
                        do_sample = true,
                        top_p = 0.9,
                        return_full_text = false,
                        repetition_penalty = 1.1
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending request to HuggingFace API: {Model}", _modelName);

                var response = await _httpClient.PostAsync($"https://api-inference.huggingface.co/models/{_modelName}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("HuggingFace API response: {Response}", responseContent);
                    
                    var result = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(responseContent);
                    
                    if (result != null && result.Count > 0 && result[0].ContainsKey("generated_text"))
                    {
                        var generatedText = result[0]["generated_text"].ToString();
                        var cleanedResponse = CleanResponse(generatedText);
                        
                        _logger.LogInformation("AI generated response: {Response}", cleanedResponse);
                        return cleanedResponse;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("HuggingFace API error: {StatusCode} - {Content}", 
                        response.StatusCode, errorContent);
                }

                return "Xin lỗi, tôi gặp sự cố kỹ thuật. Vui lòng thử lại sau hoặc liên hệ nhân viên tư vấn.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling HuggingFace API");
                return "Xin lỗi, tôi gặp sự cố kỹ thuật. Vui lòng thử lại sau hoặc liên hệ nhân viên tư vấn.";
            }
        }

        public async Task<ChatResponse> ProcessWithAIAsync(string message, string sessionId, string? userId = null)
        {
            try
            {
                // Kiểm tra xem có nên sử dụng AI hay không
                if (ShouldUseAI(message))
                {
                    var aiResponse = await GenerateResponseAsync(message);
                    
                    return new ChatResponse
                    {
                        Message = aiResponse,
                        SessionId = sessionId,
                        QuickReplies = GetAIQuickReplies()
                    };
                }

                // Fallback về logic cũ nếu không sử dụng AI
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

                var response = await _httpClient.GetAsync($"https://api-inference.huggingface.co/models/{_modelName}");
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
                "hi", "hello", "xin chào", "chào", "hey", "ok", "okay"
            };

            // Nếu chỉ là chào hỏi đơn giản và ngắn, không dùng AI
            if (simpleGreetings.Any(greeting => lowerMessage == greeting || lowerMessage == greeting + "!"))
            {
                return false;
            }

            // Sử dụng AI cho hầu hết các câu hỏi khác
            return true;
        }

        private bool ShouldTransferToHuman(string aiResponse)
        {
            var lowerResponse = aiResponse.ToLower();
            var transferKeywords = new[]
            {
                "liên hệ", "nhân viên", "tư vấn", "admin", "staff",
                "không biết", "không rõ", "không chắc"
            };

            return transferKeywords.Any(keyword => lowerResponse.Contains(keyword));
        }

        private List<string> GetAIQuickReplies()
        {
            return new List<string>
            {
                "Tư vấn thêm",
                "Xem sản phẩm", 
                "Liên hệ nhân viên"
            };
        }

        private string CleanResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
                return "Xin lỗi, tôi không hiểu câu hỏi của bạn. Vui lòng thử lại.";

            // Loại bỏ các ký tự không cần thiết
            response = response.Trim();
            
            // Loại bỏ phần "Trợ lý AI:" nếu có
            if (response.StartsWith("Trợ lý AI:"))
                response = response.Substring("Trợ lý AI:".Length).Trim();
            
            // Giới hạn độ dài
            if (response.Length > 2000)
                response = response.Substring(0, 2000) + "...";

            return response;
        }
    }
}
