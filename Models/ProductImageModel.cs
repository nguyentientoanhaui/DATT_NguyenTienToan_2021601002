using Shopping_Demo.Repository.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Shopping_Demo.Models
{
    public class ProductImageModel
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ImageName { get; set; }
        
        public string ImageUrl { get; set; }

        public bool IsDefault { get; set; }

        public int? ColorId { get; set; }

        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }

        [ForeignKey("ColorId")]
        public ColorModel Color { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile ImageUpload { get; set; }
    }
}
