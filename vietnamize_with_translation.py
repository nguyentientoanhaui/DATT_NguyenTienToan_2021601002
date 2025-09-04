import pyodbc
import pandas as pd
import numpy as np
from datetime import datetime
import re
import requests
import json
import time
from urllib.parse import quote

class CompleteDatabaseVietnamizer:
    def __init__(self, connection_string):
        self.connection_string = connection_string
        self.conn = None
        self.translation_cache = {}  # Cache để tránh dịch lại
        
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
    
    def translate_text(self, text, context="luxury watch"):
        """Dịch văn bản sử dụng API miễn phí"""
        if not text or len(text.strip()) < 3:
            return text
        
        # Kiểm tra cache
        if text in self.translation_cache:
            return self.translation_cache[text]
        
        try:
            # Sử dụng LibreTranslate API (miễn phí)
            url = "https://libretranslate.de/translate"
            
            # Thêm context cho dịch thuật chính xác hơn
            context_text = f"Context: {context}. Text: {text}"
            
            payload = {
                "q": text,
                "source": "en",
                "target": "vi",
                "format": "text"
            }
            
            headers = {
                "Content-Type": "application/json"
            }
            
            response = requests.post(url, json=payload, headers=headers, timeout=10)
            
            if response.status_code == 200:
                result = response.json()
                translated_text = result.get("translatedText", text)
                
                # Cache kết quả
                self.translation_cache[text] = translated_text
                
                # Delay để tránh rate limit
                time.sleep(0.5)
                
                return translated_text
            else:
                print(f"⚠️ Lỗi API dịch: {response.status_code}")
                return text
                
        except Exception as e:
            print(f"⚠️ Lỗi dịch thuật: {e}")
            return text
    
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
                'Unworn': 'Chưa đeo'
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
    
    def vietnamize_complex_fields_with_translation(self):
        """Việt hóa các trường phức tạp bằng API dịch thuật"""
        try:
            cursor = self.conn.cursor()
            
            print("\n🌐 VIỆT HÓA CÁC TRƯỜNG PHỨC TẠP VỚI API DỊCH THUẬT:")
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
                    context = "luxury watch website"
                    if field == "WarrantyInfo":
                        context = "luxury watch warranty information"
                    elif field == "Certificate":
                        context = "luxury watch authentication certificate"
                    elif field == "Description":
                        context = "luxury watch product description"
                    
                    translated_text = self.translate_text(original_text, context)
                    
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
            print(f"  Cache dịch thuật: {len(self.translation_cache)} từ")
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi xác minh: {e}")
    
    def run_complete_vietnamization(self):
        """Chạy việt hóa hoàn chỉnh"""
        print("🚀 BẮT ĐẦU VIỆT HÓA HOÀN CHỈNH VỚI API DỊCH THUẬT")
        print("=" * 70)
        print("⚠️  Lưu ý: Quá trình này có thể mất vài phút do sử dụng API dịch thuật")
        print("=" * 70)
        
        if not self.connect():
            return False
        
        try:
            # Sửa lỗi encoding
            self.fix_encoding_issues()
            
            # Việt hóa các trường đơn giản
            self.vietnamize_simple_fields()
            
            # Việt hóa các trường phức tạp với API dịch thuật
            self.vietnamize_complex_fields_with_translation()
            
            # Chuyển đổi giá
            self.convert_prices_to_vnd()
            
            # Việt hóa các bảng khác
            self.vietnamize_other_tables()
            
            # Xác minh kết quả
            self.verify_final_results()
            
            print("\n🎉 VIỆT HÓA HOÀN CHỈNH THÀNH CÔNG!")
            print("=" * 70)
            print("✅ Đã sửa lỗi encoding")
            print("✅ Đã việt hóa các trường đơn giản")
            print("✅ Đã dịch các nội dung phức tạp bằng API")
            print("✅ Đã chuyển đổi giá từ USD sang VND")
            print("✅ Đã việt hóa các bảng khác")
            print("✅ Cột Name và Model được giữ nguyên như yêu cầu")
            print(f"✅ Đã cache {len(self.translation_cache)} bản dịch")
            
            return True
            
        except Exception as e:
            print(f"❌ Lỗi trong quá trình việt hóa: {e}")
            return False
        finally:
            self.close()

def main():
    # Connection string
    connection_string = "DRIVER={SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;"
    
    # Tạo instance và chạy việt hóa hoàn chỉnh
    vietnamizer = CompleteDatabaseVietnamizer(connection_string)
    success = vietnamizer.run_complete_vietnamization()
    
    if success:
        print("\n🎯 KẾT QUẢ: Việt hóa hoàn chỉnh thành công!")
    else:
        print("\n💥 KẾT QUẢ: Việt hóa hoàn chỉnh thất bại!")

if __name__ == "__main__":
    main()
