import pyodbc

def quick_restore():
    connection_string = "DRIVER={SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;"
    
    try:
        conn = pyodbc.connect(connection_string)
        cursor = conn.cursor()
        
        print("🚀 QUICK ENGLISH RESTORE - KHÔI PHỤC NHANH:")
        print("=" * 50)
        
        # Các câu lệnh SQL để khôi phục nhanh
        quick_queries = [
            # Khôi phục CaseMaterial
            "UPDATE Products SET CaseMaterial = 'Stainless Steel' WHERE CaseMaterial = 'Thép không gỉ'",
            "UPDATE Products SET CaseMaterial = 'Titanium' WHERE CaseMaterial = 'Titan'",
            "UPDATE Products SET CaseMaterial = 'Ceramic' WHERE CaseMaterial = 'Gốm'",
            "UPDATE Products SET CaseMaterial = 'Gold' WHERE CaseMaterial = 'Vàng'",
            "UPDATE Products SET CaseMaterial = 'Platinum' WHERE CaseMaterial = 'Bạch kim'",
            "UPDATE Products SET CaseMaterial = 'Bronze' WHERE CaseMaterial = 'Đồng'",
            
            # Khôi phục MovementType
            "UPDATE Products SET MovementType = 'Automatic' WHERE MovementType = 'Tự động'",
            "UPDATE Products SET MovementType = 'Quartz' WHERE MovementType = 'Thạch anh'",
            "UPDATE Products SET MovementType = 'Manual' WHERE MovementType = 'Lên dây tay'",
            
            # Khôi phục Complication
            "UPDATE Products SET Complication = 'Chronograph' WHERE Complication = 'Bấm giờ'",
            "UPDATE Products SET Complication = 'Date' WHERE Complication = 'Ngày'",
            "UPDATE Products SET Complication = 'Annual Calendar' WHERE Complication = 'Lịch năm'",
            "UPDATE Products SET Complication = 'Moon Phase' WHERE Complication = 'Moonphase'",
            "UPDATE Products SET Complication = 'None' WHERE Complication = 'Không'",
            
            # Khôi phục BraceletMaterial
            "UPDATE Products SET BraceletMaterial = 'Stainless Steel' WHERE BraceletMaterial = 'Thép không gỉ'",
            "UPDATE Products SET BraceletMaterial = 'Leather' WHERE BraceletMaterial = 'Da'",
            "UPDATE Products SET BraceletMaterial = 'Rubber' WHERE BraceletMaterial = 'Cao su'",
            "UPDATE Products SET BraceletMaterial = 'Titanium' WHERE BraceletMaterial = 'Titan'",
            "UPDATE Products SET BraceletMaterial = 'Gold' WHERE BraceletMaterial = 'Vàng'",
            "UPDATE Products SET BraceletMaterial = 'Platinum' WHERE BraceletMaterial = 'Bạch kim'",
            "UPDATE Products SET BraceletMaterial = 'Rose Gold' WHERE BraceletMaterial = 'Rose Gold'",
            "UPDATE Products SET BraceletMaterial = 'Yellow Gold' WHERE BraceletMaterial = 'Yellow Gold'",
            "UPDATE Products SET BraceletMaterial = 'White Gold' WHERE BraceletMaterial = 'White Gold'",
            "UPDATE Products SET BraceletMaterial = 'Two-Tone' WHERE BraceletMaterial = 'Two-Tone'",
            "UPDATE Products SET BraceletMaterial = 'Fabric/Canvas' WHERE BraceletMaterial = 'Fabric/Canvas'",
            "UPDATE Products SET BraceletMaterial = 'PVD' WHERE BraceletMaterial = 'PVD'"
        ]
        
        total_affected = 0
        for i, query in enumerate(quick_queries):
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
        cursor.execute("SELECT TOP 3 Id, CaseMaterial, MovementType, Complication, BraceletMaterial FROM Products ORDER BY Id")
        results = cursor.fetchall()
        
        print(f"\n✅ TỔNG KẾT: {total_affected} thay đổi được thực hiện")
        print("\n📦 KẾT QUẢ CUỐI CÙNG:")
        for row in results:
            print(f"  ID: {row[0]}")
            print(f"  CaseMaterial: {row[1]}")
            print(f"  MovementType: {row[2]}")
            print(f"  Complication: {row[3]}")
            print(f"  BraceletMaterial: {row[4]}")
            print("  " + "-" * 40)
        
        cursor.close()
        conn.close()
        print("\n🎉 KHÔI PHỤC NHANH THÀNH CÔNG!")
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")

if __name__ == "__main__":
    quick_restore()
