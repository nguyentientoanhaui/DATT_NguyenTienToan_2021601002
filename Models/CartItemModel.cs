using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Demo.Models
{
    [Table("CartItems")]
    public class CartItemModel
    {
        [Key] public int Id { get; set; }
        public int ProductId { get; set; }
        
        [StringLength(500)]
        public string ProductName { get; set; }
        
        [StringLength(500)]
        public string Image { get; set; }
        
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [StringLength(50)]
        public string ColorId { get; set; }
        
        [StringLength(100)]
        public string ColorName { get; set; }
        
        [StringLength(50)]
        public string SizeId { get; set; }
        
        [StringLength(100)]
        public string SizeName { get; set; }
        
        public int? CartId { get; set; }
        
        [ForeignKey("CartId")]
        public CartModel Cart { get; set; }
        
        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
        public decimal Total
        {
            get { return Quantity * Price; }
        }
        public CartItemModel() { }
        public CartItemModel(ProductModel product)
        {
            ProductId = product.Id;
            ProductName = product.Name;
            Price = product.Price;
            Quantity = 1;
            Image = product.Image;
        }
    }
}
