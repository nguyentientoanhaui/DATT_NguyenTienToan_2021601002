namespace Shopping_Demo.Models.MoMo
{
    public class MoMoResponseModel
    {
        public bool IsSuccess { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
        public string ResponseMessage { get; set; } = string.Empty;
        public string PayUrl { get; set; } = string.Empty;
        public string QrCodeUrl { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
