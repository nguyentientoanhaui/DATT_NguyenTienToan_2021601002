-- Script cuối cùng: Thay đổi collation và force fix encoding
-- Chạy script này để sửa hoàn toàn vấn đề encoding

USE [Shopping_Demo]
GO

-- PHẦN 1: THAY ĐỔI COLLATION CỦA CÁC CỘT QUAN TRỌNG

-- 1.1. Thay đổi collation của cột Condition
ALTER TABLE Products 
ALTER COLUMN Condition NVARCHAR(200) COLLATE Vietnamese_CI_AS;

-- 1.2. Thay đổi collation của cột Gender
ALTER TABLE Products 
ALTER COLUMN Gender NVARCHAR(50) COLLATE Vietnamese_CI_AS;

-- 1.3. Thay đổi collation của cột Certificate
ALTER TABLE Products 
ALTER COLUMN Certificate NVARCHAR(100) COLLATE Vietnamese_CI_AS;

-- 1.4. Thay đổi collation của cột WarrantyInfo
ALTER TABLE Products 
ALTER COLUMN WarrantyInfo NVARCHAR(200) COLLATE Vietnamese_CI_AS;

-- PHẦN 2: FORCE FIX DỮ LIỆU BỊ CORRUPT

-- 2.1. Thay thế hoàn toàn các giá trị Condition bị lỗi
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

-- 2.2. Thay thế hoàn toàn các giá trị Gender bị lỗi
UPDATE Products 
SET Gender = N'Nam'
WHERE Gender LIKE '%N?m%' OR Gender LIKE '%Men%' OR Gender LIKE '%Gents%';

UPDATE Products 
SET Gender = N'Nữ'
WHERE Gender LIKE '%N?%' OR Gender LIKE '%Women%' OR Gender LIKE '%Ladies%';

UPDATE Products 
SET Gender = N'Unisex'
WHERE Gender LIKE '%Unisex%';

-- 2.3. Thay thế hoàn toàn các giá trị Certificate bị lỗi
UPDATE Products 
SET Certificate = N'Có'
WHERE Certificate LIKE '%C?%' OR Certificate LIKE '%Yes%' OR Certificate LIKE '%Available%' OR Certificate LIKE '%Included%';

UPDATE Products 
SET Certificate = N'Không'
WHERE Certificate LIKE '%Kh?ng%' OR Certificate LIKE '%No%' OR Certificate LIKE '%Not Available%' OR Certificate LIKE '%Not Included%';

-- 2.4. Thay thế hoàn toàn các giá trị WarrantyInfo bị lỗi
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

-- PHẦN 3: CHUYỂN ĐỔI GIÁ TỪ USD SANG VND

UPDATE Products 
SET 
    Price = Price * 24500,
    CapitalPrice = CapitalPrice * 24500,
    CreditCardPrice = CreditCardPrice * 24500
WHERE Price > 0 AND Price < 100000;

-- PHẦN 4: KIỂM TRA KẾT QUẢ

-- 4.1. Kiểm tra collation mới
SELECT 
    c.name as ColumnName,
    c.collation_name as NewCollation,
    t.name as DataType
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('Products')
AND c.name IN ('Condition', 'Gender', 'Certificate', 'WarrantyInfo')
ORDER BY c.name;

-- 4.2. Thống kê tổng quan
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

-- 4.3. Hiển thị một số sản phẩm để kiểm tra
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

-- 4.4. Kiểm tra xem còn lỗi nào không
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
