import re
import os
import json
import pyodbc
from datetime import datetime

def insert_vietnamese_descriptions():
    """Chèn các mô tả sản phẩm đã được Việt hóa vào database"""
    print("🗄️ BẮT ĐẦU CHÈN MÔ TẢ VIỆT HÓA VÀO DATABASE")
    print("=" * 60)
    
    # Kiểm tra file
    input_file = "Mô_tả_sản_phẩm_Tiếng_Việt_Hoàn_Chỉnh.txt"
    if not os.path.exists(input_file):
        print(f"❌ Không tìm thấy file {input_file}")
        return False
    
    try:
        # Đọc file
        print(f"📖 Đang đọc file {input_file}...")
        with open(input_file, 'r', encoding='utf-8') as file:
            lines = file.readlines()
        
        print(f"✅ Đã đọc file! Tổng số dòng: {len(lines)}")
        
        # Lọc ra các mô tả sản phẩm (bỏ qua header)
        descriptions = []
        for line in lines:
            line = line.strip()
            if line and line != "Chuyển sang tiếng việt" and line != "Description":
                descriptions.append(line)
        
        print(f"📝 Tìm thấy {len(descriptions)} mô tả sản phẩm")
        
        # Đọc connection string từ appsettings.json
        print(f"\n🔗 ĐỌC CẤU HÌNH DATABASE...")
        print("=" * 50)
        
        appsettings_file = "appsettings.json"
        if not os.path.exists(appsettings_file):
            print(f"❌ Không tìm thấy file {appsettings_file}")
            return False
        
        with open(appsettings_file, 'r', encoding='utf-8') as file:
            config = json.load(file)
        
        connection_string = config.get('ConnectionStrings', {}).get('ConnectedDb')
        if not connection_string:
            print("❌ Không tìm thấy connection string trong appsettings.json")
            return False
        
        print(f"✅ Đã đọc connection string")
        
        # Kết nối SQL Server
        print(f"\n🔗 KẾT NỐI SQL SERVER...")
        print("=" * 50)
        
        try:
            # Sử dụng connection string trực tiếp thay vì DSN
            conn = pyodbc.connect('DRIVER={ODBC Driver 17 for SQL Server};' + connection_string.replace('Server=', 'SERVER=').replace('Database=', 'DATABASE=').replace('Trusted_Connection=', 'Trusted_Connection=').replace('TrustServerCertificate=', 'TrustServerCertificate='))
            cursor = conn.cursor()
            print("✅ Kết nối SQL Server thành công!")
        except Exception as e:
            print(f"❌ Lỗi kết nối SQL Server: {e}")
            print("🔄 Thử kết nối với driver khác...")
            try:
                # Thử với SQL Server Native Client
                conn = pyodbc.connect('DRIVER={SQL Server Native Client 11.0};' + connection_string.replace('Server=', 'SERVER=').replace('Database=', 'DATABASE=').replace('Trusted_Connection=', 'Trusted_Connection=').replace('TrustServerCertificate=', 'TrustServerCertificate='))
                cursor = conn.cursor()
                print("✅ Kết nối SQL Server thành công với Native Client!")
            except Exception as e2:
                print(f"❌ Lỗi kết nối với Native Client: {e2}")
                print("🔄 Thử kết nối với SQL Server driver...")
                try:
                    # Thử với SQL Server driver
                    conn = pyodbc.connect('DRIVER={SQL Server};' + connection_string.replace('Server=', 'SERVER=').replace('Database=', 'DATABASE=').replace('Trusted_Connection=', 'Trusted_Connection=').replace('TrustServerCertificate=', 'TrustServerCertificate='))
                    cursor = conn.cursor()
                    print("✅ Kết nối SQL Server thành công với SQL Server driver!")
                except Exception as e3:
                    print(f"❌ Lỗi kết nối với SQL Server driver: {e3}")
                    return False
        
        # Kiểm tra bảng Products
        print(f"\n🔍 KIỂM TRA BẢNG PRODUCTS...")
        print("=" * 50)
        
        cursor.execute("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products'")
        if not cursor.fetchone():
            print("❌ Không tìm thấy bảng Products!")
            return False
        
        print("✅ Tìm thấy bảng Products")
        
        # Kiểm tra cấu trúc bảng
        cursor.execute("SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' ORDER BY ORDINAL_POSITION")
        columns = cursor.fetchall()
        print(f"📋 Cấu trúc bảng Products:")
        for col in columns:
            print(f"   - {col[0]} ({col[1]})")
        
        # Kiểm tra số lượng sản phẩm hiện tại
        cursor.execute("SELECT COUNT(*) FROM Products")
        current_count = cursor.fetchone()[0]
        print(f"📊 Số sản phẩm hiện tại: {current_count}")
        
        # Kiểm tra xem có cột Description không
        description_column = None
        for col in columns:
            if 'description' in col[0].lower():
                description_column = col[0]
                break
        
        if not description_column:
            print("❌ Không tìm thấy cột Description!")
            return False
        
        print(f"✅ Tìm thấy cột: {description_column}")
        
        # Lấy danh sách ID sản phẩm
        cursor.execute(f"SELECT Id FROM Products ORDER BY Id")
        product_ids = [row[0] for row in cursor.fetchall()]
        
        print(f"📋 Tìm thấy {len(product_ids)} ID sản phẩm")
        
        # Kiểm tra số lượng
        if len(descriptions) != len(product_ids):
            print(f"⚠️ CẢNH BÁO: Số lượng mô tả ({len(descriptions)}) khác với số sản phẩm ({len(product_ids)})")
            print("   Sẽ chỉ cập nhật cho các sản phẩm có mô tả tương ứng")
        
        # Bắt đầu cập nhật
        print(f"\n🔄 BẮT ĐẦU CẬP NHẬT MÔ TẢ...")
        print("=" * 60)
        
        updated_count = 0
        min_count = min(len(descriptions), len(product_ids))
        
        for i in range(min_count):
            product_id = product_ids[i]
            description = descriptions[i]
            
            try:
                # Cập nhật mô tả
                cursor.execute(f"UPDATE Products SET {description_column} = ? WHERE Id = ?", 
                             (description, product_id))
                
                print(f"  [{i+1}/{min_count}] Đã cập nhật sản phẩm ID {product_id}")
                updated_count += 1
                
                # Hiển thị tiến độ
                if (i + 1) % 50 == 0:
                    print(f"📊 Tiến độ: {i+1}/{min_count} ({((i+1)/min_count*100):.1f}%)")
                
            except Exception as e:
                print(f"❌ Lỗi cập nhật sản phẩm ID {product_id}: {e}")
        
        # Commit thay đổi
        print(f"\n💾 LƯU THAY ĐỔI VÀO DATABASE...")
        print("=" * 50)
        
        conn.commit()
        print("✅ Đã lưu thay đổi thành công!")
        
        # Thống kê kết quả
        print(f"\n📊 THỐNG KÊ KẾT QUẢ:")
        print("=" * 60)
        print(f"✅ Tổng số mô tả: {len(descriptions)}")
        print(f"✅ Tổng số sản phẩm: {len(product_ids)}")
        print(f"✅ Số sản phẩm đã cập nhật: {updated_count}")
        
        # Hiển thị mẫu kết quả
        print(f"\n📋 MẪU KẾT QUẢ:")
        print("=" * 60)
        
        for i in range(min(3, min_count)):
            product_id = product_ids[i]
            description = descriptions[i]
            
            print(f"\n🔸 SẢN PHẨM {i+1} (ID: {product_id}):")
            print(f"   Mô tả: {description[:200]}...")
            print("   " + "-" * 40)
        
        # Đóng kết nối
        conn.close()
        
        return True
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")
        return False

def main():
    print("🎯 SCRIPT CHÈN MÔ TẢ VIỆT HÓA VÀO DATABASE")
    print("=" * 60)
    
    # Thực hiện chèn dữ liệu
    success = insert_vietnamese_descriptions()
    
    if success:
        print("\n🎉 HOÀN THÀNH!")
        print("=" * 60)
        print("✅ Đã chèn mô tả Việt hóa vào database thành công")
        print("✅ Dữ liệu đã sẵn sàng sử dụng")
        
    else:
        print("\n💥 XỬ LÝ THẤT BẠI!")

if __name__ == "__main__":
    main()
