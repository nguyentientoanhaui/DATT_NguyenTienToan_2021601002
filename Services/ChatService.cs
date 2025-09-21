using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;

namespace Shopping_Demo.Services
{
    public class ChatService
    {
        private readonly DataContext _context;
        private readonly IAIService _aiService;

        public ChatService(DataContext context, IAIService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        public async Task<ChatResponse> ProcessMessageAsync(ChatRequest request)
        {
            try 
            {
                // Tạo sessionId nếu chưa có
                var sessionId = request.SessionId ?? Guid.NewGuid().ToString();

                // Tạo hoặc cập nhật session
                var session = await GetOrCreateSessionAsync(sessionId, request.UserId, request.UserName);
                
                if (session.Id == 0)
                {
                    _context.ChatSessions.Add(session);
                }

                session.LastActivityAt = DateTime.Now;
                session.IsActive = true;
                await _context.SaveChangesAsync();

                // Lưu tin nhắn của user
                var userMessage = new ChatMessage
                {
                    SessionId = session.Id,
                    Message = request.Message,
                    SenderType = "user",
                    SenderId = request.UserId,
                    SenderName = request.UserName,
                    Timestamp = DateTime.Now,
                    MessageType = "text"
                };

                _context.ChatMessages.Add(userMessage);
                await _context.SaveChangesAsync();

                // Tạo phản hồi từ AI
                var (botMessage, quickReplies) = await GenerateBotResponseAsync(request.Message);

                // Lưu tin nhắn của bot
                var botChatMessage = new ChatMessage
                {
                    SessionId = session.Id,
                    Message = botMessage,
                    SenderType = "bot",
                    SenderId = null,
                    SenderName = "AI Assistant",
                    Timestamp = DateTime.Now,
                    MessageType = "text"
                };

                _context.ChatMessages.Add(botChatMessage);
                await _context.SaveChangesAsync();

                return new ChatResponse
                {
                    Message = botMessage,
                    SessionId = sessionId,
                    QuickReplies = quickReplies
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                return new ChatResponse
                {
                    Message = "Xin lỗi, tôi đang gặp sự cố kỹ thuật. Vui lòng thử lại sau.",
                    SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                    QuickReplies = new List<string> { "Thử lại", "Liên hệ hỗ trợ" }
                };
            }
        }

        public async Task<List<ChatMessage>> GetMessagesAsync(string sessionId)
        {
            try
            {
                var session = await _context.ChatSessions
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId);

                if (session == null)
                    return new List<ChatMessage>();

                return await _context.ChatMessages
                    .Where(m => m.SessionId == session.Id)
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting messages: {ex.Message}");
                return new List<ChatMessage>();
            }
        }

        private async Task<ChatSession> GetOrCreateSessionAsync(string sessionId, string? userId, string? userName)
        {
            try
            {
                var session = await _context.ChatSessions
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId);

                if (session == null)
                {
                    session = new ChatSession
                    {
                        SessionId = sessionId,
                        UserId = userId,
                        UserName = userName,
                        CreatedAt = DateTime.Now,
                        LastActivityAt = DateTime.Now,
                        IsActive = true
                    };
                }

                return session;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting/creating session: {ex.Message}");
                return new ChatSession
                {
                    SessionId = sessionId,
                    UserId = userId,
                    UserName = userName,
                    CreatedAt = DateTime.Now,
                    LastActivityAt = DateTime.Now,
                    IsActive = true
                };
            }
        }

        private async Task<(string Message, List<string> QuickReplies)> GenerateBotResponseAsync(string userMessage)
        {
            try
            {
                // Sử dụng AI service
                var aiResponse = await _aiService.GenerateResponseAsync(userMessage);
                
                if (!string.IsNullOrEmpty(aiResponse) && 
                    !aiResponse.Contains("Xin lỗi, tôi gặp sự cố kỹ thuật"))
                {
                    return (aiResponse, GetQuickReplies(userMessage));
                }
                
                // Fallback response
                return ("Xin chào! Tôi là trợ lý AI của cửa hàng đồng hồ Aurum Watches. Tôi có thể giúp bạn tìm hiểu về các sản phẩm đồng hồ của chúng tôi. Bạn có muốn tìm hiểu về sản phẩm nào không?", 
                    new List<string> { "Xem sản phẩm", "Tư vấn", "Liên hệ hỗ trợ" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Service error: {ex.Message}");
                return ("Xin lỗi, tôi đang gặp sự cố kỹ thuật. Vui lòng thử lại sau hoặc liên hệ nhân viên tư vấn.", 
                    new List<string> { "Thử lại", "Liên hệ hỗ trợ", "Tư vấn trực tiếp" });
            }
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
}