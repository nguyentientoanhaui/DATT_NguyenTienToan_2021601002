using Shopping_Demo.Models;

namespace Shopping_Demo.Services
{
    public interface IAIService
    {
        Task<string> GenerateResponseAsync(string userMessage, string context = "");
        Task<ChatResponse> ProcessWithAIAsync(string message, string sessionId, string? userId = null);
        Task<bool> IsHealthyAsync();
    }
}
