import pyodbc
import os

# Thông tin kết nối database
connection_string = "Driver={ODBC Driver 17 for SQL Server};Server=localhost;Database=Shopping_Demo;Trusted_Connection=yes;"

try:
    # Kết nối database
    conn = pyodbc.connect(connection_string)
    cursor = conn.cursor()
    
    print("=== KIỂM TRA DỮ LIỆU DATABASE ===\n")
    
    # Kiểm tra bảng Products
    print("1. BẢNG PRODUCTS:")
    print("-" * 50)
    cursor.execute("SELECT TOP 5 Id, Name, Image, IsActive FROM Products")
    products = cursor.fetchall()
    
    if products:
        print(f"Tìm thấy {len(products)} sản phẩm:")
        for product in products:
            print(f"ID: {product[0]}, Name: {product[1]}")
            print(f"  Image: {product[2]}")
            print(f"  Active: {product[3]}")
            print()
    else:
        print("Không có dữ liệu trong bảng Products")
    
    print("="*60 + "\n")
    
    # Kiểm tra bảng ProductImages chi tiết hơn
    print("2. BẢNG PRODUCTIMAGES:")
    print("-" * 50)
    cursor.execute("SELECT TOP 10 Id, ProductId, ImageName, IsDefault FROM ProductImages WHERE ImageName IS NOT NULL")
    product_images = cursor.fetchall()
    
    if product_images:
        print(f"Tìm thấy {len(product_images)} hình ảnh có ImageName:")
        for img in product_images:
            print(f"ID: {img[0]}, ProductID: {img[1]}")
            print(f"  ImageName: {img[2]}")
            print(f"  IsDefault: {img[3]}")
            print()
    else:
        print("Không có dữ liệu ImageName trong bảng ProductImages")
    
    # Kiểm tra ProductImages có ImageName = NULL
    cursor.execute("SELECT COUNT(*) FROM ProductImages WHERE ImageName IS NULL")
    null_count = cursor.fetchone()[0]
    print(f"Số record có ImageName = NULL: {null_count}")
    
    print("="*60 + "\n")
    
    # Kiểm tra join giữa Products và ProductImages
    print("3. JOIN PRODUCTS VÀ PRODUCTIMAGES:")
    print("-" * 50)
    cursor.execute("""
        SELECT TOP 5 p.Id, p.Name, p.Image, pi.ImageName, pi.IsDefault
        FROM Products p
        LEFT JOIN ProductImages pi ON p.Id = pi.ProductId
        WHERE p.IsActive = 1
        ORDER BY p.Id
    """)
    joined_data = cursor.fetchall()
    
    if joined_data:
        print(f"Tìm thấy {len(joined_data)} kết quả:")
        for row in joined_data:
            print(f"ProductID: {row[0]}, Name: {row[1]}")
            print(f"  Product.Image: {row[2]}")
            print(f"  ProductImage.ImageName: {row[3]}")
            print(f"  IsDefault: {row[4]}")
            print()
    else:
        print("Không có dữ liệu join")
    
    print("="*60 + "\n")
    
    # Kiểm tra thư mục media
    print("4. KIỂM TRA THƯ MỤC MEDIA:")
    print("-" * 50)
    media_path = "wwwroot/media/products"
    if os.path.exists(media_path):
        files = os.listdir(media_path)
        print(f"Thư mục {media_path} tồn tại")
        print(f"Số file trong thư mục: {len(files)}")
        if files:
            print("5 file đầu tiên:")
            for i, file in enumerate(files[:5]):
                print(f"  {i+1}. {file}")
    else:
        print(f"Thư mục {media_path} không tồn tại")
    
    conn.close()
    print("\n=== HOÀN THÀNH KIỂM TRA ===")
    
except Exception as e:
    print(f"Lỗi: {e}")
