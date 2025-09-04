-- Script nhanh để thay thế "Bob's Watches" thành "Aurum Watches"
-- ⚠️ CẢNH BÁO: Chạy backup trước khi thực hiện!

USE [Shopping_Demo]
GO

-- Backup database trước khi thay đổi
PRINT 'Creating backup...'
BACKUP DATABASE [Shopping_Demo] 
TO DISK = 'C:\Backup\Shopping_Demo_Warranty_Quick_Backup.bak'
WITH FORMAT, INIT
GO

-- Kiểm tra số lượng bản ghi cần update
PRINT 'Records to update:'
SELECT COUNT(*) as [Records_To_Update]
FROM [dbo].[Products] 
WHERE [WarrantyInfo] LIKE '%Bob''s Watches%'
GO

-- Thực hiện update
PRINT 'Updating warranty information...'
UPDATE [dbo].[Products] 
SET [WarrantyInfo] = REPLACE([WarrantyInfo], 'Bob''s Watches', 'Aurum Watches'),
    [UpdatedDate] = GETDATE()
WHERE [WarrantyInfo] LIKE '%Bob''s Watches%'

PRINT 'Updated ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' records'
GO

-- Kiểm tra kết quả
PRINT 'Verification:'
SELECT 
    COUNT(*) as [Total_Products_With_Warranty],
    SUM(CASE WHEN [WarrantyInfo] LIKE '%Aurum Watches%' THEN 1 ELSE 0 END) as [Aurum_Watches_Count],
    SUM(CASE WHEN [WarrantyInfo] LIKE '%Bob''s Watches%' THEN 1 ELSE 0 END) as [Bobs_Watches_Remaining]
FROM [dbo].[Products] 
WHERE [WarrantyInfo] IS NOT NULL AND [WarrantyInfo] != ''
GO

PRINT '✅ Update completed!'









