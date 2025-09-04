-- Script nhanh chuyển đổi USD sang VND
-- Chạy script này trong SQL Server Management Studio

USE [Shopping_Demo]
GO

PRINT 'Bắt đầu chuyển đổi USD sang VND...'

-- Backup nhanh
SELECT * INTO Products_Backup_Before_VND FROM Products

-- Chuyển đổi với tỷ giá 24,500 VND/USD
UPDATE Products 
SET Price = ROUND(Price * 24500, 0)
WHERE Price > 0 AND Price < 100000  -- Chỉ convert giá < 100k (chắc chắn là USD)

UPDATE Products 
SET CapitalPrice = ROUND(CapitalPrice * 24500, 0)
WHERE CapitalPrice > 0 AND CapitalPrice < 100000

UPDATE Products 
SET CreditCardPrice = ROUND(CreditCardPrice * 24500, 0)
WHERE CreditCardPrice > 0 AND CreditCardPrice < 100000

-- Kiểm tra kết quả
SELECT TOP 10 
    Name, 
    Price,
    CapitalPrice
FROM Products 
ORDER BY Price

PRINT 'Chuyển đổi hoàn tất! Giá đã được chuyển sang VND.'
