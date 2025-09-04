import pyodbc
import json
from datetime import datetime

def connect_database():
    """Ket noi den database Shopping_Demo"""
    # Try multiple connection strings
    connection_strings = [
        # LocalDB with instance name
        "DRIVER={ODBC Driver 17 for SQL Server};SERVER=(localdb)\\MSSQLLocalDB;DATABASE=Shopping_Demo;Trusted_Connection=yes;",
        # LocalDB without instance name
        "DRIVER={ODBC Driver 17 for SQL Server};SERVER=(localdb)\\v11.0;DATABASE=Shopping_Demo;Trusted_Connection=yes;",
        # SQL Server Express
        "DRIVER={ODBC Driver 17 for SQL Server};SERVER=.\\SQLEXPRESS;DATABASE=Shopping_Demo;Trusted_Connection=yes;",
        # Default SQL Server
        "DRIVER={ODBC Driver 17 for SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;",
    ]
    
    for i, conn_str in enumerate(connection_strings):
        try:
            print(f"Thu ket noi {i+1}...")
            connection = pyodbc.connect(conn_str)
            print("Ket noi database thanh cong!")
            return connection
        except Exception as e:
            print(f"Ket noi {i+1} that bai: {e}")
            continue
    
    print("Tat ca cac ket noi deu that bai!")
    return None

def get_table_list(connection):
    """Lay danh sach tat ca cac bang"""
    query = """
    SELECT TABLE_NAME 
    FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_TYPE = 'BASE TABLE'
    ORDER BY TABLE_NAME
    """
    
    cursor = connection.cursor()
    cursor.execute(query)
    tables = [row[0] for row in cursor.fetchall()]
    
    print("\nDANH SACH CAC BANG TRONG DATABASE:")
    print("=" * 50)
    for i, table in enumerate(tables, 1):
        print(f"{i:2d}. {table}")
    
    return tables

def count_records(connection, table_name):
    """Dem so ban ghi trong bang"""
    try:
        cursor = connection.cursor()
        cursor.execute(f"SELECT COUNT(*) FROM [{table_name}]")
        count = cursor.fetchone()[0]
        return count
    except Exception as e:
        print(f"Loi dem bang {table_name}: {e}")
        return 0

def get_sample_data(connection, table_name, limit=5):
    """Lay du lieu mau tu bang"""
    try:
        cursor = connection.cursor()
        cursor.execute(f"SELECT TOP {limit} * FROM [{table_name}]")
        
        # Lay ten cot
        columns = [column[0] for column in cursor.description]
        
        # Lay du lieu
        rows = cursor.fetchall()
        
        print(f"\nDU LIEU MAU BANG '{table_name}' (Top {limit}):")
        print("-" * 80)
        
        if rows:
            # In header
            header = " | ".join(f"{col[:15]:15}" for col in columns)
            print(header)
            print("-" * len(header))
            
            # In du lieu
            for row in rows:
                row_str = " | ".join(f"{str(val)[:15]:15}" if val is not None else "NULL".ljust(15) for val in row)
                print(row_str)
        else:
            print("   (Khong co du lieu)")
        
        return rows, columns
    except Exception as e:
        print(f"Loi lay du lieu bang {table_name}: {e}")
        return [], []

def check_specific_product(connection, product_id):
    """Kiem tra san pham cu the"""
    print(f"\nKIEM TRA SAN PHAM ID {product_id}:")
    print("-" * 40)
    
    # Thong tin san pham
    cursor = connection.cursor()
    cursor.execute("SELECT Id, Name, Image, IsActive FROM Products WHERE Id = ?", product_id)
    product = cursor.fetchone()
    
    if product:
        print(f"San pham: {product[1]}")
        print(f"Anh chinh: {product[2] if product[2] else 'Khong co'}")
        print(f"Trang thai: {'Hoat dong' if product[3] else 'Ngung hoat dong'}")
        
        # Kiem tra ProductImages
        cursor.execute("""
            SELECT Id, ImageName, ImageUrl, IsDefault 
            FROM ProductImages 
            WHERE ProductId = ?
            ORDER BY IsDefault DESC, Id
        """, product_id)
        
        images = cursor.fetchall()
        print(f"So anh phu: {len(images)}")
        
        if images:
            print("   Chi tiet anh phu:")
            for img in images:
                default_text = " (Mac dinh)" if img[3] else ""
                url_text = img[2] if img[2] else "Khong co URL"
                name_text = img[1] if img[1] else "Khong co ten"
                print(f"     - ID {img[0]}: {name_text} | URL: {url_text[:50]}...{default_text}")
    else:
        print(f"Khong tim thay san pham ID {product_id}")

def analyze_products(connection):
    """Phan tich chi tiet bang Products"""
    print("\nPHAN TICH CHI TIET BANG PRODUCTS:")
    print("=" * 50)
    
    queries = [
        ("Tong so san pham", "SELECT COUNT(*) FROM Products"),
        ("San pham dang hoat dong", "SELECT COUNT(*) FROM Products WHERE IsActive = 1"),
        ("San pham co anh chinh", "SELECT COUNT(*) FROM Products WHERE Image IS NOT NULL AND Image != ''"),
        ("San pham co ProductImages", """
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
            print(f"{description}: {result}")
        except Exception as e:
            print(f"Loi {description}: {e}")

def analyze_product_images(connection):
    """Phan tich bang ProductImages"""
    print("\nPHAN TICH BANG PRODUCTIMAGES:")
    print("=" * 40)
    
    queries = [
        ("Tong so anh", "SELECT COUNT(*) FROM ProductImages"),
        ("Anh co URL", "SELECT COUNT(*) FROM ProductImages WHERE ImageUrl IS NOT NULL AND ImageUrl != ''"),
        ("Anh co ImageName", "SELECT COUNT(*) FROM ProductImages WHERE ImageName IS NOT NULL AND ImageName != ''"),
        ("Anh mac dinh", "SELECT COUNT(*) FROM ProductImages WHERE IsDefault = 1")
    ]
    
    cursor = connection.cursor()
    for description, query in queries:
        try:
            cursor.execute(query)
            result = cursor.fetchone()[0]
            print(f"{description}: {result}")
        except Exception as e:
            print(f"Loi {description}: {e}")

def main():
    """Ham chinh"""
    print("PHAN TICH DATABASE SHOPPING_DEMO")
    print("=" * 60)
    
    # Ket noi database
    connection = connect_database()
    if not connection:
        return
    
    try:
        # 1. Lay danh sach bang
        tables = get_table_list(connection)
        
        # 2. Dem so ban ghi trong moi bang
        print("\nSO LUONG BAN GHI:")
        print("=" * 30)
        total_records = 0
        for table in tables:
            count = count_records(connection, table)
            total_records += count
            print(f"{table}: {count} ban ghi")
        
        print(f"\nTong cong: {total_records} ban ghi")
        
        # 3. Phan tich cac bang quan trong
        analyze_products(connection)
        analyze_product_images(connection)
        
        # 4. Xem du lieu mau
        print("\n" + "="*60)
        print("DU LIEU MAU:")
        for table in ['Products', 'ProductImages']:
            if table in tables:
                get_sample_data(connection, table, 3)
        
        # 5. Kiem tra san pham cu the
        check_specific_product(connection, 174)
        check_specific_product(connection, 182)
        
    except Exception as e:
        print(f"Loi trong qua trinh phan tich: {e}")
    
    finally:
        connection.close()
        print("\nDa dong ket noi database")
    
    print("\nHOAN THANH PHAN TICH!")

if __name__ == "__main__":
    main()
