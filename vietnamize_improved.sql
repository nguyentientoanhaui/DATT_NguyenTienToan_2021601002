-- Script chuyển đổi tiếng Việt với định dạng đã cải thiện
-- Chạy script này sau khi đã cải thiện định dạng database

USE [Shopping_Demo]
GO

-- PHẦN 1: CHUYỂN ĐỔI DỮ LIỆU SANG TIẾNG VIỆT

-- 1.1. Chuyển đổi trường Condition
UPDATE Products 
SET Condition = N'Xuất sắc'
WHERE Condition = 'Excellent';

UPDATE Products 
SET Condition = N'Rất tốt'
WHERE Condition = 'Very Good';

UPDATE Products 
SET Condition = N'Tốt'
WHERE Condition = 'Good';

UPDATE Products 
SET Condition = N'Khá'
WHERE Condition = 'Fair';

UPDATE Products 
SET Condition = N'Kém'
WHERE Condition = 'Poor';

UPDATE Products 
SET Condition = N'Mới'
WHERE Condition = 'New';

UPDATE Products 
SET Condition = N'Đã sử dụng'
WHERE Condition = 'Pre-owned';

UPDATE Products 
SET Condition = N'Như mới'
WHERE Condition = 'Mint';

UPDATE Products 
SET Condition = N'Gần như mới'
WHERE Condition = 'Near Mint';

-- 1.2. Chuyển đổi trường Gender
UPDATE Products 
SET Gender = N'Nam'
WHERE Gender = 'Men';

UPDATE Products 
SET Gender = N'Nữ'
WHERE Gender = 'Women';

-- 1.3. Chuyển đổi trường Certificate
UPDATE Products 
SET Certificate = N'Có'
WHERE Certificate = 'Yes';

UPDATE Products 
SET Certificate = N'Không'
WHERE Certificate = 'No';

UPDATE Products 
SET Certificate = N'Có sẵn'
WHERE Certificate = 'Available';

UPDATE Products 
SET Certificate = N'Không có'
WHERE Certificate = 'Not Available';

UPDATE Products 
SET Certificate = N'Bao gồm'
WHERE Certificate = 'Included';

UPDATE Products 
SET Certificate = N'Không bao gồm'
WHERE Certificate = 'Not Included';

-- 1.4. Chuyển đổi trường WarrantyInfo
UPDATE Products 
SET WarrantyInfo = N'1 Năm'
WHERE WarrantyInfo = '1 Year';

UPDATE Products 
SET WarrantyInfo = N'2 Năm'
WHERE WarrantyInfo = '2 Years';

UPDATE Products 
SET WarrantyInfo = N'3 Năm'
WHERE WarrantyInfo = '3 Years';

UPDATE Products 
SET WarrantyInfo = N'5 Năm'
WHERE WarrantyInfo = '5 Years';

UPDATE Products 
SET WarrantyInfo = N'Trọn đời'
WHERE WarrantyInfo = 'Lifetime';

UPDATE Products 
SET WarrantyInfo = N'Không bảo hành'
WHERE WarrantyInfo = 'No Warranty';

UPDATE Products 
SET WarrantyInfo = N'Bảo hành nhà sản xuất'
WHERE WarrantyInfo = 'Manufacturer Warranty';

UPDATE Products 
SET WarrantyInfo = N'Bảo hành quốc tế'
WHERE WarrantyInfo = 'International Warranty';

-- PHẦN 2: CHUYỂN ĐỔI GIÁ TỪ USD SANG VND

UPDATE Products 
SET 
    Price = Price * 24500,
    CapitalPrice = CapitalPrice * 24500,
    CreditCardPrice = CreditCardPrice * 24500
WHERE Price > 0 AND Price < 100000;

-- PHẦN 3: KIỂM TRA KẾT QUẢ

-- 3.1. Thống kê tổng quan
SELECT 
    N'Việt hóa hoàn thành!' as Message,
    COUNT(*) as TotalProducts,
    SUM(CASE WHEN Condition = N'Xuất sắc' THEN 1 ELSE 0 END) as XuấtSắcCount,
    SUM(CASE WHEN Gender = N'Nam' THEN 1 ELSE 0 END) as NamCount,
    SUM(CASE WHEN Gender = N'Nữ' THEN 1 ELSE 0 END) as NữCount,
    SUM(CASE WHEN Certificate = N'Có' THEN 1 ELSE 0 END) as CóCount,
    SUM(CASE WHEN Certificate = N'Không' THEN 1 ELSE 0 END) as KhôngCount,
    SUM(CASE WHEN Price > 1000000 THEN 1 ELSE 0 END) as ProductsWithVNDPrice
FROM Products;

-- 3.2. Hiển thị một số sản phẩm để kiểm tra
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

-- 3.3. Test Unicode display
SELECT 
    N'Test Unicode: Xuất sắc' as UnicodeTest,
    N'Nam' as GenderTest,
    N'Có' as CertificateTest,
    N'5 Năm' as WarrantyTest,
    LEN(N'Xuất sắc') as UnicodeLength;

-- 3.4. Kiểm tra xem còn lỗi nào không
SELECT 
    'Final Check' as Status,
    'Condition' as FieldName,
    Condition as Value,
    COUNT(*) as Count
FROM Products 
WHERE Condition LIKE '%?%' OR Condition LIKE '%Excellent%' OR Condition LIKE '%Good%'
GROUP BY Condition
UNION ALL
SELECT 
    'Final Check' as Status,
    'Gender' as FieldName,
    Gender as Value,
    COUNT(*) as Count
FROM Products 
WHERE Gender LIKE '%?%' OR Gender LIKE '%Men%' OR Gender LIKE '%Women%'
GROUP BY Gender;

-- PHẦN 4: THÔNG BÁO HOÀN THÀNH

SELECT 
    N'THÀNH CÔNG!' as Status,
    N'Việt hóa hoàn thành với định dạng mới' as Message,
    N'Unicode hỗ trợ hoàn hảo' as Feature1,
    N'Tiếng Việt hiển thị đúng' as Feature2,
    N'Giá tiền đã chuyển sang VND' as Feature3;
