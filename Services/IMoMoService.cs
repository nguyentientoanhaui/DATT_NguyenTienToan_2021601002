using Shopping_Demo.Models;

namespace Shopping_Demo.Services
{
    public interface IMoMoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
        MomoExecuteResponseModel ProcessCallback(IQueryCollection collection);
    }
}
