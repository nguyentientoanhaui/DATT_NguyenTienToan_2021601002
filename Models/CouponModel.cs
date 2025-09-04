using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace Shopping_Demo.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class CouponModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập tên coupon")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập mô tả")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập số lượng")]
        public int Quantity { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập giá trị giảm giá")]
        public decimal DiscountValue { get; set; }
        [Required(ErrorMessage = "Yêu cầu chọn loại giảm giá")]
        public DiscountType DiscountType { get; set; }
        [Required(ErrorMessage = "Yêu cầu chọn trạng thái")]
        public int Status { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateExpired { get; set; }
        public virtual ICollection<OrderModel> Orders { get; set; }
    }

    public enum DiscountType
    {
        Percentage = 0,
        FixedAmount = 1
    }
}