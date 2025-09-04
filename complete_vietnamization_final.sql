-- Script hoàn chỉnh: Chuyển đổi giá USD sang VND + Sửa encoding tiếng Việt
-- Chạy script này MỘT LẦN DUY NHẤT để tránh nhân giá nhiều lần

USE [Shopping_Demo]
GO

-- PHẦN 1: CHUYỂN ĐỔI GIÁ TỪ USD SANG VND (chỉ áp dụng cho giá < 100,000)
UPDATE Products 
SET 
    Price = Price * 24500,
    CapitalPrice = CapitalPrice * 24500,
    CreditCardPrice = CreditCardPrice * 24500
WHERE Price > 0 AND Price < 100000; -- Chỉ áp dụng cho giá USD (không áp dụng cho giá đã chuyển đổi)

-- PHẦN 2: SỬA LỖI ENCODING TIẾNG VIỆT

-- 2.1. Sửa lỗi encoding cho trường Condition
UPDATE Products 
SET Condition = 
    CASE 
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

-- 2.2. Sửa lỗi encoding cho trường Gender
UPDATE Products 
SET Gender = 
    CASE 
        WHEN Gender LIKE '%N?m%' THEN N'Nam'
        WHEN Gender LIKE '%N?%' THEN N'Nữ'
        ELSE Gender
    END
WHERE Gender IS NOT NULL;

-- 2.3. Sửa lỗi encoding cho trường Certificate
UPDATE Products 
SET Certificate = 
    CASE 
        WHEN Certificate LIKE '%C?%' THEN N'Có'
        WHEN Certificate LIKE '%Kh?ng%' THEN N'Không'
        WHEN Certificate LIKE '%C? s?n%' THEN N'Có sẵn'
        WHEN Certificate LIKE '%Kh?ng c?%' THEN N'Không có'
        WHEN Certificate LIKE '%Bao g?m%' THEN N'Bao gồm'
        WHEN Certificate LIKE '%Kh?ng bao g?m%' THEN N'Không bao gồm'
        ELSE Certificate
    END
WHERE Certificate IS NOT NULL;

-- 2.4. Sửa lỗi encoding cho trường WarrantyInfo
UPDATE Products 
SET WarrantyInfo = 
    CASE 
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

-- PHẦN 3: HIỂN THỊ KẾT QUẢ

-- 3.1. Thống kê tổng quan
SELECT 
    N'Việt hóa hoàn thành!' as Message,
    COUNT(*) as TotalProducts,
    SUM(CASE WHEN Price > 1000000 THEN 1 ELSE 0 END) as ProductsWithVNDPrice,
    SUM(CASE WHEN Condition LIKE N'%Xuất sắc%' THEN 1 ELSE 0 END) as FixedConditions,
    SUM(CASE WHEN Gender LIKE N'%Nam%' OR Gender LIKE N'%Nữ%' THEN 1 ELSE 0 END) as FixedGenders
FROM Products;

-- 3.2. Hiển thị một số sản phẩm mẫu để kiểm tra
SELECT TOP 5
    Id,
    Name,
    Price,
    CapitalPrice,
    Gender,
    Condition,
    Certificate,
    WarrantyInfo
FROM Products
ORDER BY Id DESC;
