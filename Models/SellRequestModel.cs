using System.ComponentModel.DataAnnotations;

namespace Shopping_Demo.Models
{
    public class SellRequestModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm")]
        [Display(Name = "Sản phẩm")]
        public int ProductId { get; set; }
        
        [Display(Name = "Sản phẩm")]
        public ProductModel Product { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập thông tin đồng hồ")]
        [Display(Name = "Thông tin đồng hồ")]
        public string WatchInfo { get; set; }
        
        [Display(Name = "Tình trạng đồng hồ")]
        public string Condition { get; set; }
        
        [Display(Name = "Năm sản xuất")]
        public int? Year { get; set; }
        
        [Display(Name = "Giá mong muốn")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal? ExpectedPrice { get; set; }
        
        [Display(Name = "Ghi chú thêm")]
        public string Notes { get; set; }
        
        [Display(Name = "Hình ảnh đồng hồ")]
        public string ImageUrl { get; set; }
        
        [Display(Name = "Trạng thái")]
        public SellRequestStatus Status { get; set; } = SellRequestStatus.Pending;
        
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }
        
        [Display(Name = "Phản hồi từ admin")]
        public string AdminResponse { get; set; }
        
        [Display(Name = "Giá đề xuất")]
        public decimal? SuggestedPrice { get; set; }
        
        [Display(Name = "User ID")]
        public string UserId { get; set; }
        
        [Display(Name = "Session ID")]
        public string SessionId { get; set; }
    }
    
    public enum SellRequestStatus
    {
        [Display(Name = "Chờ xử lý")]
        Pending = 0,
        
        [Display(Name = "Đã xem")]
        Reviewed = 1,
        
        [Display(Name = "Đã liên hệ")]
        Contacted = 2,
        
        [Display(Name = "Đã thỏa thuận")]
        Agreed = 3,
        
        [Display(Name = "Đã hoàn thành")]
        Completed = 4,
        
        [Display(Name = "Từ chối")]
        Rejected = 5,
        
        [Display(Name = "Hủy")]
        Cancelled = 6
    }
}
