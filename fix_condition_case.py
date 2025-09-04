import pyodbc

def fix_condition_case():
    connection_string = "DRIVER={SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;"
    
    try:
        conn = pyodbc.connect(connection_string)
        cursor = conn.cursor()
        
        print("🔧 FIXING CONDITION COLUMN - CASE APPROACH:")
        print("=" * 50)
        
        # Sử dụng CASE statement để cập nhật
        case_query = """
        UPDATE Products 
        SET Condition = CASE 
            WHEN Condition = 'Xuất sắc' THEN 'Excellent'
            WHEN Condition = 'Rất tốt' THEN 'Very Good'
            WHEN Condition = 'Tốt' THEN 'Good'
            WHEN Condition = 'Khá' THEN 'Fair'
            WHEN Condition = 'Kém' THEN 'Poor'
            WHEN Condition = 'Mới' THEN 'New'
            WHEN Condition = 'Cổ điển' THEN 'Vintage'
            WHEN Condition = 'Đã sử dụng' THEN 'Pre-owned'
            WHEN Condition = 'Chưa đeo' THEN 'Unworn'
            WHEN Condition = 'Hoàn hảo' THEN 'Mint'
            ELSE Condition
        END
        """
        
        print("🔄 UPDATING WITH CASE STATEMENT...")
        cursor.execute(case_query)
        affected = cursor.rowcount
        conn.commit()
        
        print(f"✅ {affected} rows affected")
        
        # Kiểm tra kết quả
        cursor.execute("SELECT TOP 5 Id, Condition FROM Products ORDER BY Id")
        results = cursor.fetchall()
        
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
    fix_condition_case()
