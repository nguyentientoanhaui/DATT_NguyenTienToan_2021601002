import pyodbc
import json

# Connection string for LocalDB
conn_str = (
    "DRIVER={ODBC Driver 17 for SQL Server};"
    "SERVER=(localdb)\\MSSQLLocalDB;"
    "DATABASE=Shopping_Demo;"
    "Trusted_Connection=yes;"
)

try:
    # Connect to database
    conn = pyodbc.connect(conn_str)
    cursor = conn.cursor()
    
    # Check product 174 and its images
    query = """
    SELECT 
        p.Id as ProductId,
        p.Name as ProductName,
        p.Image as MainImage,
        pi.Id as ImageId,
        pi.ImageName,
        pi.ImageUrl,
        pi.IsDefault
    FROM Products p
    LEFT JOIN ProductImages pi ON p.Id = pi.ProductId
    WHERE p.Id = 174
    ORDER BY pi.IsDefault DESC, pi.Id
    """
    
    cursor.execute(query)
    results = cursor.fetchall()
    
    print(f"Product 174 Image Data:")
    print("-" * 50)
    
    if results:
        for row in results:
            print(f"Product ID: {row.ProductId}")
            print(f"Product Name: {row.ProductName}")
            print(f"Main Image: {row.MainImage}")
            print(f"Image ID: {row.ImageId}")
            print(f"Image Name: {row.ImageName}")
            print(f"Image URL: {row.ImageUrl}")
            print(f"Is Default: {row.IsDefault}")
            print("-" * 30)
    else:
        print("No data found for product 174")
    
    # Check total count of ProductImages for product 174
    count_query = "SELECT COUNT(*) FROM ProductImages WHERE ProductId = 174"
    cursor.execute(count_query)
    count = cursor.fetchone()[0]
    print(f"\nTotal ProductImages for product 174: {count}")
    
    # Check if product exists
    product_query = "SELECT Id, Name, Image FROM Products WHERE Id = 174"
    cursor.execute(product_query)
    product = cursor.fetchone()
    if product:
        print(f"\nProduct exists: ID={product.Id}, Name={product.Name}, Image={product.Image}")
    else:
        print("\nProduct 174 does not exist!")
    
    conn.close()
    
except Exception as e:
    print(f"Error: {e}")
