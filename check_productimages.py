print("Checking ProductImages table structure and data...")

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
    
    # Check ProductImages table structure
    print("\n=== PRODUCTIMAGES TABLE STRUCTURE ===")
    cursor.execute("""
        SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'ProductImages' 
        ORDER BY ORDINAL_POSITION
    """)
    columns = cursor.fetchall()
    
    for column in columns:
        print(f"Column: {column[0]}, Type: {column[1]}, Nullable: {column[2]}")
    
    # Check sample data from ProductImages
    print("\n=== PRODUCTIMAGES SAMPLE DATA ===")
    cursor.execute("SELECT TOP 5 * FROM ProductImages")
    product_images = cursor.fetchall()
    
    for img in product_images:
        print(f"ID: {img[0]}, ProductId: {img[1]}, ImageName: {img[2]}, IsDefault: {img[3]}")
    
    # Check if there's a URL column
    print("\n=== CHECKING FOR URL COLUMN ===")
    cursor.execute("""
        SELECT COLUMN_NAME 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'ProductImages' 
        AND COLUMN_NAME LIKE '%URL%' OR COLUMN_NAME LIKE '%url%'
    """)
    url_columns = cursor.fetchall()
    
    if url_columns:
        print("Found URL columns:")
        for col in url_columns:
            print(f"  - {col[0]}")
    else:
        print("No URL columns found in ProductImages table")
    
    # Check all columns that might contain URLs
    print("\n=== ALL COLUMNS IN PRODUCTIMAGES ===")
    cursor.execute("""
        SELECT COLUMN_NAME 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'ProductImages' 
        ORDER BY ORDINAL_POSITION
    """)
    all_columns = cursor.fetchall()
    
    for col in all_columns:
        print(f"  - {col[0]}")
    
    conn.close()
    print("\nProductImages check completed!")
    
except Exception as e:
    print(f"Error: {e}")
    import traceback
    traceback.print_exc()
