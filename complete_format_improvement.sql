-- Script hoàn thành cải thiện định dạng
-- Chạy script này sau khi đã kiểm tra bảng temp

USE [Shopping_Demo]
GO

-- PHẦN 1: BACKUP BẢNG GỐC

-- 1.1. Tạo backup bảng gốc
SELECT * INTO Products_Backup_$(CONVERT(VARCHAR(8), GETDATE(), 112)) 
FROM Products;

PRINT 'Đã tạo backup bảng Products';

-- PHẦN 2: XÓA BẢNG GỐC VÀ ĐỔI TÊN BẢNG TEMP

-- 2.1. Xóa bảng gốc
DROP TABLE Products;

-- 2.2. Đổi tên bảng temp thành Products
EXEC sp_rename 'Products_Temp', 'Products';

PRINT 'Đã đổi tên bảng Products_Temp thành Products';

-- PHẦN 3: TẠO LẠI CÁC INDEX VÀ CONSTRAINT

-- 3.1. Tạo index trên các cột quan trọng
CREATE INDEX IX_Products_Condition ON Products(Condition);
CREATE INDEX IX_Products_Gender ON Products(Gender);
CREATE INDEX IX_Products_Certificate ON Products(Certificate);
CREATE INDEX IX_Products_BrandId ON Products(BrandId);
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Products_Price ON Products(Price);

PRINT 'Đã tạo lại các index';

-- 3.2. Tạo foreign key constraints (nếu cần)
-- ALTER TABLE Products ADD CONSTRAINT FK_Products_Brands FOREIGN KEY (BrandId) REFERENCES Brands(Id);
-- ALTER TABLE Products ADD CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id);

-- PHẦN 4: KIỂM TRA KẾT QUẢ

-- 4.1. Kiểm tra collation mới
SELECT 
    c.name as ColumnName,
    c.collation_name as NewCollation,
    t.name as DataType
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('Products')
AND c.name IN ('Condition', 'Gender', 'Certificate', 'WarrantyInfo', 'Name', 'Description')
ORDER BY c.name;

-- 4.2. Thống kê tổng quan
SELECT 
    'Format Improvement Completed!' as Message,
    COUNT(*) as TotalProducts,
    SUM(CASE WHEN Condition = 'Excellent' THEN 1 ELSE 0 END) as ExcellentCount,
    SUM(CASE WHEN Gender = 'Men' THEN 1 ELSE 0 END) as MenCount,
    SUM(CASE WHEN Gender = 'Women' THEN 1 ELSE 0 END) as WomenCount,
    SUM(CASE WHEN Certificate = 'Yes' THEN 1 ELSE 0 END) as YesCount,
    SUM(CASE WHEN Certificate = 'No' THEN 1 ELSE 0 END) as NoCount
FROM Products;

-- 4.3. Test Unicode support
SELECT 
    'Unicode Test' as TestType,
    N'Xuất sắc' as VietnameseTest,
    N'Nam' as GenderTest,
    N'Có' as CertificateTest,
    N'5 Năm' as WarrantyTest;

-- 4.4. Hiển thị một số sản phẩm để kiểm tra
SELECT TOP 10
    Id,
    Name,
    Price,
    Gender,
    Condition,
    Certificate,
    WarrantyInfo
FROM Products
ORDER BY Id DESC;

-- PHẦN 5: SCRIPT CHUYỂN ĐỔI TIẾNG VIỆT (TÙY CHỌN)

-- 5.1. Script để chuyển đổi sang tiếng Việt (chạy nếu muốn)
PRINT '-- Để chuyển đổi sang tiếng Việt, chạy script vietnamize_improved.sql';
PRINT '-- Để giữ nguyên tiếng Anh, không cần làm gì thêm';

-- 5.2. Thông báo hoàn thành
SELECT 
    'SUCCESS!' as Status,
    'Database format has been improved' as Message,
    'Unicode support is now available' as Feature1,
    'Vietnamese collation is ready' as Feature2,
    'Ready for localization' as Feature3;
