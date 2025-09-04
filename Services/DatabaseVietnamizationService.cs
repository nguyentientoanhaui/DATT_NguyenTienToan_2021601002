using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping_Demo.Services
{
    public interface IDatabaseVietnamizationService
    {
        Task<bool> VietnamizeDatabaseAsync();
        Task<bool> ConvertCurrencyToVNDAsync();
        Task<bool> VietnamizeProductFieldsAsync();
    }

    public class DatabaseVietnamizationService : IDatabaseVietnamizationService
    {
        private readonly DataContext _context;
        private const decimal EXCHANGE_RATE = 24500; // 1 USD = 24,500 VND

        public DatabaseVietnamizationService(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> VietnamizeDatabaseAsync()
        {
            try
            {
                await ConvertCurrencyToVNDAsync();
                await VietnamizeProductFieldsAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi việt hóa database: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ConvertCurrencyToVNDAsync()
        {
            try
            {
                var products = await _context.Products.ToListAsync();
                
                foreach (var product in products)
                {
                    if (product.Price > 0)
                    {
                        product.Price = product.Price * EXCHANGE_RATE;
                    }
                    
                    if (product.CapitalPrice > 0)
                    {
                        product.CapitalPrice = product.CapitalPrice * EXCHANGE_RATE;
                    }
                    
                    if (product.CreditCardPrice.HasValue && product.CreditCardPrice > 0)
                    {
                        product.CreditCardPrice = product.CreditCardPrice * EXCHANGE_RATE;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi chuyển đổi tiền tệ: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> VietnamizeProductFieldsAsync()
        {
            try
            {
                var products = await _context.Products.ToListAsync();
                
                foreach (var product in products)
                {
                    // Việt hóa Gender
                    product.Gender = VietnamizeGender(product.Gender);
                    
                    // Việt hóa Condition
                    product.Condition = VietnamizeCondition(product.Condition);
                    
                    // Việt hóa Certificate
                    product.Certificate = VietnamizeCertificate(product.Certificate);
                    
                    // Việt hóa WarrantyInfo
                    product.WarrantyInfo = VietnamizeWarrantyInfo(product.WarrantyInfo);
                    
                    // Việt hóa CaseMaterial
                    product.CaseMaterial = VietnamizeCaseMaterial(product.CaseMaterial);
                    
                    // Việt hóa Crystal
                    product.Crystal = VietnamizeCrystal(product.Crystal);
                    
                    // Việt hóa DialColor
                    product.DialColor = VietnamizeDialColor(product.DialColor);
                    
                    // Việt hóa HourMarkers
                    product.HourMarkers = VietnamizeHourMarkers(product.HourMarkers);
                    
                    // Việt hóa MovementType
                    product.MovementType = VietnamizeMovementType(product.MovementType);
                    
                    // Việt hóa BraceletMaterial
                    product.BraceletMaterial = VietnamizeBraceletMaterial(product.BraceletMaterial);
                    
                    // Việt hóa BraceletType
                    product.BraceletType = VietnamizeBraceletType(product.BraceletType);
                    
                    // Việt hóa ClaspType
                    product.ClaspType = VietnamizeClaspType(product.ClaspType);
                    
                    // Việt hóa Complication
                    product.Complication = VietnamizeComplication(product.Complication);
                    
                    // Việt hóa Description
                    product.Description = VietnamizeDescription(product.Description);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi việt hóa các trường: {ex.Message}");
                return false;
            }
        }

        private string VietnamizeGender(string gender)
        {
            if (string.IsNullOrEmpty(gender)) return gender;
            
            return gender.ToLower() switch
            {
                "men" => "Nam",
                "women" => "Nữ",
                "ladies" => "Nữ",
                "gents" => "Nam",
                "unisex" => "Unisex",
                _ => gender
            };
        }

        private string VietnamizeCondition(string condition)
        {
            if (string.IsNullOrEmpty(condition)) return condition;
            
            return condition.ToLower() switch
            {
                "excellent" => "Xuất sắc",
                "very good" => "Rất tốt",
                "good" => "Tốt",
                "fair" => "Khá",
                "poor" => "Kém",
                "new" => "Mới",
                "pre-owned" => "Đã sử dụng",
                "used" => "Đã sử dụng",
                "mint" => "Như mới",
                "near mint" => "Gần như mới",
                _ => condition
            };
        }

        private string VietnamizeCertificate(string certificate)
        {
            if (string.IsNullOrEmpty(certificate)) return certificate;
            
            return certificate.ToLower() switch
            {
                "yes" => "Có",
                "no" => "Không",
                "available" => "Có sẵn",
                "not available" => "Không có",
                "included" => "Bao gồm",
                "not included" => "Không bao gồm",
                _ => certificate
            };
        }

        private string VietnamizeWarrantyInfo(string warrantyInfo)
        {
            if (string.IsNullOrEmpty(warrantyInfo)) return warrantyInfo;
            
            return warrantyInfo.ToLower() switch
            {
                "1 year" => "1 Năm",
                "2 years" => "2 Năm",
                "3 years" => "3 Năm",
                "5 years" => "5 Năm",
                "lifetime" => "Trọn đời",
                "no warranty" => "Không bảo hành",
                "manufacturer warranty" => "Bảo hành nhà sản xuất",
                "international warranty" => "Bảo hành quốc tế",
                _ => warrantyInfo
            };
        }

        private string VietnamizeCaseMaterial(string caseMaterial)
        {
            if (string.IsNullOrEmpty(caseMaterial)) return caseMaterial;
            
            return caseMaterial.ToLower() switch
            {
                "stainless steel" => "Thép không gỉ",
                "gold" => "Vàng",
                "platinum" => "Bạch kim",
                "titanium" => "Titan",
                "ceramic" => "Gốm",
                "bronze" => "Đồng",
                "aluminum" => "Nhôm",
                "carbon fiber" => "Sợi carbon",
                _ => caseMaterial
            };
        }

        private string VietnamizeCrystal(string crystal)
        {
            if (string.IsNullOrEmpty(crystal)) return crystal;
            
            return crystal.ToLower() switch
            {
                "sapphire crystal" => "Kính Sapphire",
                "mineral crystal" => "Kính khoáng",
                "acrylic crystal" => "Kính Acrylic",
                "hardlex crystal" => "Kính Hardlex",
                "anti-reflective coating" => "Lớp phủ chống phản quang",
                _ => crystal
            };
        }

        private string VietnamizeDialColor(string dialColor)
        {
            if (string.IsNullOrEmpty(dialColor)) return dialColor;
            
            return dialColor.ToLower() switch
            {
                "black" => "Đen",
                "white" => "Trắng",
                "blue" => "Xanh dương",
                "green" => "Xanh lá",
                "red" => "Đỏ",
                "yellow" => "Vàng",
                "silver" => "Bạc",
                "gold" => "Vàng",
                "brown" => "Nâu",
                "gray" => "Xám",
                "orange" => "Cam",
                "purple" => "Tím",
                "pink" => "Hồng",
                _ => dialColor
            };
        }

        private string VietnamizeHourMarkers(string hourMarkers)
        {
            if (string.IsNullOrEmpty(hourMarkers)) return hourMarkers;
            
            return hourMarkers.ToLower() switch
            {
                "applied" => "Gắn nổi",
                "printed" => "In",
                "luminous" => "Phát sáng",
                "index" => "Vạch số",
                "arabic numerals" => "Số Ả Rập",
                "roman numerals" => "Số La Mã",
                "baton" => "Que",
                "diamond" => "Kim cương",
                _ => hourMarkers
            };
        }

        private string VietnamizeMovementType(string movementType)
        {
            if (string.IsNullOrEmpty(movementType)) return movementType;
            
            return movementType.ToLower() switch
            {
                "automatic" => "Tự động",
                "manual" => "Lên dây tay",
                "quartz" => "Thạch anh",
                "mechanical" => "Cơ học",
                "solar" => "Năng lượng mặt trời",
                "kinetic" => "Động năng",
                _ => movementType
            };
        }

        private string VietnamizeBraceletMaterial(string braceletMaterial)
        {
            if (string.IsNullOrEmpty(braceletMaterial)) return braceletMaterial;
            
            return braceletMaterial.ToLower() switch
            {
                "stainless steel" => "Thép không gỉ",
                "leather" => "Da",
                "rubber" => "Cao su",
                "nylon" => "Nylon",
                "gold" => "Vàng",
                "titanium" => "Titan",
                "ceramic" => "Gốm",
                "fabric" => "Vải",
                _ => braceletMaterial
            };
        }

        private string VietnamizeBraceletType(string braceletType)
        {
            if (string.IsNullOrEmpty(braceletType)) return braceletType;
            
            return braceletType.ToLower() switch
            {
                "bracelet" => "Dây đeo",
                "strap" => "Dây da",
                "nato" => "Dây NATO",
                "mesh" => "Dây lưới",
                "link" => "Dây mắt xích",
                "oyster" => "Dây Oyster",
                "jubilee" => "Dây Jubilee",
                "president" => "Dây President",
                _ => braceletType
            };
        }

        private string VietnamizeClaspType(string claspType)
        {
            if (string.IsNullOrEmpty(claspType)) return claspType;
            
            return claspType.ToLower() switch
            {
                "folding clasp" => "Khóa gập",
                "deployant clasp" => "Khóa triển khai",
                "buckle" => "Khóa thắt lưng",
                "tang buckle" => "Khóa tang",
                "butterfly clasp" => "Khóa bướm",
                "push button" => "Khóa nút nhấn",
                "safety clasp" => "Khóa an toàn",
                _ => claspType
            };
        }

        private string VietnamizeComplication(string complication)
        {
            if (string.IsNullOrEmpty(complication)) return complication;
            
            return complication.ToLower() switch
            {
                "date" => "Ngày",
                "day-date" => "Ngày-Thứ",
                "chronograph" => "Bấm giờ",
                "moon phase" => "Chu kỳ trăng",
                "gmt" => "Giờ thế giới",
                "perpetual calendar" => "Lịch vạn niên",
                "annual calendar" => "Lịch năm",
                "power reserve" => "Dự trữ năng lượng",
                "alarm" => "Báo thức",
                "tachymeter" => "Đo tốc độ",
                "telemeter" => "Đo khoảng cách",
                _ => complication
            };
        }

        private string VietnamizeDescription(string description)
        {
            if (string.IsNullOrEmpty(description)) return description;
            
            var vietnamized = description;
            
            // Thay thế các từ tiếng Anh phổ biến
            vietnamized = vietnamized.Replace("Luxury", "Cao cấp", StringComparison.OrdinalIgnoreCase);
            vietnamized = vietnamized.Replace("Authentic", "Chính hãng", StringComparison.OrdinalIgnoreCase);
            vietnamized = vietnamized.Replace("Pre-owned", "Đã sử dụng", StringComparison.OrdinalIgnoreCase);
            vietnamized = vietnamized.Replace("Swiss Made", "Sản xuất tại Thụy Sĩ", StringComparison.OrdinalIgnoreCase);
            vietnamized = vietnamized.Replace("Automatic Movement", "Máy tự động", StringComparison.OrdinalIgnoreCase);
            vietnamized = vietnamized.Replace("Water Resistant", "Chống nước", StringComparison.OrdinalIgnoreCase);
            vietnamized = vietnamized.Replace("Stainless Steel", "Thép không gỉ", StringComparison.OrdinalIgnoreCase);
            vietnamized = vietnamized.Replace("Sapphire Crystal", "Kính Sapphire", StringComparison.OrdinalIgnoreCase);
            
            return vietnamized;
        }
    }
}
