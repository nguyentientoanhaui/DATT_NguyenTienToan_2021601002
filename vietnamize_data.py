import pyodbc
import pandas as pd
import numpy as np
from datetime import datetime

class DatabaseVietnamizer:
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
    
    def check_current_data(self):
        """Kiểm tra dữ liệu hiện tại"""
        try:
            cursor = self.conn.cursor()
            
            # Kiểm tra dữ liệu Products
            print("\n📊 KIỂM TRA DỮ LIỆU HIỆN TẠI:")
            print("=" * 50)
            
            # Lấy mẫu dữ liệu từ Products
            cursor.execute("""
                SELECT TOP 3 Id, Name, Condition, Gender, Certificate, WarrantyInfo, 
                       Price, CapitalPrice, CreditCardPrice
                FROM Products
            """)
            
            products = cursor.fetchall()
            print("📦 SAMPLE PRODUCTS:")
            for product in products:
                print(f"  ID: {product[0]}")
                print(f"  Name: {product[1]}")
                print(f"  Condition: {product[2]}")
                print(f"  Gender: {product[3]}")
                print(f"  Certificate: {product[4]}")
                print(f"  WarrantyInfo: {product[5]}")
                print(f"  Price: {product[6]}")
                print(f"  CapitalPrice: {product[7]}")
                print(f"  CreditCardPrice: {product[8]}")
                print("  " + "-" * 30)
            
            # Kiểm tra giá trị unique trong các cột
            for column in ['Condition', 'Gender', 'Certificate', 'WarrantyInfo']:
                cursor.execute(f"SELECT DISTINCT {column} FROM Products WHERE {column} IS NOT NULL")
                values = [row[0] for row in cursor.fetchall()]
                print(f"\n🔍 {column} unique values:")
                for value in values:
                    print(f"  - {value}")
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi kiểm tra dữ liệu: {e}")
    
    def vietnamize_products(self):
        """Việt hóa dữ liệu trong bảng Products"""
        try:
            cursor = self.conn.cursor()
            
            print("\n🇻🇳 VIỆT HÓA DỮ LIỆU PRODUCTS:")
            print("=" * 50)
            
            # Mapping từ tiếng Anh sang tiếng Việt
            condition_mapping = {
                'Excellent': 'Xuất sắc',
                'Very Good': 'Rất tốt',
                'Good': 'Tốt',
                'Fair': 'Khá',
                'Poor': 'Kém'
            }
            
            gender_mapping = {
                'Men': 'Nam',
                'Women': 'Nữ',
                'Unisex': 'Unisex'
            }
            
            certificate_mapping = {
                'Original Box': 'Hộp gốc',
                'Original Papers': 'Giấy tờ gốc',
                'Warranty Card': 'Thẻ bảo hành',
                'Service Book': 'Sổ bảo hành',
                'No Papers': 'Không có giấy tờ',
                'Box and Papers': 'Hộp và giấy tờ',
                'Box Only': 'Chỉ có hộp',
                'Papers Only': 'Chỉ có giấy tờ'
            }
            
            warranty_mapping = {
                '1 Year': '1 năm',
                '2 Years': '2 năm',
                '3 Years': '3 năm',
                '5 Years': '5 năm',
                'Lifetime': 'Trọn đời',
                'No Warranty': 'Không bảo hành',
                'International Warranty': 'Bảo hành quốc tế',
                'Manufacturer Warranty': 'Bảo hành nhà sản xuất'
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
            
            # Chuyển đổi giá từ USD sang VND (tỷ giá 1 USD = 24,500 VND)
            print("💰 Chuyển đổi giá từ USD sang VND...")
            cursor.execute("""
                UPDATE Products 
                SET Price = Price * 24500, 
                    CapitalPrice = CapitalPrice * 24500, 
                    CreditCardPrice = CreditCardPrice * 24500 
                WHERE Price > 0 AND Price < 100000
            """)
            affected = cursor.rowcount
            print(f"  Đã chuyển đổi giá cho {affected} sản phẩm")
            
            # Commit thay đổi
            self.conn.commit()
            print("✅ Đã commit tất cả thay đổi!")
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi việt hóa Products: {e}")
            self.conn.rollback()
    
    def vietnamize_other_tables(self):
        """Việt hóa các bảng khác nếu cần"""
        try:
            cursor = self.conn.cursor()
            
            print("\n🇻🇳 VIỆT HÓA CÁC BẢNG KHÁC:")
            print("=" * 50)
            
            # Kiểm tra và việt hóa Categories nếu có
            cursor.execute("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Categories'")
            if cursor.fetchone()[0] > 0:
                print("🔄 Việt hóa Categories...")
                category_mapping = {
                    'Luxury': 'Xa xỉ',
                    'Sport': 'Thể thao',
                    'Dress': 'Công sở',
                    'Casual': 'Thường ngày',
                    'Smart': 'Thông minh'
                }
                
                for eng, vn in category_mapping.items():
                    cursor.execute("UPDATE Categories SET Name = ? WHERE Name = ?", (vn, eng))
                    affected = cursor.rowcount
                    if affected > 0:
                        print(f"  {eng} → {vn}: {affected} danh mục")
            
            # Kiểm tra và việt hóa Brands nếu có
            cursor.execute("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Brands'")
            if cursor.fetchone()[0] > 0:
                print("🔄 Việt hóa Brands...")
                brand_mapping = {
                    'Rolex': 'Rolex',
                    'Omega': 'Omega',
                    'Cartier': 'Cartier',
                    'Patek Philippe': 'Patek Philippe',
                    'Audemars Piguet': 'Audemars Piguet'
                }
                
                for eng, vn in brand_mapping.items():
                    cursor.execute("UPDATE Brands SET Name = ? WHERE Name = ?", (vn, eng))
                    affected = cursor.rowcount
                    if affected > 0:
                        print(f"  {eng} → {vn}: {affected} thương hiệu")
            
            # Commit thay đổi
            self.conn.commit()
            print("✅ Đã commit tất cả thay đổi!")
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi việt hóa các bảng khác: {e}")
            self.conn.rollback()
    
    def verify_changes(self):
        """Xác minh các thay đổi đã được áp dụng"""
        try:
            cursor = self.conn.cursor()
            
            print("\n✅ XÁC MINH THAY ĐỔI:")
            print("=" * 50)
            
            # Kiểm tra dữ liệu sau khi việt hóa
            cursor.execute("""
                SELECT TOP 3 Id, Name, Condition, Gender, Certificate, WarrantyInfo, 
                       Price, CapitalPrice, CreditCardPrice
                FROM Products
            """)
            
            products = cursor.fetchall()
            print("📦 SAMPLE PRODUCTS SAU VIỆT HÓA:")
            for product in products:
                print(f"  ID: {product[0]}")
                print(f"  Name: {product[1]}")
                print(f"  Condition: {product[2]}")
                print(f"  Gender: {product[3]}")
                print(f"  Certificate: {product[4]}")
                print(f"  WarrantyInfo: {product[5]}")
                print(f"  Price: {product[6]:,.0f} VND")
                print(f"  CapitalPrice: {product[7]:,.0f} VND")
                print(f"  CreditCardPrice: {product[8]:,.0f} VND")
                print("  " + "-" * 30)
            
            # Kiểm tra giá trị unique sau việt hóa
            for column in ['Condition', 'Gender', 'Certificate', 'WarrantyInfo']:
                cursor.execute(f"SELECT DISTINCT {column} FROM Products WHERE {column} IS NOT NULL")
                values = [row[0] for row in cursor.fetchall()]
                print(f"\n🔍 {column} unique values sau việt hóa:")
                for value in values:
                    print(f"  - {value}")
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi xác minh: {e}")
    
    def run_vietnamization(self):
        """Chạy toàn bộ quá trình việt hóa"""
        print("🚀 BẮT ĐẦU VIỆT HÓA DỮ LIỆU")
        print("=" * 60)
        
        if not self.connect():
            return False
        
        try:
            # Kiểm tra dữ liệu hiện tại
            self.check_current_data()
            
            # Việt hóa Products
            self.vietnamize_products()
            
            # Việt hóa các bảng khác
            self.vietnamize_other_tables()
            
            # Xác minh thay đổi
            self.verify_changes()
            
            print("\n🎉 VIỆT HÓA HOÀN TẤT!")
            print("=" * 60)
            print("✅ Tất cả dữ liệu đã được việt hóa thành công")
            print("✅ Giá đã được chuyển đổi từ USD sang VND")
            print("✅ Cột Name được giữ nguyên như yêu cầu")
            
            return True
            
        except Exception as e:
            print(f"❌ Lỗi trong quá trình việt hóa: {e}")
            return False
        finally:
            self.close()

def main():
    # Connection string
    connection_string = "DRIVER={SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;"
    
    # Tạo instance và chạy việt hóa
    vietnamizer = DatabaseVietnamizer(connection_string)
    success = vietnamizer.run_vietnamization()
    
    if success:
        print("\n🎯 KẾT QUẢ: Việt hóa thành công!")
    else:
        print("\n💥 KẾT QUẢ: Việt hóa thất bại!")

if __name__ == "__main__":
    main()
