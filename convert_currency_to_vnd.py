#!/usr/bin/env python3
"""
Script chuyển đổi giá từ USD sang VND trong database
Author: Assistant
Date: 2024
"""

import pyodbc
import sys
from decimal import Decimal

# Cấu hình database
DB_CONFIG = {
    'server': 'localhost',  # Thay đổi theo server của bạn
    'database': 'Shopping_Demo',
    'trusted_connection': 'yes',  # Sử dụng Windows Authentication
    'driver': '{ODBC Driver 17 for SQL Server}'  # Hoặc {SQL Server}
}

# Tỷ giá USD sang VND
EXCHANGE_RATE = Decimal('24500')  # 1 USD = 24,500 VND

def get_connection():
    """Tạo kết nối database"""
    try:
        conn_str = (
            f"DRIVER={DB_CONFIG['driver']};"
            f"SERVER={DB_CONFIG['server']};"
            f"DATABASE={DB_CONFIG['database']};"
            f"Trusted_Connection={DB_CONFIG['trusted_connection']};"
        )
        return pyodbc.connect(conn_str)
    except Exception as e:
        print(f"❌ Lỗi kết nối database: {e}")
        return None

def backup_data(cursor):
    """Sao lưu dữ liệu hiện tại"""
    print("📦 Đang sao lưu dữ liệu...")
    
    try:
        # Tạo bảng backup
        backup_sql = """
        IF OBJECT_ID('Products_Backup_USD', 'U') IS NOT NULL
            DROP TABLE Products_Backup_USD
        
        SELECT * INTO Products_Backup_USD FROM Products
        """
        cursor.execute(backup_sql)
        print("✅ Đã tạo bảng backup: Products_Backup_USD")
        return True
    except Exception as e:
        print(f"❌ Lỗi backup: {e}")
        return False

def check_current_data(cursor):
    """Kiểm tra dữ liệu hiện tại"""
    print("🔍 Đang kiểm tra dữ liệu hiện tại...")
    
    cursor.execute("""
        SELECT 
            COUNT(*) as TotalProducts,
            MIN(Price) as MinPrice,
            MAX(Price) as MaxPrice,
            AVG(Price) as AvgPrice
        FROM Products
        WHERE Price > 0
    """)
    
    result = cursor.fetchone()
    print(f"📊 Tổng sản phẩm: {result.TotalProducts}")
    print(f"📊 Giá thấp nhất: ${result.MinPrice:,.2f}")
    print(f"📊 Giá cao nhất: ${result.MaxPrice:,.2f}")
    print(f"📊 Giá trung bình: ${result.AvgPrice:,.2f}")
    
    return result

def convert_prices(cursor):
    """Chuyển đổi giá từ USD sang VND"""
    print(f"💱 Đang chuyển đổi giá với tỷ giá: 1 USD = {EXCHANGE_RATE:,} VND")
    
    try:
        # Cập nhật Price
        print("🔄 Cập nhật cột Price...")
        cursor.execute("""
            UPDATE Products 
            SET Price = ROUND(Price * ?, 0)
            WHERE Price > 0 AND Price < 1000000
        """, float(EXCHANGE_RATE))
        
        price_updated = cursor.rowcount
        print(f"✅ Đã cập nhật {price_updated} dòng trong cột Price")
        
        # Cập nhật CapitalPrice
        print("🔄 Cập nhật cột CapitalPrice...")
        cursor.execute("""
            UPDATE Products 
            SET CapitalPrice = ROUND(CapitalPrice * ?, 0)
            WHERE CapitalPrice IS NOT NULL AND CapitalPrice > 0 AND CapitalPrice < 1000000
        """, float(EXCHANGE_RATE))
        
        capital_updated = cursor.rowcount
        print(f"✅ Đã cập nhật {capital_updated} dòng trong cột CapitalPrice")
        
        # Cập nhật CreditCardPrice
        print("🔄 Cập nhật cột CreditCardPrice...")
        cursor.execute("""
            UPDATE Products 
            SET CreditCardPrice = ROUND(CreditCardPrice * ?, 0)
            WHERE CreditCardPrice IS NOT NULL AND CreditCardPrice > 0 AND CreditCardPrice < 1000000
        """, float(EXCHANGE_RATE))
        
        credit_updated = cursor.rowcount
        print(f"✅ Đã cập nhật {credit_updated} dòng trong cột CreditCardPrice")
        
        return True
        
    except Exception as e:
        print(f"❌ Lỗi chuyển đổi: {e}")
        return False

def verify_conversion(cursor):
    """Kiểm tra kết quả sau chuyển đổi"""
    print("🔍 Đang kiểm tra kết quả...")
    
    cursor.execute("""
        SELECT 
            COUNT(*) as TotalProducts,
            MIN(Price) as MinPrice,
            MAX(Price) as MaxPrice,
            AVG(Price) as AvgPrice
        FROM Products
        WHERE Price > 0
    """)
    
    result = cursor.fetchone()
    print(f"📊 Tổng sản phẩm: {result.TotalProducts}")
    print(f"📊 Giá thấp nhất: {result.MinPrice:,.0f} VND")
    print(f"📊 Giá cao nhất: {result.MaxPrice:,.0f} VND")
    print(f"📊 Giá trung bình: {result.AvgPrice:,.0f} VND")
    
    # Hiển thị một số sản phẩm mẫu
    print("\n📋 Một số sản phẩm mẫu:")
    cursor.execute("""
        SELECT TOP 5
            Id,
            Name,
            Price,
            CapitalPrice
        FROM Products
        WHERE Price > 0
        ORDER BY Price
    """)
    
    for row in cursor.fetchall():
        capital = f"{row.CapitalPrice:,.0f}" if row.CapitalPrice else "N/A"
        print(f"  • {row.Name[:50]}... - {row.Price:,.0f} VND (Vốn: {capital} VND)")

def main():
    """Hàm chính"""
    print("🚀 Bắt đầu chuyển đổi giá từ USD sang VND")
    print("=" * 60)
    
    # Xác nhận từ người dùng
    confirm = input("⚠️  Bạn có chắc chắn muốn chuyển đổi giá? (y/N): ")
    if confirm.lower() != 'y':
        print("❌ Đã hủy thao tác")
        return
    
    # Kết nối database
    conn = get_connection()
    if not conn:
        return
    
    try:
        cursor = conn.cursor()
        
        # Kiểm tra dữ liệu hiện tại
        check_current_data(cursor)
        
        # Backup dữ liệu
        if not backup_data(cursor):
            return
        
        # Chuyển đổi giá
        if convert_prices(cursor):
            # Commit thay đổi
            conn.commit()
            print("✅ Đã commit thay đổi")
            
            # Kiểm tra kết quả
            verify_conversion(cursor)
            
            print("\n🎉 Chuyển đổi hoàn tất!")
            print("💡 Lưu ý: Hãy cập nhật code để hiển thị VND thay vì USD")
            print("💡 Backup được lưu tại: Products_Backup_USD")
        else:
            conn.rollback()
            print("❌ Đã rollback do lỗi")
            
    except Exception as e:
        print(f"❌ Lỗi: {e}")
        conn.rollback()
    finally:
        conn.close()

if __name__ == "__main__":
    main()
