# Hệ Thống Quản Lý Đơn Thu Mua - Admin Panel

## Tổng Quan
Hệ thống quản lý đơn thu mua cho phép admin xem, quản lý và xử lý các yêu cầu thu mua đồng hồ từ khách hàng.

## Các Chức Năng Chính

### 1. **Danh Sách Đơn Thu Mua (Index)**
- **URL**: `/Admin/SellRequest/Index`
- **Chức năng**:
  - Xem danh sách tất cả đơn thu mua
  - Lọc theo trạng thái (Chờ xử lý, Đã xem, Đã liên hệ, v.v.)
  - Tìm kiếm theo tên, email, SĐT, sản phẩm
  - Phân trang với 10 đơn/trang
  - Thống kê nhanh (tổng đơn, chờ xử lý, hoàn thành, tháng này)
  - Thao tác nhanh: Xem chi tiết, Chỉnh sửa, Đánh dấu đã liên hệ, Từ chối

### 2. **Chi Tiết Đơn Thu Mua (Details)**
- **URL**: `/Admin/SellRequest/Details/{id}`
- **Chức năng**:
  - Xem đầy đủ thông tin khách hàng
  - Thông tin sản phẩm được yêu cầu thu mua
  - Chi tiết đồng hồ (mô tả, tình trạng, năm sản xuất)
  - Thông tin giá cả (giá mong muốn, giá đề xuất)
  - Phản hồi từ admin
  - Lịch sử đơn hàng
  - Thao tác nhanh: Đánh dấu đã xem, Đã liên hệ, Đã thỏa thuận, Từ chối

### 3. **Chỉnh Sửa Đơn Thu Mua (Edit)**
- **URL**: `/Admin/SellRequest/Edit/{id}`
- **Chức năng**:
  - Cập nhật trạng thái đơn hàng
  - Thêm/sửa phản hồi cho khách hàng
  - Đề xuất giá thu mua
  - Auto-save draft (tự động lưu nháp)
  - Xóa đơn hàng (nếu cần)

### 4. **Xóa Đơn Thu Mua (Delete)**
- **URL**: `/Admin/SellRequest/Delete/{id}`
- **Chức năng**:
  - Xem lại thông tin trước khi xóa
  - Xác nhận xóa với cảnh báo
  - Xóa vĩnh viễn đơn hàng

### 5. **Thống Kê (Statistics)**
- **URL**: `/Admin/SellRequest/StatisticsView`
- **Chức năng**:
  - Thống kê tổng quan (tổng đơn, theo trạng thái, theo thời gian)
  - Biểu đồ phân bố trạng thái
  - Biểu đồ xu hướng theo tháng
  - Bảng thống kê chi tiết
  - Hoạt động gần đây
  - Xuất báo cáo Excel (sẽ triển khai)

## Các Trạng Thái Đơn Thu Mua

| Trạng Thái | Mô Tả | Màu Badge |
|------------|-------|-----------|
| **Pending** | Chờ xử lý | Warning (Vàng) |
| **Reviewed** | Đã xem | Info (Xanh dương) |
| **Contacted** | Đã liên hệ | Primary (Xanh đậm) |
| **Agreed** | Đã thỏa thuận | Success (Xanh lá) |
| **Completed** | Đã hoàn thành | Success (Xanh lá) |
| **Rejected** | Từ chối | Danger (Đỏ) |
| **Cancelled** | Hủy | Secondary (Xám) |

## Quy Trình Xử Lý Đơn Thu Mua

1. **Khách hàng gửi yêu cầu** → Trạng thái: `Pending`
2. **Admin xem đơn** → Trạng thái: `Reviewed`
3. **Admin liên hệ khách** → Trạng thái: `Contacted`
4. **Thỏa thuận giá** → Trạng thái: `Agreed`
5. **Hoàn thành giao dịch** → Trạng thái: `Completed`

Hoặc:
- **Từ chối đơn** → Trạng thái: `Rejected`
- **Khách hủy** → Trạng thái: `Cancelled`

## API Endpoints

### GET `/Admin/SellRequest/Statistics`
Trả về thống kê JSON:
```json
{
  "total": 100,
  "pending": 15,
  "reviewed": 20,
  "contacted": 25,
  "agreed": 10,
  "completed": 20,
  "rejected": 5,
  "cancelled": 5,
  "thisMonth": 30,
  "thisWeek": 8
}
```

### POST `/Admin/SellRequest/UpdateStatus`
Cập nhật trạng thái nhanh:
```json
{
  "id": 123,
  "status": "Contacted",
  "adminResponse": "Đã liên hệ khách hàng",
  "suggestedPrice": 5000000
}
```

## Tính Năng Đặc Biệt

### 1. **Auto-Save Draft**
- Tự động lưu nháp khi admin chỉnh sửa
- Lưu sau 2 giây không hoạt động
- Không cần lưu thủ công

### 2. **Quick Actions**
- Thao tác nhanh trực tiếp từ danh sách
- Modal cập nhật trạng thái nhanh
- Không cần vào trang chi tiết

### 3. **Responsive Design**
- Tương thích mobile và tablet
- Giao diện thân thiện
- Tối ưu trải nghiệm người dùng

### 4. **Real-time Statistics**
- Thống kê cập nhật real-time
- Biểu đồ tương tác
- Dữ liệu trực quan

## Bảo Mật

- Chỉ admin mới có quyền truy cập
- Validation dữ liệu đầu vào
- CSRF protection
- XSS protection

## Công Nghệ Sử Dụng

- **Backend**: ASP.NET Core MVC
- **Frontend**: Bootstrap 4, jQuery, Chart.js
- **Database**: Entity Framework Core
- **UI Components**: Font Awesome, DataTables

## Hướng Dẫn Sử Dụng

1. **Truy cập**: Đăng nhập với tài khoản admin
2. **Xem đơn**: Vào menu "Đơn thu mua" trong sidebar
3. **Xử lý đơn**: Click "Xem chi tiết" hoặc "Chỉnh sửa"
4. **Cập nhật trạng thái**: Sử dụng dropdown hoặc quick actions
5. **Thêm phản hồi**: Nhập phản hồi cho khách hàng
6. **Đề xuất giá**: Nhập giá thu mua đề xuất
7. **Lưu**: Click "Lưu Thay Đổi" hoặc để auto-save

## Lưu Ý Quan Trọng

- Luôn cập nhật trạng thái đúng quy trình
- Phản hồi khách hàng một cách chuyên nghiệp
- Đề xuất giá hợp lý dựa trên thị trường
- Không xóa đơn trừ khi thực sự cần thiết
- Thường xuyên kiểm tra thống kê để theo dõi hiệu suất
