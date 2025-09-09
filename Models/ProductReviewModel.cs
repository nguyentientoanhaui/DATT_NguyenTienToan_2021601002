using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Demo.Models
{
    public class ProductReviewModel
    {
        public int Id { get; set; }
        
        [Required]
        public string OrderCode { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string UserName { get; set; }
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Comment { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Foreign Key for Order
        public int? OrderId { get; set; }
        
        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual ProductModel Product { get; set; }
        
        [ForeignKey("OrderId")]
        public virtual OrderModel Order { get; set; }
    }
}
