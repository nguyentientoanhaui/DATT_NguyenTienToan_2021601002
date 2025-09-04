-- Script kiểm tra và sửa lỗi encoding chi tiết
-- Chạy script này để kiểm tra tình trạng encoding

USE [Shopping_Demo]
GO

-- 1. Kiểm tra collation của database
SELECT 
    name as DatabaseName,
    collation_name as Collation,
    CASE 
        WHEN collation_name LIKE '%_CI_AI' THEN 'Case Insensitive, Accent Insensitive'
        WHEN collation_name LIKE '%_CS_AI' THEN 'Case Sensitive, Accent Insensitive'
        WHEN collation_name LIKE '%_CI_AS' THEN 'Case Insensitive, Accent Sensitive'
        WHEN collation_name LIKE '%_CS_AS' THEN 'Case Sensitive, Accent Sensitive'
        ELSE 'Other'
    END as CollationType
FROM sys.databases 
WHERE name = 'Shopping_Demo';

-- 2. Kiểm tra collation của các cột text trong bảng Products
SELECT 
    c.name as ColumnName,
    c.collation_name as Collation,
    t.name as DataType,
    c.max_length as MaxLength
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('Products')
AND t.name IN ('nvarchar', 'varchar', 'nchar', 'char', 'ntext', 'text')
ORDER BY c.name;

-- 3. Kiểm tra dữ liệu hiện tại và encoding
SELECT TOP 10
    Id,
    Name,
    Gender,
    Condition,
    Certificate,
    WarrantyInfo,
    LEN(Condition) as ConditionLength,
    ASCII(SUBSTRING(Condition, 1, 1)) as FirstCharASCII,
    UNICODE(SUBSTRING(Condition, 1, 1)) as FirstCharUnicode
FROM Products
WHERE Condition IS NOT NULL
ORDER BY Id DESC;

-- 4. Kiểm tra các giá trị bị lỗi encoding
SELECT 
    'Condition' as FieldName,
    Condition as Value,
    COUNT(*) as Count,
    LEN(Condition) as Length
FROM Products 
WHERE Condition LIKE '%?%'
GROUP BY Condition, LEN(Condition)
UNION ALL
SELECT 
    'Gender' as FieldName,
    Gender as Value,
    COUNT(*) as Count,
    LEN(Gender) as Length
FROM Products 
WHERE Gender LIKE '%?%'
GROUP BY Gender, LEN(Gender)
UNION ALL
SELECT 
    'Certificate' as FieldName,
    Certificate as Value,
    COUNT(*) as Count,
    LEN(Certificate) as Length
FROM Products 
WHERE Certificate LIKE '%?%'
GROUP BY Certificate, LEN(Certificate);

-- 5. Test Unicode support
SELECT 
    N'Test Unicode: Xuất sắc' as UnicodeTest,
    LEN(N'Xuất sắc') as UnicodeLength,
    UNICODE(N'X') as X_Unicode,
    UNICODE(N'u') as u_Unicode,
    UNICODE(N'ấ') as a_Unicode;

-- 6. Kiểm tra giá tiền hiện tại
SELECT 
    COUNT(*) as TotalProducts,
    SUM(CASE WHEN Price > 1000000 THEN 1 ELSE 0 END) as ProductsWithVNDPrice,
    MIN(Price) as MinPrice,
    MAX(Price) as MaxPrice,
    AVG(Price) as AvgPrice
FROM Products
WHERE Price > 0;
