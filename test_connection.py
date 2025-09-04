#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Test Database Connection
Script đơn giản để test kết nối database
"""

import pyodbc
import sys

def test_connection():
    """Test kết nối database"""
    
    # Danh sách các connection string để thử
    connection_strings = [
        # SQL Server Express
        "DRIVER={SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;",
        "DRIVER={SQL Server};SERVER=.\\SQLEXPRESS;DATABASE=Shopping_Demo;Trusted_Connection=yes;",
        "DRIVER={SQL Server};SERVER=(localdb)\\MSSQLLocalDB;DATABASE=Shopping_Demo;Trusted_Connection=yes;",
        
        # ODBC Driver 17
        "DRIVER={ODBC Driver 17 for SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;",
        "DRIVER={ODBC Driver 17 for SQL Server};SERVER=.\\SQLEXPRESS;DATABASE=Shopping_Demo;Trusted_Connection=yes;",
        
        # ODBC Driver 18
        "DRIVER={ODBC Driver 18 for SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;",
        "DRIVER={ODBC Driver 18 for SQL Server};SERVER=.\\SQLEXPRESS;DATABASE=Shopping_Demo;Trusted_Connection=yes;",
    ]
    
    print("🔍 TESTING DATABASE CONNECTION")
    print("=" * 50)
    
    # Liệt kê các driver có sẵn
    print("📋 Available ODBC Drivers:")
    for driver in pyodbc.drivers():
        print(f"  - {driver}")
    print()
    
    # Test từng connection string
    for i, conn_str in enumerate(connection_strings, 1):
        print(f"🔗 Testing connection {i}:")
        print(f"   {conn_str}")
        
        try:
            conn = pyodbc.connect(conn_str)
            cursor = conn.cursor()
            
            # Test query đơn giản
            cursor.execute("SELECT @@VERSION")
            version = cursor.fetchone()[0]
            
            print(f"✅ SUCCESS! SQL Server version:")
            print(f"   {version[:100]}...")
            
            # Test query database
            cursor.execute("SELECT COUNT(*) FROM Products")
            count = cursor.fetchone()[0]
            print(f"✅ Database accessible! Products count: {count}")
            
            cursor.close()
            conn.close()
            
            print(f"🎉 Connection {i} works! Use this connection string:")
            print(f"   {conn_str}")
            return conn_str
            
        except Exception as e:
            print(f"❌ FAILED: {str(e)}")
            print()
    
    print("❌ No connection string worked!")
    return None

def check_database_info(connection_string):
    """Kiểm tra thông tin database"""
    try:
        conn = pyodbc.connect(connection_string)
        cursor = conn.cursor()
        
        print("\n📊 DATABASE INFORMATION:")
        print("=" * 30)
        
        # Kiểm tra collation database
        cursor.execute("SELECT name, collation_name FROM sys.databases WHERE name = 'Shopping_Demo'")
        db_info = cursor.fetchone()
        if db_info:
            print(f"Database: {db_info[0]}")
            print(f"Collation: {db_info[1]}")
        
        # Kiểm tra bảng Products
        cursor.execute("""
            SELECT 
                c.name as ColumnName,
                c.collation_name as Collation,
                t.name as DataType,
                c.max_length as MaxLength
            FROM sys.columns c
            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
            WHERE c.object_id = OBJECT_ID('Products')
            AND c.name IN ('Condition', 'Gender', 'Certificate', 'WarrantyInfo')
            ORDER BY c.name
        """)
        
        columns = cursor.fetchall()
        print(f"\n📋 Products table columns:")
        for col in columns:
            print(f"  {col[0]}: {col[2]} (Collation: {col[1]})")
        
        # Kiểm tra dữ liệu mẫu
        cursor.execute("SELECT TOP 3 Id, Name, Condition, Gender FROM Products")
        sample_data = cursor.fetchall()
        print(f"\n🔍 Sample data:")
        for row in sample_data:
            print(f"  ID {row[0]}: {row[1]} | Condition: {row[2]} | Gender: {row[3]}")
        
        cursor.close()
        conn.close()
        
    except Exception as e:
        print(f"❌ Error checking database info: {e}")

def main():
    """Main function"""
    print("🐍 Database Connection Tester")
    print("=" * 40)
    
    # Test kết nối
    working_connection = test_connection()
    
    if working_connection:
        # Kiểm tra thông tin database
        check_database_info(working_connection)
        
        print(f"\n✅ SUCCESS! You can now use this connection string:")
        print(f"   {working_connection}")
        
        # Cập nhật file chính
        print(f"\n💡 To use this in the main script, update the connection_string in database_format_improver.py")
    else:
        print(f"\n❌ No working connection found!")
        print(f"💡 Please check:")
        print(f"   1. SQL Server is running")
        print(f"   2. Database 'Shopping_Demo' exists")
        print(f"   3. Windows Authentication is enabled")

if __name__ == "__main__":
    main()
