namespace Shopping_Demo.Models
{
    public class MomoExecuteResponseModel
    {
        public string OrderId { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string OrderInfo { get; set; } = string.Empty;
        
        // Thêm các properties cần thiết
        public bool IsSuccess => !string.IsNullOrEmpty(OrderId);
        public string TransactionId => OrderId;
        public string ResponseCode => IsSuccess ? "00" : "99";
    }
}
