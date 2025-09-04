# 🐍 Database Format Improver - Python Script

## 📋 Mô tả
Script Python để cải thiện định dạng database và chuyển đổi dữ liệu từ tiếng Anh sang tiếng Việt với hỗ trợ Unicode hoàn hảo.

## 🚀 Cài đặt

### 1. Cài đặt Python dependencies
```bash
pip install -r requirements.txt
```

### 2. Cài đặt SQL Server ODBC Driver
- **Windows**: Tải từ Microsoft
- **Linux**: `sudo apt-get install unixodbc-dev`
- **macOS**: `brew install unixodbc`

### 3. Cấu hình Connection String
Chỉnh sửa `connection_string` trong file `database_format_improver.py`:

```python
connection_string = (
    "DRIVER={ODBC Driver 17 for SQL Server};"
    "SERVER=localhost;"  # Thay đổi server name
    "DATABASE=Shopping_Demo;"
    "Trusted_Connection=yes;"  # Hoặc dùng username/password
)
```

## 🎯 Cách sử dụng

### Chạy script
```bash
python database_format_improver.py
```

### Các tùy chọn có sẵn:
1. **Cải thiện định dạng (giữ tiếng Anh)**
   - Tạo bảng temp với Unicode support
   - Sao chép dữ liệu an toàn
   - Thay thế bảng gốc
   - Giữ nguyên dữ liệu tiếng Anh

2. **Cải thiện định dạng + Việt hóa**
   - Thực hiện tất cả bước trên
   - Chuyển đổi dữ liệu sang tiếng Việt
   - Chuyển đổi giá từ USD sang VND

3. **Chỉ kiểm tra hiện trạng**
   - Xem collation hiện tại
   - Kiểm tra encoding các cột

4. **Thoát**

## 🔧 Tính năng chính

### ✅ Kiểm tra và Backup
- Kiểm tra collation database hiện tại
- Backup tự động với timestamp
- Kiểm tra encoding các cột text

### ✅ Cải thiện định dạng
- Tạo bảng temp với `Vietnamese_CI_AS` collation
- Sao chép dữ liệu an toàn
- Test Unicode support
- Tạo lại index và constraints

### ✅ Chuyển đổi dữ liệu
- Mapping tiếng Anh → tiếng Việt
- Chuyển đổi giá USD → VND (tỷ giá 1:24500)
- Kiểm tra kết quả chi tiết

### ✅ Báo cáo và thống kê
- Thống kê tổng quan sau chuyển đổi
- Hiển thị mẫu dữ liệu
- Kiểm tra lỗi encoding

## 📊 Mapping dữ liệu

### Condition (Tình trạng)
- `Excellent` → `Xuất sắc`
- `Very Good` → `Rất tốt`
- `Good` → `Tốt`
- `Fair` → `Khá`
- `Poor` → `Kém`
- `New` → `Mới`
- `Pre-owned` → `Đã sử dụng`
- `Mint` → `Như mới`
- `Near Mint` → `Gần như mới`

### Gender (Giới tính)
- `Men` → `Nam`
- `Women` → `Nữ`

### Certificate (Chứng chỉ)
- `Yes` → `Có`
- `No` → `Không`
- `Available` → `Có sẵn`
- `Not Available` → `Không có`
- `Included` → `Bao gồm`
- `Not Included` → `Không bao gồm`

### WarrantyInfo (Bảo hành)
- `1 Year` → `1 Năm`
- `2 Years` → `2 Năm`
- `3 Years` → `3 Năm`
- `5 Years` → `5 Năm`
- `Lifetime` → `Trọn đời`
- `No Warranty` → `Không bảo hành`
- `Manufacturer Warranty` → `Bảo hành nhà sản xuất`
- `International Warranty` → `Bảo hành quốc tế`

## ⚠️ Lưu ý quan trọng

### 🔒 Bảo mật
- Script tự động backup trước khi thay đổi
- Backup được đặt tên với timestamp
- Có thể khôi phục từ backup nếu cần

### 🛡️ An toàn
- Kiểm tra kết nối trước khi thực hiện
- Xác nhận số lượng bản ghi sau sao chép
- Test Unicode support trước khi thay thế

### 🔄 Rollback
Nếu cần khôi phục:
```sql
-- Xóa bảng hiện tại
DROP TABLE Products;

-- Khôi phục từ backup
EXEC sp_rename 'Products_Backup_YYYYMMDD_HHMMSS', 'Products';
```

## 🐛 Xử lý lỗi

### Lỗi kết nối
- Kiểm tra SQL Server đang chạy
- Kiểm tra connection string
- Kiểm tra ODBC Driver đã cài đặt

### Lỗi Unicode
- Đảm bảo database hỗ trợ Unicode
- Kiểm tra collation `Vietnamese_CI_AS` có sẵn

### Lỗi quyền
- Đảm bảo user có quyền CREATE/DROP/ALTER
- Kiểm tra quyền truy cập database

## 📞 Hỗ trợ

Nếu gặp vấn đề:
1. Kiểm tra log lỗi chi tiết
2. Xác nhận cấu hình connection string
3. Kiểm tra quyền database user
4. Backup dữ liệu trước khi thử nghiệm

## 🎉 Kết quả mong đợi

Sau khi chạy thành công:
- ✅ Database hỗ trợ Unicode hoàn hảo
- ✅ Tiếng Việt hiển thị đúng không bị lỗi
- ✅ Giá tiền đã chuyển sang VND
- ✅ Index và performance được tối ưu
- ✅ Backup an toàn được tạo
