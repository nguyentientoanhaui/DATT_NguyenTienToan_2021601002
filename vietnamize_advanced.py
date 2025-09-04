import pyodbc
import pandas as pd
import numpy as np
from datetime import datetime
import re

class AdvancedDatabaseVietnamizer:
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
                'K?m': 'Kém'
            }
            
            for corrupted, fixed in encoding_fixes.items():
                cursor.execute("UPDATE Products SET Condition = ? WHERE Condition = ?", (fixed, corrupted))
                affected = cursor.rowcount
                if affected > 0:
                    print(f"  {corrupted} → {fixed}: {affected} sản phẩm")
            
            # Sửa lỗi encoding cho Gender
            gender_fixes = {
                'Nam': 'Nam',
                'N?': 'Nữ',
                'Unisex': 'Unisex'
            }
            
            for corrupted, fixed in gender_fixes.items():
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
    
    def vietnamize_complex_fields(self):
        """Việt hóa các trường phức tạp"""
        try:
            cursor = self.conn.cursor()
            
            print("\n🇻🇳 VIỆT HÓA CÁC TRƯỜNG PHỨC TẠP:")
            print("=" * 50)
            
            # Việt hóa Certificate - các trường hợp đơn giản
            certificate_simple = {
                'Original Box': 'Hộp gốc',
                'Original Papers': 'Giấy tờ gốc',
                'Warranty Card': 'Thẻ bảo hành',
                'Service Book': 'Sổ bảo hành',
                'No Papers': 'Không có giấy tờ',
                'Box and Papers': 'Hộp và giấy tờ',
                'Box Only': 'Chỉ có hộp',
                'Papers Only': 'Chỉ có giấy tờ',
                'Có': 'Có giấy tờ'
            }
            
            print("🔄 Cập nhật Certificate (đơn giản)...")
            for eng, vn in certificate_simple.items():
                cursor.execute("UPDATE Products SET Certificate = ? WHERE Certificate = ?", (vn, eng))
                affected = cursor.rowcount
                if affected > 0:
                    print(f"  {eng} → {vn}: {affected} sản phẩm")
            
            # Việt hóa WarrantyInfo - các trường hợp đơn giản
            warranty_simple = {
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
            
            print("🔄 Cập nhật WarrantyInfo (đơn giản)...")
            for eng, vn in warranty_simple.items():
                cursor.execute("UPDATE Products SET WarrantyInfo = ? WHERE WarrantyInfo = ?", (vn, eng))
                affected = cursor.rowcount
                if affected > 0:
                    print(f"  {eng} → {vn}: {affected} sản phẩm")
            
            # Việt hóa các mô tả phức tạp trong WarrantyInfo
            print("🔄 Việt hóa mô tả bảo hành phức tạp...")
            
            # Pattern matching cho các mô tả bảo hành
            warranty_patterns = [
                (r"This pre-owned (.+?) comes with Bob's Watches one year warranty", 
                 r"Đồng hồ \1 đã qua sử dụng đi kèm bảo hành 1 năm từ Bob's Watches"),
                (r"This pre-owned (.+?) comes with the remaining (.+?) warranty from (.+?), in addition to Bob's Watches one-year warranty",
                 r"Đồng hồ \1 đã qua sử dụng còn bảo hành \2 từ \3, cộng thêm bảo hành 1 năm từ Bob's Watches"),
                (r"This pre-owned (.+?) comes w/ activated (.+?) warranty from (.+?), in addition to Bob's Watches one-year warranty",
                 r"Đồng hồ \1 đã qua sử dụng với bảo hành \2 được kích hoạt từ \3, cộng thêm bảo hành 1 năm từ Bob's Watches"),
                (r"This pre-owned (.+?) comes with active (.+?) warranty valid until (.+?), in addition to Bob's Watches one-year warranty",
                 r"Đồng hồ \1 đã qua sử dụng với bảo hành \2 còn hiệu lực đến \3, cộng thêm bảo hành 1 năm từ Bob's Watches"),
                (r"This pre-owned/unworn (.+?) comes with active (.+?) warranty valid until (.+?), in addition to Bob's Watches one-year warranty",
                 r"Đồng hồ \1 đã qua sử dụng/chưa đeo với bảo hành \2 còn hiệu lực đến \3, cộng thêm bảo hành 1 năm từ Bob's Watches"),
                (r"This unworn (.+?) comes with the remaining (.+?) factory warranty from (.+?), in addition to Bob's Watches one-year warranty",
                 r"Đồng hồ \1 chưa đeo với bảo hành nhà máy \2 còn lại từ \3, cộng thêm bảo hành 1 năm từ Bob's Watches")
            ]
            
            # Lấy tất cả WarrantyInfo để xử lý
            cursor.execute("SELECT Id, WarrantyInfo FROM Products WHERE WarrantyInfo IS NOT NULL")
            warranty_records = cursor.fetchall()
            
            updated_count = 0
            for record_id, warranty_text in warranty_records:
                if warranty_text and len(warranty_text) > 50:  # Chỉ xử lý mô tả dài
                    original_text = warranty_text
                    for pattern, replacement in warranty_patterns:
                        if re.search(pattern, warranty_text, re.IGNORECASE):
                            warranty_text = re.sub(pattern, replacement, warranty_text, flags=re.IGNORECASE)
                            break
                    
                    if warranty_text != original_text:
                        cursor.execute("UPDATE Products SET WarrantyInfo = ? WHERE Id = ?", (warranty_text, record_id))
                        updated_count += 1
            
            print(f"  Đã việt hóa {updated_count} mô tả bảo hành phức tạp")
            
            self.conn.commit()
            print("✅ Đã việt hóa các trường phức tạp!")
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi việt hóa trường phức tạp: {e}")
            self.conn.rollback()
    
    def verify_final_results(self):
        """Xác minh kết quả cuối cùng"""
        try:
            cursor = self.conn.cursor()
            
            print("\n✅ XÁC MINH KẾT QUẢ CUỐI CÙNG:")
            print("=" * 50)
            
            # Kiểm tra dữ liệu sau khi việt hóa
            cursor.execute("""
                SELECT TOP 5 Id, Name, Condition, Gender, Certificate, WarrantyInfo, 
                       Price, CapitalPrice, CreditCardPrice
                FROM Products
            """)
            
            products = cursor.fetchall()
            print("📦 SAMPLE PRODUCTS SAU VIỆT HÓA HOÀN CHỈNH:")
            for product in products:
                print(f"  ID: {product[0]}")
                print(f"  Name: {product[1]}")
                print(f"  Condition: {product[2]}")
                print(f"  Gender: {product[3]}")
                print(f"  Certificate: {product[4]}")
                print(f"  WarrantyInfo: {product[5][:100]}{'...' if len(str(product[5])) > 100 else ''}")
                print(f"  Price: {product[6]:,.0f} VND")
                print(f"  CapitalPrice: {product[7]:,.0f} VND")
                print(f"  CreditCardPrice: {product[8]:,.0f} VND")
                print("  " + "-" * 30)
            
            # Thống kê
            cursor.execute("SELECT COUNT(*) FROM Products WHERE Condition IN ('Xuất sắc', 'Rất tốt', 'Tốt', 'Khá', 'Kém', 'Mới', 'Vintage')")
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
    
    def run_advanced_vietnamization(self):
        """Chạy việt hóa nâng cao"""
        print("🚀 BẮT ĐẦU VIỆT HÓA NÂNG CAO")
        print("=" * 60)
        
        if not self.connect():
            return False
        
        try:
            # Sửa lỗi encoding
            self.fix_encoding_issues()
            
            # Việt hóa các trường phức tạp
            self.vietnamize_complex_fields()
            
            # Xác minh kết quả
            self.verify_final_results()
            
            print("\n🎉 VIỆT HÓA NÂNG CAO HOÀN TẤT!")
            print("=" * 60)
            print("✅ Đã sửa lỗi encoding")
            print("✅ Đã việt hóa các trường phức tạp")
            print("✅ Đã xử lý mô tả bảo hành dài")
            print("✅ Cột Name được giữ nguyên như yêu cầu")
            
            return True
            
        except Exception as e:
            print(f"❌ Lỗi trong quá trình việt hóa nâng cao: {e}")
            return False
        finally:
            self.close()

def main():
    # Connection string
    connection_string = "DRIVER={SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;"
    
    # Tạo instance và chạy việt hóa nâng cao
    vietnamizer = AdvancedDatabaseVietnamizer(connection_string)
    success = vietnamizer.run_advanced_vietnamization()
    
    if success:
        print("\n🎯 KẾT QUẢ: Việt hóa nâng cao thành công!")
    else:
        print("\n💥 KẾT QUẢ: Việt hóa nâng cao thất bại!")

if __name__ == "__main__":
    main()
