using System.ComponentModel.DataAnnotations;

namespace Shopping_Demo.Models.ViewModels
{
    public class OrderConfirmationViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã đơn hàng")]
        public string OrderCode { get; set; }
        
        [Required(ErrorMessage = "Vui lòng xác nhận đã nhận hàng")]
        public bool IsConfirmed { get; set; }
        
        public string? CustomerNote { get; set; }
    }

    public class ProductReviewViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã đơn hàng")]
        public string OrderCode { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm")]
        public int ProductId { get; set; }
        
        [Required(ErrorMessage = "Vui lòng đánh giá sao")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1-5 sao")]
        public int Rating { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập nhận xét")]
        [StringLength(500, ErrorMessage = "Nhận xét không được quá 500 ký tự")]
        public string Comment { get; set; }
        
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
    }
}
