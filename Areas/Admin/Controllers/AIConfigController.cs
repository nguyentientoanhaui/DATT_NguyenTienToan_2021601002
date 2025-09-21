using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Shopping_Demo.Services;

namespace Shopping_Demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AIConfigController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IAIService _aiService;

        public AIConfigController(IConfiguration configuration, IAIService aiService)
        {
            _configuration = configuration;
            _aiService = aiService;
        }

        public IActionResult Index()
        {
            var currentProvider = _configuration["AI:Provider"] ?? "ONNX";
            var ollamaUrl = _configuration["LocalAI:OllamaUrl"] ?? "";
            var ollamaModel = _configuration["LocalAI:ModelName"] ?? "";

            ViewBag.CurrentProvider = currentProvider;
            ViewBag.OllamaConfigured = !string.IsNullOrEmpty(ollamaUrl) && !string.IsNullOrEmpty(ollamaModel);
            ViewBag.OllamaUrl = ollamaUrl;
            ViewBag.OllamaModel = ollamaModel;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TestAI()
        {
            try
            {
                var testMessage = "Xin chào, bạn có thể giới thiệu về đồng hồ Rolex không?";
                var response = await _aiService.GenerateResponseAsync(testMessage);
                
                return Json(new { 
                    success = true, 
                    response = response,
                    provider = _configuration["AI:Provider"] ?? "HuggingFace"
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    error = ex.Message,
                    provider = _configuration["AI:Provider"] ?? "HuggingFace"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckHealth()
        {
            try
            {
                var isHealthy = await _aiService.IsHealthyAsync();
                return Json(new { 
                    success = true, 
                    healthy = isHealthy,
                    provider = _configuration["AI:Provider"] ?? "HuggingFace"
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    error = ex.Message,
                    provider = _configuration["AI:Provider"] ?? "HuggingFace"
                });
            }
        }
    }
}