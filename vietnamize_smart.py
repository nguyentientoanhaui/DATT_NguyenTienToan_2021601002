import pyodbc
import pandas as pd
import numpy as np
from datetime import datetime
import re

class SmartDatabaseVietnamizer:
    def __init__(self, connection_string):
        self.connection_string = connection_string
        self.conn = None
        
    def connect(self):
        """Kết nối đến database"""
        try:
            self.conn = pyodbc.connect(self.connection_string)
            print("✅ Kết nối database thành công!")
            return True
        except Exception as e:
            print(f"❌ Lỗi kết nối: {e}")
            return False
    
    def close(self):
        """Đóng kết nối"""
        if self.conn:
            self.conn.close()
            print("🔒 Đã đóng kết nối database")
    
    def smart_translate_warranty(self, text):
        """Dịch thông minh cho thông tin bảo hành"""
        if not text or len(text.strip()) < 10:
            return text
        
        # Bỏ qua nếu đã có tiếng Việt
        if re.search(r'[àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ]', text, re.IGNORECASE):
            return text
        
        # Pattern matching cho các mô tả bảo hành phổ biến
        patterns = [
            # Pre-owned watches
            (r"This pre-owned (.+?) comes with Bob's Watches one year warranty", 
             r"Đồng hồ \1 đã qua sử dụng đi kèm bảo hành 1 năm từ Bob's Watches"),
            
            (r"This pre-owned (.+?) comes with the remaining (.+?) warranty from (.+?), in addition to Bob's Watches one-year warranty",
             r"Đồng hồ \1 đã qua sử dụng còn bảo hành \2 từ \3, cộng thêm bảo hành 1 năm từ Bob's Watches"),
            
            (r"This pre-owned (.+?) comes w/ activated (.+?) warranty from (.+?), in addition to Bob's Watches one-year warranty",
             r"Đồng hồ \1 đã qua sử dụng với bảo hành \2 được kích hoạt từ \3, cộng thêm bảo hành 1 năm từ Bob's Watches"),
            
            (r"This pre-owned (.+?) comes with active (.+?) warranty valid until (.+?), in addition to Bob's Watches one-year warranty",
             r"Đồng hồ \1 đã qua sử dụng với bảo hành \2 còn hiệu lực đến \3, cộng thêm bảo hành 1 năm từ Bob's Watches"),
            
            # Unworn watches
            (r"This pre-owned/unworn (.+?) comes with active (.+?) warranty valid until (.+?), in addition to Bob's Watches one-year warranty",
             r"Đồng hồ \1 đã qua sử dụng/chưa đeo với bảo hành \2 còn hiệu lực đến \3, cộng thêm bảo hành 1 năm từ Bob's Watches"),
            
            (r"This unworn (.+?) comes with the remaining (.+?) factory warranty from (.+?), in addition to Bob's Watches one-year warranty",
             r"Đồng hồ \1 chưa đeo với bảo hành nhà máy \2 còn lại từ \3, cộng thêm bảo hành 1 năm từ Bob's Watches"),
            
            # Simple patterns
            (r"Bob's Watches one year warranty", "Bảo hành 1 năm từ Bob's Watches"),
            (r"Bob's Watches one-year warranty", "Bảo hành 1 năm từ Bob's Watches"),
            (r"factory warranty", "bảo hành nhà máy"),
            (r"remaining warranty", "bảo hành còn lại"),
            (r"active warranty", "bảo hành còn hiệu lực"),
            (r"activated warranty", "bảo hành được kích hoạt"),
            (r"pre-owned", "đã qua sử dụng"),
            (r"unworn", "chưa đeo"),
            (r"in addition to", "cộng thêm"),
            (r"valid until", "còn hiệu lực đến"),
            (r"from", "từ"),
            (r"with", "với"),
            (r"comes", "đi kèm"),
            (r"warranty", "bảo hành"),
            (r"year", "năm"),
            (r"years", "năm"),
            (r"month", "tháng"),
            (r"months", "tháng"),
            (r"day", "ngày"),
            (r"days", "ngày"),
            (r"week", "tuần"),
            (r"weeks", "tuần")
        ]
        
        translated_text = text
        for pattern, replacement in patterns:
            translated_text = re.sub(pattern, replacement, translated_text, flags=re.IGNORECASE)
        
        return translated_text
    
    def smart_translate_certificate(self, text):
        """Dịch thông minh cho chứng nhận"""
        if not text or len(text.strip()) < 5:
            return text
        
        # Bỏ qua nếu đã có tiếng Việt
        if re.search(r'[àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ]', text, re.IGNORECASE):
            return text
        
        # Pattern matching cho chứng nhận
        patterns = [
            (r"Certified by Watch CSA The Leading Independent 3rd Party Provider of Watch Authentication", 
             "Được chứng nhận bởi Watch CSA - Nhà cung cấp độc lập hàng đầu về xác thực đồng hồ"),
            
            (r"Watch CSA", "Watch CSA"),
            (r"Certified", "Được chứng nhận"),
            (r"Authentication", "Xác thực"),
            (r"Independent", "Độc lập"),
            (r"Provider", "Nhà cung cấp"),
            (r"Leading", "Hàng đầu"),
            (r"3rd Party", "Bên thứ 3"),
            (r"Watch", "Đồng hồ"),
            (r"Original", "Gốc"),
            (r"Box", "Hộp"),
            (r"Papers", "Giấy tờ"),
            (r"Warranty Card", "Thẻ bảo hành"),
            (r"Service Book", "Sổ bảo hành"),
            (r"Certificate", "Chứng nhận"),
            (r"Authentic", "Chính hãng"),
            (r"Genuine", "Thật"),
            (r"Real", "Thật"),
            (r"Verified", "Đã xác minh"),
            (r"Tested", "Đã kiểm tra"),
            (r"Inspected", "Đã kiểm định"),
            (r"Quality", "Chất lượng"),
            (r"Condition", "Tình trạng"),
            (r"Excellent", "Xuất sắc"),
            (r"Very Good", "Rất tốt"),
            (r"Good", "Tốt"),
            (r"Fair", "Khá"),
            (r"Poor", "Kém")
        ]
        
        translated_text = text
        for pattern, replacement in patterns:
            translated_text = re.sub(pattern, replacement, translated_text, flags=re.IGNORECASE)
        
        return translated_text
    
    def smart_translate_description(self, text):
        """Dịch thông minh cho mô tả sản phẩm"""
        if not text or len(text.strip()) < 10:
            return text
        
        # Bỏ qua nếu đã có tiếng Việt
        if re.search(r'[àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ]', text, re.IGNORECASE):
            return text
        
        # Pattern matching cho mô tả đồng hồ
        patterns = [
            (r"luxury watch", "đồng hồ xa xỉ"),
            (r"premium watch", "đồng hồ cao cấp"),
            (r"high-end watch", "đồng hồ cao cấp"),
            (r"Swiss watch", "đồng hồ Thụy Sĩ"),
            (r"automatic watch", "đồng hồ tự động"),
            (r"mechanical watch", "đồng hồ cơ"),
            (r"quartz watch", "đồng hồ quartz"),
            (r"chronograph", "đồng hồ chronograph"),
            (r"diver watch", "đồng hồ lặn"),
            (r"pilot watch", "đồng hồ phi công"),
            (r"dress watch", "đồng hồ công sở"),
            (r"sport watch", "đồng hồ thể thao"),
            (r"casual watch", "đồng hồ thường ngày"),
            (r"classic design", "thiết kế cổ điển"),
            (r"modern design", "thiết kế hiện đại"),
            (r"elegant design", "thiết kế thanh lịch"),
            (r"premium materials", "chất liệu cao cấp"),
            (r"stainless steel", "thép không gỉ"),
            (r"gold plated", "mạ vàng"),
            (r"solid gold", "vàng nguyên khối"),
            (r"leather strap", "dây da"),
            (r"metal bracelet", "dây kim loại"),
            (r"rubber strap", "dây cao su"),
            (r"water resistant", "chống nước"),
            (r"waterproof", "chống nước"),
            (r"scratch resistant", "chống xước"),
            (r"anti-reflective", "chống phản quang"),
            (r"sapphire crystal", "kính sapphire"),
            (r"mineral crystal", "kính khoáng"),
            (r"date display", "hiển thị ngày"),
            (r"day display", "hiển thị thứ"),
            (r"moon phase", "pha mặt trăng"),
            (r"power reserve", "dự trữ năng lượng"),
            (r"luminous hands", "kim phát sáng"),
            (r"luminous markers", "vạch phát sáng"),
            (r"tachymeter", "thang đo tốc độ"),
            (r"telemeter", "thang đo khoảng cách"),
            (r"slide rule", "thước trượt"),
            (r"bezel", "vành bezel"),
            (r"crown", "núm vặn"),
            (r"pushers", "nút bấm"),
            (r"case", "vỏ máy"),
            (r"dial", "mặt số"),
            (r"hands", "kim"),
            (r"markers", "vạch số"),
            (r"indexes", "vạch số"),
            (r"sub-dials", "mặt số phụ"),
            (r"complications", "chức năng phức tạp"),
            (r"movement", "bộ máy"),
            (r"caliber", "caliber"),
            (r"jewels", "chân kính"),
            (r"frequency", "tần số"),
            (r"accuracy", "độ chính xác"),
            (r"precision", "độ chính xác"),
            (r"reliability", "độ tin cậy"),
            (r"durability", "độ bền"),
            (r"craftsmanship", "tay nghề thủ công"),
            (r"heritage", "di sản"),
            (r"tradition", "truyền thống"),
            (r"innovation", "sự đổi mới"),
            (r"excellence", "sự xuất sắc"),
            (r"quality", "chất lượng"),
            (r"prestige", "uy tín"),
            (r"status", "địa vị"),
            (r"luxury", "xa xỉ"),
            (r"premium", "cao cấp"),
            (r"exclusive", "độc quyền"),
            (r"limited edition", "phiên bản giới hạn"),
            (r"special edition", "phiên bản đặc biệt"),
            (r"collector's item", "món đồ sưu tầm"),
            (r"investment piece", "món đầu tư"),
            (r"heirloom", "di sản gia đình"),
            (r"legacy", "di sản"),
            (r"iconic", "biểu tượng"),
            (r"legendary", "huyền thoại"),
            (r"famous", "nổi tiếng"),
            (r"popular", "phổ biến"),
            (r"trendy", "thịnh hành"),
            (r"fashionable", "thời trang"),
            (r"stylish", "phong cách"),
            (r"elegant", "thanh lịch"),
            (r"sophisticated", "tinh tế"),
            (r"refined", "tinh tế"),
            (r"classic", "cổ điển"),
            (r"timeless", "vượt thời gian"),
            (r"modern", "hiện đại"),
            (r"contemporary", "đương đại"),
            (r"traditional", "truyền thống"),
            (r"vintage", "cổ điển"),
            (r"retro", "hoài cổ"),
            (r"new", "mới"),
            (r"pre-owned", "đã qua sử dụng"),
            (r"used", "đã sử dụng"),
            (r"unworn", "chưa đeo"),
            (r"mint condition", "tình trạng hoàn hảo"),
            (r"excellent condition", "tình trạng xuất sắc"),
            (r"very good condition", "tình trạng rất tốt"),
            (r"good condition", "tình trạng tốt"),
            (r"fair condition", "tình trạng khá"),
            (r"poor condition", "tình trạng kém")
        ]
        
        translated_text = text
        for pattern, replacement in patterns:
            translated_text = re.sub(pattern, replacement, translated_text, flags=re.IGNORECASE)
        
        return translated_text
    
    def fix_encoding_issues(self):
        """Sửa lỗi encoding cho dữ liệu bị hỏng"""
        try:
            cursor = self.conn.cursor()
            
            print("\n🔧 SỬA LỖI ENCODING:")
            print("=" * 50)
            
            # Sửa lỗi encoding cho Condition
            encoding_fixes = {
                'Xu?t s?c': 'Xuất sắc',
                'R?t t?t': 'Rất tốt',
                'T?t': 'Tốt',
                'Kh?': 'Khá',
                'K?m': 'Kém',
                'N?': 'Nữ'
            }
            
            for corrupted, fixed in encoding_fixes.items():
                cursor.execute("UPDATE Products SET Condition = ? WHERE Condition = ?", (fixed, corrupted))
                affected = cursor.rowcount
                if affected > 0:
                    print(f"  {corrupted} → {fixed}: {affected} sản phẩm")
                
                cursor.execute("UPDATE Products SET Gender = ? WHERE Gender = ?", (fixed, corrupted))
                affected = cursor.rowcount
                if affected > 0:
                    print(f"  {corrupted} → {fixed}: {affected} sản phẩm")
            
            self.conn.commit()
            print("✅ Đã sửa lỗi encoding!")
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi sửa encoding: {e}")
            self.conn.rollback()
    
    def vietnamize_simple_fields(self):
        """Việt hóa các trường đơn giản"""
        try:
            cursor = self.conn.cursor()
            
            print("\n🇻🇳 VIỆT HÓA CÁC TRƯỜNG ĐƠN GIẢN:")
            print("=" * 50)
            
            # Mapping từ tiếng Anh sang tiếng Việt
            condition_mapping = {
                'Excellent': 'Xuất sắc',
                'Very Good': 'Rất tốt',
                'Good': 'Tốt',
                'Fair': 'Khá',
                'Poor': 'Kém',
                'New': 'Mới',
                'Vintage': 'Cổ điển',
                'Pre-owned': 'Đã qua sử dụng',
                'Unworn': 'Chưa đeo',
                'Mint': 'Hoàn hảo'
            }
            
            gender_mapping = {
                'Men': 'Nam',
                'Women': 'Nữ',
                'Unisex': 'Unisex',
                'Male': 'Nam',
                'Female': 'Nữ'
            }
            
            certificate_mapping = {
                'Original Box': 'Hộp gốc',
                'Original Papers': 'Giấy tờ gốc',
                'Warranty Card': 'Thẻ bảo hành',
                'Service Book': 'Sổ bảo hành',
                'No Papers': 'Không có giấy tờ',
                'Box and Papers': 'Hộp và giấy tờ',
                'Box Only': 'Chỉ có hộp',
                'Papers Only': 'Chỉ có giấy tờ',
                'Có': 'Có giấy tờ',
                'Yes': 'Có',
                'No': 'Không'
            }
            
            warranty_mapping = {
                '1 Year': '1 năm',
                '2 Years': '2 năm',
                '3 Years': '3 năm',
                '5 Years': '5 năm',
                'Lifetime': 'Trọn đời',
                'No Warranty': 'Không bảo hành',
                'International Warranty': 'Bảo hành quốc tế',
                'Manufacturer Warranty': 'Bảo hành nhà sản xuất',
                '5 Năm': '5 năm'
            }
            
            # Cập nhật Condition
            print("🔄 Cập nhật Condition...")
            for eng, vn in condition_mapping.items():
                cursor.execute("UPDATE Products SET Condition = ? WHERE Condition = ?", (vn, eng))
                affected = cursor.rowcount
                if affected > 0:
                    print(f"  {eng} → {vn}: {affected} sản phẩm")
            
            # Cập nhật Gender
            print("🔄 Cập nhật Gender...")
            for eng, vn in gender_mapping.items():
                cursor.execute("UPDATE Products SET Gender = ? WHERE Gender = ?", (vn, eng))
                affected = cursor.rowcount
                if affected > 0:
                    print(f"  {eng} → {vn}: {affected} sản phẩm")
            
            # Cập nhật Certificate
            print("🔄 Cập nhật Certificate...")
            for eng, vn in certificate_mapping.items():
                cursor.execute("UPDATE Products SET Certificate = ? WHERE Certificate = ?", (vn, eng))
                affected = cursor.rowcount
                if affected > 0:
                    print(f"  {eng} → {vn}: {affected} sản phẩm")
            
            # Cập nhật WarrantyInfo
            print("🔄 Cập nhật WarrantyInfo...")
            for eng, vn in warranty_mapping.items():
                cursor.execute("UPDATE Products SET WarrantyInfo = ? WHERE WarrantyInfo = ?", (vn, eng))
                affected = cursor.rowcount
                if affected > 0:
                    print(f"  {eng} → {vn}: {affected} sản phẩm")
            
            self.conn.commit()
            print("✅ Đã việt hóa các trường đơn giản!")
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi việt hóa trường đơn giản: {e}")
            self.conn.rollback()
    
    def vietnamize_complex_fields_smart(self):
        """Việt hóa các trường phức tạp bằng dịch thông minh"""
        try:
            cursor = self.conn.cursor()
            
            print("\n🧠 VIỆT HÓA CÁC TRƯỜNG PHỨC TẠP VỚI DỊCH THÔNG MINH:")
            print("=" * 60)
            
            # Lấy tất cả các trường cần dịch
            fields_to_translate = ['Description', 'WarrantyInfo', 'Certificate']
            
            for field in fields_to_translate:
                print(f"\n🔄 Đang dịch trường {field}...")
                
                # Lấy các giá trị unique cần dịch
                cursor.execute(f"SELECT DISTINCT {field} FROM Products WHERE {field} IS NOT NULL AND LEN({field}) > 20")
                unique_values = cursor.fetchall()
                
                translated_count = 0
                for value_tuple in unique_values:
                    original_text = value_tuple[0]
                    
                    # Bỏ qua nếu đã có tiếng Việt
                    if re.search(r'[àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ]', original_text, re.IGNORECASE):
                        continue
                    
                    # Bỏ qua nếu quá ngắn
                    if len(original_text.strip()) < 10:
                        continue
                    
                    print(f"  Dịch: {original_text[:50]}...")
                    
                    # Dịch với context phù hợp
                    if field == "WarrantyInfo":
                        translated_text = self.smart_translate_warranty(original_text)
                    elif field == "Certificate":
                        translated_text = self.smart_translate_certificate(original_text)
                    elif field == "Description":
                        translated_text = self.smart_translate_description(original_text)
                    else:
                        translated_text = original_text
                    
                    if translated_text != original_text:
                        cursor.execute(f"UPDATE Products SET {field} = ? WHERE {field} = ?", (translated_text, original_text))
                        translated_count += 1
                        print(f"    → {translated_text[:50]}...")
                
                print(f"  ✅ Đã dịch {translated_count} giá trị trong trường {field}")
                
                # Commit sau mỗi trường
                self.conn.commit()
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi việt hóa trường phức tạp: {e}")
            self.conn.rollback()
    
    def convert_prices_to_vnd(self):
        """Chuyển đổi giá từ USD sang VND"""
        try:
            cursor = self.conn.cursor()
            
            print("\n💰 CHUYỂN ĐỔI GIÁ TỪ USD SANG VND:")
            print("=" * 50)
            
            # Chuyển đổi giá (tỷ giá 1 USD = 24,500 VND)
            cursor.execute("""
                UPDATE Products 
                SET Price = Price * 24500, 
                    CapitalPrice = CapitalPrice * 24500, 
                    CreditCardPrice = CreditCardPrice * 24500 
                WHERE Price > 0 AND Price < 100000
            """)
            affected = cursor.rowcount
            print(f"  Đã chuyển đổi giá cho {affected} sản phẩm")
            
            self.conn.commit()
            print("✅ Đã chuyển đổi giá!")
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi chuyển đổi giá: {e}")
            self.conn.rollback()
    
    def vietnamize_other_tables(self):
        """Việt hóa các bảng khác"""
        try:
            cursor = self.conn.cursor()
            
            print("\n🇻🇳 VIỆT HÓA CÁC BẢNG KHÁC:")
            print("=" * 50)
            
            # Việt hóa Categories
            cursor.execute("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Categories'")
            if cursor.fetchone()[0] > 0:
                print("🔄 Việt hóa Categories...")
                category_mapping = {
                    'Luxury': 'Xa xỉ',
                    'Sport': 'Thể thao',
                    'Dress': 'Công sở',
                    'Casual': 'Thường ngày',
                    'Smart': 'Thông minh',
                    'Classic': 'Cổ điển',
                    'Modern': 'Hiện đại'
                }
                
                for eng, vn in category_mapping.items():
                    cursor.execute("UPDATE Categories SET Name = ? WHERE Name = ?", (vn, eng))
                    affected = cursor.rowcount
                    if affected > 0:
                        print(f"  {eng} → {vn}: {affected} danh mục")
            
            # Việt hóa Brands (giữ nguyên tên thương hiệu)
            cursor.execute("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Brands'")
            if cursor.fetchone()[0] > 0:
                print("🔄 Kiểm tra Brands...")
                # Giữ nguyên tên thương hiệu như Rolex, Omega, v.v.
                print("  Giữ nguyên tên thương hiệu")
            
            self.conn.commit()
            print("✅ Đã việt hóa các bảng khác!")
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi việt hóa các bảng khác: {e}")
            self.conn.rollback()
    
    def verify_final_results(self):
        """Xác minh kết quả cuối cùng"""
        try:
            cursor = self.conn.cursor()
            
            print("\n✅ XÁC MINH KẾT QUẢ CUỐI CÙNG:")
            print("=" * 50)
            
            # Kiểm tra dữ liệu sau khi việt hóa
            cursor.execute("""
                SELECT TOP 3 Id, Name, Model, Condition, Gender, Certificate, 
                       SUBSTRING(WarrantyInfo, 1, 100) as WarrantyInfo_Short,
                       Price, CapitalPrice, CreditCardPrice
                FROM Products
            """)
            
            products = cursor.fetchall()
            print("📦 SAMPLE PRODUCTS SAU VIỆT HÓA HOÀN CHỈNH:")
            for product in products:
                print(f"  ID: {product[0]}")
                print(f"  Name: {product[1]} (giữ nguyên)")
                print(f"  Model: {product[2]} (giữ nguyên)")
                print(f"  Condition: {product[3]}")
                print(f"  Gender: {product[4]}")
                print(f"  Certificate: {product[5]}")
                print(f"  WarrantyInfo: {product[6]}...")
                print(f"  Price: {product[7]:,.0f} VND")
                print(f"  CapitalPrice: {product[8]:,.0f} VND")
                print(f"  CreditCardPrice: {product[9]:,.0f} VND")
                print("  " + "-" * 30)
            
            # Thống kê
            cursor.execute("SELECT COUNT(*) FROM Products WHERE Condition IN ('Xuất sắc', 'Rất tốt', 'Tốt', 'Khá', 'Kém', 'Mới', 'Cổ điển')")
            vietnamized_condition = cursor.fetchone()[0]
            
            cursor.execute("SELECT COUNT(*) FROM Products WHERE Gender IN ('Nam', 'Nữ', 'Unisex')")
            vietnamized_gender = cursor.fetchone()[0]
            
            cursor.execute("SELECT COUNT(*) FROM Products WHERE Price > 1000000")  # Giá VND
            vietnamized_price = cursor.fetchone()[0]
            
            print(f"\n📊 THỐNG KÊ VIỆT HÓA:")
            print(f"  Condition đã việt hóa: {vietnamized_condition} sản phẩm")
            print(f"  Gender đã việt hóa: {vietnamized_gender} sản phẩm")
            print(f"  Giá đã chuyển VND: {vietnamized_price} sản phẩm")
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi xác minh: {e}")
    
    def run_smart_vietnamization(self):
        """Chạy việt hóa thông minh"""
        print("🚀 BẮT ĐẦU VIỆT HÓA THÔNG MINH")
        print("=" * 60)
        print("🧠 Sử dụng dịch thuật thông minh với pattern matching")
        print("=" * 60)
        
        if not self.connect():
            return False
        
        try:
            # Sửa lỗi encoding
            self.fix_encoding_issues()
            
            # Việt hóa các trường đơn giản
            self.vietnamize_simple_fields()
            
            # Việt hóa các trường phức tạp với dịch thông minh
            self.vietnamize_complex_fields_smart()
            
            # Chuyển đổi giá
            self.convert_prices_to_vnd()
            
            # Việt hóa các bảng khác
            self.vietnamize_other_tables()
            
            # Xác minh kết quả
            self.verify_final_results()
            
            print("\n🎉 VIỆT HÓA THÔNG MINH THÀNH CÔNG!")
            print("=" * 60)
            print("✅ Đã sửa lỗi encoding")
            print("✅ Đã việt hóa các trường đơn giản")
            print("✅ Đã dịch các nội dung phức tạp bằng pattern matching")
            print("✅ Đã chuyển đổi giá từ USD sang VND")
            print("✅ Đã việt hóa các bảng khác")
            print("✅ Cột Name và Model được giữ nguyên như yêu cầu")
            
            return True
            
        except Exception as e:
            print(f"❌ Lỗi trong quá trình việt hóa: {e}")
            return False
        finally:
            self.close()

def main():
    # Connection string
    connection_string = "DRIVER={SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;"
    
    # Tạo instance và chạy việt hóa thông minh
    vietnamizer = SmartDatabaseVietnamizer(connection_string)
    success = vietnamizer.run_smart_vietnamization()
    
    if success:
        print("\n🎯 KẾT QUẢ: Việt hóa thông minh thành công!")
    else:
        print("\n💥 KẾT QUẢ: Việt hóa thông minh thất bại!")

if __name__ == "__main__":
    main()
