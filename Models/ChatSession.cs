using System.ComponentModel.DataAnnotations;

namespace Shopping_Demo.Models
{
    public class ChatSession
    {
        public int Id { get; set; }
        
        [Required]
        public string SessionId { get; set; } = string.Empty;
        
        public string? UserId { get; set; }
        
        public string? UserName { get; set; }
        
        public string? UserEmail { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? LastActivityAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [Required]
        public string Status { get; set; } = "active";
        
        public string? LastMessage { get; set; }
    }
}
