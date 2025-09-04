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

def restore_original_product_images():
    """Khoi phuc ProductImages chi hien thi anh chinh cua tung san pham"""
    connection = connect_database()
    if not connection:
        print("Khong the ket noi database!")
        return
    
    cursor = connection.cursor()
    
    print("KHOI PHUC PRODUCTIMAGES CHI HIEN THI ANH CHINH")
    print("=" * 60)
    
    # 1. Xoa tat ca ProductImages hien tai
    print("\n1. Xoa ProductImages cu...")
    cursor.execute("DELETE FROM ProductImages")
    connection.commit()
    print("Da xoa tat ca ProductImages cu")
    
    # 2. Lay tat ca san pham co anh chinh
    print("\n2. Lay danh sach san pham co anh chinh...")
    cursor.execute("""
        SELECT Id, Name, Image 
        FROM Products 
        WHERE Image IS NOT NULL AND Image != ''
        ORDER BY Id
    """)
    
    all_products = cursor.fetchall()
    print(f"Tim thay {len(all_products)} san pham co anh chinh")
    
    # 3. Chi tao 1 ProductImage cho moi san pham (anh chinh)
    print("\n3. Tao ProductImages chi voi anh chinh...")
    
    insert_query = """
        INSERT INTO ProductImages (ProductId, ImageUrl, ImageName, IsDefault)
        VALUES (?, ?, ?, ?)
    """
    
    created_count = 0
    for product in all_products:
        product_id, product_name, main_image = product
        
        # Chi tao 1 anh chinh duy nhat
        cursor.execute(insert_query, (
            product_id,
            main_image,
            f"main_{product_id}.jpg",
            True
        ))
        created_count += 1
    
    # 4. Commit thay doi
    connection.commit()
    print(f"Da tao {created_count} ProductImages (chi anh chinh)")
    
    # 5. Kiem tra ket qua
    print("\n4. Kiem tra ket qua...")
    
    # Tong quan
    cursor.execute("""
        SELECT 
            COUNT(DISTINCT ProductId) as Products_With_Images,
            COUNT(*) as Total_Images,
            COUNT(CASE WHEN IsDefault = 1 THEN 1 END) as Default_Images
        FROM ProductImages
    """)
    
    result = cursor.fetchone()
    print(f"San pham co anh: {result[0]}")
    print(f"Tong so anh: {result[1]}")
    print(f"Anh mac dinh: {result[2]}")
    
    # Test san pham 174
    print("\n5. Test san pham 174...")
    cursor.execute("""
        SELECT ImageUrl, IsDefault
        FROM ProductImages 
        WHERE ProductId = 174
        ORDER BY IsDefault DESC, Id
    """)
    
    images_174 = cursor.fetchall()
    print(f"San pham 174 co {len(images_174)} anh:")
    for i, (url, is_default) in enumerate(images_174):
        status = "CHINH" if is_default else "PHU"
        print(f"  {i+1}. [{status}] {url[:70]}...")
    
    # Test them vai san pham khac
    print("\n6. Test them san pham khac...")
    test_products = [1, 50, 100, 182]
    
    for test_id in test_products:
        cursor.execute("""
            SELECT COUNT(*) as TotalImages
            FROM ProductImages 
            WHERE ProductId = ?
        """, test_id)
        
        check = cursor.fetchone()
        if check:
            print(f"San pham {test_id}: {check[0]} anh")
    
    connection.close()
    print("\nHOAN THANH KHOI PHUC - MOI SAN PHAM CHI CO 1 ANH CHINH!")

if __name__ == "__main__":
    restore_original_product_images()
