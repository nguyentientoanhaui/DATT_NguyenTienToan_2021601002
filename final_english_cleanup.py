import pyodbc

def final_cleanup():
    connection_string = "DRIVER={SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;"
    
    try:
        conn = pyodbc.connect(connection_string)
        cursor = conn.cursor()
        
        print("🔄 FINAL CLEANUP - XÓA BỎ TIẾNG VIỆT CÒN LẠI:")
        print("=" * 50)
        
        # Các câu lệnh SQL để xử lý triệt để
        cleanup_queries = [
            "UPDATE Products SET Condition = 'Excellent' WHERE Condition = 'Xuất sắc'",
            "UPDATE Products SET Gender = 'Men' WHERE Gender = 'Nam'",
            "UPDATE Products SET Description = REPLACE(Description, 'Đã sử dụng', 'Pre-owned')",
            "UPDATE Products SET Description = REPLACE(Description, 'di sản', 'heritage')",
            "UPDATE Products SET Description = REPLACE(Description, 'cổ điển', 'classic')",
            "UPDATE Products SET Description = REPLACE(Description, 'mới nhất', 'latest')",
            "UPDATE Products SET Description = REPLACE(Description, 'tuyệt vời', 'excellent')",
            "UPDATE Products SET Description = REPLACE(Description, 'chất lượng', 'quality')",
            "UPDATE Products SET Description = REPLACE(Description, 'thép không gỉ', 'stainless steel')",
            "UPDATE Products SET Description = REPLACE(Description, 'bộ máy tự động', 'automatic movement')"
        ]
        
        for i, query in enumerate(cleanup_queries):
            try:
                cursor.execute(query)
                print(f"  ✅ Query {i+1}: {cursor.rowcount} rows affected")
            except Exception as e:
                print(f"  ⚠️ Query {i+1} error: {e}")
        
        conn.commit()
        
        # Kiểm tra kết quả
        cursor.execute("SELECT TOP 2 Id, Condition, Gender, SUBSTRING(Description, 1, 100) as Desc_Short FROM Products ORDER BY Id")
        results = cursor.fetchall()
        
        print("\n✅ KẾT QUẢ SAU KHI DỌN DẸP:")
        for row in results:
            print(f"  ID: {row[0]}, Condition: {row[1]}, Gender: {row[2]}")
            print(f"  Description: {row[3]}...")
            print("  " + "-" * 40)
        
        cursor.close()
        conn.close()
        print("\n🎉 DỌN DẸP HOÀN THÀNH!")
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")

if __name__ == "__main__":
    final_cleanup()
