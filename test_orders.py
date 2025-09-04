import pyodbc
import json

# Connection string from appsettings.json
connection_string = "Server=DESKTOP-C3O3FBL\\SQLEXPRESS;Database=Shopping_Demo;Trusted_Connection=True;TrustServerCertificate=True;"

try:
    # Connect to database
    conn = pyodbc.connect(connection_string)
    cursor = conn.cursor()
    
    print("✅ Connected to database successfully!")
    
    # Check Orders table
    cursor.execute("SELECT COUNT(*) FROM Orders")
    total_orders = cursor.fetchone()[0]
    print(f"📦 Total Orders: {total_orders}")
    
    # Check OrderDetails table
    cursor.execute("SELECT COUNT(*) FROM OrderDetails")
    total_order_details = cursor.fetchone()[0]
    print(f"📄 Total Order Details: {total_order_details}")
    
    # Check Orders by Status
    cursor.execute("SELECT Status, COUNT(*) FROM Orders GROUP BY Status")
    status_counts = cursor.fetchall()
    print("🔍 Orders by Status:")
    for status, count in status_counts:
        print(f"   Status {status}: {count} orders")
    
    # Check some sample orders
    cursor.execute("SELECT TOP 5 OrderCode, Status, CreatedDate FROM Orders ORDER BY CreatedDate DESC")
    sample_orders = cursor.fetchall()
    print("📋 Sample Orders:")
    for order in sample_orders:
        print(f"   {order[0]} - Status: {order[1]} - Date: {order[2]}")
    
    # Check some sample order details
    cursor.execute("SELECT TOP 5 od.OrderCode, od.ProductId, od.Price, od.Quantity, o.Status FROM OrderDetails od JOIN Orders o ON od.OrderCode = o.OrderCode")
    sample_details = cursor.fetchall()
    print("📋 Sample Order Details:")
    for detail in sample_details:
        print(f"   Order: {detail[0]} - Product: {detail[1]} - Price: {detail[2]} - Qty: {detail[3]} - Status: {detail[4]}")
    
    conn.close()
    print("✅ Database connection closed")
    
except Exception as e:
    print(f"❌ Error: {e}")
