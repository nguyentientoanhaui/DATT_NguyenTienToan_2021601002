
namespace Shopping_Demo.Models
{
    public class StatisticalModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int Sold { get; set; }
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
