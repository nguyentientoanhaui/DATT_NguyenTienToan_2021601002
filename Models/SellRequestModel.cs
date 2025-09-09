using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Demo.Models
{
    public class SellRequestModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(200, ErrorMessage = "Họ tên không được vượt quá 200 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(200, ErrorMessage = "Email không được vượt quá 200 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm")]
        [Display(Name = "Sản phẩm")]
        public int ProductId { get; set; }
        
        [Display(Name = "Sản phẩm")]
        public ProductModel Product { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập thông tin đồng hồ")]
        [StringLength(1000, ErrorMessage = "Thông tin đồng hồ không được vượt quá 1000 ký tự")]
        [Display(Name = "Thông tin đồng hồ")]
        public string WatchInfo { get; set; }
        
        [StringLength(200, ErrorMessage = "Tình trạng đồng hồ không được vượt quá 200 ký tự")]
        [Display(Name = "Tình trạng đồng hồ")]
        public string Condition { get; set; }
        
        [Display(Name = "Năm sản xuất")]
        public int? Year { get; set; }
        
        [Display(Name = "Giá mong muốn")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal? ExpectedPrice { get; set; }
        
        [StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
        [Display(Name = "Ghi chú thêm")]
        public string Notes { get; set; }
        
        [StringLength(500, ErrorMessage = "URL hình ảnh không được vượt quá 500 ký tự")]
        [Display(Name = "Hình ảnh đồng hồ")]
        public string ImageUrl { get; set; }
        
        [Display(Name = "Trạng thái")]
        public SellRequestStatus Status { get; set; } = SellRequestStatus.Pending;
        
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }
        
        [StringLength(1000, ErrorMessage = "Phản hồi admin không được vượt quá 1000 ký tự")]
        [Display(Name = "Phản hồi từ admin")]
        public string AdminResponse { get; set; }
        
        [Display(Name = "Giá đề xuất")]
        public decimal? SuggestedPrice { get; set; }
        
        [Display(Name = "User ID")]
        public string UserId { get; set; }
        
        [Display(Name = "Session ID")]
        public string SessionId { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual AppUserModel User { get; set; }
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