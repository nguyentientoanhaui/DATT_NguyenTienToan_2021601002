-- Script khôi phục giá về USD ban đầu
-- Chạy script này để đưa giá về trạng thái ban đầu

USE [Shopping_Demo]
GO

-- 1. Khôi phục giá về USD (chia cho 24500)
UPDATE Products 
SET 
    Price = Price / 24500,
    CapitalPrice = CapitalPrice / 24500,
    CreditCardPrice = CreditCardPrice / 24500
WHERE Price > 1000000; -- Chỉ áp dụng cho những sản phẩm có giá > 1 triệu VND

-- 2. Hiển thị kết quả sau khi khôi phục
SELECT 
    'Prices restored to USD!' as Message,
    COUNT(*) as TotalProducts,
    SUM(CASE WHEN Price < 100000 THEN 1 ELSE 0 END) as ProductsWithUSDPrice,
    MIN(Price) as MinPrice,
    MAX(Price) as MaxPrice,
    AVG(Price) as AvgPrice
FROM Products
WHERE Price > 0;

-- 3. Hiển thị một số sản phẩm để kiểm tra
SELECT TOP 10
    Id,
    Name,
    Price,
    CapitalPrice,
    CreditCardPrice
FROM Products
ORDER BY Id DESC;
