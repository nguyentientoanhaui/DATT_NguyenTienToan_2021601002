
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Demo.Models
{
    public class StatisticalModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int Sold { get; set; }
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
        public DateTime DateCreated { get; set; }

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual ProductModel Product { get; set; }
    }
}
