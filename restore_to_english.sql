-- Script khôi phục dữ liệu về tiếng Anh như ban đầu
-- Khôi phục tất cả các trường về tiếng Anh và giá USD

USE [Shopping_Demo]
GO

-- PHẦN 1: KHÔI PHỤC CÁC TRƯỜNG VỀ TIẾNG ANH

-- 1.1. Khôi phục trường Condition về tiếng Anh
UPDATE Products 
SET Condition = 'Excellent'
WHERE Condition = N'Xuất sắc' OR Condition LIKE '%Xu?ts?c%';

UPDATE Products 
SET Condition = 'Very Good'
WHERE Condition = N'Rất tốt' OR Condition LIKE '%R?t t?t%';

UPDATE Products 
SET Condition = 'Good'
WHERE Condition = N'Tốt' OR Condition LIKE '%T?t%';

UPDATE Products 
SET Condition = 'Fair'
WHERE Condition = N'Khá' OR Condition LIKE '%Kh?%';

UPDATE Products 
SET Condition = 'Poor'
WHERE Condition = N'Kém' OR Condition LIKE '%K?m%';

UPDATE Products 
SET Condition = 'New'
WHERE Condition = N'Mới' OR Condition LIKE '%M?i%';

UPDATE Products 
SET Condition = 'Pre-owned'
WHERE Condition = N'Đã sử dụng' OR Condition LIKE '%Đ? s? d?ng%';

UPDATE Products 
SET Condition = 'Mint'
WHERE Condition = N'Như mới' OR Condition LIKE '%Nh? m?i%';

UPDATE Products 
SET Condition = 'Near Mint'
WHERE Condition = N'Gần như mới' OR Condition LIKE '%G?n nh? m?i%';

-- 1.2. Khôi phục trường Gender về tiếng Anh
UPDATE Products 
SET Gender = 'Men'
WHERE Gender = N'Nam' OR Gender LIKE '%N?m%';

UPDATE Products 
SET Gender = 'Women'
WHERE Gender = N'Nữ' OR Gender LIKE '%N?%';

UPDATE Products 
SET Gender = 'Unisex'
WHERE Gender = N'Unisex';

-- 1.3. Khôi phục trường Certificate về tiếng Anh
UPDATE Products 
SET Certificate = 'Yes'
WHERE Certificate = N'Có' OR Certificate LIKE '%C?%';

UPDATE Products 
SET Certificate = 'No'
WHERE Certificate = N'Không' OR Certificate LIKE '%Kh?ng%';

UPDATE Products 
SET Certificate = 'Available'
WHERE Certificate = N'Có sẵn' OR Certificate LIKE '%C? s?n%';

UPDATE Products 
SET Certificate = 'Not Available'
WHERE Certificate = N'Không có' OR Certificate LIKE '%Kh?ng c?%';

UPDATE Products 
SET Certificate = 'Included'
WHERE Certificate = N'Bao gồm' OR Certificate LIKE '%Bao g?m%';

UPDATE Products 
SET Certificate = 'Not Included'
WHERE Certificate = N'Không bao gồm' OR Certificate LIKE '%Kh?ng bao g?m%';

-- 1.4. Khôi phục trường WarrantyInfo về tiếng Anh
UPDATE Products 
SET WarrantyInfo = '1 Year'
WHERE WarrantyInfo = N'1 Năm' OR WarrantyInfo LIKE '%1 N?m%';

UPDATE Products 
SET WarrantyInfo = '2 Years'
WHERE WarrantyInfo = N'2 Năm' OR WarrantyInfo LIKE '%2 N?m%';

UPDATE Products 
SET WarrantyInfo = '3 Years'
WHERE WarrantyInfo = N'3 Năm' OR WarrantyInfo LIKE '%3 N?m%';

UPDATE Products 
SET WarrantyInfo = '5 Years'
WHERE WarrantyInfo = N'5 Năm' OR WarrantyInfo LIKE '%5 N?m%';

UPDATE Products 
SET WarrantyInfo = 'Lifetime'
WHERE WarrantyInfo = N'Trọn đời' OR WarrantyInfo LIKE '%Tr?n đ?i%';

UPDATE Products 
SET WarrantyInfo = 'No Warranty'
WHERE WarrantyInfo = N'Không bảo hành' OR WarrantyInfo LIKE '%Kh?ng b?o h?nh%';

UPDATE Products 
SET WarrantyInfo = 'Manufacturer Warranty'
WHERE WarrantyInfo = N'Bảo hành nhà sản xuất' OR WarrantyInfo LIKE '%B?o h?nh nh? s?n xu?t%';

UPDATE Products 
SET WarrantyInfo = 'International Warranty'
WHERE WarrantyInfo = N'Bảo hành quốc tế' OR WarrantyInfo LIKE '%B?o h?nh qu?c t?%';

-- PHẦN 2: KHÔI PHỤC GIÁ VỀ USD

UPDATE Products 
SET 
    Price = Price / 24500,
    CapitalPrice = CapitalPrice / 24500,
    CreditCardPrice = CreditCardPrice / 24500
WHERE Price > 1000000; -- Chỉ áp dụng cho những sản phẩm có giá > 1 triệu VND

-- PHẦN 3: KIỂM TRA KẾT QUẢ

-- 3.1. Thống kê tổng quan
SELECT 
    'Restored to English!' as Message,
    COUNT(*) as TotalProducts,
    SUM(CASE WHEN Condition = 'Excellent' THEN 1 ELSE 0 END) as ExcellentCount,
    SUM(CASE WHEN Gender = 'Men' THEN 1 ELSE 0 END) as MenCount,
    SUM(CASE WHEN Gender = 'Women' THEN 1 ELSE 0 END) as WomenCount,
    SUM(CASE WHEN Certificate = 'Yes' THEN 1 ELSE 0 END) as YesCount,
    SUM(CASE WHEN Certificate = 'No' THEN 1 ELSE 0 END) as NoCount,
    SUM(CASE WHEN Price < 100000 THEN 1 ELSE 0 END) as ProductsWithUSDPrice
FROM Products;

-- 3.2. Hiển thị một số sản phẩm để kiểm tra
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

-- 3.3. Kiểm tra xem còn lỗi nào không
SELECT 
    'Final Check' as Status,
    'Condition' as FieldName,
    Condition as Value,
    COUNT(*) as Count
FROM Products 
WHERE Condition LIKE '%?%' OR Condition LIKE N'%ấ%' OR Condition LIKE N'%ứ%'
GROUP BY Condition
UNION ALL
SELECT 
    'Final Check' as Status,
    'Gender' as FieldName,
    Gender as Value,
    COUNT(*) as Count
FROM Products 
WHERE Gender LIKE '%?%' OR Gender LIKE N'%ữ%' OR Gender LIKE N'%am%'
GROUP BY Gender;

-- 3.4. Test English display
SELECT 
    'Test English: Excellent' as EnglishTest,
    'Men' as GenderTest,
    'Yes' as CertificateTest,
    '1 Year' as WarrantyTest;
