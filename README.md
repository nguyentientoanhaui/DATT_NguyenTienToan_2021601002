# Shopping Demo - ASP.NET Core E-commerce Application

## Mô tả
Đây là một ứng dụng e-commerce được xây dựng bằng ASP.NET Core MVC, hỗ trợ đa ngôn ngữ (Tiếng Việt và Tiếng Anh) với các tính năng thanh toán tích hợp.

## Tính năng chính
- **Quản lý sản phẩm**: Hiển thị, tìm kiếm, so sánh sản phẩm
- **Giỏ hàng**: Thêm, xóa, cập nhật số lượng sản phẩm
- **Đăng nhập/Đăng ký**: Hệ thống xác thực người dùng
- **Thanh toán**: Tích hợp VNPAY và các phương thức thanh toán khác
- **Đánh giá sản phẩm**: Hệ thống review và rating
- **Đa ngôn ngữ**: Hỗ trợ Tiếng Việt và Tiếng Anh
- **Quản lý đơn hàng**: Theo dõi trạng thái đơn hàng

## Công nghệ sử dụng
- **Backend**: ASP.NET Core 6.0
- **Database**: SQL Server
- **Frontend**: HTML, CSS, JavaScript, Bootstrap
- **Authentication**: ASP.NET Core Identity
- **Payment**: VNPAY Integration
- **Translation**: Custom translation system

## Cài đặt và chạy

### Yêu cầu hệ thống
- .NET 6.0 SDK
- SQL Server
- Visual Studio 2022 hoặc VS Code

### Các bước cài đặt

1. **Clone repository**
```bash
git clone https://github.com/nguyentientoanhaui/DATT_NguyenTienToan_2021601002.git
cd DATT_NguyenTienToan_2021601002
```

2. **Cài đặt dependencies**
```bash
dotnet restore
```

3. **Cấu hình database**
- Cập nhật connection string trong `appsettings.json`
- Chạy migrations:
```bash
dotnet ef database update
```

4. **Chạy ứng dụng**
```bash
dotnet run
```

5. **Truy cập ứng dụng**
- Mở trình duyệt và truy cập: `https://localhost:7001` hoặc `http://localhost:5000`

## Cấu trúc project

```
Shopping_Demo/
├── Controllers/          # Controllers cho MVC
├── Models/              # Data models và Entity Framework
├── Views/               # Razor views
├── Services/            # Business logic services
├── Repository/          # Data access layer
├── Areas/               # Feature areas
├── wwwroot/             # Static files (CSS, JS, Images)
├── Migrations/          # Database migrations
└── Properties/          # Project properties
```

## Tính năng đặc biệt

### Hệ thống đa ngôn ngữ
- Tự động dịch nội dung sản phẩm
- Hỗ trợ chuyển đổi ngôn ngữ real-time
- Cache translation để tối ưu hiệu suất

### Tích hợp thanh toán
- VNPAY payment gateway
- Hỗ trợ nhiều phương thức thanh toán
- Xử lý callback và webhook

### Quản lý sản phẩm
- Upload và quản lý hình ảnh sản phẩm
- Phân loại sản phẩm theo danh mục
- Tìm kiếm và lọc sản phẩm nâng cao

## Tác giả
**Nguyễn Tiến Toàn** - 2021601002
- Trường Đại học Công nghiệp Hà Nội
- Đồ án tốt nghiệp ngành Công nghệ thông tin

## License
MIT License

## Liên hệ
- Email: [your-email@example.com]
- GitHub: [https://github.com/nguyentientoanhaui]
