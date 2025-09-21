using System.ComponentModel.DataAnnotations;

namespace Shopping_Demo.Models
{
    public class ChatRequest
    {
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public string? SessionId { get; set; }
        
        public string? UserId { get; set; }
        
        public string? UserName { get; set; }
    }
}
