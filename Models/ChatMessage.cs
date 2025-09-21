using System.ComponentModel.DataAnnotations;

namespace Shopping_Demo.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        
        [Required]
        public int SessionId { get; set; }
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public string SenderType { get; set; } = string.Empty;
        
        public string? SenderId { get; set; }
        
        public string? SenderName { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        public bool IsRead { get; set; } = false;
        
        [Required]
        public string MessageType { get; set; } = "text";
        
        public string? Metadata { get; set; }
    }
}
