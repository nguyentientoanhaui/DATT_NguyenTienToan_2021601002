import pyodbc
import sys

def test_product_reviews():
    try:
        # Connection string - adjust as needed
        conn_str = (
            "DRIVER={ODBC Driver 17 for SQL Server};"
            "SERVER=DESKTOP-JCTJFJI;"
            "DATABASE=Shopping_Demo;"
            "Trusted_Connection=yes;"
            "TrustServerCertificate=yes;"
        )
        
        print("Connecting to database...")
        conn = pyodbc.connect(conn_str)
        cursor = conn.cursor()
        
        # Check if ProductReviews table exists
        print("\n=== Checking ProductReviews table ===")
        cursor.execute("""
            SELECT TABLE_NAME 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = 'ProductReviews'
        """)
        
        table_exists = cursor.fetchone()
        if table_exists:
            print("✓ ProductReviews table exists")
        else:
            print("✗ ProductReviews table does not exist")
            return
        
        # Check table structure
        print("\n=== ProductReviews table structure ===")
        cursor.execute("""
            SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'ProductReviews'
            ORDER BY ORDINAL_POSITION
        """)
        
        columns = cursor.fetchall()
        for col in columns:
            print(f"Column: {col[0]}, Type: {col[1]}, Nullable: {col[2]}, Default: {col[3]}")
        
        # Check existing reviews
        print("\n=== Existing reviews ===")
        cursor.execute("SELECT COUNT(*) FROM ProductReviews")
        count = cursor.fetchone()[0]
        print(f"Total reviews: {count}")
        
        if count > 0:
            cursor.execute("""
                SELECT TOP 5 Id, ProductId, UserName, Rating, Comment, CreatedDate
                FROM ProductReviews
                ORDER BY CreatedDate DESC
            """)
            reviews = cursor.fetchall()
            for review in reviews:
                print(f"ID: {review[0]}, ProductID: {review[1]}, User: {review[2]}, Rating: {review[3]}")
                print(f"  Comment: {review[4][:50]}...")
                print(f"  Date: {review[5]}")
                print()
        
        # Check if Product 377 exists
        print("\n=== Checking Product 377 ===")
        cursor.execute("SELECT Id, Name FROM Products WHERE Id = 377")
        product = cursor.fetchone()
        if product:
            print(f"✓ Product 377 exists: {product[1]}")
        else:
            print("✗ Product 377 does not exist")
        
        # Check reviews for Product 377
        print("\n=== Reviews for Product 377 ===")
        cursor.execute("""
            SELECT Id, UserName, Rating, Comment, CreatedDate
            FROM ProductReviews
            WHERE ProductId = 377
            ORDER BY CreatedDate DESC
        """)
        reviews_377 = cursor.fetchall()
        print(f"Reviews for Product 377: {len(reviews_377)}")
        for review in reviews_377:
            print(f"  User: {review[1]}, Rating: {review[2]}, Date: {review[4]}")
            print(f"  Comment: {review[3]}")
            print()
        
        conn.close()
        print("✓ Database connection test completed successfully")
        
    except Exception as e:
        print(f"✗ Error: {e}")
        return False
    
    return True

if __name__ == "__main__":
    test_product_reviews()