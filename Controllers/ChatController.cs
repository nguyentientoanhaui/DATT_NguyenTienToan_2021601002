using Microsoft.AspNetCore.Mvc;
using Shopping_Demo.Models;
using Shopping_Demo.Services;

namespace Shopping_Demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("send")]
        public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Message))
                {
                    return BadRequest(new { error = "Message is required" });
                }

                var response = await _chatService.ProcessMessageAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("messages/{sessionId}")]
        public async Task<ActionResult<List<ChatMessage>>> GetMessages(string sessionId)
        {
            try
            {
                var messages = await _chatService.GetMessagesAsync(sessionId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("health")]
        public async Task<ActionResult> HealthCheck()
        {
            try
            {
                return Ok(new { status = "healthy", timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}