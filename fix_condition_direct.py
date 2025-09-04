import pyodbc

def fix_condition_direct():
    connection_string = "DRIVER={SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;"
    
    try:
        conn = pyodbc.connect(connection_string)
        cursor = conn.cursor()
        
        print("🔧 FIXING CONDITION COLUMN - DIRECT APPROACH:")
        print("=" * 50)
        
        # Kiểm tra dữ liệu hiện tại
        cursor.execute("SELECT DISTINCT Condition FROM Products ORDER BY Condition")
        current_conditions = cursor.fetchall()
        print("📋 CURRENT CONDITIONS IN DATABASE:")
        for condition in current_conditions:
            print(f"  '{condition[0]}'")
        
        # Thực hiện cập nhật trực tiếp
        print("\n🔄 UPDATING CONDITIONS...")
        
        # Sử dụng REPLACE để tránh vấn đề encoding
        update_queries = [
            "UPDATE Products SET Condition = REPLACE(Condition, 'Xuất sắc', 'Excellent')",
            "UPDATE Products SET Condition = REPLACE(Condition, 'Rất tốt', 'Very Good')",
            "UPDATE Products SET Condition = REPLACE(Condition, 'Tốt', 'Good')",
            "UPDATE Products SET Condition = REPLACE(Condition, 'Khá', 'Fair')",
            "UPDATE Products SET Condition = REPLACE(Condition, 'Kém', 'Poor')",
            "UPDATE Products SET Condition = REPLACE(Condition, 'Mới', 'New')",
            "UPDATE Products SET Condition = REPLACE(Condition, 'Cổ điển', 'Vintage')",
            "UPDATE Products SET Condition = REPLACE(Condition, 'Đã sử dụng', 'Pre-owned')",
            "UPDATE Products SET Condition = REPLACE(Condition, 'Chưa đeo', 'Unworn')",
            "UPDATE Products SET Condition = REPLACE(Condition, 'Hoàn hảo', 'Mint')"
        ]
        
        total_affected = 0
        for i, query in enumerate(update_queries):
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
    fix_condition_direct()
