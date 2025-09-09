namespace Shopping_Demo.Models
{
    public class OrderInfoModel
    {
        public string OrderId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}