import pyodbc
import os
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

# Database connection
server = os.getenv('DB_SERVER', 'localhost')
database = os.getenv('DB_NAME', 'Shopping_Demo')
username = os.getenv('DB_USER', '')
password = os.getenv('DB_PASSWORD', '')

# Create connection string
connection_string = f'DRIVER={{ODBC Driver 17 for SQL Server}};SERVER={server};DATABASE={database};'
if username and password:
    connection_string += f'UID={username};PWD={password}'
else:
    connection_string += 'Trusted_Connection=yes;'

try:
    # Connect to the database
    conn = pyodbc.connect(connection_string)
    cursor = conn.cursor()

    # Get total number of products and product images
    cursor.execute("SELECT COUNT(*) FROM Products")
    total_products = cursor.fetchone()[0]
    
    cursor.execute("SELECT COUNT(*) FROM ProductImages")
    total_product_images = cursor.fetchone()[0]
    
    print(f"Total Products: {total_products}")
    print(f"Total Product Images: {total_product_images}")
    print(f"Average images per product: {total_product_images / total_products:.2f}")
    
    # Get distribution of images per product
    print("\nImages per product distribution:")
    cursor.execute("""
        SELECT COUNT(pi.Id) as ImageCount, COUNT(*) as ProductCount
        FROM Products p
        LEFT JOIN ProductImages pi ON p.Id = pi.ProductId
        GROUP BY p.Id
        ORDER BY ImageCount DESC
    """)
    
    image_counts = {}
    for row in cursor.fetchall():
        count = row.ImageCount
        if count in image_counts:
            image_counts[count] += 1
        else:
            image_counts[count] = 1
    
    for count, num_products in sorted(image_counts.items()):
        print(f"{count} image(s): {num_products} products")
    
    # Check for products with no images
    cursor.execute("""
        SELECT COUNT(*) 
        FROM Products p
        LEFT JOIN ProductImages pi ON p.Id = pi.ProductId
        WHERE pi.Id IS NULL
    """)
    products_without_images = cursor.fetchone()[0]
    print(f"\nProducts without any images: {products_without_images}")
    
    # Check for duplicate image URLs within the same product
    print("\nChecking for duplicate image URLs within products...")
    cursor.execute("""
        SELECT pi1.ProductId, pi1.ImageUrl, COUNT(*) as DuplicateCount
        FROM ProductImages pi1
        INNER JOIN ProductImages pi2 ON pi1.ProductId = pi2.ProductId AND pi1.ImageUrl = pi2.ImageUrl
        WHERE pi1.Id <= pi2.Id
        GROUP BY pi1.ProductId, pi1.ImageUrl
        HAVING COUNT(*) > 1
        ORDER BY DuplicateCount DESC
    """)
    duplicate_urls = cursor.fetchall()
    print(f"Products with duplicate image URLs: {len(duplicate_urls)}")
    if duplicate_urls:
        print("Sample of products with duplicate image URLs:")
        for i, row in enumerate(duplicate_urls[:5], 1):
            print(f"{i}. Product ID: {row.ProductId}, URL: {row.ImageUrl}, Duplicates: {row.DuplicateCount}")
    
    # Check for invalid image URLs
    print("\nChecking for invalid image URLs...")
    cursor.execute("""
        SELECT TOP 5 Id, ProductId, ImageUrl
        FROM ProductImages
        WHERE ImageUrl IS NULL OR ImageUrl = ''
    """)
    invalid_urls = cursor.fetchall()
    print(f"Found {len(invalid_urls)} invalid image URLs")
    if invalid_urls:
        print("Sample of invalid image URLs:")
        for i, row in enumerate(invalid_urls, 1):
            print(f"{i}. ID: {row.Id}, Product ID: {row.ProductId}, URL: {row.ImageUrl}")
    
    conn.close()

except Exception as e:
    print(f"Error: {str(e)}")
