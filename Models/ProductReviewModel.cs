using System.ComponentModel.DataAnnotations;

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
        public string UserName { get; set; }
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Comment { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual ProductModel Product { get; set; }
        public virtual OrderModel Order { get; set; }
    }
}
