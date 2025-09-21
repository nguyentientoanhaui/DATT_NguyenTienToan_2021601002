using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Demo.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
        public decimal ShippingCost { get; set; }

        // Thay thế CouponCode bằng CouponId
        public string? CouponCode { get; set; }
        // public int? CouponId { get; set; } // Removed - Coupon table not exists

        public decimal DiscountAmount { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingDistrict { get; set; }
        public string ShippingWard { get; set; }
        public string ShippingAddress { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public virtual ICollection<OrderDetails> OrderDetails { get; set; }
        public virtual ICollection<ProductReviewModel> ProductReviews { get; set; }
        public virtual ICollection<PaymentModel> Payments { get; set; }
        public virtual ICollection<MomoInfoModel> MomoInfos { get; set; }
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUserModel User { get; set; }

        // [ForeignKey("CouponId")] // Removed - Coupon table not exists
        // public virtual CouponModel? Coupon { get; set; } // Removed - Coupon table not exists
    }
}
