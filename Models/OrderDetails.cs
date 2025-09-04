using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Demo.Models
{
    public class OrderDetails
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public string UserName { get; set; }
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ColorName { get; set; }
        public string SizeName { get; set; }

        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }

        [ForeignKey("OrderCode")]
        public virtual OrderModel Order { get; set; }
    }
}
