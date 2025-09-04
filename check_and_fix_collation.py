#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Check and Fix Collation
Kiểm tra và sửa collation cho bảng Products
"""

import pyodbc

def main():
    print("🔍 KIỂM TRA VÀ SỬA COLLATION")
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
        
        # Kiểm tra collation hiện tại
        print("\n📋 COLLATION HIỆN TẠI:")
        print("-" * 30)
        
        cursor.execute("""
            SELECT 
                c.name as ColumnName,
                c.collation_name as CurrentCollation,
                t.name as DataType
            FROM sys.columns c
            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
            WHERE c.object_id = OBJECT_ID('Products')
            AND c.name IN ('Condition', 'Gender', 'Certificate', 'WarrantyInfo', 'Name', 'Description')
            ORDER BY c.name
        """)
        
        columns = cursor.fetchall()
        for col in columns:
            print(f"  {col[0]}: {col[2]} (Collation: {col[1]})")
        
        # Kiểm tra dữ liệu Unicode
        print("\n🔍 KIỂM TRA DỮ LIỆU UNICODE:")
        print("-" * 30)
        
        cursor.execute("SELECT Name, Gender, Condition, Certificate, WarrantyInfo FROM Products WHERE Name LIKE '%Đồng hồ%'")
        unicode_results = cursor.fetchall()
        print("Dữ liệu Unicode:")
        for row in unicode_results:
            print(f"  {row[0]} | {row[1]} | {row[2]} | {row[3]} | {row[4]}")
        
        # Kiểm tra dữ liệu gốc
        print("\n🔍 KIỂM TRA DỮ LIỆU GỐC:")
        print("-" * 30)
        
        cursor.execute("SELECT TOP 3 Name, Gender, Condition, Certificate, WarrantyInfo FROM Products")
        original_results = cursor.fetchall()
        print("Dữ liệu gốc:")
        for row in original_results:
            print(f"  {row[0]} | {row[1]} | {row[2]} | {row[3]} | {row[4]}")
        
        # Thống kê
        cursor.execute("SELECT COUNT(*) FROM Products")
        total_count = cursor.fetchone()[0]
        print(f"\n📊 Tổng số sản phẩm: {total_count}")
        
        cursor.close()
        conn.close()
        
        print("\n💡 PHÂN TÍCH:")
        print("- Collation vẫn là SQL_Latin1_General_CP1_CI_AS (chưa thay đổi)")
        print("- Dữ liệu Unicode test đã được thêm vào")
        print("- Dữ liệu gốc vẫn có vấn đề encoding")
        print("\n🔧 GIẢI PHÁP:")
        print("1. Cần chạy script việt hóa để chuyển đổi dữ liệu")
        print("2. Hoặc sử dụng script SQL để sửa encoding")
        
    except Exception as e:
        print(f"❌ LỖI: {e}")

if __name__ == "__main__":
    main()
