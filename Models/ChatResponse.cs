namespace Shopping_Demo.Models
{
    public class ChatResponse
    {
        public string Message { get; set; } = string.Empty;
        
        public string SessionId { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        public List<string> QuickReplies { get; set; } = new List<string>();
    }
}
