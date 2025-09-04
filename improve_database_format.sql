-- Script cải thiện định dạng database để hỗ trợ Unicode tốt hơn
-- Thay đổi collation và cấu trúc để hỗ trợ tiếng Việt

USE [Shopping_Demo]
GO

-- PHẦN 1: KIỂM TRA COLLATION HIỆN TẠI

-- 1.1. Kiểm tra collation database
SELECT 
    name as DatabaseName,
    collation_name as CurrentCollation,
    CASE 
        WHEN collation_name LIKE '%_CI_AI' THEN 'Case Insensitive, Accent Insensitive'
        WHEN collation_name LIKE '%_CS_AI' THEN 'Case Sensitive, Accent Insensitive'
        WHEN collation_name LIKE '%_CI_AS' THEN 'Case Insensitive, Accent Sensitive'
        WHEN collation_name LIKE '%_CS_AS' THEN 'Case Sensitive, Accent Sensitive'
        ELSE 'Other'
    END as CollationType
FROM sys.databases 
WHERE name = 'Shopping_Demo';

-- 1.2. Kiểm tra collation các cột text
SELECT 
    c.name as ColumnName,
    c.collation_name as CurrentCollation,
    t.name as DataType,
    c.max_length as MaxLength
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('Products')
AND c.name IN ('Condition', 'Gender', 'Certificate', 'WarrantyInfo', 'Name', 'Description')
ORDER BY c.name;

-- PHẦN 2: TẠO BẢNG TEMP VỚI COLLATION TỐT HƠN

-- 2.1. Tạo bảng temp với Unicode support
CREATE TABLE Products_Temp (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(500) COLLATE Vietnamese_CI_AS,
    Price DECIMAL(18,2),
    CapitalPrice DECIMAL(18,2),
    CreditCardPrice DECIMAL(18,2),
    Gender NVARCHAR(50) COLLATE Vietnamese_CI_AS,
    Condition NVARCHAR(200) COLLATE Vietnamese_CI_AS,
    Certificate NVARCHAR(100) COLLATE Vietnamese_CI_AS,
    WarrantyInfo NVARCHAR(200) COLLATE Vietnamese_CI_AS,
    Description NVARCHAR(MAX) COLLATE Vietnamese_CI_AS,
    Image NVARCHAR(500),
    BrandId INT,
    CategoryId INT,
    -- Thêm các trường khác nếu cần
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);

-- PHẦN 3: SAO CHÉP DỮ LIỆU VỚI ĐỊNH DẠNG MỚI

-- 3.1. Sao chép dữ liệu cơ bản
INSERT INTO Products_Temp (Name, Price, CapitalPrice, CreditCardPrice, Gender, Condition, Certificate, WarrantyInfo, Description, Image, BrandId, CategoryId)
SELECT 
    Name,
    Price,
    CapitalPrice,
    CreditCardPrice,
    Gender,
    Condition,
    Certificate,
    WarrantyInfo,
    Description,
    Image,
    BrandId,
    CategoryId
FROM Products;

-- PHẦN 4: KIỂM TRA DỮ LIỆU SAU KHI SAO CHÉP

-- 4.1. So sánh số lượng bản ghi
SELECT 
    'Original Table' as TableName,
    COUNT(*) as RecordCount
FROM Products
UNION ALL
SELECT 
    'Temp Table' as TableName,
    COUNT(*) as RecordCount
FROM Products_Temp;

-- 4.2. Kiểm tra encoding trong bảng temp
SELECT TOP 10
    Id,
    Name,
    Gender,
    Condition,
    Certificate,
    WarrantyInfo,
    LEN(Condition) as ConditionLength
FROM Products_Temp
ORDER BY Id DESC;

-- PHẦN 5: TEST UNICODE SUPPORT

-- 5.1. Test chèn dữ liệu tiếng Việt
INSERT INTO Products_Temp (Name, Gender, Condition, Certificate, WarrantyInfo)
VALUES 
    (N'Đồng hồ Rolex Submariner', N'Nam', N'Xuất sắc', N'Có', N'5 Năm'),
    (N'Đồng hồ Omega Speedmaster', N'Nam', N'Rất tốt', N'Không', N'3 Năm'),
    (N'Đồng hồ Cartier Tank', N'Nữ', N'Tốt', N'Có', N'2 Năm');

-- 5.2. Kiểm tra dữ liệu test
SELECT 
    'Test Data' as DataType,
    Name,
    Gender,
    Condition,
    Certificate,
    WarrantyInfo
FROM Products_Temp
WHERE Name LIKE N'%Đồng hồ%';

-- PHẦN 6: HƯỚNG DẪN HOÀN THÀNH

-- 6.1. Thông báo các bước tiếp theo
SELECT 
    'NEXT STEPS:' as Instruction,
    '1. Backup original table' as Step1,
    '2. Drop original table' as Step2,
    '3. Rename temp table to Products' as Step3,
    '4. Recreate indexes and constraints' as Step4;

-- 6.2. Script để hoàn thành (chạy sau khi kiểm tra)
PRINT '-- Để hoàn thành, chạy các lệnh sau:';
PRINT '-- 1. BACKUP TABLE: SELECT * INTO Products_Backup FROM Products;';
PRINT '-- 2. DROP ORIGINAL: DROP TABLE Products;';
PRINT '-- 3. RENAME TEMP: EXEC sp_rename ''Products_Temp'', ''Products'';';
PRINT '-- 4. RECREATE INDEXES: CREATE INDEX IX_Products_Condition ON Products(Condition);';
