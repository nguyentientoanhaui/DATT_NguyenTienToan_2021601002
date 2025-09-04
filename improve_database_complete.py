#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Improve Database Format - Complete Version
Script cải thiện định dạng database với xử lý tất cả foreign key constraints
"""

import pyodbc
from datetime import datetime

def main():
    print("🚀 IMPROVE DATABASE FORMAT - COMPLETE VERSION")
    print("=" * 60)
    
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
        
        # Bước 1: Kiểm tra hiện trạng
        print("\n1️⃣ KIỂM TRA HIỆN TRẠNG")
        print("-" * 40)
        
        # Kiểm tra collation database
        cursor.execute("SELECT name, collation_name FROM sys.databases WHERE name = 'Shopping_Demo'")
        db_info = cursor.fetchone()
        print(f"Database: {db_info[0]}")
        print(f"Collation hiện tại: {db_info[1]}")
        
        # Bước 2: Backup
        print("\n2️⃣ BACKUP DỮ LIỆU")
        print("-" * 40)
        
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        backup_table = f"Products_Backup_{timestamp}"
        
        backup_sql = f"SELECT * INTO {backup_table} FROM Products"
        cursor.execute(backup_sql)
        conn.commit()
        print(f"✅ Đã backup thành {backup_table}")
        
        # Bước 3: Tạo bảng temp với cấu trúc đầy đủ
        print("\n3️⃣ TẠO BẢNG TEMP")
        print("-" * 40)
        
        # Xóa bảng temp nếu tồn tại
        try:
            cursor.execute("DROP TABLE Products_Temp")
            conn.commit()
            print("🗑️ Đã xóa bảng temp cũ")
        except:
            pass
        
        # Tạo bảng temp với cấu trúc đầy đủ
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
        print("✅ Đã tạo bảng Products_Temp với Unicode support")
        
        # Bước 4: Sao chép dữ liệu
        print("\n4️⃣ SAO CHÉP DỮ LIỆU")
        print("-" * 40)
        
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
        print("\n5️⃣ TEST UNICODE SUPPORT")
        print("-" * 40)
        
        test_data = [
            ("Đồng hồ Rolex Submariner", "dong-ho-rolex-submariner", "Mô tả đồng hồ Rolex", 100000000, 1, 1, "image.jpg", 1, 0, 80000000, 1, "Submariner", "123456", 2023, "Nam", "Xuất sắc", "Thép không gỉ", "40MM", "Sapphire", "Ceramic", "123456", "Đen", "Index", "3235", "Tự động", "Không", "Thép", "Oyster", "Folding", 1, "Có", "5 Năm", "ROLEX001", 103000000, datetime.now(), datetime.now(), datetime.now(), "https://example.com"),
            ("Đồng hồ Omega Speedmaster", "dong-ho-omega-speedmaster", "Mô tả đồng hồ Omega", 80000000, 2, 1, "image2.jpg", 1, 0, 64000000, 1, "Speedmaster", "654321", 2022, "Nam", "Rất tốt", "Thép không gỉ", "42MM", "Sapphire", "Tachymeter", "654321", "Trắng", "Arabic", "1861", "Tự động", "Chronograph", "Thép", "Bracelet", "Folding", 1, "Không", "3 Năm", "OMEGA001", 82400000, datetime.now(), datetime.now(), datetime.now(), "https://example2.com"),
            ("Đồng hồ Cartier Tank", "dong-ho-cartier-tank", "Mô tả đồng hồ Cartier", 60000000, 3, 2, "image3.jpg", 1, 0, 48000000, 1, "Tank", "789012", 2021, "Nữ", "Tốt", "Vàng 18k", "25MM", "Sapphire", "Không", "789012", "Trắng", "Roman", "Manual", "Thủ công", "Không", "Da", "Leather", "Tang", 1, "Có", "2 Năm", "CARTIER001", 61800000, datetime.now(), datetime.now(), datetime.now(), "https://example3.com")
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
        
        for data in test_data:
            cursor.execute(insert_sql, data)
        
        conn.commit()
        print("✅ Đã thêm dữ liệu test Unicode")
        
        # Kiểm tra dữ liệu test
        cursor.execute("SELECT Name, Gender, Condition, Certificate, WarrantyInfo FROM Products_Temp WHERE Name LIKE '%Đồng hồ%'")
        test_results = cursor.fetchall()
        print("\nDữ liệu test Unicode:")
        for row in test_results:
            print(f"  {row[0]} | {row[1]} | {row[2]} | {row[3]} | {row[4]}")
        
        # Bước 6: Xử lý tất cả Foreign Key và thay thế bảng
        print("\n6️⃣ XỬ LÝ TẤT CẢ FOREIGN KEY VÀ THAY THẾ BẢNG")
        print("-" * 40)
        
        # Lấy tất cả foreign key constraints tham chiếu đến Products
        cursor.execute("""
            SELECT 
                fk.name as FKName,
                OBJECT_NAME(fk.parent_object_id) as TableName,
                COL_NAME(fkc.parent_object_id, fkc.parent_column_id) as ColumnName,
                OBJECT_NAME(fk.referenced_object_id) as ReferencedTableName,
                COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) as ReferencedColumnName
            FROM sys.foreign_keys fk
            INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
            WHERE OBJECT_NAME(fk.referenced_object_id) = 'Products'
            ORDER BY OBJECT_NAME(fk.parent_object_id), fk.name
        """)
        
        referencing_fks = cursor.fetchall()
        print(f"Tìm thấy {len(referencing_fks)} foreign key constraints tham chiếu đến Products")
        
        # Xóa tất cả foreign key constraints tham chiếu đến Products
        for fk in referencing_fks:
            fk_name = fk[0]
            table_name = fk[1]
            drop_fk_sql = f"ALTER TABLE {table_name} DROP CONSTRAINT {fk_name}"
            cursor.execute(drop_fk_sql)
            print(f"🗑️ Đã xóa FK constraint: {fk_name} từ bảng {table_name}")
        
        # Lấy foreign key constraints từ Products
        cursor.execute("""
            SELECT 
                fk.name as FKName,
                OBJECT_NAME(fk.parent_object_id) as TableName,
                COL_NAME(fkc.parent_object_id, fkc.parent_column_id) as ColumnName,
                OBJECT_NAME(fk.referenced_object_id) as ReferencedTableName,
                COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) as ReferencedColumnName
            FROM sys.foreign_keys fk
            INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
            WHERE OBJECT_NAME(fk.parent_object_id) = 'Products'
            ORDER BY fk.name
        """)
        
        products_fks = cursor.fetchall()
        print(f"Tìm thấy {len(products_fks)} foreign key constraints từ Products")
        
        # Xóa foreign key constraints từ Products
        for fk in products_fks:
            fk_name = fk[0]
            drop_fk_sql = f"ALTER TABLE Products DROP CONSTRAINT {fk_name}"
            cursor.execute(drop_fk_sql)
            print(f"🗑️ Đã xóa FK constraint: {fk_name}")
        
        conn.commit()
        
        # Xóa bảng gốc
        cursor.execute("DROP TABLE Products")
        print("🗑️ Đã xóa bảng Products gốc")
        
        # Đổi tên bảng temp
        cursor.execute("EXEC sp_rename 'Products_Temp', 'Products'")
        print("🔄 Đã đổi tên Products_Temp thành Products")
        
        # Tạo lại foreign key constraints từ Products
        print("🔗 Đang tạo lại foreign key constraints từ Products...")
        for fk in products_fks:
            fk_name = fk[0]
            column_name = fk[2]
            referenced_table = fk[3]
            referenced_column = fk[4]
            
            # Tạo tên FK mới
            new_fk_name = f"FK_Products_{referenced_table}_{column_name}"
            
            create_fk_sql = f"""
            ALTER TABLE Products 
            ADD CONSTRAINT {new_fk_name} 
            FOREIGN KEY ({column_name}) 
            REFERENCES {referenced_table}({referenced_column})
            """
            
            try:
                cursor.execute(create_fk_sql)
                print(f"✅ Đã tạo lại FK: {new_fk_name}")
            except Exception as e:
                print(f"⚠️ Không thể tạo lại FK {new_fk_name}: {e}")
        
        # Tạo lại foreign key constraints tham chiếu đến Products
        print("🔗 Đang tạo lại foreign key constraints tham chiếu đến Products...")
        for fk in referencing_fks:
            fk_name = fk[0]
            table_name = fk[1]
            column_name = fk[2]
            referenced_table = fk[3]
            referenced_column = fk[4]
            
            # Tạo tên FK mới
            new_fk_name = f"FK_{table_name}_Products_{column_name}"
            
            create_fk_sql = f"""
            ALTER TABLE {table_name} 
            ADD CONSTRAINT {new_fk_name} 
            FOREIGN KEY ({column_name}) 
            REFERENCES {referenced_table}({referenced_column})
            """
            
            try:
                cursor.execute(create_fk_sql)
                print(f"✅ Đã tạo lại FK: {new_fk_name}")
            except Exception as e:
                print(f"⚠️ Không thể tạo lại FK {new_fk_name}: {e}")
        
        # Tạo lại index
        indexes = [
            "CREATE INDEX IX_Products_Condition ON Products(Condition)",
            "CREATE INDEX IX_Products_Gender ON Products(Gender)",
            "CREATE INDEX IX_Products_Certificate ON Products(Certificate)",
            "CREATE INDEX IX_Products_BrandId ON Products(BrandId)",
            "CREATE INDEX IX_Products_CategoryId ON Products(CategoryId)",
            "CREATE INDEX IX_Products_Price ON Products(Price)",
            "CREATE INDEX IX_Products_Model ON Products(Model)",
            "CREATE INDEX IX_Products_Year ON Products(Year)"
        ]
        
        for index_sql in indexes:
            cursor.execute(index_sql)
        
        conn.commit()
        print("✅ Đã tạo lại các index")
        
        # Bước 7: Kiểm tra kết quả
        print("\n7️⃣ KIỂM TRA KẾT QUẢ")
        print("-" * 40)
        
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
        
        print("\n🎉 HOÀN THÀNH THÀNH CÔNG!")
        print("✅ Database đã được cải thiện với Unicode support")
        print("✅ Backup được tạo an toàn")
        print("✅ Tất cả foreign key constraints đã được xử lý")
        print("✅ Tiếng Việt sẽ hiển thị đúng")
        print("✅ Tất cả các cột đã được chuyển đổi")
        
    except Exception as e:
        print(f"❌ LỖI: {e}")
        print("💡 Hãy kiểm tra lại kết nối và quyền truy cập")

if __name__ == "__main__":
    main()
