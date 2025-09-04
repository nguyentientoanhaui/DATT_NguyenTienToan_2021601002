import pyodbc

def connect_database():
    """Ket noi den database"""
    connection_strings = [
        "DRIVER={ODBC Driver 17 for SQL Server};SERVER=(localdb)\\MSSQLLocalDB;DATABASE=Shopping_Demo;Trusted_Connection=yes;",
        "DRIVER={ODBC Driver 17 for SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;",
    ]
    
    for conn_str in connection_strings:
        try:
            connection = pyodbc.connect(conn_str)
            return connection
        except:
            continue
    return None

def analyze_product_images_detailed():
    """Phan tich chi tiet van de anh san pham"""
    connection = connect_database()
    if not connection:
        print("Khong the ket noi database!")
        return
    
    cursor = connection.cursor()
    
    print("PHAN TICH CHI TIET VAN DE ANH SAN PHAM")
    print("=" * 60)
    
    # 1. Kiem tra ProductId trong ProductImages
    print("\n1. KIEM TRA PRODUCTID TRONG PRODUCTIMAGES:")
    print("-" * 50)
    
    cursor.execute("""
        SELECT 
            MIN(ProductId) as Min_ProductId,
            MAX(ProductId) as Max_ProductId,
            COUNT(DISTINCT ProductId) as Unique_ProductIds,
            COUNT(*) as Total_Images
        FROM ProductImages
    """)
    result = cursor.fetchone()
    print(f"ProductId nho nhat: {result[0]}")
    print(f"ProductId lon nhat: {result[1]}")
    print(f"So ProductId duy nhat: {result[2]}")
    print(f"Tong so anh: {result[3]}")
    
    # 2. Kiem tra ID trong Products
    print("\n2. KIEM TRA ID TRONG PRODUCTS:")
    print("-" * 40)
    
    cursor.execute("""
        SELECT 
            MIN(Id) as Min_Id,
            MAX(Id) as Max_Id,
            COUNT(*) as Total_Products
        FROM Products
    """)
    result = cursor.fetchone()
    print(f"Product Id nho nhat: {result[0]}")
    print(f"Product Id lon nhat: {result[1]}")
    print(f"Tong so san pham: {result[2]}")
    
    # 3. Kiem tra su khop giua ProductImages va Products
    print("\n3. KIEM TRA SU KHOP GIUA PRODUCTIMAGES VA PRODUCTS:")
    print("-" * 55)
    
    cursor.execute("""
        SELECT COUNT(*) 
        FROM ProductImages pi
        INNER JOIN Products p ON pi.ProductId = p.Id
    """)
    matched_count = cursor.fetchone()[0]
    print(f"So anh co ProductId khop voi Products: {matched_count}")
    
    cursor.execute("""
        SELECT COUNT(*) 
        FROM ProductImages pi
        LEFT JOIN Products p ON pi.ProductId = p.Id
        WHERE p.Id IS NULL
    """)
    unmatched_count = cursor.fetchone()[0]
    print(f"So anh co ProductId KHONG khop: {unmatched_count}")
    
    # 4. Lay mau ProductId tu ProductImages
    print("\n4. MAU PRODUCTID TU PRODUCTIMAGES (Top 10):")
    print("-" * 45)
    
    cursor.execute("""
        SELECT TOP 10 ProductId, COUNT(*) as ImageCount
        FROM ProductImages
        GROUP BY ProductId
        ORDER BY ProductId
    """)
    
    for row in cursor.fetchall():
        print(f"ProductId: {row[0]}, So anh: {row[1]}")
    
    # 5. Kiem tra san pham co nhieu anh nhat
    print("\n5. SAN PHAM CO NHIEU ANH NHAT:")
    print("-" * 35)
    
    cursor.execute("""
        SELECT TOP 5 
            pi.ProductId, 
            p.Name,
            COUNT(pi.Id) as ImageCount
        FROM ProductImages pi
        LEFT JOIN Products p ON pi.ProductId = p.Id
        GROUP BY pi.ProductId, p.Name
        ORDER BY ImageCount DESC
    """)
    
    for row in cursor.fetchall():
        product_name = row[1] if row[1] else "KHONG TIM THAY"
        print(f"ProductId: {row[0]}, Ten: {product_name[:30]}..., So anh: {row[2]}")
    
    # 6. Kiem tra cau truc ImageUrl
    print("\n6. KIEM TRA CAU TRUC IMAGEURL:")
    print("-" * 35)
    
    cursor.execute("""
        SELECT TOP 5 Id, ProductId, ImageUrl
        FROM ProductImages
        WHERE ImageUrl IS NOT NULL
        ORDER BY Id
    """)
    
    for row in cursor.fetchall():
        print(f"ID: {row[0]}, ProductId: {row[1]}")
        print(f"  URL: {row[2][:80]}...")
        print()
    
    # 7. Kiem tra san pham cu the 174 va 182
    print("\n7. KIEM TRA SAN PHAM 174 VA 182:")
    print("-" * 35)
    
    for product_id in [174, 182]:
        print(f"\nSan pham ID {product_id}:")
        
        # Kiem tra trong Products
        cursor.execute("SELECT Id, Name FROM Products WHERE Id = ?", product_id)
        product = cursor.fetchone()
        if product:
            print(f"  Ton tai trong Products: {product[1][:50]}...")
        else:
            print(f"  KHONG ton tai trong Products!")
        
        # Kiem tra trong ProductImages
        cursor.execute("SELECT COUNT(*) FROM ProductImages WHERE ProductId = ?", product_id)
        image_count = cursor.fetchone()[0]
        print(f"  So anh trong ProductImages: {image_count}")
        
        # Tim ProductId gan nhat
        cursor.execute("""
            SELECT TOP 1 ProductId, ABS(ProductId - ?) as Distance
            FROM ProductImages
            ORDER BY Distance
        """, product_id)
        nearest = cursor.fetchone()
        if nearest:
            print(f"  ProductId gan nhat trong ProductImages: {nearest[0]} (khoang cach: {nearest[1]})")
    
    # 8. Kiem tra range ProductId
    print("\n8. PHAN TICH RANGE PRODUCTID:")
    print("-" * 30)
    
    cursor.execute("""
        SELECT 
            CASE 
                WHEN ProductId BETWEEN 1 AND 1000 THEN '1-1000'
                WHEN ProductId BETWEEN 1001 AND 2000 THEN '1001-2000'
                WHEN ProductId BETWEEN 2001 AND 3000 THEN '2001-3000'
                WHEN ProductId BETWEEN 3001 AND 4000 THEN '3001-4000'
                WHEN ProductId BETWEEN 4001 AND 5000 THEN '4001-5000'
                ELSE 'Khac'
            END as Range,
            COUNT(*) as Count
        FROM ProductImages
        GROUP BY 
            CASE 
                WHEN ProductId BETWEEN 1 AND 1000 THEN '1-1000'
                WHEN ProductId BETWEEN 1001 AND 2000 THEN '1001-2000'
                WHEN ProductId BETWEEN 2001 AND 3000 THEN '2001-3000'
                WHEN ProductId BETWEEN 3001 AND 4000 THEN '3001-4000'
                WHEN ProductId BETWEEN 4001 AND 5000 THEN '4001-5000'
                ELSE 'Khac'
            END
        ORDER BY Range
    """)
    
    for row in cursor.fetchall():
        print(f"Range {row[0]}: {row[1]} anh")
    
    connection.close()
    print("\nHOAN THANH PHAN TICH!")

if __name__ == "__main__":
    analyze_product_images_detailed()
