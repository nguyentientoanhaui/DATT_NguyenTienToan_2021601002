import pyodbc
import pandas as pd
import json
from datetime import datetime

class DatabaseReader:
    def __init__(self):
        # Connection string for LocalDB
        self.conn_str = (
            "DRIVER={ODBC Driver 17 for SQL Server};"
            "SERVER=(localdb)\\MSSQLLocalDB;"
            "DATABASE=Shopping_Demo;"
            "Trusted_Connection=yes;"
        )
        self.connection = None
    
    def connect(self):
        """Kết nối đến database"""
        try:
            self.connection = pyodbc.connect(self.conn_str)
            print("✅ Kết nối database thành công!")
            return True
        except Exception as e:
            print(f"❌ Lỗi kết nối database: {e}")
            return False
    
    def get_table_info(self):
        """Lấy thông tin về tất cả các bảng trong database"""
        if not self.connection:
            return None
        
        query = """
        SELECT 
            t.TABLE_NAME,
            COUNT(c.COLUMN_NAME) as COLUMN_COUNT
        FROM INFORMATION_SCHEMA.TABLES t
        LEFT JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME
        WHERE t.TABLE_TYPE = 'BASE TABLE'
        GROUP BY t.TABLE_NAME
        ORDER BY t.TABLE_NAME
        """
        
        try:
            df = pd.read_sql(query, self.connection)
            print("\n📊 THÔNG TIN CÁC BẢNG TRONG DATABASE:")
            print("=" * 50)
            for _, row in df.iterrows():
                print(f"📋 {row['TABLE_NAME']}: {row['COLUMN_COUNT']} cột")
            return df
        except Exception as e:
            print(f"❌ Lỗi lấy thông tin bảng: {e}")
            return None
    
    def get_table_structure(self, table_name):
        """Lấy cấu trúc chi tiết của một bảng"""
        if not self.connection:
            return None
        
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
            df = pd.read_sql(query, self.connection, params=[table_name])
            print(f"\n🏗️  CẤU TRÚC BẢNG '{table_name}':")
            print("-" * 60)
            for _, row in df.iterrows():
                nullable = "NULL" if row['IS_NULLABLE'] == 'YES' else "NOT NULL"
                length = f"({row['CHARACTER_MAXIMUM_LENGTH']})" if row['CHARACTER_MAXIMUM_LENGTH'] else ""
                default = f" DEFAULT: {row['COLUMN_DEFAULT']}" if row['COLUMN_DEFAULT'] else ""
                print(f"  🔹 {row['COLUMN_NAME']}: {row['DATA_TYPE']}{length} {nullable}{default}")
            return df
        except Exception as e:
            print(f"❌ Lỗi lấy cấu trúc bảng {table_name}: {e}")
            return None
    
    def get_table_data(self, table_name, limit=10):
        """Lấy dữ liệu mẫu từ một bảng"""
        if not self.connection:
            return None
        
        query = f"SELECT TOP {limit} * FROM [{table_name}]"
        
        try:
            df = pd.read_sql(query, self.connection)
            print(f"\n📄 DỮ LIỆU MẪU BẢNG '{table_name}' (Top {limit}):")
            print("-" * 80)
            print(df.to_string(index=False, max_colwidth=50))
            return df
        except Exception as e:
            print(f"❌ Lỗi lấy dữ liệu bảng {table_name}: {e}")
            return None
    
    def get_record_counts(self):
        """Đếm số bản ghi trong mỗi bảng"""
        if not self.connection:
            return None
        
        # Lấy danh sách tất cả các bảng
        tables_query = """
        SELECT TABLE_NAME 
        FROM INFORMATION_SCHEMA.TABLES 
        WHERE TABLE_TYPE = 'BASE TABLE'
        ORDER BY TABLE_NAME
        """
        
        try:
            tables_df = pd.read_sql(tables_query, self.connection)
            counts = {}
            
            print("\n📊 SỐ LƯỢNG BẢN GHI TRONG MỖI BẢNG:")
            print("=" * 40)
            
            for _, row in tables_df.iterrows():
                table_name = row['TABLE_NAME']
                try:
                    count_query = f"SELECT COUNT(*) as count FROM [{table_name}]"
                    count_df = pd.read_sql(count_query, self.connection)
                    count = count_df.iloc[0]['count']
                    counts[table_name] = count
                    print(f"📈 {table_name}: {count:,} bản ghi")
                except Exception as e:
                    print(f"❌ Lỗi đếm bảng {table_name}: {e}")
                    counts[table_name] = 0
            
            return counts
        except Exception as e:
            print(f"❌ Lỗi đếm bản ghi: {e}")
            return None
    
    def analyze_products_table(self):
        """Phân tích chi tiết bảng Products"""
        if not self.connection:
            return None
        
        print("\n🔍 PHÂN TÍCH CHI TIẾT BẢNG PRODUCTS:")
        print("=" * 50)
        
        queries = {
            "Tổng số sản phẩm": "SELECT COUNT(*) as count FROM Products",
            "Sản phẩm đang hoạt động": "SELECT COUNT(*) as count FROM Products WHERE IsActive = 1",
            "Sản phẩm ngừng hoạt động": "SELECT COUNT(*) as count FROM Products WHERE IsActive = 0",
            "Sản phẩm có ảnh": "SELECT COUNT(*) as count FROM Products WHERE Image IS NOT NULL AND Image != ''",
            "Sản phẩm có ProductImages": """
                SELECT COUNT(DISTINCT p.Id) as count 
                FROM Products p 
                INNER JOIN ProductImages pi ON p.Id = pi.ProductId
            """,
            "Top 5 thương hiệu": """
                SELECT TOP 5 b.Name, COUNT(*) as count 
                FROM Products p 
                INNER JOIN Brands b ON p.BrandId = b.Id 
                GROUP BY b.Name 
                ORDER BY count DESC
            """,
            "Top 5 danh mục": """
                SELECT TOP 5 c.Name, COUNT(*) as count 
                FROM Products p 
                INNER JOIN Categories c ON p.CategoryId = c.Id 
                GROUP BY c.Name 
                ORDER BY count DESC
            """
        }
        
        try:
            for description, query in queries.items():
                df = pd.read_sql(query, self.connection)
                print(f"\n📊 {description}:")
                if len(df.columns) == 1:  # Single count query
                    print(f"   {df.iloc[0, 0]:,}")
                else:  # Multiple rows
                    for _, row in df.iterrows():
                        print(f"   🔹 {row.iloc[0]}: {row.iloc[1]:,}")
        except Exception as e:
            print(f"❌ Lỗi phân tích Products: {e}")
    
    def analyze_product_images(self):
        """Phân tích bảng ProductImages"""
        if not self.connection:
            return None
        
        print("\n🖼️  PHÂN TÍCH BẢNG PRODUCTIMAGES:")
        print("=" * 40)
        
        queries = {
            "Tổng số ảnh": "SELECT COUNT(*) as count FROM ProductImages",
            "Ảnh có URL": "SELECT COUNT(*) as count FROM ProductImages WHERE ImageUrl IS NOT NULL AND ImageUrl != ''",
            "Ảnh có ImageName": "SELECT COUNT(*) as count FROM ProductImages WHERE ImageName IS NOT NULL AND ImageName != ''",
            "Ảnh mặc định": "SELECT COUNT(*) as count FROM ProductImages WHERE IsDefault = 1",
            "Sản phẩm có nhiều ảnh nhất": """
                SELECT TOP 5 p.Name, COUNT(pi.Id) as image_count
                FROM Products p
                INNER JOIN ProductImages pi ON p.Id = pi.ProductId
                GROUP BY p.Id, p.Name
                ORDER BY image_count DESC
            """
        }
        
        try:
            for description, query in queries.items():
                df = pd.read_sql(query, self.connection)
                print(f"\n📊 {description}:")
                if len(df.columns) == 1:  # Single count query
                    print(f"   {df.iloc[0, 0]:,}")
                else:  # Multiple rows
                    for _, row in df.iterrows():
                        print(f"   🔹 {row.iloc[0]}: {row.iloc[1]:,}")
        except Exception as e:
            print(f"❌ Lỗi phân tích ProductImages: {e}")
    
    def export_summary_to_json(self, filename="database_summary.json"):
        """Xuất tóm tắt database ra file JSON"""
        if not self.connection:
            return None
        
        try:
            summary = {
                "generated_at": datetime.now().isoformat(),
                "database_name": "Shopping_Demo",
                "tables": {},
                "analysis": {}
            }
            
            # Lấy thông tin bảng
            tables_info = self.get_table_info()
            if tables_info is not None:
                summary["tables"] = tables_info.to_dict('records')
            
            # Lấy số lượng bản ghi
            record_counts = self.get_record_counts()
            if record_counts:
                summary["record_counts"] = record_counts
            
            # Lưu file
            with open(filename, 'w', encoding='utf-8') as f:
                json.dump(summary, f, ensure_ascii=False, indent=2)
            
            print(f"\n💾 Đã xuất tóm tắt database ra file: {filename}")
            return summary
        except Exception as e:
            print(f"❌ Lỗi xuất file JSON: {e}")
            return None
    
    def close(self):
        """Đóng kết nối database"""
        if self.connection:
            self.connection.close()
            print("\n🔒 Đã đóng kết nối database")

def main():
    """Hàm chính để chạy phân tích database"""
    print("🚀 BẮT ĐẦU PHÂN TÍCH DATABASE SHOPPING_DEMO")
    print("=" * 60)
    
    # Khởi tạo database reader
    db_reader = DatabaseReader()
    
    # Kết nối database
    if not db_reader.connect():
        return
    
    try:
        # 1. Lấy thông tin tổng quan các bảng
        db_reader.get_table_info()
        
        # 2. Đếm số bản ghi trong mỗi bảng
        db_reader.get_record_counts()
        
        # 3. Phân tích chi tiết bảng Products
        db_reader.analyze_products_table()
        
        # 4. Phân tích bảng ProductImages
        db_reader.analyze_product_images()
        
        # 5. Xem cấu trúc một số bảng quan trọng
        important_tables = ['Products', 'ProductImages', 'Categories', 'Brands']
        for table in important_tables:
            db_reader.get_table_structure(table)
        
        # 6. Xem dữ liệu mẫu
        print("\n" + "="*60)
        print("📄 DỮ LIỆU MẪU:")
        for table in ['Products', 'ProductImages', 'Categories', 'Brands']:
            db_reader.get_table_data(table, limit=5)
        
        # 7. Xuất tóm tắt ra file JSON
        db_reader.export_summary_to_json()
        
    finally:
        # Đóng kết nối
        db_reader.close()
    
    print("\n✅ HOÀN THÀNH PHÂN TÍCH DATABASE!")

if __name__ == "__main__":
    main()
