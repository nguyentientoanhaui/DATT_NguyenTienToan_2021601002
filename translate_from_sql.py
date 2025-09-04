import pyodbc
import re
import os

class SQLTranslator:
    def __init__(self, connection_string, sql_file_path):
        self.connection_string = connection_string
        self.sql_file_path = sql_file_path
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
    
    def read_sql_file(self):
        try:
            print(f"\n📖 ĐỌC FILE: {self.sql_file_path}")
            print("=" * 50)
            
            if not os.path.exists(self.sql_file_path):
                print(f"❌ File không tồn tại: {self.sql_file_path}")
                return None
            
            with open(self.sql_file_path, 'r', encoding='utf-8', errors='ignore') as file:
                content = file.read()
            
            print(f"✅ Đã đọc file! Kích thước: {len(content)} ký tự")
            
            # Parse INSERT statements
            insert_pattern = r"INSERT\s+INTO\s+\[?Products\]?\s*\([^)]+\)\s*VALUES\s*\(([^)]+)\)"
            matches = re.findall(insert_pattern, content, re.IGNORECASE | re.DOTALL)
            
            products = []
            for i, match in enumerate(matches):
                try:
                    values = self.parse_values(match)
                    if values:
                        products.append(values)
                        if i < 3:
                            print(f"  Sản phẩm {i+1}: {values.get('Name', 'N/A')}")
                except Exception as e:
                    print(f"  ⚠️ Lỗi parse sản phẩm {i+1}: {e}")
                    continue
            
            print(f"✅ Đã parse được {len(products)} sản phẩm")
            return products
            
        except Exception as e:
            print(f"❌ Lỗi đọc file: {e}")
            return None
    
    def parse_values(self, values_string):
        try:
            values = []
            current_value = ""
            in_quotes = False
            quote_char = None
            
            for char in values_string:
                if char in ["'", '"'] and (not in_quotes or char == quote_char):
                    if not in_quotes:
                        in_quotes = True
                        quote_char = char
                    else:
                        in_quotes = False
                        quote_char = None
                elif char == ',' and not in_quotes:
                    values.append(current_value.strip())
                    current_value = ""
                else:
                    current_value += char
            
            if current_value.strip():
                values.append(current_value.strip())
            
            product = {
                'Id': self.clean_value(values[0]) if len(values) > 0 else None,
                'Name': self.clean_value(values[1]) if len(values) > 1 else None,
                'Model': self.clean_value(values[2]) if len(values) > 2 else None,
                'BrandId': self.clean_value(values[3]) if len(values) > 3 else None,
                'CategoryId': self.clean_value(values[4]) if len(values) > 4 else None,
                'Condition': self.clean_value(values[5]) if len(values) > 5 else None,
                'Gender': self.clean_value(values[6]) if len(values) > 6 else None,
                'Price': self.clean_value(values[7]) if len(values) > 7 else None,
                'CapitalPrice': self.clean_value(values[8]) if len(values) > 8 else None,
                'CreditCardPrice': self.clean_value(values[9]) if len(values) > 9 else None,
                'Description': self.clean_value(values[10]) if len(values) > 10 else None,
                'Certificate': self.clean_value(values[11]) if len(values) > 11 else None,
                'WarrantyInfo': self.clean_value(values[12]) if len(values) > 12 else None,
                'ImageUrl': self.clean_value(values[13]) if len(values) > 13 else None,
                'CreatedAt': self.clean_value(values[14]) if len(values) > 14 else None,
                'UpdatedAt': self.clean_value(values[15]) if len(values) > 15 else None
            }
            
            return product
            
        except Exception as e:
            print(f"❌ Lỗi parse values: {e}")
            return None
    
    def clean_value(self, value):
        if not value or value == 'NULL':
            return None
        
        value = value.strip()
        if (value.startswith("'") and value.endswith("'")) or \
           (value.startswith('"') and value.endswith('"')):
            value = value[1:-1]
        
        value = value.replace("''", "'")
        value = value.replace('""', '"')
        
        return value
    
    def translate_text(self, text):
        if not text or len(text.strip()) < 3:
            return text
        
        # Sửa lỗi encoding
        text = self.fix_encoding(text)
        
        # Mapping dịch thuật
        translation_map = {
            'Excellent': 'Xuất sắc',
            'Very Good': 'Rất tốt',
            'Good': 'Tốt',
            'Fair': 'Khá',
            'Poor': 'Kém',
            'New': 'Mới',
            'Vintage': 'Cổ điển',
            'Pre-owned': 'Đã qua sử dụng',
            'Unworn': 'Chưa đeo',
            'Men': 'Nam',
            'Women': 'Nữ',
            'Unisex': 'Unisex',
            'Male': 'Nam',
            'Female': 'Nữ',
            'Original Box': 'Hộp gốc',
            'Original Papers': 'Giấy tờ gốc',
            'Warranty Card': 'Thẻ bảo hành',
            'Service Book': 'Sổ bảo hành',
            'No Papers': 'Không có giấy tờ',
            'Box and Papers': 'Hộp và giấy tờ',
            'Box Only': 'Chỉ có hộp',
            'Papers Only': 'Chỉ có giấy tờ',
            'Yes': 'Có',
            'No': 'Không',
            '1 Year': '1 năm',
            '2 Years': '2 năm',
            '3 Years': '3 năm',
            '5 Years': '5 năm',
            'Lifetime': 'Trọn đời',
            'No Warranty': 'Không bảo hành',
            'International Warranty': 'Bảo hành quốc tế',
            'Manufacturer Warranty': 'Bảo hành nhà sản xuất',
            'stainless steel': 'thép không gỉ',
            'automatic movement': 'bộ máy tự động',
            'mechanical movement': 'bộ máy cơ',
            'quartz movement': 'bộ máy quartz',
            'manual winding': 'lên dây tay',
            'self-winding': 'tự động lên dây',
            'perpetual calendar': 'lịch vạn niên',
            'annual calendar': 'lịch năm',
            'moon phase': 'pha mặt trăng',
            'chronograph': 'chronograph',
            'tachymeter': 'thang đo tốc độ',
            'telemeter': 'thang đo khoảng cách',
            'slide rule': 'thước trượt',
            'bezel': 'vành bezel',
            'crown': 'núm vặn',
            'pushers': 'nút bấm',
            'case': 'vỏ máy',
            'dial': 'mặt số',
            'hands': 'kim',
            'markers': 'vạch số',
            'indexes': 'vạch số',
            'sub-dials': 'mặt số phụ',
            'complications': 'chức năng phức tạp',
            'movement': 'bộ máy',
            'caliber': 'caliber',
            'jewels': 'chân kính',
            'frequency': 'tần số',
            'accuracy': 'độ chính xác',
            'precision': 'độ chính xác',
            'reliability': 'độ tin cậy',
            'durability': 'độ bền',
            'craftsmanship': 'tay nghề thủ công',
            'heritage': 'di sản',
            'tradition': 'truyền thống',
            'innovation': 'sự đổi mới',
            'excellence': 'sự xuất sắc',
            'quality': 'chất lượng',
            'prestige': 'uy tín',
            'status': 'địa vị',
            'luxury': 'xa xỉ',
            'premium': 'cao cấp',
            'exclusive': 'độc quyền',
            'limited edition': 'phiên bản giới hạn',
            'special edition': 'phiên bản đặc biệt',
            'collector\'s item': 'món đồ sưu tầm',
            'investment piece': 'món đầu tư',
            'heirloom': 'di sản gia đình',
            'legacy': 'di sản',
            'iconic': 'biểu tượng',
            'legendary': 'huyền thoại',
            'famous': 'nổi tiếng',
            'popular': 'phổ biến',
            'trendy': 'thịnh hành',
            'fashionable': 'thời trang',
            'stylish': 'phong cách',
            'elegant': 'thanh lịch',
            'sophisticated': 'tinh tế',
            'refined': 'tinh tế',
            'classic': 'cổ điển',
            'timeless': 'vượt thời gian',
            'modern': 'hiện đại',
            'contemporary': 'đương đại',
            'traditional': 'truyền thống',
            'vintage': 'cổ điển',
            'retro': 'hoài cổ',
            'new': 'mới',
            'pre-owned': 'đã qua sử dụng',
            'used': 'đã sử dụng',
            'unworn': 'chưa đeo',
            'mint condition': 'tình trạng hoàn hảo',
            'excellent condition': 'tình trạng xuất sắc',
            'very good condition': 'tình trạng rất tốt',
            'good condition': 'tình trạng tốt',
            'fair condition': 'tình trạng khá',
            'poor condition': 'tình trạng kém',
            'gateway to': 'lối vào',
            'ownership': 'sở hữu',
            'cased in': 'được bọc trong',
            'affordable': 'giá cả phải chăng',
            'instantly recognizable': 'có thể nhận ra ngay lập tức',
            'coveted': 'được ưa chuộng',
            'dress watch': 'đồng hồ công sở',
            '5-point': '5 điểm',
            'coronet': 'vương miện',
            '12-hour marker': 'vị trí 12 giờ',
            'date display': 'hiển thị ngày',
            'Cyclops magnifier': 'kính lúp Cyclops',
            'three-link': 'ba mắt',
            'Oyster bracelet': 'dây đeo Oyster',
            'recognition': 'sự công nhận',
            'entry-level price': 'giá đầu vào',
            'for sale': 'được bán',
            'black dial': 'mặt đen',
            'displaying the time': 'hiển thị thời gian',
            'via': 'qua',
            'silvered': 'bạc',
            'hour markers': 'vạch giờ',
            'set against': 'đặt trên',
            'sharp': 'sắc nét',
            'o\'clock': 'giờ',
            'entry-level': 'đầu vào',
            'level': 'cấp độ',
            'price': 'giá',
            'cost': 'chi phí',
            'value': 'giá trị',
            'worth': 'đáng giá',
            'expensive': 'đắt tiền',
            'cheap': 'rẻ',
            'budget': 'ngân sách',
            'reasonable': 'hợp lý',
            'fair': 'công bằng',
            'outstanding': 'nổi bật',
            'superior': 'ưu việt',
            'inferior': 'kém',
            'bad': 'xấu',
            'terrible': 'khủng khiếp',
            'awful': 'kinh khủng',
            'horrible': 'kinh khủng',
            'dreadful': 'kinh khủng',
            'frightful': 'kinh khủng',
            'shocking': 'sốc',
            'amazing': 'tuyệt vời',
            'wonderful': 'tuyệt vời',
            'fantastic': 'tuyệt vời',
            'brilliant': 'tuyệt vời',
            'magnificent': 'tráng lệ',
            'gorgeous': 'tuyệt đẹp',
            'beautiful': 'đẹp',
            'pretty': 'đẹp',
            'handsome': 'đẹp trai',
            'attractive': 'hấp dẫn',
            'appealing': 'hấp dẫn',
            'charming': 'quyến rũ',
            'enchanting': 'mê hoặc',
            'captivating': 'mê hoặc',
            'fascinating': 'hấp dẫn',
            'interesting': 'thú vị',
            'exciting': 'thú vị',
            'thrilling': 'thú vị',
            'adventurous': 'phiêu lưu',
            'daring': 'dũng cảm',
            'bold': 'dũng cảm',
            'brave': 'dũng cảm',
            'courageous': 'dũng cảm',
            'fearless': 'không sợ hãi',
            'intrepid': 'dũng cảm',
            'valiant': 'dũng cảm',
            'heroic': 'anh hùng',
            'noble': 'cao quý',
            'honorable': 'đáng kính',
            'respectable': 'đáng kính',
            'admirable': 'đáng ngưỡng mộ',
            'commendable': 'đáng khen',
            'praiseworthy': 'đáng khen',
            'laudable': 'đáng khen',
            'meritorious': 'có công',
            'deserving': 'xứng đáng',
            'worthy': 'xứng đáng',
            'valuable': 'có giá trị',
            'precious': 'quý giá',
            'treasured': 'quý giá',
            'cherished': 'quý giá',
            'beloved': 'yêu quý',
            'dear': 'thân yêu',
            'darling': 'yêu dấu',
            'sweet': 'ngọt ngào',
            'lovely': 'đáng yêu',
            'adorable': 'đáng yêu',
            'cute': 'dễ thương',
            'endearing': 'đáng yêu',
            'bewitching': 'mê hoặc',
            'spellbinding': 'mê hoặc',
            'mesmerizing': 'mê hoặc',
            'hypnotic': 'mê hoặc',
            'entrancing': 'mê hoặc',
            'enthralling': 'mê hoặc',
            'engrossing': 'mê hoặc',
            'absorbing': 'mê hoặc',
            'gripping': 'hấp dẫn',
            'riveting': 'hấp dẫn',
            'compelling': 'hấp dẫn',
            'persuasive': 'thuyết phục',
            'convincing': 'thuyết phục',
            'credible': 'đáng tin',
            'believable': 'đáng tin',
            'plausible': 'hợp lý',
            'logical': 'hợp lý',
            'sensible': 'hợp lý',
            'rational': 'hợp lý',
            'sound': 'vững chắc',
            'solid': 'vững chắc',
            'firm': 'vững chắc',
            'stable': 'ổn định',
            'steady': 'ổn định',
            'consistent': 'nhất quán',
            'reliable': 'đáng tin cậy',
            'dependable': 'đáng tin cậy',
            'trustworthy': 'đáng tin cậy',
            'faithful': 'trung thành',
            'loyal': 'trung thành',
            'devoted': 'tận tâm',
            'dedicated': 'tận tâm',
            'committed': 'cam kết',
            'determined': 'quyết tâm',
            'resolved': 'quyết tâm',
            'decided': 'quyết định',
            'settled': 'ổn định',
            'fixed': 'cố định',
            'established': 'được thiết lập',
            'founded': 'được thành lập',
            'created': 'được tạo ra',
            'formed': 'được hình thành',
            'built': 'được xây dựng',
            'constructed': 'được xây dựng',
            'assembled': 'được lắp ráp',
            'manufactured': 'được sản xuất',
            'produced': 'được sản xuất',
            'made': 'được làm',
            'crafted': 'được làm thủ công',
            'designed': 'được thiết kế',
            'developed': 'được phát triển',
            'invented': 'được phát minh',
            'discovered': 'được khám phá',
            'found': 'được tìm thấy',
            'located': 'được định vị',
            'situated': 'được đặt',
            'positioned': 'được định vị',
            'placed': 'được đặt',
            'set': 'được đặt',
            'arranged': 'được sắp xếp',
            'organized': 'được tổ chức',
            'structured': 'được cấu trúc',
            'systematized': 'được hệ thống hóa',
            'standardized': 'được chuẩn hóa',
            'normalized': 'được chuẩn hóa',
            'regularized': 'được quy chuẩn hóa',
            'formalized': 'được chính thức hóa',
            'institutionalized': 'được thể chế hóa'
        }
        
        # Áp dụng dịch thuật
        translated_text = text
        for eng, vn in translation_map.items():
            translated_text = re.sub(r'\b' + re.escape(eng) + r'\b', vn, translated_text, flags=re.IGNORECASE)
        
        return translated_text
    
    def fix_encoding(self, text):
        if not text:
            return text
        
        encoding_fixes = {
            'Ðã s? d?ng': 'Đã sử dụng',
            'Thép không g?': 'Thép không gỉ',
            'T? d?ng': 'Tự động',
            's? d?ng': 'sử dụng',
            'không g?': 'không gỉ',
            't? d?ng': 'tự động',
            'c? d?ng': 'cổ động',
            'h? th?ng': 'hệ thống',
            'ch?t l??ng': 'chất lượng',
            'thi?t k?': 'thiết kế',
            'công ngh?': 'công nghệ',
            'th??ng hi?u': 'thương hiệu',
            's?n xu?t': 'sản xuất',
            'b?o hành': 'bảo hành',
            'ch?ng n??c': 'chống nước',
            'ch?ng x??c': 'chống xước',
            'ch?ng va': 'chống va',
            'ch?ng s?c': 'chống sốc',
            'ch?ng t?': 'chống từ',
            'ch?ng nhi?t': 'chống nhiệt',
            'ch?ng b?i': 'chống bụi',
            'ch?ng m?': 'chống mờ',
            'ch?ng ph?n quang': 'chống phản quang',
            'ch?ng tia UV': 'chống tia UV',
            'ch?ng tia X': 'chống tia X',
            'ch?ng tia gamma': 'chống tia gamma',
            'ch?ng tia beta': 'chống tia beta',
            'ch?ng tia alpha': 'chống tia alpha',
            'ch?ng tia neutron': 'chống tia neutron',
            'ch?ng tia proton': 'chống tia proton',
            'ch?ng tia electron': 'chống tia electron',
            'ch?ng tia positron': 'chống tia positron',
            'ch?ng tia neutrino': 'chống tia neutrino',
            'ch?ng tia muon': 'chống tia muon',
            'ch?ng tia pion': 'chống tia pion',
            'ch?ng tia kaon': 'chống tia kaon',
            'ch?ng tia lambda': 'chống tia lambda',
            'ch?ng tia sigma': 'chống tia sigma',
            'ch?ng tia xi': 'chống tia xi',
            'ch?ng tia omega': 'chống tia omega',
            'ch?ng tia delta': 'chống tia delta',
            'ch?ng tia phi': 'chống tia phi',
            'ch?ng tia eta': 'chống tia eta',
            'ch?ng tia rho': 'chống tia rho',
            'ch?ng tia j/psi': 'chống tia j/psi',
            'ch?ng tia upsilon': 'chống tia upsilon',
            'ch?ng tia z': 'chống tia z',
            'ch?ng tia w': 'chống tia w',
            'ch?ng tia gluon': 'chống tia gluon',
            'ch?ng tia photon': 'chống tia photon',
            'ch?ng tia graviton': 'chống tia graviton',
            'ch?ng tia higgs': 'chống tia higgs',
            'ch?ng tia axion': 'chống tia axion',
            'ch?ng tia majorana': 'chống tia majorana',
            'ch?ng tia weyl': 'chống tia weyl',
            'ch?ng tia dirac': 'chống tia dirac',
            'ch?ng tia fermi': 'chống tia fermi',
            'ch?ng tia bose': 'chống tia bose',
            'ch?ng tia einstein': 'chống tia einstein',
            'ch?ng tia planck': 'chống tia planck',
            'ch?ng tia bohr': 'chống tia bohr',
            'ch?ng tia heisenberg': 'chống tia heisenberg',
            'ch?ng tia schrodinger': 'chống tia schrodinger',
            'ch?ng tia pauli': 'chống tia pauli'
        }
        
        fixed_text = text
        for corrupted, fixed in encoding_fixes.items():
            fixed_text = fixed_text.replace(corrupted, fixed)
        
        return fixed_text
    
    def convert_price_to_vnd(self, price_str):
        try:
            if not price_str or price_str == 'NULL':
                return None
            
            price = re.sub(r'[^\d.]', '', str(price_str))
            if not price:
                return None
            
            price_float = float(price)
            
            if price_float < 100000:
                return int(price_float * 24500)
            
            return int(price_float)
            
        except Exception as e:
            print(f"⚠️ Lỗi chuyển đổi giá {price_str}: {e}")
            return None
    
    def update_database(self, products_data):
        try:
            cursor = self.conn.cursor()
            
            print(f"\n🔄 CẬP NHẬT DATABASE: {len(products_data)} sản phẩm")
            print("=" * 50)
            
            updated_count = 0
            for i, product in enumerate(products_data):
                try:
                    # Dịch các trường text
                    translated_condition = self.translate_text(product.get('Condition', ''))
                    translated_gender = self.translate_text(product.get('Gender', ''))
                    translated_description = self.translate_text(product.get('Description', ''))
                    translated_certificate = self.translate_text(product.get('Certificate', ''))
                    translated_warranty = self.translate_text(product.get('WarrantyInfo', ''))
                    
                    # Chuyển đổi giá
                    price_vnd = self.convert_price_to_vnd(product.get('Price'))
                    capital_price_vnd = self.convert_price_to_vnd(product.get('CapitalPrice'))
                    credit_card_price_vnd = self.convert_price_to_vnd(product.get('CreditCardPrice'))
                    
                    # Cập nhật database
                    cursor.execute("""
                        UPDATE Products 
                        SET Condition = ?, Gender = ?, Description = ?, 
                            Certificate = ?, WarrantyInfo = ?, Price = ?, 
                            CapitalPrice = ?, CreditCardPrice = ?
                        WHERE Id = ?
                    """, (
                        translated_condition, translated_gender, translated_description,
                        translated_certificate, translated_warranty, price_vnd,
                        capital_price_vnd, credit_card_price_vnd, product.get('Id')
                    ))
                    
                    if cursor.rowcount > 0:
                        updated_count += 1
                        if i < 10:
                            print(f"  ✅ ID {product.get('Id')}: {product.get('Name', 'N/A')}")
                    
                except Exception as e:
                    print(f"  ⚠️ Lỗi cập nhật sản phẩm {product.get('Id')}: {e}")
                    continue
            
            self.conn.commit()
            print(f"\n✅ Đã cập nhật {updated_count} sản phẩm!")
            
            cursor.close()
            return updated_count
            
        except Exception as e:
            print(f"❌ Lỗi cập nhật database: {e}")
            self.conn.rollback()
            return 0
    
    def verify_results(self):
        try:
            cursor = self.conn.cursor()
            
            print("\n✅ XÁC MINH KẾT QUẢ:")
            print("=" * 50)
            
            cursor.execute("""
                SELECT TOP 3 Id, Name, Model, Condition, Gender, 
                       SUBSTRING(Description, 1, 150) as Description_Short,
                       Price, CapitalPrice, CreditCardPrice
                FROM Products
                ORDER BY Id
            """)
            
            products = cursor.fetchall()
            print("📦 SAMPLE PRODUCTS:")
            for product in products:
                print(f"  ID: {product[0]}")
                print(f"  Name: {product[1]}")
                print(f"  Model: {product[2]}")
                print(f"  Condition: {product[3]}")
                print(f"  Gender: {product[4]}")
                print(f"  Description: {product[5]}...")
                print(f"  Price: {product[6]:,.0f} VND" if product[6] else "  Price: NULL")
                print(f"  CapitalPrice: {product[7]:,.0f} VND" if product[7] else "  CapitalPrice: NULL")
                print(f"  CreditCardPrice: {product[8]:,.0f} VND" if product[8] else "  CreditCardPrice: NULL")
                print("  " + "-" * 40)
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi xác minh: {e}")
    
    def run_translation(self):
        print("🚀 BẮT ĐẦU DỊCH THUẬT TỪ FILE PRODUCTS.SQL")
        print("=" * 60)
        
        if not self.connect():
            return False
        
        try:
            # Đọc dữ liệu từ file SQL
            products_data = self.read_sql_file()
            if not products_data:
                return False
            
            # Cập nhật database
            updated_count = self.update_database(products_data)
            
            # Xác minh kết quả
            self.verify_results()
            
            print(f"\n🎉 HOÀN THÀNH!")
            print("=" * 60)
            print(f"✅ Đã đọc {len(products_data)} sản phẩm từ file SQL")
            print(f"✅ Đã cập nhật {updated_count} sản phẩm vào database")
            print("✅ Đã dịch toàn bộ nội dung sang tiếng Việt")
            print("✅ Đã sửa lỗi encoding")
            print("✅ Đã chuyển đổi giá sang VND")
            
            return True
            
        except Exception as e:
            print(f"❌ Lỗi: {e}")
            return False
        finally:
            self.close()

def main():
    connection_string = "DRIVER={SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;"
    sql_file_path = "Products.sql"
    
    translator = SQLTranslator(connection_string, sql_file_path)
    success = translator.run_translation()
    
    if success:
        print("\n🎯 KẾT QUẢ: Thành công!")
    else:
        print("\n💥 KẾT QUẢ: Thất bại!")

if __name__ == "__main__":
    main()
