-- Script chuyển đổi giá từ USD sang VND
-- Tỷ giá: 1 USD = 24,500 VND (có thể điều chỉnh theo tỷ giá thực tế)

USE [Shopping_Demo]
GO

-- Backup dữ liệu trước khi cập nhật
PRINT 'Bắt đầu chuyển đổi giá từ USD sang VND...'
PRINT 'Tỷ giá sử dụng: 1 USD = 24,500 VND'

-- Kiểm tra dữ liệu hiện tại
SELECT 
    COUNT(*) as TotalProducts,
    MIN(Price) as MinPrice,
    MAX(Price) as MaxPrice,
    AVG(Price) as AvgPrice
FROM Products
WHERE Price > 0

PRINT 'Đang cập nhật cột Price...'

-- Cập nhật Price (USD -> VND)
UPDATE Products 
SET Price = ROUND(Price * 24500, 0)
WHERE Price > 0 AND Price < 1000000 -- Chỉ convert những giá nhỏ hơn 1 triệu (giả định là USD)

PRINT 'Đang cập nhật cột CapitalPrice...'

-- Cập nhật CapitalPrice nếu có
UPDATE Products 
SET CapitalPrice = ROUND(CapitalPrice * 24500, 0)
WHERE CapitalPrice > 0 AND CapitalPrice < 1000000

PRINT 'Đang cập nhật cột CreditCardPrice...'

-- Cập nhật CreditCardPrice nếu có  
UPDATE Products 
SET CreditCardPrice = ROUND(CreditCardPrice * 24500, 0)
WHERE CreditCardPrice > 0 AND CreditCardPrice < 1000000

-- Kiểm tra kết quả sau khi cập nhật
SELECT 
    COUNT(*) as TotalProducts,
    MIN(Price) as MinPrice,
    MAX(Price) as MaxPrice,
    AVG(Price) as AvgPrice
FROM Products
WHERE Price > 0

-- Hiển thị một số sản phẩm mẫu
SELECT TOP 10
    Id,
    Name,
    Price,
    CapitalPrice,
    CreditCardPrice
FROM Products
ORDER BY Price

PRINT 'Hoàn thành chuyển đổi giá sang VND!'
PRINT 'Vui lòng kiểm tra kết quả và cập nhật giao diện để hiển thị VND thay vì USD'
