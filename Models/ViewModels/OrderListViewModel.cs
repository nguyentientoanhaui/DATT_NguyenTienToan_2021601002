using Shopping_Demo.Models;

namespace Shopping_Demo.Models.ViewModels
{
    public class OrderListViewModel
    {
        public OrderModel Order { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalAmount { get; set; }
        public int ProductCount { get; set; }
    }
}
