-- Script thực hiện việt hóa database ngay lập tức
-- Tỷ giá: 1 USD = 24,500 VND
-- Encoding: UTF-8

USE [Shopping_Demo]
GO

-- 1. Cập nhật giá tiền từ USD sang VND
UPDATE Products 
SET 
    Price = Price * 24500,
    CapitalPrice = CapitalPrice * 24500,
    CreditCardPrice = CreditCardPrice * 24500
WHERE Price > 0;

-- 2. Việt hóa các trường Gender
UPDATE Products 
SET Gender = 
    CASE 
        WHEN Gender = 'Men' THEN N'Nam'
        WHEN Gender = 'Women' THEN N'Nữ'
        WHEN Gender = 'Unisex' THEN N'Unisex'
        WHEN Gender = 'Ladies' THEN N'Nữ'
        WHEN Gender = 'Gents' THEN N'Nam'
        ELSE Gender
    END
WHERE Gender IS NOT NULL;

-- 3. Việt hóa các trường Condition
UPDATE Products 
SET Condition = 
    CASE 
        WHEN Condition = 'Excellent' THEN N'Xuất sắc'
        WHEN Condition = 'Very Good' THEN N'Rất tốt'
        WHEN Condition = 'Good' THEN N'Tốt'
        WHEN Condition = 'Fair' THEN N'Khá'
        WHEN Condition = 'Poor' THEN N'Kém'
        WHEN Condition = 'New' THEN N'Mới'
        WHEN Condition = 'Pre-owned' THEN N'Đã sử dụng'
        WHEN Condition = 'Used' THEN N'Đã sử dụng'
        WHEN Condition = 'Mint' THEN N'Như mới'
        WHEN Condition = 'Near Mint' THEN N'Gần như mới'
        ELSE Condition
    END
WHERE Condition IS NOT NULL;

-- 4. Việt hóa các trường Certificate
UPDATE Products 
SET Certificate = 
    CASE 
        WHEN Certificate = 'Yes' THEN N'Có'
        WHEN Certificate = 'No' THEN N'Không'
        WHEN Certificate = 'Available' THEN N'Có sẵn'
        WHEN Certificate = 'Not Available' THEN N'Không có'
        WHEN Certificate = 'Included' THEN N'Bao gồm'
        WHEN Certificate = 'Not Included' THEN N'Không bao gồm'
        ELSE Certificate
    END
WHERE Certificate IS NOT NULL;

-- 5. Việt hóa các trường WarrantyInfo
UPDATE Products 
SET WarrantyInfo = 
    CASE 
        WHEN WarrantyInfo = '1 Year' THEN N'1 Năm'
        WHEN WarrantyInfo = '2 Years' THEN N'2 Năm'
        WHEN WarrantyInfo = '3 Years' THEN N'3 Năm'
        WHEN WarrantyInfo = '5 Years' THEN N'5 Năm'
        WHEN WarrantyInfo = 'Lifetime' THEN N'Trọn đời'
        WHEN WarrantyInfo = 'No Warranty' THEN N'Không bảo hành'
        WHEN WarrantyInfo = 'Manufacturer Warranty' THEN N'Bảo hành nhà sản xuất'
        WHEN WarrantyInfo = 'International Warranty' THEN N'Bảo hành quốc tế'
        ELSE WarrantyInfo
    END
WHERE WarrantyInfo IS NOT NULL;

-- 6. Việt hóa các trường CaseMaterial
UPDATE Products 
SET CaseMaterial = 
    CASE 
        WHEN CaseMaterial = 'Stainless Steel' THEN N'Thép không gỉ'
        WHEN CaseMaterial = 'Gold' THEN N'Vàng'
        WHEN CaseMaterial = 'Platinum' THEN N'Bạch kim'
        WHEN CaseMaterial = 'Titanium' THEN N'Titan'
        WHEN CaseMaterial = 'Ceramic' THEN N'Gốm'
        WHEN CaseMaterial = 'Bronze' THEN N'Đồng'
        WHEN CaseMaterial = 'Aluminum' THEN N'Nhôm'
        WHEN CaseMaterial = 'Carbon Fiber' THEN N'Sợi carbon'
        ELSE CaseMaterial
    END
WHERE CaseMaterial IS NOT NULL;

-- 7. Việt hóa các trường Crystal
UPDATE Products 
SET Crystal = 
    CASE 
        WHEN Crystal = 'Sapphire Crystal' THEN N'Kính Sapphire'
        WHEN Crystal = 'Mineral Crystal' THEN N'Kính khoáng'
        WHEN Crystal = 'Acrylic Crystal' THEN N'Kính Acrylic'
        WHEN Crystal = 'Hardlex Crystal' THEN N'Kính Hardlex'
        WHEN Crystal = 'Anti-reflective Coating' THEN N'Lớp phủ chống phản quang'
        ELSE Crystal
    END
WHERE Crystal IS NOT NULL;

-- 8. Việt hóa các trường DialColor
UPDATE Products 
SET DialColor = 
    CASE 
        WHEN DialColor = 'Black' THEN N'Đen'
        WHEN DialColor = 'White' THEN N'Trắng'
        WHEN DialColor = 'Blue' THEN N'Xanh dương'
        WHEN DialColor = 'Green' THEN N'Xanh lá'
        WHEN DialColor = 'Red' THEN N'Đỏ'
        WHEN DialColor = 'Yellow' THEN N'Vàng'
        WHEN DialColor = 'Silver' THEN N'Bạc'
        WHEN DialColor = 'Gold' THEN N'Vàng'
        WHEN DialColor = 'Brown' THEN N'Nâu'
        WHEN DialColor = 'Gray' THEN N'Xám'
        WHEN DialColor = 'Orange' THEN N'Cam'
        WHEN DialColor = 'Purple' THEN N'Tím'
        WHEN DialColor = 'Pink' THEN N'Hồng'
        ELSE DialColor
    END
WHERE DialColor IS NOT NULL;

-- 9. Việt hóa các trường HourMarkers
UPDATE Products 
SET HourMarkers = 
    CASE 
        WHEN HourMarkers = 'Applied' THEN N'Gắn nổi'
        WHEN HourMarkers = 'Printed' THEN N'In'
        WHEN HourMarkers = 'Luminous' THEN N'Phát sáng'
        WHEN HourMarkers = 'Index' THEN N'Vạch số'
        WHEN HourMarkers = 'Arabic Numerals' THEN N'Số Ả Rập'
        WHEN HourMarkers = 'Roman Numerals' THEN N'Số La Mã'
        WHEN HourMarkers = 'Baton' THEN N'Que'
        WHEN HourMarkers = 'Diamond' THEN N'Kim cương'
        ELSE HourMarkers
    END
WHERE HourMarkers IS NOT NULL;

-- 10. Việt hóa các trường MovementType
UPDATE Products 
SET MovementType = 
    CASE 
        WHEN MovementType = 'Automatic' THEN N'Tự động'
        WHEN MovementType = 'Manual' THEN N'Lên dây tay'
        WHEN MovementType = 'Quartz' THEN N'Thạch anh'
        WHEN MovementType = 'Mechanical' THEN N'Cơ học'
        WHEN MovementType = 'Solar' THEN N'Năng lượng mặt trời'
        WHEN MovementType = 'Kinetic' THEN N'Động năng'
        ELSE MovementType
    END
WHERE MovementType IS NOT NULL;

-- 11. Việt hóa các trường BraceletMaterial
UPDATE Products 
SET BraceletMaterial = 
    CASE 
        WHEN BraceletMaterial = 'Stainless Steel' THEN N'Thép không gỉ'
        WHEN BraceletMaterial = 'Leather' THEN N'Da'
        WHEN BraceletMaterial = 'Rubber' THEN N'Cao su'
        WHEN BraceletMaterial = 'Nylon' THEN N'Nylon'
        WHEN BraceletMaterial = 'Gold' THEN N'Vàng'
        WHEN BraceletMaterial = 'Titanium' THEN N'Titan'
        WHEN BraceletMaterial = 'Ceramic' THEN N'Gốm'
        WHEN BraceletMaterial = 'Fabric' THEN N'Vải'
        ELSE BraceletMaterial
    END
WHERE BraceletMaterial IS NOT NULL;

-- 12. Việt hóa các trường BraceletType
UPDATE Products 
SET BraceletType = 
    CASE 
        WHEN BraceletType = 'Bracelet' THEN N'Dây đeo'
        WHEN BraceletType = 'Strap' THEN N'Dây da'
        WHEN BraceletType = 'NATO' THEN N'Dây NATO'
        WHEN BraceletType = 'Mesh' THEN N'Dây lưới'
        WHEN BraceletType = 'Link' THEN N'Dây mắt xích'
        WHEN BraceletType = 'Oyster' THEN N'Dây Oyster'
        WHEN BraceletType = 'Jubilee' THEN N'Dây Jubilee'
        WHEN BraceletType = 'President' THEN N'Dây President'
        ELSE BraceletType
    END
WHERE BraceletType IS NOT NULL;

-- 13. Việt hóa các trường ClaspType
UPDATE Products 
SET ClaspType = 
    CASE 
        WHEN ClaspType = 'Folding Clasp' THEN N'Khóa gập'
        WHEN ClaspType = 'Deployant Clasp' THEN N'Khóa triển khai'
        WHEN ClaspType = 'Buckle' THEN N'Khóa thắt lưng'
        WHEN ClaspType = 'Tang Buckle' THEN N'Khóa tang'
        WHEN ClaspType = 'Butterfly Clasp' THEN N'Khóa bướm'
        WHEN ClaspType = 'Push Button' THEN N'Khóa nút nhấn'
        WHEN ClaspType = 'Safety Clasp' THEN N'Khóa an toàn'
        ELSE ClaspType
    END
WHERE ClaspType IS NOT NULL;

-- 14. Việt hóa các trường Complication
UPDATE Products 
SET Complication = 
    CASE 
        WHEN Complication = 'Date' THEN N'Ngày'
        WHEN Complication = 'Day-Date' THEN N'Ngày-Thứ'
        WHEN Complication = 'Chronograph' THEN N'Bấm giờ'
        WHEN Complication = 'Moon Phase' THEN N'Chu kỳ trăng'
        WHEN Complication = 'GMT' THEN N'Giờ thế giới'
        WHEN Complication = 'Perpetual Calendar' THEN N'Lịch vạn niên'
        WHEN Complication = 'Annual Calendar' THEN N'Lịch năm'
        WHEN Complication = 'Power Reserve' THEN N'Dự trữ năng lượng'
        WHEN Complication = 'Alarm' THEN N'Báo thức'
        WHEN Complication = 'Tachymeter' THEN N'Đo tốc độ'
        WHEN Complication = 'Telemeter' THEN N'Đo khoảng cách'
        ELSE Complication
    END
WHERE Complication IS NOT NULL;

-- 15. Cập nhật Description để thay thế các từ tiếng Anh
UPDATE Products 
SET Description = REPLACE(Description, 'Luxury', N'Cao cấp')
WHERE Description LIKE '%Luxury%';

UPDATE Products 
SET Description = REPLACE(Description, 'Authentic', N'Chính hãng')
WHERE Description LIKE '%Authentic%';

UPDATE Products 
SET Description = REPLACE(Description, 'Pre-owned', N'Đã sử dụng')
WHERE Description LIKE '%Pre-owned%';

UPDATE Products 
SET Description = REPLACE(Description, 'Swiss Made', N'Sản xuất tại Thụy Sĩ')
WHERE Description LIKE '%Swiss Made%';

UPDATE Products 
SET Description = REPLACE(Description, 'Automatic Movement', N'Máy tự động')
WHERE Description LIKE '%Automatic Movement%';

UPDATE Products 
SET Description = REPLACE(Description, 'Water Resistant', N'Chống nước')
WHERE Description LIKE '%Water Resistant%';

UPDATE Products 
SET Description = REPLACE(Description, 'Stainless Steel', N'Thép không gỉ')
WHERE Description LIKE '%Stainless Steel%';

UPDATE Products 
SET Description = REPLACE(Description, 'Sapphire Crystal', N'Kính Sapphire')
WHERE Description LIKE '%Sapphire Crystal%';

-- 16. Hiển thị kết quả
SELECT 
    N'Việt hóa hoàn thành!' as Message,
    COUNT(*) as TotalProducts,
    SUM(CASE WHEN Price > 1000000 THEN 1 ELSE 0 END) as ProductsWithVNDPrice
FROM Products;

-- 17. Hiển thị một số sản phẩm mẫu để kiểm tra
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

