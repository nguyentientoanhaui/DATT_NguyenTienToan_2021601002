import pyodbc
import pandas as pd

def connect_to_database():
    """
    Kết nối đến database SQL Server
    """
    try:
        # Connection string từ appsettings.json
        connection_string = (
            "Driver={ODBC Driver 17 for SQL Server};"
            "Server=localhost;"
            "Database=Shopping_Demo;"
            "Trusted_Connection=yes;"
        )
        
        connection = pyodbc.connect(connection_string)
        print("✅ Kết nối thành công đến database Shopping_Demo!")
        return connection
    except Exception as e:
        print(f"❌ Lỗi kết nối: {e}")
        return None

def show_database_info(connection):
    """
    Hiển thị thông tin cơ bản về database
    """
    print("\n" + "="*60)
    print("📊 THÔNG TIN DATABASE SHOPPING_DEMO")
    print("="*60)
    
    # Lấy danh sách tất cả các bảng
    query = """
    SELECT 
        TABLE_NAME,
        TABLE_SCHEMA
    FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_TYPE = 'BASE TABLE'
    ORDER BY TABLE_NAME
    """
    
    try:
        tables_df = pd.read_sql(query, connection)
        print(f"\n📋 Tổng số bảng: {len(tables_df)}")
        print("\nDanh sách các bảng:")
        for i, row in tables_df.iterrows():
            print(f"  {i+1}. {row['TABLE_NAME']} (Schema: {row['TABLE_SCHEMA']})")
        
        return tables_df
    except Exception as e:
        print(f"❌ Lỗi khi lấy danh sách bảng: {e}")
        return pd.DataFrame()

def show_table_structure(connection, table_name):
    """
    Hiển thị cấu trúc của một bảng cụ thể
    """
    print(f"\n🔍 CẤU TRÚC BẢNG: {table_name}")
    print("-" * 50)
    
    query = """
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        COLUMN_DEFAULT,
        CHARACTER_MAXIMUM_LENGTH
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = ?
    ORDER BY ORDINAL_POSITION
    """
    
    try:
        structure_df = pd.read_sql(query, connection, params=[table_name])
        
        if not structure_df.empty:
            print(f"{'Tên cột':<20} {'Kiểu dữ liệu':<15} {'NULL':<8} {'Mặc định':<15} {'Độ dài':<10}")
            print("-" * 70)
            
            for _, row in structure_df.iterrows():
                column_name = row['COLUMN_NAME']
                data_type = row['DATA_TYPE']
                is_nullable = row['IS_NULLABLE']
                default_value = str(row['COLUMN_DEFAULT']) if row['COLUMN_DEFAULT'] else 'NULL'
                max_length = str(row['CHARACTER_MAXIMUM_LENGTH']) if row['CHARACTER_MAXIMUM_LENGTH'] else '-'
                
                print(f"{column_name:<20} {data_type:<15} {is_nullable:<8} {default_value:<15} {max_length:<10}")
        else:
            print("❌ Không tìm thấy thông tin cấu trúc bảng")
            
    except Exception as e:
        print(f"❌ Lỗi khi lấy cấu trúc bảng: {e}")

def show_sample_data(connection, table_name, limit=3):
    """
    Hiển thị dữ liệu mẫu từ bảng
    """
    print(f"\n📄 DỮ LIỆU MẪU TỪ BẢNG: {table_name} (Top {limit} dòng)")
    print("-" * 50)
    
    query = f"SELECT TOP {limit} * FROM [{table_name}]"
    
    try:
        data_df = pd.read_sql(query, connection)
        
        if not data_df.empty:
            print(data_df.to_string(index=False))
        else:
            print("📭 Bảng không có dữ liệu")
            
    except Exception as e:
        print(f"❌ Lỗi khi lấy dữ liệu mẫu: {e}")

def show_table_relationships(connection):
    """
    Hiển thị các mối quan hệ giữa các bảng
    """
    print(f"\n🔗 MỐI QUAN HỆ GIỮA CÁC BẢNG")
    print("-" * 50)
    
    query = """
    SELECT 
        fk.TABLE_NAME as FK_TABLE,
        fk.COLUMN_NAME as FK_COLUMN,
        pk.TABLE_NAME as PK_TABLE,
        pk.COLUMN_NAME as PK_COLUMN
    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE fk
    INNER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc 
        ON fk.CONSTRAINT_NAME = rc.CONSTRAINT_NAME
    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE pk 
        ON rc.UNIQUE_CONSTRAINT_NAME = pk.CONSTRAINT_NAME
    WHERE fk.CONSTRAINT_NAME LIKE 'FK_%'
    ORDER BY fk.TABLE_NAME, fk.COLUMN_NAME
    """
    
    try:
        relationships_df = pd.read_sql(query, connection)
        
        if not relationships_df.empty:
            print(f"{'Bảng FK':<20} {'Cột FK':<20} {'Bảng PK':<20} {'Cột PK':<20}")
            print("-" * 80)
            
            for _, row in relationships_df.iterrows():
                fk_table = row['FK_TABLE']
                fk_column = row['FK_COLUMN']
                pk_table = row['PK_TABLE']
                pk_column = row['PK_COLUMN']
                
                print(f"{fk_table:<20} {fk_column:<20} {pk_table:<20} {pk_column:<20}")
        else:
            print("📭 Không tìm thấy mối quan hệ foreign key")
            
    except Exception as e:
        print(f"❌ Lỗi khi lấy thông tin mối quan hệ: {e}")

def main():
    """
    Hàm chính
    """
    print("🐍 PYTHON DATABASE READER - SHOPPING_DEMO")
    print("=" * 60)
    
    # Kết nối database
    connection = connect_to_database()
    if not connection:
        return
    
    try:
        # Hiển thị thông tin database
        tables_df = show_database_info(connection)
        
        if not tables_df.empty:
            # Hiển thị mối quan hệ
            show_table_relationships(connection)
            
            # Cho phép người dùng chọn bảng để xem chi tiết
            print(f"\n🔍 Bạn có muốn xem chi tiết bảng nào không?")
            print("Nhập số thứ tự bảng (hoặc 'all' để xem tất cả, 'q' để thoát):")
            
            while True:
                choice = input("Lựa chọn của bạn: ").strip().lower()
                
                if choice == 'q':
                    break
                elif choice == 'all':
                    # Hiển thị tất cả các bảng
                    for _, row in tables_df.iterrows():
                        table_name = row['TABLE_NAME']
                        show_table_structure(connection, table_name)
                        show_sample_data(connection, table_name)
                        print("\n" + "="*60)
                elif choice.isdigit():
                    index = int(choice) - 1
                    if 0 <= index < len(tables_df):
                        table_name = tables_df.iloc[index]['TABLE_NAME']
                        show_table_structure(connection, table_name)
                        show_sample_data(connection, table_name)
                    else:
                        print("❌ Số thứ tự không hợp lệ!")
                else:
                    print("❌ Lựa chọn không hợp lệ! Vui lòng nhập số hoặc 'all'/'q'")
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")
    
    finally:
        # Đóng kết nối
        connection.close()
        print("\n🔌 Đã đóng kết nối database")

if __name__ == "__main__":
    main()
