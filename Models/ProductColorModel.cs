using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Shopping_Demo.Models
{
    public class ProductColorModel
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int ColorId { get; set; }

        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }

        [ForeignKey("ColorId")]
        public ColorModel Color { get; set; }
    }
}
