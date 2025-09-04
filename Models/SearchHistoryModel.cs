using System.ComponentModel.DataAnnotations;

namespace Shopping_Demo.Models
{
    public class SearchHistoryModel
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string SearchTerm { get; set; }
        
        // SessionId cho người dùng chưa đăng nhập
        public string? SessionId { get; set; }
        
        // UserId cho người dùng đã đăng nhập
        public string? UserId { get; set; }
        
        public DateTime SearchedAt { get; set; } = DateTime.Now;
        
        // Số lần tìm kiếm từ khóa này
        public int SearchCount { get; set; } = 1;
    }
}

