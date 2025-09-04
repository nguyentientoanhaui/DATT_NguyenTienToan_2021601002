-- Script sửa lỗi encoding nâng cao
-- Sử dụng nhiều phương pháp để đảm bảo tiếng Việt hiển thị đúng

USE [Shopping_Demo]
GO

-- PHẦN 1: SỬA LỖI ENCODING VỚI NHIỀU PHƯƠNG PHÁP

-- 1.1. Sửa lỗi encoding cho trường Condition (Phương pháp 1: Thay thế trực tiếp)
UPDATE Products 
SET Condition = 
    CASE 
        WHEN Condition = 'Xu?ts?c' THEN N'Xuất sắc'
        WHEN Condition = 'R?t t?t' THEN N'Rất tốt'
        WHEN Condition = 'T?t' THEN N'Tốt'
        WHEN Condition = 'Kh?' THEN N'Khá'
        WHEN Condition = 'K?m' THEN N'Kém'
        WHEN Condition = 'M?i' THEN N'Mới'
        WHEN Condition = 'Đ? s? d?ng' THEN N'Đã sử dụng'
        WHEN Condition = 'Nh? m?i' THEN N'Như mới'
        WHEN Condition = 'G?n nh? m?i' THEN N'Gần như mới'
        WHEN Condition LIKE '%Xu?ts?c%' THEN N'Xuất sắc'
        WHEN Condition LIKE '%R?t t?t%' THEN N'Rất tốt'
        WHEN Condition LIKE '%T?t%' THEN N'Tốt'
        WHEN Condition LIKE '%Kh?%' THEN N'Khá'
        WHEN Condition LIKE '%K?m%' THEN N'Kém'
        WHEN Condition LIKE '%M?i%' THEN N'Mới'
        WHEN Condition LIKE '%Đ? s? d?ng%' THEN N'Đã sử dụng'
        WHEN Condition LIKE '%Nh? m?i%' THEN N'Như mới'
        WHEN Condition LIKE '%G?n nh? m?i%' THEN N'Gần như mới'
        ELSE Condition
    END
WHERE Condition IS NOT NULL;

-- 1.2. Sửa lỗi encoding cho trường Gender
UPDATE Products 
SET Gender = 
    CASE 
        WHEN Gender = 'N?m' THEN N'Nam'
        WHEN Gender = 'N?' THEN N'Nữ'
        WHEN Gender LIKE '%N?m%' THEN N'Nam'
        WHEN Gender LIKE '%N?%' THEN N'Nữ'
        ELSE Gender
    END
WHERE Gender IS NOT NULL;

-- 1.3. Sửa lỗi encoding cho trường Certificate
UPDATE Products 
SET Certificate = 
    CASE 
        WHEN Certificate = 'C?' THEN N'Có'
        WHEN Certificate = 'Kh?ng' THEN N'Không'
        WHEN Certificate = 'C? s?n' THEN N'Có sẵn'
        WHEN Certificate = 'Kh?ng c?' THEN N'Không có'
        WHEN Certificate = 'Bao g?m' THEN N'Bao gồm'
        WHEN Certificate = 'Kh?ng bao g?m' THEN N'Không bao gồm'
        WHEN Certificate LIKE '%C?%' THEN N'Có'
        WHEN Certificate LIKE '%Kh?ng%' THEN N'Không'
        WHEN Certificate LIKE '%C? s?n%' THEN N'Có sẵn'
        WHEN Certificate LIKE '%Kh?ng c?%' THEN N'Không có'
        WHEN Certificate LIKE '%Bao g?m%' THEN N'Bao gồm'
        WHEN Certificate LIKE '%Kh?ng bao g?m%' THEN N'Không bao gồm'
        ELSE Certificate
    END
WHERE Certificate IS NOT NULL;

-- 1.4. Sửa lỗi encoding cho trường WarrantyInfo
UPDATE Products 
SET WarrantyInfo = 
    CASE 
        WHEN WarrantyInfo = '1 N?m' THEN N'1 Năm'
        WHEN WarrantyInfo = '2 N?m' THEN N'2 Năm'
        WHEN WarrantyInfo = '3 N?m' THEN N'3 Năm'
        WHEN WarrantyInfo = '5 N?m' THEN N'5 Năm'
        WHEN WarrantyInfo = 'Tr?n đ?i' THEN N'Trọn đời'
        WHEN WarrantyInfo = 'Kh?ng b?o h?nh' THEN N'Không bảo hành'
        WHEN WarrantyInfo = 'B?o h?nh nh? s?n xu?t' THEN N'Bảo hành nhà sản xuất'
        WHEN WarrantyInfo = 'B?o h?nh qu?c t?' THEN N'Bảo hành quốc tế'
        WHEN WarrantyInfo LIKE '%1 N?m%' THEN N'1 Năm'
        WHEN WarrantyInfo LIKE '%2 N?m%' THEN N'2 Năm'
        WHEN WarrantyInfo LIKE '%3 N?m%' THEN N'3 Năm'
        WHEN WarrantyInfo LIKE '%5 N?m%' THEN N'5 Năm'
        WHEN WarrantyInfo LIKE '%Tr?n đ?i%' THEN N'Trọn đời'
        WHEN WarrantyInfo LIKE '%Kh?ng b?o h?nh%' THEN N'Không bảo hành'
        WHEN WarrantyInfo LIKE '%B?o h?nh nh? s?n xu?t%' THEN N'Bảo hành nhà sản xuất'
        WHEN WarrantyInfo LIKE '%B?o h?nh qu?c t?%' THEN N'Bảo hành quốc tế'
        ELSE WarrantyInfo
    END
WHERE WarrantyInfo IS NOT NULL;

-- PHẦN 2: CHUYỂN ĐỔI GIÁ (chỉ áp dụng cho giá USD)
UPDATE Products 
SET 
    Price = Price * 24500,
    CapitalPrice = CapitalPrice * 24500,
    CreditCardPrice = CreditCardPrice * 24500
WHERE Price > 0 AND Price < 100000;

-- PHẦN 3: KIỂM TRA KẾT QUẢ

-- 3.1. Kiểm tra encoding sau khi sửa
SELECT 
    'Encoding Check' as TestType,
    COUNT(*) as TotalProducts,
    SUM(CASE WHEN Condition = N'Xuất sắc' THEN 1 ELSE 0 END) as XuấtSắcCount,
    SUM(CASE WHEN Gender = N'Nam' THEN 1 ELSE 0 END) as NamCount,
    SUM(CASE WHEN Gender = N'Nữ' THEN 1 ELSE 0 END) as NữCount,
    SUM(CASE WHEN Certificate = N'Có' THEN 1 ELSE 0 END) as CóCount,
    SUM(CASE WHEN Certificate = N'Không' THEN 1 ELSE 0 END) as KhôngCount
FROM Products;

-- 3.2. Hiển thị một số sản phẩm để kiểm tra
SELECT TOP 5
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

-- 3.3. Kiểm tra các giá trị còn bị lỗi
SELECT 
    'Remaining Issues' as Status,
    'Condition' as FieldName,
    Condition as Value,
    COUNT(*) as Count
FROM Products 
WHERE Condition LIKE '%?%'
GROUP BY Condition
UNION ALL
SELECT 
    'Remaining Issues' as Status,
    'Gender' as FieldName,
    Gender as Value,
    COUNT(*) as Count
FROM Products 
WHERE Gender LIKE '%?%'
GROUP BY Gender;
