using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Shopping_Demo.Models
{
    public class ProductSizeModel
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int SizeId { get; set; }

        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }

        [ForeignKey("SizeId")]
        public SizeModel Size { get; set; }
    }
}
