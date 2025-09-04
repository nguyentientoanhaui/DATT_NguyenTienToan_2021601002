import pyodbc
import json
from datetime import datetime

def connect_database():
    """Kết nối đến database Shopping_Demo"""
    conn_str = (
        "DRIVER={ODBC Driver 17 for SQL Server};"
        "SERVER=(localdb)\\MSSQLLocalDB;"
        "DATABASE=Shopping_Demo;"
        "Trusted_Connection=yes;"
    )
    
    try:
        connection = pyodbc.connect(conn_str)
        print("Ket noi database thanh cong!")
        return connection
    except Exception as e:
        print(f"❌ Lỗi kết nối database: {e}")
        return None

def get_table_list(connection):
    """Lấy danh sách tất cả các bảng"""
    query = """
    SELECT TABLE_NAME 
    FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_TYPE = 'BASE TABLE'
    ORDER BY TABLE_NAME
    """
    
    cursor = connection.cursor()
    cursor.execute(query)
    tables = [row[0] for row in cursor.fetchall()]
    
    print("\n📊 DANH SÁCH CÁC BẢNG TRONG DATABASE:")
    print("=" * 50)
    for i, table in enumerate(tables, 1):
        print(f"{i:2d}. 📋 {table}")
    
    return tables

def get_table_structure(connection, table_name):
    """Lấy cấu trúc của một bảng"""
    query = """
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        CHARACTER_MAXIMUM_LENGTH
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = ?
    ORDER BY ORDINAL_POSITION
    """
    
    cursor = connection.cursor()
    cursor.execute(query, table_name)
    
    print(f"\n🏗️  CẤU TRÚC BẢNG '{table_name}':")
    print("-" * 60)
    
    columns = []
    for row in cursor.fetchall():
        column_name, data_type, is_nullable, max_length = row
        nullable = "NULL" if is_nullable == 'YES' else "NOT NULL"
        length = f"({max_length})" if max_length else ""
        
        column_info = {
            'name': column_name,
            'type': data_type,
            'nullable': is_nullable == 'YES',
            'max_length': max_length
        }
        columns.append(column_info)
        
        print(f"  🔹 {column_name}: {data_type}{length} {nullable}")
    
    return columns

def count_records(connection, table_name):
    """Đếm số bản ghi trong bảng"""
    try:
        cursor = connection.cursor()
        cursor.execute(f"SELECT COUNT(*) FROM [{table_name}]")
        count = cursor.fetchone()[0]
        return count
    except Exception as e:
        print(f"❌ Lỗi đếm bảng {table_name}: {e}")
        return 0

def get_sample_data(connection, table_name, limit=5):
    """Lấy dữ liệu mẫu từ bảng"""
    try:
        cursor = connection.cursor()
        cursor.execute(f"SELECT TOP {limit} * FROM [{table_name}]")
        
        # Lấy tên cột
        columns = [column[0] for column in cursor.description]
        
        # Lấy dữ liệu
        rows = cursor.fetchall()
        
        print(f"\n📄 DỮ LIỆU MẪU BẢNG '{table_name}' (Top {limit}):")
        print("-" * 80)
        
        if rows:
            # In header
            header = " | ".join(f"{col[:15]:15}" for col in columns)
            print(header)
            print("-" * len(header))
            
            # In dữ liệu
            for row in rows:
                row_str = " | ".join(f"{str(val)[:15]:15}" if val is not None else "NULL".ljust(15) for val in row)
                print(row_str)
        else:
            print("   (Không có dữ liệu)")
        
        return rows, columns
    except Exception as e:
        print(f"❌ Lỗi lấy dữ liệu bảng {table_name}: {e}")
        return [], []

def analyze_products(connection):
    """Phân tích chi tiết bảng Products"""
    print("\n🔍 PHÂN TÍCH CHI TIẾT BẢNG PRODUCTS:")
    print("=" * 50)
    
    queries = [
        ("Tổng số sản phẩm", "SELECT COUNT(*) FROM Products"),
        ("Sản phẩm đang hoạt động", "SELECT COUNT(*) FROM Products WHERE IsActive = 1"),
        ("Sản phẩm có ảnh chính", "SELECT COUNT(*) FROM Products WHERE Image IS NOT NULL AND Image != ''"),
        ("Sản phẩm có ProductImages", """
            SELECT COUNT(DISTINCT p.Id) 
            FROM Products p 
            INNER JOIN ProductImages pi ON p.Id = pi.ProductId
        """)
    ]
    
    cursor = connection.cursor()
    for description, query in queries:
        try:
            cursor.execute(query)
            result = cursor.fetchone()[0]
            print(f"📊 {description}: {result:,}")
        except Exception as e:
            print(f"❌ Lỗi {description}: {e}")

def analyze_product_images(connection):
    """Phân tích bảng ProductImages"""
    print("\n🖼️  PHÂN TÍCH BẢNG PRODUCTIMAGES:")
    print("=" * 40)
    
    queries = [
        ("Tổng số ảnh", "SELECT COUNT(*) FROM ProductImages"),
        ("Ảnh có URL", "SELECT COUNT(*) FROM ProductImages WHERE ImageUrl IS NOT NULL AND ImageUrl != ''"),
        ("Ảnh có ImageName", "SELECT COUNT(*) FROM ProductImages WHERE ImageName IS NOT NULL AND ImageName != ''"),
        ("Ảnh mặc định", "SELECT COUNT(*) FROM ProductImages WHERE IsDefault = 1")
    ]
    
    cursor = connection.cursor()
    for description, query in queries:
        try:
            cursor.execute(query)
            result = cursor.fetchone()[0]
            print(f"📊 {description}: {result:,}")
        except Exception as e:
            print(f"❌ Lỗi {description}: {e}")

def check_specific_product(connection, product_id):
    """Kiểm tra sản phẩm cụ thể"""
    print(f"\n🔍 KIỂM TRA SẢN PHẨM ID {product_id}:")
    print("-" * 40)
    
    # Thông tin sản phẩm
    cursor = connection.cursor()
    cursor.execute("SELECT Id, Name, Image, IsActive FROM Products WHERE Id = ?", product_id)
    product = cursor.fetchone()
    
    if product:
        print(f"📦 Sản phẩm: {product[1]}")
        print(f"🖼️  Ảnh chính: {product[2] if product[2] else 'Không có'}")
        print(f"✅ Trạng thái: {'Hoạt động' if product[3] else 'Ngừng hoạt động'}")
        
        # Kiểm tra ProductImages
        cursor.execute("""
            SELECT Id, ImageName, ImageUrl, IsDefault 
            FROM ProductImages 
            WHERE ProductId = ?
            ORDER BY IsDefault DESC, Id
        """, product_id)
        
        images = cursor.fetchall()
        print(f"🖼️  Số ảnh phụ: {len(images)}")
        
        if images:
            print("   Chi tiết ảnh phụ:")
            for img in images:
                default_text = " (Mặc định)" if img[3] else ""
                url_text = img[2] if img[2] else "Không có URL"
                name_text = img[1] if img[1] else "Không có tên"
                print(f"     - ID {img[0]}: {name_text} | URL: {url_text[:50]}...{default_text}")
    else:
        print(f"❌ Không tìm thấy sản phẩm ID {product_id}")

def main():
    """Hàm chính"""
    print("PHAN TICH DATABASE SHOPPING_DEMO")
    print("=" * 60)
    
    # Kết nối database
    connection = connect_database()
    if not connection:
        return
    
    try:
        # 1. Lấy danh sách bảng
        tables = get_table_list(connection)
        
        # 2. Đếm số bản ghi trong mỗi bảng
        print("\n📊 SỐ LƯỢNG BẢN GHI:")
        print("=" * 30)
        total_records = 0
        for table in tables:
            count = count_records(connection, table)
            total_records += count
            print(f"📈 {table}: {count:,} bản ghi")
        
        print(f"\n📊 Tổng cộng: {total_records:,} bản ghi")
        
        # 3. Phân tích các bảng quan trọng
        analyze_products(connection)
        analyze_product_images(connection)
        
        # 4. Xem cấu trúc bảng quan trọng
        important_tables = ['Products', 'ProductImages', 'Categories', 'Brands']
        for table in important_tables:
            if table in tables:
                get_table_structure(connection, table)
        
        # 5. Xem dữ liệu mẫu
        print("\n" + "="*60)
        print("📄 DỮ LIỆU MẪU:")
        for table in ['Products', 'ProductImages']:
            if table in tables:
                get_sample_data(connection, table, 3)
        
        # 6. Kiểm tra sản phẩm cụ thể
        check_specific_product(connection, 174)
        check_specific_product(connection, 182)
        
    except Exception as e:
        print(f"❌ Lỗi trong quá trình phân tích: {e}")
    
    finally:
        connection.close()
        print("\n🔒 Đã đóng kết nối database")
    
    print("\n✅ HOÀN THÀNH PHÂN TÍCH!")

if __name__ == "__main__":
    main()
