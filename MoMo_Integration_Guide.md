# MoMo Payment Integration Guide

## Tổng quan
Hệ thống đã được tích hợp MoMo để xử lý thanh toán trực tuyến theo hướng dẫn chính thức.

## Cấu hình

### appsettings.json
```json
{
  "MomoAPI": {
    "MomoApiUrl": "https://test-payment.momo.vn/gw_payment/transactionProcessor",
    "SecretKey": "K951B6PE1waDMi640xX08PD3vg6EkVlz",
    "AccessKey": "F8BBA842ECF85",
    "ReturnUrl": "http://localhost:53813/Payment/MomoCallback",
    "NotifyUrl": "http://localhost:53813/Payment/MomoNotify",
    "PartnerCode": "MOMO",
    "RequestType": "captureMoMoWallet"
  }
}
```

## Các thành phần chính

### Models
- `MoMoOptionModel.cs` - Cấu hình MoMo
- `MomoCreatePaymentResponseModel.cs` - Model response tạo thanh toán
- `MomoExecuteResponseModel.cs` - Model response xử lý callback
- `OrderInfoModel.cs` - Model thông tin đơn hàng

### Services
- `IMoMoService.cs` - Interface service MoMo
- `MoMoService.cs` - Implementation service MoMo

### Controllers
- `PaymentController.cs` - Xử lý thanh toán MoMo
- `CheckoutController.cs` - Checkout với MoMo

## API Endpoints

### Payment Processing
- `POST /Payment/CreatePaymentUrl` - Tạo URL thanh toán MoMo
- `GET /Payment/MomoCallback` - Callback từ MoMo
- `POST /Payment/MomoNotify` - Notify từ MoMo

### Testing
- `GET /Payment/TestMomo` - Test MoMo API
- `GET /Payment/TestMomo` - View test MoMo với form thanh toán

## Cách sử dụng

### 1. Test Payment
Truy cập `/Payment/TestMomo` để test tích hợp MoMo.

### 2. Process Payment
Sử dụng form HTML để gửi request:
```html
<form method="POST" asp-action="CreatePaymentUrl" asp-controller="Payment">
    <input type="hidden" name="FullName" value="@User.Identity.Name" />
    <input type="hidden" name="Amount" value="@Model.GrandTotal" />
    <input type="hidden" name="OrderInfo" value="Thanh toán đặt hàng qua Momo tại Shopping Demo" />
    <button class="btn btn-danger" name="PayUrl" type="submit">Pay with MoMo</button>
</form>
```

Hoặc sử dụng AJAX:
```javascript
$.ajax({
    url: '/Payment/CreatePaymentUrl',
    type: 'POST',
    data: {
        FullName: 'Customer Name',
        Amount: 50000,
        OrderInfo: 'Thanh toán đặt hàng qua Momo'
    },
    success: function(response) {
        // Redirect to MoMo payment page
        window.location.href = response.PayUrl;
    }
});
```

## Môi trường Test

### Sandbox
- URL: https://test-payment.momo.vn/gw_payment/transactionProcessor
- Partner Code: MOMO
- Request Type: captureMoMoWallet
- Test với số tiền từ 1,000 VND trở lên

### Production
- URL: https://payment.momo.vn/gw_payment/transactionProcessor
- Cần cập nhật Partner Code, Access Key, Secret Key thực tế

## Lưu ý quan trọng

1. **Security**: Luôn bảo mật Secret Key
2. **Validation**: Validate tất cả input từ user
3. **Logging**: Log tất cả giao dịch để debug
4. **Error Handling**: Xử lý lỗi một cách graceful
5. **Testing**: Test kỹ trước khi deploy production

## Troubleshooting

### Lỗi thường gặp
1. **Invalid Signature**: Kiểm tra Secret Key và cách tạo signature
2. **Amount Invalid**: Kiểm tra số tiền trong khoảng MinAmount - MaxAmount
3. **Network Error**: Kiểm tra kết nối internet và URL API

### Debug
- Kiểm tra logs trong console
- Sử dụng `/Payment/TestMomo` để test API
- Kiểm tra response từ MoMo API

## Migration từ VNPay

Các thay đổi chính:
1. Xóa tất cả VNPay services và models
2. Thay thế bằng MoMo services và models
3. Cập nhật PaymentController và CheckoutController
4. Cập nhật cấu hình trong appsettings.json
5. Xóa các file test VNPay

## Support

Để được hỗ trợ, vui lòng liên hệ:
- Email: support@momo.vn
- Hotline: 1900 55 55 77
- Documentation: https://developers.momo.vn/
