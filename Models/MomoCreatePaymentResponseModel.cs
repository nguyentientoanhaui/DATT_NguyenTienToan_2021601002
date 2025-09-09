namespace Shopping_Demo.Models
{
    public class MomoCreatePaymentResponseModel
    {
        public string RequestId { get; set; } = string.Empty;
        public int ErrorCode { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string LocalMessage { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string PayUrl { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public string QrCodeUrl { get; set; } = string.Empty;
        public string Deeplink { get; set; } = string.Empty;
        public string DeeplinkWebInApp { get; set; } = string.Empty;
        
        // Thêm các properties cần thiết
        public bool IsSuccess => ErrorCode == 0;
        public string TransactionId => RequestId;
        public string ResponseMessage => Message;
    }
}
