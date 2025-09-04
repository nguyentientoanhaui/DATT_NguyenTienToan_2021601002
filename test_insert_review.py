import pyodbc
from datetime import datetime

def test_insert_review():
    try:
        # Connection string
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
        
        # Test data
        test_review = {
            'OrderCode': 'REVIEW_377_TEST_' + str(int(datetime.now().timestamp())),
            'ProductId': 377,
            'UserName': 'Test User',
            'Rating': 5,
            'Comment': 'This is a test review comment with more than 10 characters.',
            'CreatedDate': datetime.now()
        }
        
        print("Inserting test review...")
        print(f"Data: {test_review}")
        
        # Insert review
        cursor.execute("""
            INSERT INTO ProductReviews (OrderCode, ProductId, UserName, Rating, Comment, CreatedDate)
            VALUES (?, ?, ?, ?, ?, ?)
        """, (
            test_review['OrderCode'],
            test_review['ProductId'],
            test_review['UserName'],
            test_review['Rating'],
            test_review['Comment'],
            test_review['CreatedDate']
        ))
        
        conn.commit()
        print("✓ Review inserted successfully")
        
        # Verify insertion
        cursor.execute("""
            SELECT Id, OrderCode, ProductId, UserName, Rating, Comment, CreatedDate
            FROM ProductReviews
            WHERE OrderCode = ?
        """, (test_review['OrderCode'],))
        
        inserted_review = cursor.fetchone()
        if inserted_review:
            print("✓ Review verified in database:")
            print(f"  ID: {inserted_review[0]}")
            print(f"  OrderCode: {inserted_review[1]}")
            print(f"  ProductId: {inserted_review[2]}")
            print(f"  UserName: {inserted_review[3]}")
            print(f"  Rating: {inserted_review[4]}")
            print(f"  Comment: {inserted_review[5]}")
            print(f"  CreatedDate: {inserted_review[6]}")
        else:
            print("✗ Review not found after insertion")
        
        # Check total reviews for Product 377
        cursor.execute("SELECT COUNT(*) FROM ProductReviews WHERE ProductId = 377")
        count = cursor.fetchone()[0]
        print(f"\nTotal reviews for Product 377: {count}")
        
        conn.close()
        print("✓ Test completed successfully")
        
    except Exception as e:
        print(f"✗ Error: {e}")
        return False
    
    return True

if __name__ == "__main__":
    test_insert_review()