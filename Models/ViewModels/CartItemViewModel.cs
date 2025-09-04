namespace Shopping_Demo.Models.ViewModels
{
    public class CartItemViewModel
    {
        public List<CartItemModel> CartItems { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal ShippingPrice { get; set; }
        public string CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalTotal { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingDistrict { get; set; }
        public string ShippingWard { get; set; }
        public string ShippingAddress { get; set; }
        public bool HasShippingAddress => !string.IsNullOrEmpty(ShippingCity);
    }
}
