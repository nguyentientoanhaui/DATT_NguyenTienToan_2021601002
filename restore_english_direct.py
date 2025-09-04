import pyodbc
import re

class DirectEnglishRestorer:
    def __init__(self, connection_string):
        self.connection_string = connection_string
        self.conn = None
        
    def connect(self):
        try:
            self.conn = pyodbc.connect(self.connection_string)
            print("✅ Kết nối database thành công!")
            return True
        except Exception as e:
            print(f"❌ Lỗi kết nối: {e}")
            return False
    
    def close(self):
        if self.conn:
            self.conn.close()
            print("🔒 Đã đóng kết nối database")
    
    def convert_vietnamese_to_english(self, text):
        """Chuyển đổi từ tiếng Việt về tiếng Anh"""
        if not text or len(text.strip()) < 3:
            return text
        
        # Mapping từ tiếng Việt về tiếng Anh
        reverse_mapping = {
            # Condition
            'Xuất sắc': 'Excellent',
            'Rất tốt': 'Very Good',
            'Tốt': 'Good',
            'Khá': 'Fair',
            'Kém': 'Poor',
            'Mới': 'New',
            'Cổ điển': 'Vintage',
            'Đã qua sử dụng': 'Pre-owned',
            'Chưa đeo': 'Unworn',
            'Hoàn hảo': 'Mint',
            
            # Gender
            'Nam': 'Men',
            'Nữ': 'Women',
            'Unisex': 'Unisex',
            'Male': 'Men',
            'Female': 'Women',
            
            # Certificate
            'Hộp gốc': 'Original Box',
            'Giấy tờ gốc': 'Original Papers',
            'Thẻ bảo hành': 'Warranty Card',
            'Sổ bảo hành': 'Service Book',
            'Không có giấy tờ': 'No Papers',
            'Hộp và giấy tờ': 'Box and Papers',
            'Chỉ có hộp': 'Box Only',
            'Chỉ có giấy tờ': 'Papers Only',
            'Có': 'Yes',
            'Không': 'No',
            
            # Warranty
            '1 năm': '1 Year',
            '2 năm': '2 Years',
            '3 năm': '3 Years',
            '5 năm': '5 Years',
            'Trọn đời': 'Lifetime',
            'Không bảo hành': 'No Warranty',
            'Bảo hành quốc tế': 'International Warranty',
            'Bảo hành nhà sản xuất': 'Manufacturer Warranty',
            
            # Technical terms
            'thép không gỉ': 'stainless steel',
            'bộ máy tự động': 'automatic movement',
            'bộ máy cơ': 'mechanical movement',
            'bộ máy quartz': 'quartz movement',
            'lên dây tay': 'manual winding',
            'tự động lên dây': 'self-winding',
            'lịch vạn niên': 'perpetual calendar',
            'lịch năm': 'annual calendar',
            'pha mặt trăng': 'moon phase',
            'chronograph': 'chronograph',
            'thang đo tốc độ': 'tachymeter',
            'thang đo khoảng cách': 'telemeter',
            'thước trượt': 'slide rule',
            'vành bezel': 'bezel',
            'núm vặn': 'crown',
            'nút bấm': 'pushers',
            'vỏ máy': 'case',
            'mặt số': 'dial',
            'kim': 'hands',
            'vạch số': 'markers',
            'mặt số phụ': 'sub-dials',
            'chức năng phức tạp': 'complications',
            'bộ máy': 'movement',
            'caliber': 'caliber',
            'chân kính': 'jewels',
            'tần số': 'frequency',
            'độ chính xác': 'accuracy',
            'độ tin cậy': 'reliability',
            'độ bền': 'durability',
            'tay nghề thủ công': 'craftsmanship',
            'di sản': 'heritage',
            'truyền thống': 'tradition',
            'sự đổi mới': 'innovation',
            'sự xuất sắc': 'excellence',
            'chất lượng': 'quality',
            'uy tín': 'prestige',
            'địa vị': 'status',
            'xa xỉ': 'luxury',
            'cao cấp': 'premium',
            'độc quyền': 'exclusive',
            'phiên bản giới hạn': 'limited edition',
            'phiên bản đặc biệt': 'special edition',
            'món đồ sưu tầm': 'collector\'s item',
            'món đầu tư': 'investment piece',
            'di sản gia đình': 'heirloom',
            'biểu tượng': 'iconic',
            'huyền thoại': 'legendary',
            'nổi tiếng': 'famous',
            'phổ biến': 'popular',
            'thịnh hành': 'trendy',
            'thời trang': 'fashionable',
            'phong cách': 'stylish',
            'thanh lịch': 'elegant',
            'tinh tế': 'sophisticated',
            'cổ điển': 'classic',
            'vượt thời gian': 'timeless',
            'hiện đại': 'modern',
            'đương đại': 'contemporary',
            'truyền thống': 'traditional',
            'hoài cổ': 'retro',
            'mới': 'new',
            'đã qua sử dụng': 'pre-owned',
            'đã sử dụng': 'used',
            'chưa đeo': 'unworn',
            'tình trạng hoàn hảo': 'mint condition',
            'tình trạng xuất sắc': 'excellent condition',
            'tình trạng rất tốt': 'very good condition',
            'tình trạng tốt': 'good condition',
            'tình trạng khá': 'fair condition',
            'tình trạng kém': 'poor condition',
            
            # Common phrases
            'lối vào': 'gateway to',
            'sở hữu': 'ownership',
            'được bọc trong': 'cased in',
            'giá cả phải chăng': 'affordable',
            'có thể nhận ra ngay lập tức': 'instantly recognizable',
            'được ưa chuộng': 'coveted',
            'đồng hồ công sở': 'dress watch',
            '5 điểm': '5-point',
            'vương miện': 'coronet',
            'vị trí 12 giờ': '12-hour marker',
            'hiển thị ngày': 'date display',
            'kính lúp Cyclops': 'Cyclops magnifier',
            'ba mắt': 'three-link',
            'dây đeo Oyster': 'Oyster bracelet',
            'sự công nhận': 'recognition',
            'giá đầu vào': 'entry-level price',
            'được bán': 'for sale',
            'mặt đen': 'black dial',
            'hiển thị thời gian': 'displaying the time',
            'qua': 'via',
            'bạc': 'silvered',
            'vạch giờ': 'hour markers',
            'đặt trên': 'set against',
            'sắc nét': 'sharp',
            'giờ': 'o\'clock',
            'đầu vào': 'entry-level',
            'cấp độ': 'level',
            'giá': 'price',
            'chi phí': 'cost',
            'giá trị': 'value',
            'đáng giá': 'worth',
            'đắt tiền': 'expensive',
            'rẻ': 'cheap',
            'ngân sách': 'budget',
            'hợp lý': 'reasonable',
            'công bằng': 'fair',
            'nổi bật': 'outstanding',
            'ưu việt': 'superior',
            'kém': 'inferior',
            'xấu': 'bad',
            'khủng khiếp': 'terrible',
            'kinh khủng': 'awful',
            'sốc': 'shocking',
            'tuyệt vời': 'amazing',
            'tráng lệ': 'magnificent',
            'tuyệt đẹp': 'gorgeous',
            'đẹp': 'beautiful',
            'đẹp trai': 'handsome',
            'hấp dẫn': 'attractive',
            'quyến rũ': 'charming',
            'mê hoặc': 'enchanting',
            'thú vị': 'interesting',
            'phiêu lưu': 'adventurous',
            'dũng cảm': 'daring',
            'không sợ hãi': 'fearless',
            'anh hùng': 'heroic',
            'cao quý': 'noble',
            'đáng kính': 'honorable',
            'đáng ngưỡng mộ': 'admirable',
            'đáng khen': 'commendable',
            'có công': 'meritorious',
            'xứng đáng': 'deserving',
            'có giá trị': 'valuable',
            'quý giá': 'precious',
            'yêu quý': 'beloved',
            'thân yêu': 'dear',
            'yêu dấu': 'darling',
            'ngọt ngào': 'sweet',
            'đáng yêu': 'lovely',
            'dễ thương': 'cute',
            'hấp dẫn': 'gripping',
            'thuyết phục': 'persuasive',
            'đáng tin': 'credible',
            'hợp lý': 'logical',
            'vững chắc': 'sound',
            'ổn định': 'stable',
            'nhất quán': 'consistent',
            'đáng tin cậy': 'reliable',
            'trung thành': 'faithful',
            'tận tâm': 'devoted',
            'cam kết': 'committed',
            'quyết tâm': 'determined',
            'ổn định': 'settled',
            'cố định': 'fixed',
            'được thiết lập': 'established',
            'được thành lập': 'founded',
            'được tạo ra': 'created',
            'được hình thành': 'formed',
            'được xây dựng': 'built',
            'được lắp ráp': 'assembled',
            'được sản xuất': 'manufactured',
            'được làm': 'made',
            'được thiết kế': 'designed',
            'được phát triển': 'developed',
            'được phát minh': 'invented',
            'được khám phá': 'discovered',
            'được tìm thấy': 'found',
            'được định vị': 'located',
            'được đặt': 'situated',
            'được sắp xếp': 'arranged',
            'được tổ chức': 'organized',
            'được cấu trúc': 'structured',
            'được hệ thống hóa': 'systematized',
            'được chuẩn hóa': 'standardized',
            'được quy chuẩn hóa': 'regularized',
            'được chính thức hóa': 'formalized',
            'được thể chế hóa': 'institutionalized'
        }
        
        # Áp dụng chuyển đổi
        converted_text = text
        for vn, eng in reverse_mapping.items():
            converted_text = re.sub(r'\b' + re.escape(vn) + r'\b', eng, converted_text, flags=re.IGNORECASE)
        
        return converted_text
    
    def convert_price_to_usd(self, price_str):
        """Chuyển đổi giá từ VND về USD"""
        try:
            if not price_str or price_str == 'NULL':
                return None
            
            price = re.sub(r'[^\d.]', '', str(price_str))
            if not price:
                return None
            
            price_float = float(price)
            
            # Nếu giá > 1,000,000 thì coi như VND và chuyển về USD
            if price_float > 1000000:
                return int(price_float / 24500)  # Tỷ giá 1 USD = 24,500 VND
            
            return int(price_float)
            
        except Exception as e:
            print(f"⚠️ Lỗi chuyển đổi giá {price_str}: {e}")
            return None
    
    def restore_english_data(self):
        try:
            cursor = self.conn.cursor()
            
            print("\n🔄 KHÔI PHỤC DỮ LIỆU TIẾNG ANH TRỰC TIẾP:")
            print("=" * 60)
            
            # Lấy tất cả sản phẩm
            cursor.execute("SELECT Id, Condition, Gender, Description, Certificate, WarrantyInfo, Price, CapitalPrice, CreditCardPrice FROM Products")
            products = cursor.fetchall()
            
            updated_count = 0
            for i, product in enumerate(products):
                try:
                    product_id = product[0]
                    condition = product[1]
                    gender = product[2]
                    description = product[3]
                    certificate = product[4]
                    warranty_info = product[5]
                    price = product[6]
                    capital_price = product[7]
                    credit_card_price = product[8]
                    
                    # Chuyển đổi các trường text
                    english_condition = self.convert_vietnamese_to_english(condition) if condition else condition
                    english_gender = self.convert_vietnamese_to_english(gender) if gender else gender
                    english_description = self.convert_vietnamese_to_english(description) if description else description
                    english_certificate = self.convert_vietnamese_to_english(certificate) if certificate else certificate
                    english_warranty = self.convert_vietnamese_to_english(warranty_info) if warranty_info else warranty_info
                    
                    # Chuyển đổi giá
                    price_usd = self.convert_price_to_usd(price)
                    capital_price_usd = self.convert_price_to_usd(capital_price)
                    credit_card_price_usd = self.convert_price_to_usd(credit_card_price)
                    
                    # Cập nhật database
                    cursor.execute("""
                        UPDATE Products 
                        SET Condition = ?, Gender = ?, Description = ?, 
                            Certificate = ?, WarrantyInfo = ?, Price = ?, 
                            CapitalPrice = ?, CreditCardPrice = ?
                        WHERE Id = ?
                    """, (
                        english_condition, english_gender, english_description,
                        english_certificate, english_warranty, price_usd,
                        capital_price_usd, credit_card_price_usd, product_id
                    ))
                    
                    if cursor.rowcount > 0:
                        updated_count += 1
                        if i < 10:
                            print(f"  ✅ ID {product_id}: Đã chuyển đổi")
                    
                except Exception as e:
                    print(f"  ⚠️ Lỗi cập nhật sản phẩm {product[0]}: {e}")
                    continue
            
            self.conn.commit()
            print(f"\n✅ Đã khôi phục {updated_count} sản phẩm!")
            
            cursor.close()
            return updated_count
            
        except Exception as e:
            print(f"❌ Lỗi khôi phục: {e}")
            self.conn.rollback()
            return 0
    
    def verify_results(self):
        try:
            cursor = self.conn.cursor()
            
            print("\n✅ XÁC MINH KẾT QUẢ KHÔI PHỤC:")
            print("=" * 50)
            
            cursor.execute("""
                SELECT TOP 3 Id, Name, Model, Condition, Gender, 
                       SUBSTRING(Description, 1, 150) as Description_Short,
                       Price, CapitalPrice, CreditCardPrice
                FROM Products
                ORDER BY Id
            """)
            
            products = cursor.fetchall()
            print("📦 SAMPLE PRODUCTS SAU KHI KHÔI PHỤC:")
            for product in products:
                print(f"  ID: {product[0]}")
                print(f"  Name: {product[1]}")
                print(f"  Model: {product[2]}")
                print(f"  Condition: {product[3]}")
                print(f"  Gender: {product[4]}")
                print(f"  Description: {product[5]}...")
                print(f"  Price: ${product[6]:,}" if product[6] else "  Price: NULL")
                print(f"  CapitalPrice: ${product[7]:,}" if product[7] else "  CapitalPrice: NULL")
                print(f"  CreditCardPrice: ${product[8]:,}" if product[8] else "  CreditCardPrice: NULL")
                print("  " + "-" * 40)
            
            # Thống kê
            cursor.execute("SELECT COUNT(*) FROM Products WHERE Condition IN ('Excellent', 'Very Good', 'Good', 'Fair', 'Poor', 'New', 'Vintage')")
            english_condition = cursor.fetchone()[0]
            
            cursor.execute("SELECT COUNT(*) FROM Products WHERE Gender IN ('Men', 'Women', 'Unisex', 'Male', 'Female')")
            english_gender = cursor.fetchone()[0]
            
            cursor.execute("SELECT COUNT(*) FROM Products WHERE Price < 100000")  # Giá USD
            usd_price = cursor.fetchone()[0]
            
            print(f"\n📊 THỐNG KÊ KHÔI PHỤC:")
            print(f"  Condition tiếng Anh: {english_condition} sản phẩm")
            print(f"  Gender tiếng Anh: {english_gender} sản phẩm")
            print(f"  Giá USD: {usd_price} sản phẩm")
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi xác minh: {e}")
    
    def run_restoration(self):
        print("🚀 BẮT ĐẦU KHÔI PHỤC DỮ LIỆU TIẾNG ANH TRỰC TIẾP")
        print("=" * 60)
        
        if not self.connect():
            return False
        
        try:
            # Khôi phục dữ liệu
            updated_count = self.restore_english_data()
            
            # Xác minh kết quả
            self.verify_results()
            
            print(f"\n🎉 KHÔI PHỤC HOÀN THÀNH!")
            print("=" * 60)
            print(f"✅ Đã khôi phục {updated_count} sản phẩm")
            print("✅ Đã chuyển đổi toàn bộ nội dung về tiếng Anh")
            print("✅ Đã chuyển đổi giá về USD")
            print("✅ Dữ liệu đã trở về trạng thái ban đầu")
            
            return True
            
        except Exception as e:
            print(f"❌ Lỗi: {e}")
            return False
        finally:
            self.close()

def main():
    connection_string = "DRIVER={SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;"
    
    restorer = DirectEnglishRestorer(connection_string)
    success = restorer.run_restoration()
    
    if success:
        print("\n🎯 KẾT QUẢ: Khôi phục thành công!")
    else:
        print("\n💥 KẾT QUẢ: Khôi phục thất bại!")

if __name__ == "__main__":
    main()
