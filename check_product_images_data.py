import pyodbc
import os

# Database connection string from appsettings.json
connection_string = "DRIVER={ODBC Driver 17 for SQL Server};SERVER=DESKTOP-C3O3FBL\\SQLEXPRESS;DATABASE=Shopping_Demo;Trusted_Connection=yes;"

try:
    # Connect to database
    conn = pyodbc.connect(connection_string)
    cursor = conn.cursor()
    
    # Query to check ProductImages table for product ID 959
    query = """
    SELECT p.Id, p.Name, pi.Id as ImageId, pi.ImageName, pi.IsDefault, pi.ColorId
    FROM Products p
    LEFT JOIN ProductImages pi ON p.Id = pi.ProductId
    WHERE p.Id = 959
    ORDER BY pi.IsDefault DESC, pi.Id
    """
    
    cursor.execute(query)
    rows = cursor.fetchall()
    
    print("=== Product Images for Product ID 959 ===")
    if rows:
        for row in rows:
            product_id, product_name, image_id, image_name, is_default, color_id = row
            print(f"Product: {product_id} - {product_name}")
            if image_name:
                print(f"  Image ID: {image_id}")
                print(f"  Image Name: {image_name}")
                print(f"  Is Default: {is_default}")
                print(f"  Color ID: {color_id}")
                
                # Check if image file exists
                image_path = f"wwwroot/media/products/{image_name}"
                if os.path.exists(image_path):
                    print(f"  ✅ File exists: {image_path}")
                else:
                    print(f"  ❌ File missing: {image_path}")
                print("  ---")
            else:
                print(f"  ❌ No images found for this product")
    else:
        print("❌ Product not found or no images")
    
    # Also check what images are actually in the directory
    print("\n=== Files in wwwroot/media/products ===")
    products_dir = "wwwroot/media/products"
    if os.path.exists(products_dir):
        files = os.listdir(products_dir)
        if files:
            print(f"Found {len(files)} files:")
            for file in files[:10]:  # Show first 10 files
                print(f"  - {file}")
        else:
            print("Directory is empty")
    else:
        print("Directory doesn't exist")
    
    # Check all products that have images
    print("\n=== All Products with Images ===")
    query_all = """
    SELECT p.Id, p.Name, COUNT(pi.Id) as ImageCount
    FROM Products p
    LEFT JOIN ProductImages pi ON p.Id = pi.ProductId
    GROUP BY p.Id, p.Name
    HAVING COUNT(pi.Id) > 0
    ORDER BY p.Id
    """
    
    cursor.execute(query_all)
    all_rows = cursor.fetchall()
    
    if all_rows:
        for row in all_rows:
            product_id, product_name, image_count = row
            print(f"Product {product_id}: {product_name} - {image_count} images")
    else:
        print("No products have images")
        
    conn.close()
    
except Exception as e:
    print(f"Error: {e}")

