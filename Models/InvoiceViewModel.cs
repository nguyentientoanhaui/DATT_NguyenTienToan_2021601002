using Shopping_Demo.Models;

namespace Shopping_Demo.Models
{
    public class InvoiceViewModel
    {
        public OrderModel Order { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        public MomoInfoModel PaymentInfo { get; set; }
    }
}
