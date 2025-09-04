using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using Shopping_Demo.Models;
using Shopping_Demo.Models.Momo;
using System.Security.Cryptography;
using System.Text;
namespace Shopping_Demo.Services.Momo
{
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoOptionModel> _options;
        public MomoService(IOptions<MomoOptionModel> options)
        {
            _options = options;
        }
        public async Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model)
        {
            // Làm tròn amount thành số nguyên (đơn vị đồng, không có phần thập phân)
            long amount = (long)model.Amount;

            // Tạo orderId và requestId độc nhất
            string orderId = DateTime.UtcNow.Ticks.ToString();
            string requestId = Guid.NewGuid().ToString();

            // Đơn giản hóa orderInfo để tránh lỗi ký tự
            string orderInfo = "Thanh toan don hang " + orderId;

            // Tạo chuỗi raw data theo đúng định dạng và thứ tự mà MoMo yêu cầu
            var rawData =
                $"accessKey={_options.Value.AccessKey}" +
                $"&amount={amount}" +
                $"&extraData=" +
                $"&ipnUrl={_options.Value.NotifyUrl}" +
                $"&orderId={orderId}" +
                $"&orderInfo={orderInfo}" +
                $"&partnerCode={_options.Value.PartnerCode}" +
                $"&redirectUrl={_options.Value.ReturnUrl}" +  // Sử dụng ReturnUrl cho redirectUrl
                $"&requestId={requestId}" +
                $"&requestType={_options.Value.RequestType}";

            // Tính signature
            var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

            // In ra raw data và signature để debug
            Console.WriteLine("Raw data: " + rawData);
            Console.WriteLine("Signature: " + signature);

            var client = new RestClient(_options.Value.MomoApiUrl);
            var request = new RestRequest() { Method = Method.Post };
            request.AddHeader("Content-Type", "application/json; charset=UTF-8");

            // Tạo payload
            var requestData = new
            {
                partnerCode = _options.Value.PartnerCode,
                accessKey = _options.Value.AccessKey,
                requestId = requestId,
                amount = amount.ToString(),
                orderId = orderId,
                orderInfo = orderInfo,
                redirectUrl = _options.Value.ReturnUrl,  // MoMo sử dụng redirectUrl thay vì returnUrl
                ipnUrl = _options.Value.NotifyUrl,       // ipnUrl là URL nhận thông báo từ MoMo
                extraData = "",
                requestType = _options.Value.RequestType,
                signature = signature
            };

            // In ra json request để debug
            var jsonRequest = JsonConvert.SerializeObject(requestData);
            Console.WriteLine("JSON Request: " + jsonRequest);

            request.AddParameter("application/json", jsonRequest, ParameterType.RequestBody);

            try
            {
                var response = await client.ExecuteAsync(request);
                Console.WriteLine("Response Content: " + response.Content);
                Console.WriteLine("Status Code: " + response.StatusCode);
                Console.WriteLine("Error Message: " + response.ErrorMessage);

                Console.WriteLine("MoMo API Response: " + response.Content);

                if (response.IsSuccessful)
                {
                    var momoResponse = JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);
                    return momoResponse;
                }
                else
                {
                    Console.WriteLine("MoMo API Error: " + response.ErrorMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return null;
            }
        }

        public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
        {
            // Kiểm tra xem các tham số tồn tại trước khi truy cập
            var amount = collection.FirstOrDefault(s => s.Key == "amount").Value.ToString() ?? "0";
            var orderInfo = collection.FirstOrDefault(s => s.Key == "orderInfo").Value.ToString() ?? "";
            var orderId = collection.FirstOrDefault(s => s.Key == "orderId").Value.ToString() ?? "";

            // Xử lý trường hợp fullName không tồn tại
            string fullName = "";
            var fullNameParam = collection.FirstOrDefault(s => s.Key == "fullName");
            if (fullNameParam.Value.Count > 0)
            {
                fullName = fullNameParam.Value.ToString();
            }
            else
            {
                // Có thể lấy thông tin user từ nguồn khác, ví dụ như context
                // Hoặc để trống
            }

            var response = new MomoExecuteResponseModel()
            {
                Amount = amount,
                OrderId = orderId,
                OrderInfo = orderInfo,
                FullName = fullName
            };

            return response;
        }
        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

    }
}
