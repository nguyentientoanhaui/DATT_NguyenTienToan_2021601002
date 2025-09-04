-- Script chuyển đổi tiền tệ từ USD sang VND
-- Tỷ giá: 1 USD = 24,500 VND

USE [Shopping_Demo]
GO

-- Cập nhật giá tiền từ USD sang VND
UPDATE Products 
SET 
    Price = Price * 24500,
    CapitalPrice = CapitalPrice * 24500,
    CreditCardPrice = CreditCardPrice * 24500
WHERE Price > 0;

-- Hiển thị kết quả
SELECT 
    'Currency conversion completed!' as Message,
    COUNT(*) as TotalProducts,
    SUM(CASE WHEN Price > 1000000 THEN 1 ELSE 0 END) as ProductsWithVNDPrice,
    MIN(Price) as MinPrice,
    MAX(Price) as MaxPrice,
    AVG(Price) as AvgPrice
FROM Products;

-- Hiển thị một số sản phẩm mẫu để kiểm tra
SELECT TOP 10
    Id,
    Name,
    Price,
    CapitalPrice,
    CreditCardPrice
FROM Products
ORDER BY Id DESC;

