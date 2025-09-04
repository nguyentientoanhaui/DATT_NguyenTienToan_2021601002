print("Starting simple database check...")

try:
    import pyodbc
    print("pyodbc imported successfully")
    
    # Connection string
    conn_str = (
        "DRIVER={ODBC Driver 17 for SQL Server};"
        "SERVER=localhost;"
        "DATABASE=Shopping_Demo;"
        "Trusted_Connection=yes;"
    )
    
    print("Attempting to connect...")
    conn = pyodbc.connect(conn_str)
    print("Connected successfully!")
    
    cursor = conn.cursor()
    
    # Check Products table
    print("\n=== PRODUCTS TABLE ===")
    cursor.execute("SELECT TOP 3 Id, Name, Price, BrandId, CategoryId, Quantity, Sold FROM Products")
    products = cursor.fetchall()
    
    for product in products:
        print(f"ID: {product[0]}, Name: {product[1]}, Price: {product[2]}, BrandId: {product[3]}, CategoryId: {product[4]}, Quantity: {product[5]}, Sold: {product[6]}")
    
    # Check Brands
    print("\n=== BRANDS ===")
    cursor.execute("SELECT Id, Name FROM Brands")
    brands = cursor.fetchall()
    for brand in brands:
        print(f"Brand {brand[0]}: {brand[1]}")
    
    # Check Categories
    print("\n=== CATEGORIES ===")
    cursor.execute("SELECT Id, Name FROM Categories")
    categories = cursor.fetchall()
    for category in categories:
        print(f"Category {category[0]}: {category[1]}")
    
    conn.close()
    print("\nDatabase check completed!")
    
except Exception as e:
    print(f"Error: {e}")
    import traceback
    traceback.print_exc()
