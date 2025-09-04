using System.Collections.Generic;

namespace Shopping_Demo.Models
{
    public class ShippingModel
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public string City { get; set; }
    }
}