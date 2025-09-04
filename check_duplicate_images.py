print("Checking for duplicate images in ProductImages table...")

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
    
    # Check for duplicate ImageUrl values
    print("\n=== CHECKING FOR DUPLICATE IMAGEURL ===")
    cursor.execute("""
        SELECT ImageUrl, COUNT(*) as Count
        FROM ProductImages 
        WHERE ImageUrl IS NOT NULL AND ImageUrl != ''
        GROUP BY ImageUrl 
        HAVING COUNT(*) > 1
        ORDER BY Count DESC
    """)
    duplicates = cursor.fetchall()
    
    if duplicates:
        print("Found duplicate ImageUrl values:")
        for dup in duplicates:
            print(f"  - {dup[0]}: {dup[1]} times")
    else:
        print("No duplicate ImageUrl values found")
    
    # Check for duplicate ImageName values
    print("\n=== CHECKING FOR DUPLICATE IMAGENAME ===")
    cursor.execute("""
        SELECT ImageName, COUNT(*) as Count
        FROM ProductImages 
        WHERE ImageName IS NOT NULL AND ImageName != ''
        GROUP BY ImageName 
        HAVING COUNT(*) > 1
        ORDER BY Count DESC
    """)
    duplicates_name = cursor.fetchall()
    
    if duplicates_name:
        print("Found duplicate ImageName values:")
        for dup in duplicates_name:
            print(f"  - {dup[0]}: {dup[1]} times")
    else:
        print("No duplicate ImageName values found")
    
    # Check sample data for a specific product
    print("\n=== SAMPLE PRODUCTIMAGES DATA ===")
    cursor.execute("""
        SELECT TOP 10 Id, ProductId, ImageUrl, ImageName, IsDefault
        FROM ProductImages 
        ORDER BY ProductId, Id
    """)
    sample_data = cursor.fetchall()
    
    for row in sample_data:
        print(f"ID: {row[0]}, ProductId: {row[1]}, ImageUrl: {row[2]}, ImageName: {row[3]}, IsDefault: {row[4]}")
    
    # Check if Product.Image matches any ProductImages.ImageUrl
    print("\n=== CHECKING PRODUCT.IMAGE vs PRODUCTIMAGES.IMAGEURL ===")
    cursor.execute("""
        SELECT p.Id, p.Name, p.Image, pi.ImageUrl, pi.ImageName
        FROM Products p
        LEFT JOIN ProductImages pi ON p.Id = pi.ProductId
        WHERE p.Image IS NOT NULL AND p.Image != ''
        AND (p.Image = pi.ImageUrl OR p.Image = pi.ImageName)
        ORDER BY p.Id
    """)
    matches = cursor.fetchall()
    
    if matches:
        print("Found products where Product.Image matches ProductImages:")
        for match in matches:
            print(f"  - Product {match[0]} ({match[1]}): Product.Image = '{match[2]}' matches ProductImages")
    else:
        print("No matches found between Product.Image and ProductImages")
    
    conn.close()
    print("\nDuplicate check completed!")
    
except Exception as e:
    print(f"Error: {e}")
    import traceback
    traceback.print_exc()
