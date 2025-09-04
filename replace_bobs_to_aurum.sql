-- Script để thay thế "Bob's Watches" thành "Aurum Watches" trong cột WarrantyInfo
-- Tạo bởi: Assistant
-- Ngày: $(Get-Date)

USE [Shopping_Demo]
GO

-- Bước 1: Backup dữ liệu trước khi thay đổi
PRINT 'Creating backup...'
BACKUP DATABASE [Shopping_Demo] 
TO DISK = 'C:\Backup\Shopping_Demo_Before_Warranty_Update.bak'
WITH FORMAT, INIT;
PRINT 'Backup completed successfully!'

-- Bước 2: Kiểm tra số lượng bản ghi cần cập nhật
PRINT 'Checking records to be updated...'
SELECT 
    COUNT(*) as [Total_Records_To_Update],
    'Bob''s Watches' as [Search_Text]
FROM [dbo].[Products] 
WHERE [WarrantyInfo] LIKE '%Bob''s Watches%'

-- Bước 3: Preview một số bản ghi sẽ được cập nhật
PRINT 'Preview of records to be updated:'
SELECT TOP 5 
    [Id],
    [Name],
    LEFT([WarrantyInfo], 100) + '...' as [Current_WarrantyInfo_Preview]
FROM [dbo].[Products] 
WHERE [WarrantyInfo] LIKE '%Bob''s Watches%'

-- Bước 4: Bắt đầu transaction để có thể rollback nếu cần
BEGIN TRANSACTION

BEGIN TRY
    PRINT 'Starting warranty info update...'
    
    -- Thực hiện cập nhật
    UPDATE [dbo].[Products] 
    SET [WarrantyInfo] = REPLACE([WarrantyInfo], 'Bob''s Watches', 'Aurum Watches'),
        [UpdatedDate] = GETDATE()
    WHERE [WarrantyInfo] LIKE '%Bob''s Watches%'
    
    -- Kiểm tra kết quả
    DECLARE @UpdatedCount INT = @@ROWCOUNT
    PRINT 'Updated ' + CAST(@UpdatedCount AS VARCHAR(10)) + ' records successfully!'
    
    -- Verification: Kiểm tra xem còn "Bob's Watches" nào không
    SELECT COUNT(*) as [Remaining_Bobs_Watches]
    FROM [dbo].[Products] 
    WHERE [WarrantyInfo] LIKE '%Bob''s Watches%'
    
    -- Verification: Đếm số "Aurum Watches" mới
    SELECT COUNT(*) as [New_Aurum_Watches]
    FROM [dbo].[Products] 
    WHERE [WarrantyInfo] LIKE '%Aurum Watches%'
    
    -- Preview kết quả sau cập nhật
    PRINT 'Sample of updated records:'
    SELECT TOP 5 
        [Id],
        [Name],
        LEFT([WarrantyInfo], 100) + '...' as [Updated_WarrantyInfo_Preview]
    FROM [dbo].[Products] 
    WHERE [WarrantyInfo] LIKE '%Aurum Watches%'
    ORDER BY [UpdatedDate] DESC
    
    -- Nếu mọi thứ OK, commit transaction
    COMMIT TRANSACTION
    PRINT '✅ Transaction committed successfully!'
    PRINT '✅ All "Bob''s Watches" have been replaced with "Aurum Watches"'
    
END TRY
BEGIN CATCH
    -- Nếu có lỗi, rollback
    ROLLBACK TRANSACTION
    
    PRINT '❌ Error occurred during update!'
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR(10))
    PRINT 'Error Message: ' + ERROR_MESSAGE()
    PRINT '❌ Transaction rolled back!'
    
END CATCH

-- Bước 5: Final verification
PRINT '=== FINAL VERIFICATION ==='
SELECT 
    COUNT(*) as [Total_Products_With_Warranty],
    SUM(CASE WHEN [WarrantyInfo] LIKE '%Aurum Watches%' THEN 1 ELSE 0 END) as [Products_With_Aurum_Watches],
    SUM(CASE WHEN [WarrantyInfo] LIKE '%Bob''s Watches%' THEN 1 ELSE 0 END) as [Products_With_Bobs_Watches_Remaining]
FROM [dbo].[Products] 
WHERE [WarrantyInfo] IS NOT NULL AND [WarrantyInfo] != ''

PRINT '✅ Update process completed!'
