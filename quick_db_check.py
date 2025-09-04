import pyodbc
import pandas as pd

def main():
    print("🔍 QUICK DATABASE CHECK - SHOPPING_DEMO")
    print("=" * 60)
    
    # Connection string
    connection_string = (
        "Driver={ODBC Driver 17 for SQL Server};"
        "Server=localhost;"
        "Database=Shopping_Demo;"
        "Trusted_Connection=yes;"
    )
    
    try:
        # Kết nối database
        print("📡 Đang kết nối đến database...")
        connection = pyodbc.connect(connection_string)
        print("✅ Kết nối thành công!")
        
        # Lấy danh sách bảng
        print("\n📋 DANH SÁCH CÁC BẢNG:")
        print("-" * 50)
        tables_query = """
        SELECT TABLE_NAME, TABLE_SCHEMA
        FROM INFORMATION_SCHEMA.TABLES 
        WHERE TABLE_TYPE = 'BASE TABLE'
        ORDER BY TABLE_NAME
        """
        tables_df = pd.read_sql(tables_query, connection)
        print(f"Tổng số bảng: {len(tables_df)}")
        for i, row in tables_df.iterrows():
            print(f"  {i+1}. {row['TABLE_NAME']} (Schema: {row['TABLE_SCHEMA']})")
        
        # Kiểm tra bảng Products
        print("\n🛍️ KIỂM TRA BẢNG PRODUCTS:")
        print("-" * 50)
        products_query = "SELECT TOP 5 Id, Name, Price, BrandId, CategoryId, Quantity, Sold FROM Products"
        products_df = pd.read_sql(products_query, connection)
        print(f"Số sản phẩm (top 5): {len(products_df)}")
        for _, row in products_df.iterrows():
            print(f"  ID: {row['Id']}, Name: {row['Name'][:50]}..., Price: {row['Price']}")
        
        # Kiểm tra bảng Brands
        print("\n🏷️ KIỂM TRA BẢNG BRANDS:")
        print("-" * 50)
        brands_query = "SELECT Id, Name FROM Brands"
        brands_df = pd.read_sql(brands_query, connection)
        print(f"Số thương hiệu: {len(brands_df)}")
        for _, row in brands_df.iterrows():
            print(f"  {row['Id']}: {row['Name']}")
        
        # Kiểm tra bảng Categories
        print("\n📂 KIỂM TRA BẢNG CATEGORIES:")
        print("-" * 50)
        categories_query = "SELECT Id, Name FROM Categories"
        categories_df = pd.read_sql(categories_query, connection)
        print(f"Số danh mục: {len(categories_df)}")
        for _, row in categories_df.iterrows():
            print(f"  {row['Id']}: {row['Name']}")
        
        # Kiểm tra bảng Users
        print("\n👥 KIỂM TRA BẢNG USERS:")
        print("-" * 50)
        users_query = "SELECT TOP 5 Id, UserName, Email FROM AspNetUsers"
        users_df = pd.read_sql(users_query, connection)
        print(f"Số người dùng (top 5): {len(users_df)}")
        for _, row in users_df.iterrows():
            print(f"  {row['Id']}: {row['UserName']} ({row['Email']})")
        
        # Kiểm tra bảng Orders
        print("\n📦 KIỂM TRA BẢNG ORDERS:")
        print("-" * 50)
        orders_query = "SELECT TOP 5 Id, UserId, OrderDate, TotalAmount, Status FROM Orders"
        orders_df = pd.read_sql(orders_query, connection)
        print(f"Số đơn hàng (top 5): {len(orders_df)}")
        for _, row in orders_df.iterrows():
            print(f"  {row['Id']}: {row['OrderDate']} - ${row['TotalAmount']} - {row['Status']}")
        
        connection.close()
        print("\n✅ Hoàn thành kiểm tra database!")
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    main()
