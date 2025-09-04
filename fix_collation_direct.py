#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Fix Collation Direct
Sửa collation trực tiếp cho bảng Products
"""

import pyodbc

def main():
    print("🔧 SỬA COLLATION TRỰC TIẾP")
    print("=" * 50)
    
    # Connection string
    connection_string = (
        "DRIVER={SQL Server};"
        "SERVER=localhost;"
        "DATABASE=Shopping_Demo;"
        "Trusted_Connection=yes;"
    )
    
    try:
        print("🔗 Đang kết nối database...")
        conn = pyodbc.connect(connection_string)
        cursor = conn.cursor()
        print("✅ Kết nối thành công!")
        
        # Bước 1: Backup
        print("\n1️⃣ BACKUP DỮ LIỆU")
        print("-" * 30)
        
        from datetime import datetime
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        backup_table = f"Products_Backup_{timestamp}"
        
        backup_sql = f"SELECT * INTO {backup_table} FROM Products"
        cursor.execute(backup_sql)
        conn.commit()
        print(f"✅ Đã backup thành {backup_table}")
        
        # Bước 2: Tạo bảng temp với collation đúng
        print("\n2️⃣ TẠO BẢNG TEMP VỚI COLLATION ĐÚNG")
        print("-" * 30)
        
        # Xóa bảng temp nếu tồn tại
        try:
            cursor.execute("DROP TABLE Products_Temp")
            conn.commit()
            print("🗑️ Đã xóa bảng temp cũ")
        except:
            pass
        
        # Tạo bảng temp với collation Vietnamese_CI_AS
        create_table_sql = """
        CREATE TABLE Products_Temp (
            Id INT IDENTITY(1,1) PRIMARY KEY,
            Name NVARCHAR(MAX) COLLATE Vietnamese_CI_AS,
            Slug NVARCHAR(MAX) COLLATE Vietnamese_CI_AS,
            Description NVARCHAR(MAX) COLLATE Vietnamese_CI_AS,
            Price DECIMAL(18,2),
            BrandId INT,
            CategoryId INT,
            Image NVARCHAR(MAX),
            Quantity INT,
            Sold INT,
            CapitalPrice DECIMAL(18,2),
            IsActive BIT,
            Model NVARCHAR(100) COLLATE Vietnamese_CI_AS,
            ModelNumber NVARCHAR(100),
            Year INT,
            Gender NVARCHAR(100) COLLATE Vietnamese_CI_AS,
            Condition NVARCHAR(400) COLLATE Vietnamese_CI_AS,
            CaseMaterial NVARCHAR(200) COLLATE Vietnamese_CI_AS,
            CaseSize NVARCHAR(40),
            Crystal NVARCHAR(100),
            BezelMaterial NVARCHAR(200) COLLATE Vietnamese_CI_AS,
            SerialNumber NVARCHAR(200),
            DialColor NVARCHAR(100) COLLATE Vietnamese_CI_AS,
            HourMarkers NVARCHAR(200) COLLATE Vietnamese_CI_AS,
            Calibre NVARCHAR(100) COLLATE Vietnamese_CI_AS,
            MovementType NVARCHAR(100) COLLATE Vietnamese_CI_AS,
            Complication NVARCHAR(200) COLLATE Vietnamese_CI_AS,
            BraceletMaterial NVARCHAR(200) COLLATE Vietnamese_CI_AS,
            BraceletType NVARCHAR(100) COLLATE Vietnamese_CI_AS,
            ClaspType NVARCHAR(100) COLLATE Vietnamese_CI_AS,
            BoxAndPapers BIT,
            Certificate NVARCHAR(200) COLLATE Vietnamese_CI_AS,
            WarrantyInfo NVARCHAR(1000) COLLATE Vietnamese_CI_AS,
            ItemNumber NVARCHAR(100),
            CreditCardPrice DECIMAL(18,2),
            CreatedDate DATETIME2,
            UpdatedDate DATETIME2,
            ScrapedAt DATETIME2,
            SourceUrl NVARCHAR(1000)
        )
        """
        
        cursor.execute(create_table_sql)
        conn.commit()
        print("✅ Đã tạo bảng Products_Temp với Vietnamese_CI_AS collation")
        
        # Bước 3: Kiểm tra collation của bảng temp
        print("\n3️⃣ KIỂM TRA COLLATION BẢNG TEMP")
        print("-" * 30)
        
        cursor.execute("""
            SELECT 
                c.name as ColumnName,
                c.collation_name as TempCollation,
                t.name as DataType
            FROM sys.columns c
            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
            WHERE c.object_id = OBJECT_ID('Products_Temp')
            AND c.name IN ('Condition', 'Gender', 'Certificate', 'WarrantyInfo', 'Name', 'Description')
            ORDER BY c.name
        """)
        
        temp_columns = cursor.fetchall()
        print("Collation bảng temp:")
        for col in temp_columns:
            print(f"  {col[0]}: {col[2]} (Collation: {col[1]})")
        
        # Bước 4: Sao chép dữ liệu
        print("\n4️⃣ SAO CHÉP DỮ LIỆU")
        print("-" * 30)
        
        copy_sql = """
        INSERT INTO Products_Temp (
            Name, Slug, Description, Price, BrandId, CategoryId, Image, Quantity, Sold, 
            CapitalPrice, IsActive, Model, ModelNumber, Year, Gender, Condition, CaseMaterial, 
            CaseSize, Crystal, BezelMaterial, SerialNumber, DialColor, HourMarkers, Calibre, 
            MovementType, Complication, BraceletMaterial, BraceletType, ClaspType, BoxAndPapers, 
            Certificate, WarrantyInfo, ItemNumber, CreditCardPrice, CreatedDate, UpdatedDate, 
            ScrapedAt, SourceUrl
        )
        SELECT 
            Name, Slug, Description, Price, BrandId, CategoryId, Image, Quantity, Sold, 
            CapitalPrice, IsActive, Model, ModelNumber, Year, Gender, Condition, CaseMaterial, 
            CaseSize, Crystal, BezelMaterial, SerialNumber, DialColor, HourMarkers, Calibre, 
            MovementType, Complication, BraceletMaterial, BraceletType, ClaspType, BoxAndPapers, 
            Certificate, WarrantyInfo, ItemNumber, CreditCardPrice, CreatedDate, UpdatedDate, 
            ScrapedAt, SourceUrl
        FROM Products
        """
        
        cursor.execute(copy_sql)
        conn.commit()
        
        # Kiểm tra số lượng
        cursor.execute("SELECT COUNT(*) FROM Products")
        original_count = cursor.fetchone()[0]
        
        cursor.execute("SELECT COUNT(*) FROM Products_Temp")
        temp_count = cursor.fetchone()[0]
        
        print(f"✅ Đã sao chép {temp_count}/{original_count} bản ghi")
        
        # Bước 5: Test Unicode
        print("\n5️⃣ TEST UNICODE")
        print("-" * 30)
        
        test_data = [
            ("Đồng hồ Rolex Submariner", "dong-ho-rolex-submariner", "Mô tả đồng hồ Rolex", 100000000, 1, 1, "image.jpg", 1, 0, 80000000, 1, "Submariner", "123456", 2023, "Nam", "Xuất sắc", "Thép không gỉ", "40MM", "Sapphire", "Ceramic", "123456", "Đen", "Index", "3235", "Tự động", "Không", "Thép", "Oyster", "Folding", 1, "Có", "5 Năm", "ROLEX001", 103000000, datetime.now(), datetime.now(), datetime.now(), "https://example.com")
        ]
        
        insert_sql = """
        INSERT INTO Products_Temp (
            Name, Slug, Description, Price, BrandId, CategoryId, Image, Quantity, Sold, 
            CapitalPrice, IsActive, Model, ModelNumber, Year, Gender, Condition, CaseMaterial, 
            CaseSize, Crystal, BezelMaterial, SerialNumber, DialColor, HourMarkers, Calibre, 
            MovementType, Complication, BraceletMaterial, BraceletType, ClaspType, BoxAndPapers, 
            Certificate, WarrantyInfo, ItemNumber, CreditCardPrice, CreatedDate, UpdatedDate, 
            ScrapedAt, SourceUrl
        )
        VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        """
        
        cursor.execute(insert_sql, test_data[0])
        conn.commit()
        print("✅ Đã thêm dữ liệu test Unicode")
        
        # Kiểm tra dữ liệu test
        cursor.execute("SELECT Name, Gender, Condition, Certificate, WarrantyInfo FROM Products_Temp WHERE Name LIKE '%Đồng hồ%'")
        test_results = cursor.fetchall()
        print("\nDữ liệu test Unicode:")
        for row in test_results:
            print(f"  {row[0]} | {row[1]} | {row[2]} | {row[3]} | {row[4]}")
        
        # Bước 6: Thay thế bảng
        print("\n6️⃣ THAY THẾ BẢNG")
        print("-" * 30)
        
        # Xóa bảng gốc
        cursor.execute("DROP TABLE Products")
        print("🗑️ Đã xóa bảng Products gốc")
        
        # Đổi tên bảng temp
        cursor.execute("EXEC sp_rename 'Products_Temp', 'Products'")
        print("🔄 Đã đổi tên Products_Temp thành Products")
        
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
            try:
                cursor.execute(index_sql)
                print(f"✅ Đã tạo index: {index_sql}")
            except Exception as e:
                print(f"⚠️ Không thể tạo index: {e}")
        
        conn.commit()
        
        # Bước 7: Kiểm tra kết quả
        print("\n7️⃣ KIỂM TRA KẾT QUẢ")
        print("-" * 30)
        
        # Kiểm tra collation mới
        cursor.execute("""
            SELECT 
                c.name as ColumnName,
                c.collation_name as NewCollation,
                t.name as DataType
            FROM sys.columns c
            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
            WHERE c.object_id = OBJECT_ID('Products')
            AND c.name IN ('Condition', 'Gender', 'Certificate', 'WarrantyInfo', 'Name', 'Description')
            ORDER BY c.name
        """)
        
        new_columns = cursor.fetchall()
        print("Collation mới:")
        for col in new_columns:
            print(f"  {col[0]}: {col[2]} (Collation: {col[1]})")
        
        # Kiểm tra dữ liệu Unicode
        cursor.execute("SELECT Name, Gender, Condition, Certificate, WarrantyInfo FROM Products WHERE Name LIKE '%Đồng hồ%'")
        unicode_results = cursor.fetchall()
        print("\nDữ liệu Unicode:")
        for row in unicode_results:
            print(f"  {row[0]} | {row[1]} | {row[2]} | {row[3]} | {row[4]}")
        
        # Thống kê
        cursor.execute("SELECT COUNT(*) FROM Products")
        total_count = cursor.fetchone()[0]
        print(f"\n📊 Tổng số sản phẩm: {total_count}")
        
        cursor.close()
        conn.close()
        
        print("\n🎉 HOÀN THÀNH!")
        print("✅ Collation đã được sửa thành Vietnamese_CI_AS")
        print("✅ Unicode support đã được kích hoạt")
        print("✅ Backup được tạo an toàn")
        
    except Exception as e:
        print(f"❌ LỖI: {e}")

if __name__ == "__main__":
    main()
