import pyodbc

def fix_condition_unicode():
    connection_string = "DRIVER={SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;"
    
    try:
        conn = pyodbc.connect(connection_string)
        cursor = conn.cursor()
        
        print("🔧 FIXING CONDITION COLUMN - UNICODE APPROACH:")
        print("=" * 50)
        
        # Sử dụng Unicode escape sequences
        unicode_queries = [
            "UPDATE Products SET Condition = 'Excellent' WHERE Condition = N'Xuất sắc'",
            "UPDATE Products SET Condition = 'Very Good' WHERE Condition = N'Rất tốt'",
            "UPDATE Products SET Condition = 'Good' WHERE Condition = N'Tốt'",
            "UPDATE Products SET Condition = 'Fair' WHERE Condition = N'Khá'",
            "UPDATE Products SET Condition = 'Poor' WHERE Condition = N'Kém'",
            "UPDATE Products SET Condition = 'New' WHERE Condition = N'Mới'",
            "UPDATE Products SET Condition = 'Vintage' WHERE Condition = N'Cổ điển'",
            "UPDATE Products SET Condition = 'Pre-owned' WHERE Condition = N'Đã sử dụng'",
            "UPDATE Products SET Condition = 'Unworn' WHERE Condition = N'Chưa đeo'",
            "UPDATE Products SET Condition = 'Mint' WHERE Condition = N'Hoàn hảo'"
        ]
        
        total_affected = 0
        for i, query in enumerate(unicode_queries):
            try:
                cursor.execute(query)
                affected = cursor.rowcount
                total_affected += affected
                if affected > 0:
                    print(f"  ✅ Query {i+1}: {affected} rows affected")
            except Exception as e:
                print(f"  ⚠️ Query {i+1} error: {e}")
        
        conn.commit()
        
        # Kiểm tra kết quả
        cursor.execute("SELECT TOP 5 Id, Condition FROM Products ORDER BY Id")
        results = cursor.fetchall()
        
        print(f"\n✅ TỔNG KẾT: {total_affected} thay đổi được thực hiện")
        print("\n📦 KẾT QUẢ CUỐI CÙNG:")
        for row in results:
            print(f"  ID: {row[0]} - Condition: {row[1]}")
        
        # Thống kê mới
        cursor.execute("SELECT Condition, COUNT(*) as Count FROM Products GROUP BY Condition ORDER BY Count DESC")
        stats = cursor.fetchall()
        
        print("\n📊 THỐNG KÊ CONDITION SAU KHI CẬP NHẬT:")
        for stat in stats:
            print(f"  {stat[0]}: {stat[1]} sản phẩm")
        
        cursor.close()
        conn.close()
        print("\n🎉 KHÔI PHỤC CONDITION THÀNH CÔNG!")
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")

if __name__ == "__main__":
    fix_condition_unicode()
