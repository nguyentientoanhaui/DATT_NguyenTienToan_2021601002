#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Database Format Improver
Script Python để cải thiện định dạng database và chuyển đổi dữ liệu
"""

import pyodbc
import pandas as pd
from datetime import datetime
import json
import os
import sys

class DatabaseFormatImprover:
    def __init__(self, connection_string):
        """Khởi tạo kết nối database"""
        self.connection_string = connection_string
        self.conn = None
        self.cursor = None
        
    def connect(self):
        """Kết nối đến database"""
        try:
            self.conn = pyodbc.connect(self.connection_string)
            self.cursor = self.conn.cursor()
            print("✅ Kết nối database thành công!")
            return True
        except Exception as e:
            print(f"❌ Lỗi kết nối database: {e}")
            return False
    
    def disconnect(self):
        """Đóng kết nối database"""
        if self.cursor:
            self.cursor.close()
        if self.conn:
            self.conn.close()
        print("🔌 Đã đóng kết nối database")
    
    def check_current_collation(self):
        """Kiểm tra collation hiện tại của database"""
        try:
            query = """
            SELECT 
                name as DatabaseName,
                collation_name as CurrentCollation,
                CASE 
                    WHEN collation_name LIKE '%_CI_AI' THEN 'Case Insensitive, Accent Insensitive'
                    WHEN collation_name LIKE '%_CS_AI' THEN 'Case Sensitive, Accent Insensitive'
                    WHEN collation_name LIKE '%_CI_AS' THEN 'Case Insensitive, Accent Sensitive'
                    WHEN collation_name LIKE '%_CS_AS' THEN 'Case Sensitive, Accent Sensitive'
                    ELSE 'Other'
                END as CollationType
            FROM sys.databases 
            WHERE name = 'Shopping_Demo'
            """
            
            df = pd.read_sql(query, self.conn)
            print("\n📊 THÔNG TIN COLLATION HIỆN TẠI:")
            print(df.to_string(index=False))
            return df
            
        except Exception as e:
            print(f"❌ Lỗi kiểm tra collation: {e}")
            return None
    
    def check_column_collation(self):
        """Kiểm tra collation của các cột text"""
        try:
            query = """
            SELECT 
                c.name as ColumnName,
                c.collation_name as CurrentCollation,
                t.name as DataType,
                c.max_length as MaxLength
            FROM sys.columns c
            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
            WHERE c.object_id = OBJECT_ID('Products')
            AND c.name IN ('Condition', 'Gender', 'Certificate', 'WarrantyInfo', 'Name', 'Description')
            ORDER BY c.name
            """
            
            df = pd.read_sql(query, self.conn)
            print("\n📋 COLLATION CÁC CỘT TEXT:")
            print(df.to_string(index=False))
            return df
            
        except Exception as e:
            print(f"❌ Lỗi kiểm tra collation cột: {e}")
            return None
    
    def backup_products_table(self):
        """Backup bảng Products"""
        try:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            backup_table = f"Products_Backup_{timestamp}"
            
            query = f"SELECT * INTO {backup_table} FROM Products"
            self.cursor.execute(query)
            self.conn.commit()
            
            print(f"✅ Đã backup bảng Products thành {backup_table}")
            return backup_table
            
        except Exception as e:
            print(f"❌ Lỗi backup: {e}")
            return None
    
    def create_temp_table(self):
        """Tạo bảng temp với Unicode support"""
        try:
            create_table_sql = """
            CREATE TABLE Products_Temp (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Name NVARCHAR(500) COLLATE Vietnamese_CI_AS,
                Price DECIMAL(18,2),
                CapitalPrice DECIMAL(18,2),
                CreditCardPrice DECIMAL(18,2),
                Gender NVARCHAR(50) COLLATE Vietnamese_CI_AS,
                Condition NVARCHAR(200) COLLATE Vietnamese_CI_AS,
                Certificate NVARCHAR(100) COLLATE Vietnamese_CI_AS,
                WarrantyInfo NVARCHAR(200) COLLATE Vietnamese_CI_AS,
                Description NVARCHAR(MAX) COLLATE Vietnamese_CI_AS,
                Image NVARCHAR(500),
                BrandId INT,
                CategoryId INT,
                CreatedAt DATETIME2 DEFAULT GETDATE(),
                UpdatedAt DATETIME2 DEFAULT GETDATE()
            )
            """
            
            self.cursor.execute(create_table_sql)
            self.conn.commit()
            print("✅ Đã tạo bảng Products_Temp với Unicode support")
            return True
            
        except Exception as e:
            print(f"❌ Lỗi tạo bảng temp: {e}")
            return False
    
    def copy_data_to_temp(self):
        """Sao chép dữ liệu từ bảng gốc sang bảng temp"""
        try:
            copy_sql = """
            INSERT INTO Products_Temp (Name, Price, CapitalPrice, CreditCardPrice, Gender, Condition, Certificate, WarrantyInfo, Description, Image, BrandId, CategoryId)
            SELECT Name, Price, CapitalPrice, CreditCardPrice, Gender, Condition, Certificate, WarrantyInfo, Description, Image, BrandId, CategoryId
            FROM Products
            """
            
            self.cursor.execute(copy_sql)
            self.conn.commit()
            
            # Kiểm tra số lượng bản ghi
            self.cursor.execute("SELECT COUNT(*) FROM Products")
            original_count = self.cursor.fetchone()[0]
            
            self.cursor.execute("SELECT COUNT(*) FROM Products_Temp")
            temp_count = self.cursor.fetchone()[0]
            
            print(f"✅ Đã sao chép {temp_count}/{original_count} bản ghi")
            return temp_count == original_count
            
        except Exception as e:
            print(f"❌ Lỗi sao chép dữ liệu: {e}")
            return False
    
    def test_unicode_support(self):
        """Test Unicode support trong bảng temp"""
        try:
            test_data = [
                ("Đồng hồ Rolex Submariner", "Nam", "Xuất sắc", "Có", "5 Năm"),
                ("Đồng hồ Omega Speedmaster", "Nam", "Rất tốt", "Không", "3 Năm"),
                ("Đồng hồ Cartier Tank", "Nữ", "Tốt", "Có", "2 Năm")
            ]
            
            insert_sql = """
            INSERT INTO Products_Temp (Name, Gender, Condition, Certificate, WarrantyInfo)
            VALUES (?, ?, ?, ?, ?)
            """
            
            for data in test_data:
                self.cursor.execute(insert_sql, data)
            
            self.conn.commit()
            
            # Kiểm tra dữ liệu test
            query = "SELECT Name, Gender, Condition, Certificate, WarrantyInfo FROM Products_Temp WHERE Name LIKE '%Đồng hồ%'"
            df = pd.read_sql(query, self.conn)
            
            print("\n🧪 TEST UNICODE SUPPORT:")
            print(df.to_string(index=False))
            
            return True
            
        except Exception as e:
            print(f"❌ Lỗi test Unicode: {e}")
            return False
    
    def replace_original_table(self):
        """Thay thế bảng gốc bằng bảng temp"""
        try:
            # Xóa bảng gốc
            self.cursor.execute("DROP TABLE Products")
            
            # Đổi tên bảng temp
            self.cursor.execute("EXEC sp_rename 'Products_Temp', 'Products'")
            
            # Tạo lại index
            indexes = [
                "CREATE INDEX IX_Products_Condition ON Products(Condition)",
                "CREATE INDEX IX_Products_Gender ON Products(Gender)",
                "CREATE INDEX IX_Products_Certificate ON Products(Certificate)",
                "CREATE INDEX IX_Products_BrandId ON Products(BrandId)",
                "CREATE INDEX IX_Products_CategoryId ON Products(CategoryId)",
                "CREATE INDEX IX_Products_Price ON Products(Price)"
            ]
            
            for index_sql in indexes:
                self.cursor.execute(index_sql)
            
            self.conn.commit()
            print("✅ Đã thay thế bảng Products và tạo lại index")
            return True
            
        except Exception as e:
            print(f"❌ Lỗi thay thế bảng: {e}")
            return False
    
    def vietnamize_data(self):
        """Chuyển đổi dữ liệu sang tiếng Việt"""
        try:
            # Mapping dữ liệu
            mappings = {
                'Condition': {
                    'Excellent': 'Xuất sắc',
                    'Very Good': 'Rất tốt',
                    'Good': 'Tốt',
                    'Fair': 'Khá',
                    'Poor': 'Kém',
                    'New': 'Mới',
                    'Pre-owned': 'Đã sử dụng',
                    'Mint': 'Như mới',
                    'Near Mint': 'Gần như mới'
                },
                'Gender': {
                    'Men': 'Nam',
                    'Women': 'Nữ'
                },
                'Certificate': {
                    'Yes': 'Có',
                    'No': 'Không',
                    'Available': 'Có sẵn',
                    'Not Available': 'Không có',
                    'Included': 'Bao gồm',
                    'Not Included': 'Không bao gồm'
                },
                'WarrantyInfo': {
                    '1 Year': '1 Năm',
                    '2 Years': '2 Năm',
                    '3 Years': '3 Năm',
                    '5 Years': '5 Năm',
                    'Lifetime': 'Trọn đời',
                    'No Warranty': 'Không bảo hành',
                    'Manufacturer Warranty': 'Bảo hành nhà sản xuất',
                    'International Warranty': 'Bảo hành quốc tế'
                }
            }
            
            # Thực hiện chuyển đổi
            for field, mapping in mappings.items():
                for english, vietnamese in mapping.items():
                    update_sql = f"UPDATE Products SET {field} = ? WHERE {field} = ?"
                    self.cursor.execute(update_sql, (vietnamese, english))
            
            # Chuyển đổi giá từ USD sang VND
            price_sql = """
            UPDATE Products 
            SET Price = Price * 24500, CapitalPrice = CapitalPrice * 24500, CreditCardPrice = CreditCardPrice * 24500
            WHERE Price > 0 AND Price < 100000
            """
            self.cursor.execute(price_sql)
            
            self.conn.commit()
            print("✅ Đã chuyển đổi dữ liệu sang tiếng Việt")
            return True
            
        except Exception as e:
            print(f"❌ Lỗi việt hóa: {e}")
            return False
    
    def check_results(self):
        """Kiểm tra kết quả sau khi chuyển đổi"""
        try:
            # Thống kê tổng quan
            stats_sql = """
            SELECT 
                COUNT(*) as TotalProducts,
                SUM(CASE WHEN Condition = N'Xuất sắc' THEN 1 ELSE 0 END) as XuấtSắcCount,
                SUM(CASE WHEN Gender = N'Nam' THEN 1 ELSE 0 END) as NamCount,
                SUM(CASE WHEN Gender = N'Nữ' THEN 1 ELSE 0 END) as NữCount,
                SUM(CASE WHEN Certificate = N'Có' THEN 1 ELSE 0 END) as CóCount,
                SUM(CASE WHEN Price > 1000000 THEN 1 ELSE 0 END) as ProductsWithVNDPrice
            FROM Products
            """
            
            df = pd.read_sql(stats_sql, self.conn)
            print("\n📈 THỐNG KÊ SAU CHUYỂN ĐỔI:")
            print(df.to_string(index=False))
            
            # Hiển thị một số sản phẩm
            sample_sql = """
            SELECT TOP 10 Id, Name, Price, Gender, Condition, Certificate, WarrantyInfo
            FROM Products ORDER BY Id DESC
            """
            
            df_sample = pd.read_sql(sample_sql, self.conn)
            print("\n🔍 MẪU DỮ LIỆU:")
            print(df_sample.to_string(index=False))
            
            return True
            
        except Exception as e:
            print(f"❌ Lỗi kiểm tra kết quả: {e}")
            return False
    
    def run_full_process(self, vietnamize=False):
        """Chạy toàn bộ quy trình cải thiện"""
        print("🚀 BẮT ĐẦU QUY TRÌNH CẢI THIỆN ĐỊNH DẠNG DATABASE")
        print("=" * 60)
        
        # Bước 1: Kiểm tra hiện trạng
        print("\n1️⃣ KIỂM TRA HIỆN TRẠNG")
        self.check_current_collation()
        self.check_column_collation()
        
        # Bước 2: Backup
        print("\n2️⃣ BACKUP DỮ LIỆU")
        backup_table = self.backup_products_table()
        if not backup_table:
            return False
        
        # Bước 3: Tạo bảng temp
        print("\n3️⃣ TẠO BẢNG TEMP")
        if not self.create_temp_table():
            return False
        
        # Bước 4: Sao chép dữ liệu
        print("\n4️⃣ SAO CHÉP DỮ LIỆU")
        if not self.copy_data_to_temp():
            return False
        
        # Bước 5: Test Unicode
        print("\n5️⃣ TEST UNICODE SUPPORT")
        if not self.test_unicode_support():
            return False
        
        # Bước 6: Thay thế bảng
        print("\n6️⃣ THAY THẾ BẢNG")
        if not self.replace_original_table():
            return False
        
        # Bước 7: Việt hóa (nếu cần)
        if vietnamize:
            print("\n7️⃣ VIỆT HÓA DỮ LIỆU")
            if not self.vietnamize_data():
                return False
        
        # Bước 8: Kiểm tra kết quả
        print("\n8️⃣ KIỂM TRA KẾT QUẢ")
        self.check_results()
        
        print("\n✅ QUY TRÌNH HOÀN THÀNH THÀNH CÔNG!")
        return True

def main():
    """Hàm main"""
    # Connection string - thay đổi theo cấu hình của bạn
    connection_string = (
        "DRIVER={SQL Server};"
        "SERVER=localhost;"
        "DATABASE=Shopping_Demo;"
        "Trusted_Connection=yes;"
    )
    
    # Tạo instance
    improver = DatabaseFormatImprover(connection_string)
    
    # Kết nối
    if not improver.connect():
        return
    
    try:
        print("🔧 DATABASE FORMAT IMPROVER")
        print("=" * 40)
        print("1. Cải thiện định dạng (giữ tiếng Anh)")
        print("2. Cải thiện định dạng + Việt hóa")
        print("3. Chỉ kiểm tra hiện trạng")
        print("4. Thoát")
        
        choice = input("\nChọn tùy chọn (1-4): ").strip()
        
        if choice == "1":
            improver.run_full_process(vietnamize=False)
        elif choice == "2":
            improver.run_full_process(vietnamize=True)
        elif choice == "3":
            improver.check_current_collation()
            improver.check_column_collation()
        elif choice == "4":
            print("👋 Tạm biệt!")
        else:
            print("❌ Lựa chọn không hợp lệ!")
            
    except KeyboardInterrupt:
        print("\n⚠️ Đã hủy bởi người dùng")
    except Exception as e:
        print(f"❌ Lỗi: {e}")
    finally:
        improver.disconnect()

if __name__ == "__main__":
    main()
