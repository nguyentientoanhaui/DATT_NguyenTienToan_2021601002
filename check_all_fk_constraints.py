#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Check All Foreign Key Constraints
Kiểm tra tất cả foreign key constraints trong database
"""

import pyodbc

def main():
    print("🔍 KIỂM TRA TẤT CẢ FOREIGN KEY CONSTRAINTS")
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
        
        # Kiểm tra tất cả foreign key constraints
        print("\n📋 TẤT CẢ FOREIGN KEY CONSTRAINTS:")
        print("-" * 60)
        
        cursor.execute("""
            SELECT 
                fk.name as FKName,
                OBJECT_NAME(fk.parent_object_id) as TableName,
                COL_NAME(fkc.parent_object_id, fkc.parent_column_id) as ColumnName,
                OBJECT_NAME(fk.referenced_object_id) as ReferencedTableName,
                COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) as ReferencedColumnName
            FROM sys.foreign_keys fk
            INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
            ORDER BY OBJECT_NAME(fk.parent_object_id), fk.name
        """)
        
        fk_constraints = cursor.fetchall()
        print(f"Tổng số foreign key constraints: {len(fk_constraints)}")
        print()
        
        # Nhóm theo bảng
        table_fks = {}
        for fk in fk_constraints:
            table_name = fk[1]
            if table_name not in table_fks:
                table_fks[table_name] = []
            table_fks[table_name].append(fk)
        
        for table_name, fks in table_fks.items():
            print(f"📋 Bảng: {table_name}")
            for fk in fks:
                print(f"  {fk[0]}: {fk[1]}.{fk[2]} -> {fk[3]}.{fk[4]}")
            print()
        
        # Kiểm tra foreign key constraints tham chiếu đến Products
        print("🔍 FOREIGN KEY CONSTRAINTS THAM CHIẾU ĐẾN PRODUCTS:")
        print("-" * 60)
        
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
        print(f"Số foreign key constraints tham chiếu đến Products: {len(referencing_fks)}")
        for fk in referencing_fks:
            print(f"  {fk[0]}: {fk[1]}.{fk[2]} -> {fk[3]}.{fk[4]}")
        
        cursor.close()
        conn.close()
        
    except Exception as e:
        print(f"❌ LỖI: {e}")

if __name__ == "__main__":
    main()
