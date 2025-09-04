-- Script sửa lỗi encoding với xử lý index
-- Xóa index trước khi thay đổi collation, sau đó tạo lại

USE [Shopping_Demo]
GO

-- PHẦN 1: XÓA CÁC INDEX PHỤ THUỘC

-- 1.1. Xóa index trên cột Condition
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_Condition')
BEGIN
    DROP INDEX IX_Products_Condition ON Products;
    PRINT 'Đã xóa index IX_Products_Condition';
END

-- 1.2. Xóa các index khác có thể ảnh hưởng
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_Gender')
BEGIN
    DROP INDEX IX_Products_Gender ON Products;
    PRINT 'Đã xóa index IX_Products_Gender';
END

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_Certificate')
BEGIN
    DROP INDEX IX_Products_Certificate ON Products;
    PRINT 'Đã xóa index IX_Products_Certificate';
END

-- PHẦN 2: THAY ĐỔI COLLATION (KHÔNG CẦN THAY ĐỔI COLLATION, CHỈ FIX DỮ LIỆU)

-- PHẦN 3: FORCE FIX DỮ LIỆU BỊ CORRUPT

-- 3.1. Thay thế hoàn toàn các giá trị Condition bị lỗi
UPDATE Products 
SET Condition = N'Xuất sắc'
WHERE Condition LIKE '%Xu?ts?c%' OR Condition LIKE '%Excellent%';

UPDATE Products 
SET Condition = N'Rất tốt'
WHERE Condition LIKE '%R?t t?t%' OR Condition LIKE '%Very Good%';

UPDATE Products 
SET Condition = N'Tốt'
WHERE Condition LIKE '%T?t%' OR Condition LIKE '%Good%';

UPDATE Products 
SET Condition = N'Khá'
WHERE Condition LIKE '%Kh?%' OR Condition LIKE '%Fair%';

UPDATE Products 
SET Condition = N'Kém'
WHERE Condition LIKE '%K?m%' OR Condition LIKE '%Poor%';

UPDATE Products 
SET Condition = N'Mới'
WHERE Condition LIKE '%M?i%' OR Condition LIKE '%New%';

UPDATE Products 
SET Condition = N'Đã sử dụng'
WHERE Condition LIKE '%Đ? s? d?ng%' OR Condition LIKE '%Pre-owned%' OR Condition LIKE '%Used%';

UPDATE Products 
SET Condition = N'Như mới'
WHERE Condition LIKE '%Nh? m?i%' OR Condition LIKE '%Mint%';

UPDATE Products 
SET Condition = N'Gần như mới'
WHERE Condition LIKE '%G?n nh? m?i%' OR Condition LIKE '%Near Mint%';

-- 3.2. Thay thế hoàn toàn các giá trị Gender bị lỗi
UPDATE Products 
SET Gender = N'Nam'
WHERE Gender LIKE '%N?m%' OR Gender LIKE '%Men%' OR Gender LIKE '%Gents%';

UPDATE Products 
SET Gender = N'Nữ'
WHERE Gender LIKE '%N?%' OR Gender LIKE '%Women%' OR Gender LIKE '%Ladies%';

UPDATE Products 
SET Gender = N'Unisex'
WHERE Gender LIKE '%Unisex%';

-- 3.3. Thay thế hoàn toàn các giá trị Certificate bị lỗi
UPDATE Products 
SET Certificate = N'Có'
WHERE Certificate LIKE '%C?%' OR Certificate LIKE '%Yes%' OR Certificate LIKE '%Available%' OR Certificate LIKE '%Included%';

UPDATE Products 
SET Certificate = N'Không'
WHERE Certificate LIKE '%Kh?ng%' OR Certificate LIKE '%No%' OR Certificate LIKE '%Not Available%' OR Certificate LIKE '%Not Included%';

-- 3.4. Thay thế hoàn toàn các giá trị WarrantyInfo bị lỗi
UPDATE Products 
SET WarrantyInfo = N'1 Năm'
WHERE WarrantyInfo LIKE '%1 N?m%' OR WarrantyInfo LIKE '%1 Year%';

UPDATE Products 
SET WarrantyInfo = N'2 Năm'
WHERE WarrantyInfo LIKE '%2 N?m%' OR WarrantyInfo LIKE '%2 Years%';

UPDATE Products 
SET WarrantyInfo = N'3 Năm'
WHERE WarrantyInfo LIKE '%3 N?m%' OR WarrantyInfo LIKE '%3 Years%';

UPDATE Products 
SET WarrantyInfo = N'5 Năm'
WHERE WarrantyInfo LIKE '%5 N?m%' OR WarrantyInfo LIKE '%5 Years%';

UPDATE Products 
SET WarrantyInfo = N'Trọn đời'
WHERE WarrantyInfo LIKE '%Tr?n đ?i%' OR WarrantyInfo LIKE '%Lifetime%';

UPDATE Products 
SET WarrantyInfo = N'Không bảo hành'
WHERE WarrantyInfo LIKE '%Kh?ng b?o h?nh%' OR WarrantyInfo LIKE '%No Warranty%';

UPDATE Products 
SET WarrantyInfo = N'Bảo hành nhà sản xuất'
WHERE WarrantyInfo LIKE '%B?o h?nh nh? s?n xu?t%' OR WarrantyInfo LIKE '%Manufacturer Warranty%';

UPDATE Products 
SET WarrantyInfo = N'Bảo hành quốc tế'
WHERE WarrantyInfo LIKE '%B?o h?nh qu?c t?%' OR WarrantyInfo LIKE '%International Warranty%';

-- PHẦN 4: CHUYỂN ĐỔI GIÁ TỪ USD SANG VND

UPDATE Products 
SET 
    Price = Price * 24500,
    CapitalPrice = CapitalPrice * 24500,
    CreditCardPrice = CreditCardPrice * 24500
WHERE Price > 0 AND Price < 100000;

-- PHẦN 5: TẠO LẠI INDEX (TÙY CHỌN)

-- 5.1. Tạo lại index trên cột Condition
CREATE INDEX IX_Products_Condition ON Products(Condition);
PRINT 'Đã tạo lại index IX_Products_Condition';

-- 5.2. Tạo lại index trên cột Gender
CREATE INDEX IX_Products_Gender ON Products(Gender);
PRINT 'Đã tạo lại index IX_Products_Gender';

-- 5.3. Tạo lại index trên cột Certificate
CREATE INDEX IX_Products_Certificate ON Products(Certificate);
PRINT 'Đã tạo lại index IX_Products_Certificate';

-- PHẦN 6: KIỂM TRA KẾT QUẢ

-- 6.1. Thống kê tổng quan
SELECT 
    N'Hoàn thành việt hóa!' as Message,
    COUNT(*) as TotalProducts,
    SUM(CASE WHEN Condition = N'Xuất sắc' THEN 1 ELSE 0 END) as XuấtSắcCount,
    SUM(CASE WHEN Gender = N'Nam' THEN 1 ELSE 0 END) as NamCount,
    SUM(CASE WHEN Gender = N'Nữ' THEN 1 ELSE 0 END) as NữCount,
    SUM(CASE WHEN Certificate = N'Có' THEN 1 ELSE 0 END) as CóCount,
    SUM(CASE WHEN Certificate = N'Không' THEN 1 ELSE 0 END) as KhôngCount,
    SUM(CASE WHEN Price > 1000000 THEN 1 ELSE 0 END) as ProductsWithVNDPrice
FROM Products;

-- 6.2. Hiển thị một số sản phẩm để kiểm tra
SELECT TOP 10
    Id,
    Name,
    Price,
    Gender,
    Condition,
    Certificate,
    WarrantyInfo,
    LEN(Condition) as ConditionLength
FROM Products
ORDER BY Id DESC;

-- 6.3. Kiểm tra xem còn lỗi nào không
SELECT 
    'Final Check' as Status,
    'Condition' as FieldName,
    Condition as Value,
    COUNT(*) as Count
FROM Products 
WHERE Condition LIKE '%?%'
GROUP BY Condition
UNION ALL
SELECT 
    'Final Check' as Status,
    'Gender' as FieldName,
    Gender as Value,
    COUNT(*) as Count
FROM Products 
WHERE Gender LIKE '%?%'
GROUP BY Gender;

-- 6.4. Test Unicode display
SELECT 
    N'Test Unicode: Xuất sắc' as UnicodeTest,
    N'Nam' as GenderTest,
    N'Có' as CertificateTest,
    N'1 Năm' as WarrantyTest;
