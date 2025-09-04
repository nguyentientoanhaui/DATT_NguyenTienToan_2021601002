-- Script kiểm tra encoding database
-- Chạy script này để kiểm tra tình trạng encoding

USE [Shopping_Demo]
GO

-- 1. Kiểm tra collation của database
SELECT 
    name as DatabaseName,
    collation_name as Collation
FROM sys.databases 
WHERE name = 'Shopping_Demo';

-- 2. Kiểm tra collation của bảng Products
SELECT 
    t.name as TableName,
    c.name as ColumnName,
    c.collation_name as Collation
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE t.name = 'Products' 
AND c.collation_name IS NOT NULL
ORDER BY c.name;

-- 3. Kiểm tra dữ liệu hiện tại
SELECT TOP 10
    Id,
    Name,
    Gender,
    Condition,
    Certificate,
    WarrantyInfo,
    LEN(Condition) as ConditionLength,
    ASCII(SUBSTRING(Condition, 1, 1)) as FirstCharASCII
FROM Products
WHERE Condition IS NOT NULL
ORDER BY Id DESC;

-- 4. Kiểm tra các giá trị bị lỗi encoding
SELECT 
    'Condition' as FieldName,
    Condition as Value,
    COUNT(*) as Count
FROM Products 
WHERE Condition LIKE '%?%'
GROUP BY Condition
UNION ALL
SELECT 
    'Gender' as FieldName,
    Gender as Value,
    COUNT(*) as Count
FROM Products 
WHERE Gender LIKE '%?%'
GROUP BY Gender
UNION ALL
SELECT 
    'Certificate' as FieldName,
    Certificate as Value,
    COUNT(*) as Count
FROM Products 
WHERE Certificate LIKE '%?%'
GROUP BY Certificate;

-- 5. Kiểm tra giá tiền đã chuyển đổi
SELECT 
    COUNT(*) as TotalProducts,
    SUM(CASE WHEN Price > 1000000 THEN 1 ELSE 0 END) as ProductsWithVNDPrice,
    MIN(Price) as MinPrice,
    MAX(Price) as MaxPrice,
    AVG(Price) as AvgPrice
FROM Products
WHERE Price > 0;

