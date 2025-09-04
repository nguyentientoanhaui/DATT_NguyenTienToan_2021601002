import sys
import traceback

def main():
    try:
        print("🔍 Kiểm tra imports...")
        
        # Kiểm tra pyodbc
        try:
            import pyodbc
            print("✅ pyodbc import thành công")
        except ImportError as e:
            print(f"❌ Lỗi import pyodbc: {e}")
            print("💡 Hãy chạy: pip install pyodbc")
            return
        
        # Kiểm tra pandas
        try:
            import pandas as pd
            print("✅ pandas import thành công")
        except ImportError as e:
            print(f"❌ Lỗi import pandas: {e}")
            print("💡 Hãy chạy: pip install pandas")
            return
        
        # Kiểm tra numpy
        try:
            import numpy as np
            print("✅ numpy import thành công")
        except ImportError as e:
            print(f"❌ Lỗi import numpy: {e}")
            print("💡 Hãy chạy: pip install numpy")
            return
        
        print("\n🔍 Kiểm tra kết nối database...")
        
        # Connection string
        connection_string = "DRIVER={SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;"
        
        try:
            conn = pyodbc.connect(connection_string)
            print("✅ Kết nối database thành công!")
            
            # Kiểm tra dữ liệu
            cursor = conn.cursor()
            cursor.execute("SELECT COUNT(*) FROM Products")
            count = cursor.fetchone()[0]
            print(f"📊 Số sản phẩm trong database: {count}")
            
            # Lấy mẫu dữ liệu
            cursor.execute("SELECT TOP 1 Id, Name, Condition, Gender, Certificate, WarrantyInfo, Price FROM Products")
            sample = cursor.fetchone()
            if sample:
                print("📦 Mẫu dữ liệu:")
                print(f"  ID: {sample[0]}")
                print(f"  Name: {sample[1]}")
                print(f"  Condition: {sample[2]}")
                print(f"  Gender: {sample[3]}")
                print(f"  Certificate: {sample[4]}")
                print(f"  WarrantyInfo: {sample[5]}")
                print(f"  Price: {sample[6]}")
            
            cursor.close()
            conn.close()
            print("✅ Kiểm tra hoàn tất!")
            
        except Exception as e:
            print(f"❌ Lỗi kết nối database: {e}")
            print(f"📋 Chi tiết lỗi: {traceback.format_exc()}")
            return
        
        print("\n🚀 Bắt đầu việt hóa...")
        
        # Import và chạy việt hóa
        from vietnamize_data import DatabaseVietnamizer
        vietnamizer = DatabaseVietnamizer(connection_string)
        success = vietnamizer.run_vietnamization()
        
        if success:
            print("\n🎯 KẾT QUẢ: Việt hóa thành công!")
        else:
            print("\n💥 KẾT QUẢ: Việt hóa thất bại!")
            
    except Exception as e:
        print(f"❌ Lỗi không xác định: {e}")
        print(f"📋 Chi tiết lỗi: {traceback.format_exc()}")

if __name__ == "__main__":
    main()
