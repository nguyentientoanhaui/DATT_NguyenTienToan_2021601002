import pyodbc
import pandas as pd

print("Script started...")

# Connection string
conn_str = (
    "DRIVER={ODBC Driver 17 for SQL Server};"
    "SERVER=localhost;"
    "DATABASE=Shopping_Demo;"
    "Trusted_Connection=yes;"
)

print(f"Connection string: {conn_str}")

try:
    print("Attempting to connect to database...")
    # Connect to database
    conn = pyodbc.connect(conn_str)
    cursor = conn.cursor()
    
    print("Connected successfully!")
    print("=== DATABASE FIELDS ANALYSIS ===\n")
    
    # Check Products table structure
    print("1. PRODUCTS TABLE STRUCTURE:")
    cursor.execute("""
        SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'Products'
        ORDER BY ORDINAL_POSITION
    """)
    
    products_columns = cursor.fetchall()
    for col in products_columns:
        print(f"  {col[0]}: {col[1]} ({'NULL' if col[2] == 'YES' else 'NOT NULL'})")
    
    print("\n2. SAMPLE PRODUCT DATA:")
    cursor.execute("SELECT TOP 3 * FROM Products")
    products_sample = cursor.fetchall()
    
    # Get column names
    cursor.execute("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' ORDER BY ORDINAL_POSITION")
    column_names = [col[0] for col in cursor.fetchall()]
    
    for i, product in enumerate(products_sample, 1):
        print(f"\n  Product {i}:")
        for j, value in enumerate(product):
            print(f"    {column_names[j]}: {value}")
    
    print("\n3. BRANDS TABLE:")
    cursor.execute("SELECT Id, Name FROM Brands")
    brands = cursor.fetchall()
    for brand in brands:
        print(f"  {brand[0]}: {brand[1]}")
    
    print("\n4. CATEGORIES TABLE:")
    cursor.execute("SELECT Id, Name FROM Categories")
    categories = cursor.fetchall()
    for category in categories:
        print(f"  {category[0]}: {category[1]}")
    
    print("\n5. PRODUCT COLORS (if exists):")
    try:
        cursor.execute("SELECT TOP 5 * FROM ProductColors")
        product_colors = cursor.fetchall()
        for pc in product_colors:
            print(f"  {pc}")
    except Exception as e:
        print(f"  ProductColors table not found: {e}")
    
    print("\n6. PRODUCT SIZES (if exists):")
    try:
        cursor.execute("SELECT TOP 5 * FROM ProductSizes")
        product_sizes = cursor.fetchall()
        for ps in product_sizes:
            print(f"  {ps}")
    except Exception as e:
        print(f"  ProductSizes table not found: {e}")
    
    print("\n7. PRODUCT IMAGES:")
    cursor.execute("SELECT TOP 5 * FROM ProductImages")
    product_images = cursor.fetchall()
    for pi in product_images:
        print(f"  {pi}")
    
    print("\n8. RATINGS TABLE:")
    cursor.execute("SELECT TOP 5 * FROM Ratings")
    ratings = cursor.fetchall()
    for rating in ratings:
        print(f"  {rating}")
    
    conn.close()
    print("\n=== ANALYSIS COMPLETE ===")
    
except Exception as e:
    print(f"Error: {e}")
    import traceback
    traceback.print_exc()
