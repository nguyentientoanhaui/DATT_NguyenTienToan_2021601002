using System.ComponentModel.DataAnnotations;

namespace Shopping_Demo.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public ProductModel ProductDetail { get; set; }

        // For rating form
        [Required(ErrorMessage = "Yêu cầu nhập tên")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập bình luận")]
        public string Comment { get; set; }

        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        public int Stars { get; set; } = 5;

        // List of existing ratings for this product
        public List<ProductReviewModel> ProductReviews { get; set; }

        // Average rating
        public double AverageRating { get; set; }
    }
}
