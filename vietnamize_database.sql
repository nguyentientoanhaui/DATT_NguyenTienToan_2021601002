-- Script việt hóa database và chuyển đổi tiền tệ từ USD sang VND
-- Tỷ giá hiện tại: 1 USD = 24,500 VND (có thể điều chỉnh)

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
        WHEN Gender = 'Men' THEN 'Nam'
        WHEN Gender = 'Women' THEN 'Nữ'
        WHEN Gender = 'Unisex' THEN 'Unisex'
        WHEN Gender = 'Ladies' THEN 'Nữ'
        WHEN Gender = 'Gents' THEN 'Nam'
        ELSE Gender
    END
WHERE Gender IS NOT NULL;

-- 3. Việt hóa các trường Condition
UPDATE Products 
SET Condition = 
    CASE 
        WHEN Condition = 'Excellent' THEN 'Xuất sắc'
        WHEN Condition = 'Very Good' THEN 'Rất tốt'
        WHEN Condition = 'Good' THEN 'Tốt'
        WHEN Condition = 'Fair' THEN 'Khá'
        WHEN Condition = 'Poor' THEN 'Kém'
        WHEN Condition = 'New' THEN 'Mới'
        WHEN Condition = 'Pre-owned' THEN 'Đã sử dụng'
        WHEN Condition = 'Used' THEN 'Đã sử dụng'
        WHEN Condition = 'Mint' THEN 'Như mới'
        WHEN Condition = 'Near Mint' THEN 'Gần như mới'
        ELSE Condition
    END
WHERE Condition IS NOT NULL;

-- 4. Việt hóa các trường Certificate
UPDATE Products 
SET Certificate = 
    CASE 
        WHEN Certificate = 'Yes' THEN 'Có'
        WHEN Certificate = 'No' THEN 'Không'
        WHEN Certificate = 'Available' THEN 'Có sẵn'
        WHEN Certificate = 'Not Available' THEN 'Không có'
        WHEN Certificate = 'Included' THEN 'Bao gồm'
        WHEN Certificate = 'Not Included' THEN 'Không bao gồm'
        ELSE Certificate
    END
WHERE Certificate IS NOT NULL;

-- 5. Việt hóa các trường WarrantyInfo
UPDATE Products 
SET WarrantyInfo = 
    CASE 
        WHEN WarrantyInfo = '1 Year' THEN '1 Năm'
        WHEN WarrantyInfo = '2 Years' THEN '2 Năm'
        WHEN WarrantyInfo = '3 Years' THEN '3 Năm'
        WHEN WarrantyInfo = '5 Years' THEN '5 Năm'
        WHEN WarrantyInfo = 'Lifetime' THEN 'Trọn đời'
        WHEN WarrantyInfo = 'No Warranty' THEN 'Không bảo hành'
        WHEN WarrantyInfo = 'Manufacturer Warranty' THEN 'Bảo hành nhà sản xuất'
        WHEN WarrantyInfo = 'International Warranty' THEN 'Bảo hành quốc tế'
        ELSE WarrantyInfo
    END
WHERE WarrantyInfo IS NOT NULL;

-- 6. Việt hóa các trường CaseMaterial
UPDATE Products 
SET CaseMaterial = 
    CASE 
        WHEN CaseMaterial = 'Stainless Steel' THEN 'Thép không gỉ'
        WHEN CaseMaterial = 'Gold' THEN 'Vàng'
        WHEN CaseMaterial = 'Platinum' THEN 'Bạch kim'
        WHEN CaseMaterial = 'Titanium' THEN 'Titan'
        WHEN CaseMaterial = 'Ceramic' THEN 'Gốm'
        WHEN CaseMaterial = 'Bronze' THEN 'Đồng'
        WHEN CaseMaterial = 'Aluminum' THEN 'Nhôm'
        WHEN CaseMaterial = 'Carbon Fiber' THEN 'Sợi carbon'
        ELSE CaseMaterial
    END
WHERE CaseMaterial IS NOT NULL;

-- 7. Việt hóa các trường Crystal
UPDATE Products 
SET Crystal = 
    CASE 
        WHEN Crystal = 'Sapphire Crystal' THEN 'Kính Sapphire'
        WHEN Crystal = 'Mineral Crystal' THEN 'Kính khoáng'
        WHEN Crystal = 'Acrylic Crystal' THEN 'Kính Acrylic'
        WHEN Crystal = 'Hardlex Crystal' THEN 'Kính Hardlex'
        WHEN Crystal = 'Anti-reflective Coating' THEN 'Lớp phủ chống phản quang'
        ELSE Crystal
    END
WHERE Crystal IS NOT NULL;

-- 8. Việt hóa các trường DialColor
UPDATE Products 
SET DialColor = 
    CASE 
        WHEN DialColor = 'Black' THEN 'Đen'
        WHEN DialColor = 'White' THEN 'Trắng'
        WHEN DialColor = 'Blue' THEN 'Xanh dương'
        WHEN DialColor = 'Green' THEN 'Xanh lá'
        WHEN DialColor = 'Red' THEN 'Đỏ'
        WHEN DialColor = 'Yellow' THEN 'Vàng'
        WHEN DialColor = 'Silver' THEN 'Bạc'
        WHEN DialColor = 'Gold' THEN 'Vàng'
        WHEN DialColor = 'Brown' THEN 'Nâu'
        WHEN DialColor = 'Gray' THEN 'Xám'
        WHEN DialColor = 'Orange' THEN 'Cam'
        WHEN DialColor = 'Purple' THEN 'Tím'
        WHEN DialColor = 'Pink' THEN 'Hồng'
        ELSE DialColor
    END
WHERE DialColor IS NOT NULL;

-- 9. Việt hóa các trường HourMarkers
UPDATE Products 
SET HourMarkers = 
    CASE 
        WHEN HourMarkers = 'Applied' THEN 'Gắn nổi'
        WHEN HourMarkers = 'Printed' THEN 'In'
        WHEN HourMarkers = 'Luminous' THEN 'Phát sáng'
        WHEN HourMarkers = 'Index' THEN 'Vạch số'
        WHEN HourMarkers = 'Arabic Numerals' THEN 'Số Ả Rập'
        WHEN HourMarkers = 'Roman Numerals' THEN 'Số La Mã'
        WHEN HourMarkers = 'Baton' THEN 'Que'
        WHEN HourMarkers = 'Diamond' THEN 'Kim cương'
        ELSE HourMarkers
    END
WHERE HourMarkers IS NOT NULL;

-- 10. Việt hóa các trường MovementType
UPDATE Products 
SET MovementType = 
    CASE 
        WHEN MovementType = 'Automatic' THEN 'Tự động'
        WHEN MovementType = 'Manual' THEN 'Lên dây tay'
        WHEN MovementType = 'Quartz' THEN 'Thạch anh'
        WHEN MovementType = 'Mechanical' THEN 'Cơ học'
        WHEN MovementType = 'Solar' THEN 'Năng lượng mặt trời'
        WHEN MovementType = 'Kinetic' THEN 'Động năng'
        ELSE MovementType
    END
WHERE MovementType IS NOT NULL;

-- 11. Việt hóa các trường BraceletMaterial
UPDATE Products 
SET BraceletMaterial = 
    CASE 
        WHEN BraceletMaterial = 'Stainless Steel' THEN 'Thép không gỉ'
        WHEN BraceletMaterial = 'Leather' THEN 'Da'
        WHEN BraceletMaterial = 'Rubber' THEN 'Cao su'
        WHEN BraceletMaterial = 'Nylon' THEN 'Nylon'
        WHEN BraceletMaterial = 'Gold' THEN 'Vàng'
        WHEN BraceletMaterial = 'Titanium' THEN 'Titan'
        WHEN BraceletMaterial = 'Ceramic' THEN 'Gốm'
        WHEN BraceletMaterial = 'Fabric' THEN 'Vải'
        ELSE BraceletMaterial
    END
WHERE BraceletMaterial IS NOT NULL;

-- 12. Việt hóa các trường BraceletType
UPDATE Products 
SET BraceletType = 
    CASE 
        WHEN BraceletType = 'Bracelet' THEN 'Dây đeo'
        WHEN BraceletType = 'Strap' THEN 'Dây da'
        WHEN BraceletType = 'NATO' THEN 'Dây NATO'
        WHEN BraceletType = 'Mesh' THEN 'Dây lưới'
        WHEN BraceletType = 'Link' THEN 'Dây mắt xích'
        WHEN BraceletType = 'Oyster' THEN 'Dây Oyster'
        WHEN BraceletType = 'Jubilee' THEN 'Dây Jubilee'
        WHEN BraceletType = 'President' THEN 'Dây President'
        ELSE BraceletType
    END
WHERE BraceletType IS NOT NULL;

-- 13. Việt hóa các trường ClaspType
UPDATE Products 
SET ClaspType = 
    CASE 
        WHEN ClaspType = 'Folding Clasp' THEN 'Khóa gập'
        WHEN ClaspType = 'Deployant Clasp' THEN 'Khóa triển khai'
        WHEN ClaspType = 'Buckle' THEN 'Khóa thắt lưng'
        WHEN ClaspType = 'Tang Buckle' THEN 'Khóa tang'
        WHEN ClaspType = 'Butterfly Clasp' THEN 'Khóa bướm'
        WHEN ClaspType = 'Push Button' THEN 'Khóa nút nhấn'
        WHEN ClaspType = 'Safety Clasp' THEN 'Khóa an toàn'
        ELSE ClaspType
    END
WHERE ClaspType IS NOT NULL;

-- 14. Việt hóa các trường Complication
UPDATE Products 
SET Complication = 
    CASE 
        WHEN Complication = 'Date' THEN 'Ngày'
        WHEN Complication = 'Day-Date' THEN 'Ngày-Thứ'
        WHEN Complication = 'Chronograph' THEN 'Bấm giờ'
        WHEN Complication = 'Moon Phase' THEN 'Chu kỳ trăng'
        WHEN Complication = 'GMT' THEN 'Giờ thế giới'
        WHEN Complication = 'Perpetual Calendar' THEN 'Lịch vạn niên'
        WHEN Complication = 'Annual Calendar' THEN 'Lịch năm'
        WHEN Complication = 'Power Reserve' THEN 'Dự trữ năng lượng'
        WHEN Complication = 'Alarm' THEN 'Báo thức'
        WHEN Complication = 'Tachymeter' THEN 'Đo tốc độ'
        WHEN Complication = 'Telemeter' THEN 'Đo khoảng cách'
        ELSE Complication
    END
WHERE Complication IS NOT NULL;

-- 15. Cập nhật Description để thay thế các từ tiếng Anh
UPDATE Products 
SET Description = REPLACE(Description, 'Luxury', 'Cao cấp')
WHERE Description LIKE '%Luxury%';

UPDATE Products 
SET Description = REPLACE(Description, 'Authentic', 'Chính hãng')
WHERE Description LIKE '%Authentic%';

UPDATE Products 
SET Description = REPLACE(Description, 'Pre-owned', 'Đã sử dụng')
WHERE Description LIKE '%Pre-owned%';

UPDATE Products 
SET Description = REPLACE(Description, 'Swiss Made', 'Sản xuất tại Thụy Sĩ')
WHERE Description LIKE '%Swiss Made%';

UPDATE Products 
SET Description = REPLACE(Description, 'Automatic Movement', 'Máy tự động')
WHERE Description LIKE '%Automatic Movement%';

UPDATE Products 
SET Description = REPLACE(Description, 'Water Resistant', 'Chống nước')
WHERE Description LIKE '%Water Resistant%';

UPDATE Products 
SET Description = REPLACE(Description, 'Stainless Steel', 'Thép không gỉ')
WHERE Description LIKE '%Stainless Steel%';

UPDATE Products 
SET Description = REPLACE(Description, 'Sapphire Crystal', 'Kính Sapphire')
WHERE Description LIKE '%Sapphire Crystal%';

-- 16. Thêm thông báo hoàn thành
PRINT 'Đã hoàn thành việt hóa database và chuyển đổi tiền tệ!';
PRINT 'Tỷ giá áp dụng: 1 USD = 24,500 VND';
PRINT 'Các trường đã được việt hóa: Gender, Condition, Certificate, WarrantyInfo, CaseMaterial, Crystal, DialColor, HourMarkers, MovementType, BraceletMaterial, BraceletType, ClaspType, Complication, Description';
