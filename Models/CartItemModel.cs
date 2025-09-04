using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Demo.Models
{
    [Table("CartItems")]
    public class CartItemModel
    {
        [Key] public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Image { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ColorId { get; set; }
        public string ColorName { get; set; }
        public string SizeId { get; set; }
        public string SizeName { get; set; }
        public int? CartId { get; set; }
        public CartModel Cart { get; set; }
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
