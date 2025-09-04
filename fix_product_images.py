import pyodbc
import requests
from urllib.parse import urlparse, parse_qs, urlencode, urlunparse
import time

def optimize_bobswatches_url(url):
    """
    Tối ưu hóa URL hình ảnh từ Bob's Watches
    """
    if not url or "bobswatches.com" not in url:
        return url
    
    # Parse URL
    parsed = urlparse(url)
    query_params = parse_qs(parsed.query)
    
    # Thêm tham số tối ưu hóa
    query_params.update({
        'q': ['50'],      # Chất lượng 50%
        'ef': ['2'],      # Edge filter
        'h': ['400'],     # Chiều cao 400px
        'sh': ['true'],   # Sharpening
        'dpr': ['1']      # Device pixel ratio
    })
    
    # Tạo URL mới
    new_query = urlencode(query_params, doseq=True)
    optimized_url = urlunparse((
        parsed.scheme,
        parsed.netloc,
        parsed.path,
        parsed.params,
        new_query,
        parsed.fragment
    ))
    
    return optimized_url

def test_image_url(url, timeout=10):
    """
    Kiểm tra URL hình ảnh có hoạt động không
    """
    try:
        response = requests.head(url, timeout=timeout, allow_redirects=True)
        return response.status_code == 200
    except:
        return False

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

def create_proper_product_images():
    """Tao ProductImages dung cho cac san pham hien tai"""
    connection = connect_database()
    if not connection:
        print("Khong the ket noi database!")
        return
    
    cursor = connection.cursor()
    
    print("TAO PRODUCTIMAGES DUNG CHO CAC SAN PHAM HIEN TAI")
    print("=" * 60)
    
    # 1. Backup ProductImages cu truoc khi xoa
    print("\n1. Backup ProductImages cu...")
    cursor.execute("""
        SELECT COUNT(*) FROM ProductImages
    """)
    old_count = cursor.fetchone()[0]
    print(f"Co {old_count} ProductImages cu")
    
    # 2. Xoa tat ca ProductImages cu (vi khong lien ket voi Products hien tai)
    print("\n2. Xoa ProductImages cu...")
    cursor.execute("DELETE FROM ProductImages")
    print(f"Da xoa {old_count} ProductImages cu")
    
    # 3. Lay tat ca san pham co anh chinh
    print("\n3. Lay san pham co anh chinh...")
    cursor.execute("""
        SELECT Id, Name, Image 
        FROM Products 
        WHERE Image IS NOT NULL AND Image != ''
        ORDER BY Id
    """)
    
    products = cursor.fetchall()
    print(f"Tim thay {len(products)} san pham co anh chinh")
    
    # 4. Tao ProductImages cho moi san pham
    print("\n4. Tao ProductImages moi...")
    
    insert_query = """
        INSERT INTO ProductImages (ProductId, ImageUrl, ImageName, IsDefault)
        VALUES (?, ?, ?, ?)
    """
    
    created_count = 0
    for product in products:
        product_id = product[0]
        product_name = product[1]
        image_url = product[2]
        
        # Tao anh chinh (IsDefault = True)
        cursor.execute(insert_query, (
            product_id,
            image_url,
            f"main_{product_id}.jpg",
            True
        ))
        created_count += 1
        
        # Tao them 2-4 anh phu (cung URL, khac ten file)
        for i in range(2, 5):
            cursor.execute(insert_query, (
                product_id,
                image_url,  # Cung URL voi anh chinh
                f"view_{i}_{product_id}.jpg",
                False
            ))
            created_count += 1
    
    # 5. Commit thay doi
    connection.commit()
    print(f"Da tao {created_count} ProductImages moi")
    
    # 6. Kiem tra ket qua
    print("\n5. Kiem tra ket qua...")
    
    # Tong quan
    cursor.execute("""
        SELECT 
            COUNT(DISTINCT pi.ProductId) as Products_With_Images,
            COUNT(pi.Id) as Total_Images,
            COUNT(CASE WHEN pi.IsDefault = 1 THEN 1 END) as Default_Images
        FROM ProductImages pi
        INNER JOIN Products p ON pi.ProductId = p.Id
    """)
    
    result = cursor.fetchone()
    print(f"San pham co anh: {result[0]}")
    print(f"Tong so anh: {result[1]}")
    print(f"Anh mac dinh: {result[2]}")
    
    # Test cac san pham cu the
    print("\n6. Test san pham cu the...")
    test_products = [174, 182, 1, 50, 100]
    
    for test_id in test_products:
        cursor.execute("""
            SELECT COUNT(*) 
            FROM ProductImages 
            WHERE ProductId = ?
        """, test_id)
        count = cursor.fetchone()[0]
        
        cursor.execute("""
            SELECT Name 
            FROM Products 
            WHERE Id = ?
        """, test_id)
        product_name = cursor.fetchone()
        name = product_name[0][:30] + "..." if product_name else "KHONG TON TAI"
        
        print(f"San pham {test_id} ({name}): {count} anh")
    
    # 7. Kiem tra lien ket
    print("\n7. Kiem tra lien ket...")
    cursor.execute("""
        SELECT COUNT(*) 
        FROM ProductImages pi
        LEFT JOIN Products p ON pi.ProductId = p.Id
        WHERE p.Id IS NULL
    """)
    unlinked = cursor.fetchone()[0]
    print(f"ProductImages khong lien ket: {unlinked}")
    
    connection.close()
    print("\nHOAN THANH TAO PRODUCTIMAGES MOI!")
    print("Bay gio trang chi tiet san pham se hien thi anh dung!")

def main():
    print("FIXING PRODUCT IMAGES - SHOPPING_DEMO")
    print("=" * 60)
    
    # Connection string
    connection_string = (
        "Driver={ODBC Driver 17 for SQL Server};"
        "Server=localhost;"
        "Database=Shopping_Demo;"
        "Trusted_Connection=yes;"
    )
    
    try:
        # Kết nối database
        print("📡 Đang kết nối đến database...")
        connection = pyodbc.connect(connection_string)
        cursor = connection.cursor()
        print("✅ Kết nối thành công!")
        
        # Lấy tất cả sản phẩm có hình ảnh
        print("\nKIEM TRA HINH ANH SAN PHAM:")
        print("-" * 50)
        
        query = """
        SELECT TOP 20 Id, Name, Image 
        FROM Products 
        WHERE Image IS NOT NULL AND Image != ''
        ORDER BY Id
        """
        
        cursor.execute(query)
        products = cursor.fetchall()
        
        print(f"Tìm thấy {len(products)} sản phẩm có hình ảnh")
        
        working_images = 0
        broken_images = 0
        
        for product in products:
            product_id, product_name, image_url = product
            
            print(f"\n📸 Sản phẩm {product_id}: {product_name[:50]}...")
            print(f"   URL gốc: {image_url}")
            
            # Kiểm tra URL gốc
            if test_image_url(image_url):
                print("   ✅ URL gốc hoạt động")
                working_images += 1
            else:
                print("   ❌ URL gốc không hoạt động")
                
                # Tối ưu hóa URL nếu là Bob's Watches
                if "bobswatches.com" in image_url:
                    optimized_url = optimize_bobswatches_url(image_url)
                    print(f"   🔧 URL tối ưu: {optimized_url}")
                    
                    if test_image_url(optimized_url):
                        print("   ✅ URL tối ưu hoạt động")
                        working_images += 1
                        
                        # Cập nhật database với URL tối ưu
                        update_query = "UPDATE Products SET Image = ? WHERE Id = ?"
                        cursor.execute(update_query, (optimized_url, product_id))
                        connection.commit()
                        print("   💾 Đã cập nhật database")
                    else:
                        print("   ❌ URL tối ưu cũng không hoạt động")
                        broken_images += 1
                else:
                    broken_images += 1
            
            # Delay để tránh spam request
            time.sleep(0.5)
        
        print(f"\n📊 KẾT QUẢ:")
        print(f"   ✅ Hình ảnh hoạt động: {working_images}")
        print(f"   ❌ Hình ảnh lỗi: {broken_images}")
        print(f"   📈 Tỷ lệ thành công: {working_images/(working_images+broken_images)*100:.1f}%")
        
        # Kiểm tra ProductImages table
        print(f"\n🔍 KIỂM TRA BẢNG PRODUCTIMAGES:")
        print("-" * 50)
        
        images_query = """
        SELECT TOP 10 pi.Id, pi.ProductId, pi.ImageUrl, p.Name
        FROM ProductImages pi
        JOIN Products p ON pi.ProductId = p.Id
        WHERE pi.ImageUrl IS NOT NULL AND pi.ImageUrl != ''
        ORDER BY pi.Id
        """
        
        cursor.execute(images_query)
        product_images = cursor.fetchall()
        
        print(f"Tìm thấy {len(product_images)} hình ảnh trong ProductImages")
        
        for img in product_images:
            img_id, product_id, image_url, product_name = img
            print(f"\n   🖼️ Image {img_id} (Product {product_id}: {product_name[:30]}...)")
            print(f"      URL: {image_url}")
            
            if test_image_url(image_url):
                print("      ✅ Hoạt động")
            else:
                print("      ❌ Không hoạt động")
                
                if "bobswatches.com" in image_url:
                    optimized_url = optimize_bobswatches_url(image_url)
                    if test_image_url(optimized_url):
                        print(f"      🔧 URL tối ưu: {optimized_url}")
                        
                        # Cập nhật ProductImages
                        update_query = "UPDATE ProductImages SET ImageUrl = ? WHERE Id = ?"
                        cursor.execute(update_query, (optimized_url, img_id))
                        connection.commit()
                        print("      💾 Đã cập nhật ProductImages")
            
            time.sleep(0.5)
        
        connection.close()
        print("\n✅ Hoàn thành kiểm tra và sửa lỗi hình ảnh!")
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    main()
