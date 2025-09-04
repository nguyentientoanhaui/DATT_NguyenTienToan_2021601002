import pyodbc
import random

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

def create_diverse_product_images():
    """Tao ProductImages voi anh khac nhau cho moi san pham"""
    connection = connect_database()
    if not connection:
        print("Khong the ket noi database!")
        return
    
    cursor = connection.cursor()
    
    print("TAO PRODUCTIMAGES VOI ANH KHAC NHAU")
    print("=" * 50)
    
    # 1. Xoa tat ca ProductImages hien tai
    print("\n1. Xoa ProductImages cu...")
    cursor.execute("DELETE FROM ProductImages")
    connection.commit()
    print("Da xoa tat ca ProductImages cu")
    
    # 2. Lay tat ca san pham co anh
    print("\n2. Lay danh sach san pham co anh...")
    cursor.execute("""
        SELECT Id, Name, Image 
        FROM Products 
        WHERE Image IS NOT NULL AND Image != ''
        ORDER BY Id
    """)
    
    all_products = cursor.fetchall()
    print(f"Tim thay {len(all_products)} san pham co anh")
    
    # 3. Tao pool anh de chon ngau nhien
    all_image_urls = [p[2] for p in all_products]
    random.shuffle(all_image_urls)
    
    print(f"Co {len(all_image_urls)} anh khac nhau trong pool")
    
    # 4. Tao ProductImages cho moi san pham
    print("\n3. Tao ProductImages moi...")
    
    insert_query = """
        INSERT INTO ProductImages (ProductId, ImageUrl, ImageName, IsDefault)
        VALUES (?, ?, ?, ?)
    """
    
    created_count = 0
    for product in all_products:
        product_id, product_name, main_image = product
        
        # Tao anh chinh (anh goc cua san pham)
        cursor.execute(insert_query, (
            product_id,
            main_image,
            f"main_{product_id}.jpg",
            True
        ))
        created_count += 1
        
        # Chon 3 anh khac ngau nhien (khong trung voi anh chinh)
        available_images = [img for img in all_image_urls if img != main_image]
        selected_images = random.sample(available_images, min(3, len(available_images)))
        
        # Tao 3 anh phu
        for i, image_url in enumerate(selected_images, 2):
            cursor.execute(insert_query, (
                product_id,
                image_url,
                f"alt_{i}_{product_id}.jpg",
                False
            ))
            created_count += 1
    
    # 5. Commit thay doi
    connection.commit()
    print(f"Da tao {created_count} ProductImages moi")
    
    # 6. Kiem tra ket qua
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
    
    # Kiem tra anh co khac nhau khong
    cursor.execute("""
        SELECT COUNT(DISTINCT ImageUrl) as UniqueUrls, COUNT(*) as TotalImages
        FROM ProductImages 
        WHERE ProductId = 174
    """)
    
    unique_check = cursor.fetchone()
    print(f"\nSan pham 174: {unique_check[1]} anh tong, {unique_check[0]} anh duy nhat")
    
    if unique_check[0] == unique_check[1]:
        print("✓ Tat ca anh deu khac nhau!")
    else:
        print("✗ Van con anh trung lap!")
    
    # Test them vai san pham khac
    print("\n6. Test them san pham khac...")
    test_products = [1, 50, 100, 182]
    
    for test_id in test_products:
        cursor.execute("""
            SELECT COUNT(DISTINCT ImageUrl) as UniqueUrls, COUNT(*) as TotalImages
            FROM ProductImages 
            WHERE ProductId = ?
        """, test_id)
        
        check = cursor.fetchone()
        if check and check[1] > 0:
            status = "OK" if check[0] == check[1] else "TRUNG LAP"
            print(f"San pham {test_id}: {check[1]} anh, {check[0]} duy nhat - {status}")
    
    connection.close()
    print("\nHOAN THANH TAO PRODUCTIMAGES VOI ANH KHAC NHAU!")
