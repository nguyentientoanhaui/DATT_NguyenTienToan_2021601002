using System.ComponentModel.DataAnnotations;

namespace Shopping_Demo.Models
{
    public class UserBehaviorModel
    {
        [Key]
        public int Id { get; set; }
        
        // Session ID cho user chưa đăng nhập
        public string? SessionId { get; set; }
        
        // User ID nếu đã đăng nhập
        public string? UserId { get; set; }
        
        // Product ID được xem
        public int ProductId { get; set; }
        
        // Loại hành vi: View, Click, AddToCart, etc.
        public string ActionType { get; set; } = "View";
        
        // Thời gian thực hiện
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        // IP Address để phân biệt user
        public string? IpAddress { get; set; }
        
        // User Agent để phân biệt browser
        public string? UserAgent { get; set; }
        
        // Navigation properties
        public ProductModel? Product { get; set; }
        public AppUserModel? User { get; set; }
    }
}
