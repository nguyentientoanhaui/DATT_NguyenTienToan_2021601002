#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Simple Database Format Improver
Script đơn giản để cải thiện định dạng database
"""

import pyodbc
from datetime import datetime

def main():
    print("🚀 SIMPLE DATABASE FORMAT IMPROVER")
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
        
        # Bước 1: Kiểm tra hiện trạng
        print("\n1️⃣ KIỂM TRA HIỆN TRẠNG")
        print("-" * 30)
        
        # Kiểm tra collation database
        cursor.execute("SELECT name, collation_name FROM sys.databases WHERE name = 'Shopping_Demo'")
        db_info = cursor.fetchone()
        print(f"Database: {db_info[0]}")
        print(f"Collation hiện tại: {db_info[1]}")
        
        # Kiểm tra collation các cột
        cursor.execute("""
            SELECT 
                c.name as ColumnName,
                c.collation_name as CurrentCollation,
                t.name as DataType
            FROM sys.columns c
            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
            WHERE c.object_id = OBJECT_ID('Products')
            AND c.name IN ('Condition', 'Gender', 'Certificate', 'WarrantyInfo')
            ORDER BY c.name
        """)
        
        columns = cursor.fetchall()
        print("\nCollation các cột:")
        for col in columns:
            print(f"  {col[0]}: {col[2]} (Collation: {col[1]})")
        
        # Kiểm tra dữ liệu mẫu
        cursor.execute("SELECT TOP 3 Id, Name, Condition, Gender FROM Products")
        sample_data = cursor.fetchall()
        print("\nDữ liệu mẫu:")
        for row in sample_data:
            print(f"  ID {row[0]}: {row[1]} | Condition: {row[2]} | Gender: {row[3]}")
        
        # Bước 2: Backup
        print("\n2️⃣ BACKUP DỮ LIỆU")
        print("-" * 30)
        
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        backup_table = f"Products_Backup_{timestamp}"
        
        backup_sql = f"SELECT * INTO {backup_table} FROM Products"
        cursor.execute(backup_sql)
        conn.commit()
        print(f"✅ Đã backup thành {backup_table}")
        
        # Bước 3: Tạo bảng temp
        print("\n3️⃣ TẠO BẢNG TEMP")
        print("-" * 30)
        
        # Xóa bảng temp nếu tồn tại
        try:
            cursor.execute("DROP TABLE Products_Temp")
            conn.commit()
            print("🗑️ Đã xóa bảng temp cũ")
        except:
            pass
        
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
        
        cursor.execute(create_table_sql)
        conn.commit()
        print("✅ Đã tạo bảng Products_Temp với Unicode support")
        
        # Bước 4: Sao chép dữ liệu
        print("\n4️⃣ SAO CHÉP DỮ LIỆU")
        print("-" * 30)
        
        copy_sql = """
        INSERT INTO Products_Temp (Name, Price, CapitalPrice, CreditCardPrice, Gender, Condition, Certificate, WarrantyInfo, Description, Image, BrandId, CategoryId)
        SELECT Name, Price, CapitalPrice, CreditCardPrice, Gender, Condition, Certificate, WarrantyInfo, Description, Image, BrandId, CategoryId
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
        print("-" * 30)
        
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
            cursor.execute(insert_sql, data)
        
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
            cursor.execute(index_sql)
        
        conn.commit()
        print("✅ Đã tạo lại các index")
        
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
            AND c.name IN ('Condition', 'Gender', 'Certificate', 'WarrantyInfo')
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
        print("✅ Tiếng Việt sẽ hiển thị đúng")
        
    except Exception as e:
        print(f"❌ LỖI: {e}")
        print("💡 Hãy kiểm tra lại kết nối và quyền truy cập")

if __name__ == "__main__":
    main()
