#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Check Table Structure
Kiểm tra cấu trúc bảng Products
"""

import pyodbc

def main():
    print("🔍 KIỂM TRA CẤU TRÚC BẢNG PRODUCTS")
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
        
        # Kiểm tra cấu trúc bảng Products
        print("\n📋 CẤU TRÚC BẢNG PRODUCTS:")
        print("-" * 50)
        
        cursor.execute("""
            SELECT 
                c.name as ColumnName,
                t.name as DataType,
                c.max_length as MaxLength,
                c.precision as Precision,
                c.scale as Scale,
                c.is_nullable as IsNullable,
                c.collation_name as Collation
            FROM sys.columns c
            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
            WHERE c.object_id = OBJECT_ID('Products')
            ORDER BY c.column_id
        """)
        
        columns = cursor.fetchall()
        print(f"{'Tên cột':<20} {'Kiểu dữ liệu':<15} {'Độ dài':<10} {'Precision':<10} {'Scale':<8} {'Null':<5} {'Collation'}")
        print("-" * 100)
        
        for col in columns:
            print(f"{col[0]:<20} {col[1]:<15} {col[2]:<10} {col[3]:<10} {col[4]:<8} {'Yes' if col[5] else 'No':<5} {col[6] or 'N/A'}")
        
        # Kiểm tra số lượng bản ghi
        cursor.execute("SELECT COUNT(*) FROM Products")
        count = cursor.fetchone()[0]
        print(f"\n📊 Tổng số bản ghi: {count}")
        
        # Kiểm tra dữ liệu mẫu
        print("\n🔍 DỮ LIỆU MẪU:")
        print("-" * 50)
        
        cursor.execute("SELECT TOP 3 * FROM Products")
        sample_data = cursor.fetchall()
        
        # Lấy tên cột
        column_names = [column[0] for column in cursor.description]
        print("Các cột:", ", ".join(column_names))
        print()
        
        for i, row in enumerate(sample_data, 1):
            print(f"Bản ghi {i}:")
            for j, value in enumerate(row):
                print(f"  {column_names[j]}: {value}")
            print()
        
        cursor.close()
        conn.close()
        
    except Exception as e:
        print(f"❌ LỖI: {e}")

if __name__ == "__main__":
    main()
