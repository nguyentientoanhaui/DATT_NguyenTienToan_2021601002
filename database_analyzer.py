import pyodbc
import pandas as pd
from tabulate import tabulate
import json
from datetime import datetime
import os

class DatabaseAnalyzer:
    def __init__(self, connection_string):
        """
        Khởi tạo Database Analyzer với connection string
        """
        self.connection_string = connection_string
        self.connection = None
        
    def connect(self):
        """
        Kết nối đến database
        """
        try:
            self.connection = pyodbc.connect(self.connection_string)
            print("✅ Kết nối database thành công!")
            return True
        except Exception as e:
            print(f"❌ Lỗi kết nối database: {e}")
            return False
    
    def disconnect(self):
        """
        Đóng kết nối database
        """
        if self.connection:
            self.connection.close()
            print("🔌 Đã đóng kết nối database")
    
    def get_all_tables(self):
        """
        Lấy danh sách tất cả các bảng trong database
        """
        query = """
        SELECT 
            TABLE_SCHEMA,
            TABLE_NAME,
            TABLE_TYPE
        FROM INFORMATION_SCHEMA.TABLES 
        WHERE TABLE_TYPE = 'BASE TABLE'
        ORDER BY TABLE_SCHEMA, TABLE_NAME
        """
        
        try:
            df = pd.read_sql(query, self.connection)
            return df
        except Exception as e:
            print(f"❌ Lỗi khi lấy danh sách bảng: {e}")
            return pd.DataFrame()
    
    def get_table_structure(self, table_name):
        """
        Lấy cấu trúc của một bảng cụ thể
        """
        query = """
        SELECT 
            COLUMN_NAME,
            DATA_TYPE,
            IS_NULLABLE,
            COLUMN_DEFAULT,
            CHARACTER_MAXIMUM_LENGTH,
            NUMERIC_PRECISION,
            NUMERIC_SCALE
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = ?
        ORDER BY ORDINAL_POSITION
        """
        
        try:
            df = pd.read_sql(query, self.connection, params=[table_name])
            return df
        except Exception as e:
            print(f"❌ Lỗi khi lấy cấu trúc bảng {table_name}: {e}")
            return pd.DataFrame()
    
    def get_table_relationships(self):
        """
        Lấy thông tin về các mối quan hệ giữa các bảng
        """
        query = """
        SELECT 
            fk.TABLE_NAME as FK_TABLE,
            fk.COLUMN_NAME as FK_COLUMN,
            pk.TABLE_NAME as PK_TABLE,
            pk.COLUMN_NAME as PK_COLUMN,
            fk.CONSTRAINT_NAME
        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE fk
        INNER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc 
            ON fk.CONSTRAINT_NAME = rc.CONSTRAINT_NAME
        INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE pk 
            ON rc.UNIQUE_CONSTRAINT_NAME = pk.CONSTRAINT_NAME
        WHERE fk.CONSTRAINT_NAME LIKE 'FK_%'
        ORDER BY fk.TABLE_NAME, fk.COLUMN_NAME
        """
        
        try:
            df = pd.read_sql(query, self.connection)
            return df
        except Exception as e:
            print(f"❌ Lỗi khi lấy thông tin mối quan hệ: {e}")
            return pd.DataFrame()
    
    def get_table_row_count(self, table_name):
        """
        Lấy số lượng dòng trong bảng
        """
        query = f"SELECT COUNT(*) as row_count FROM [{table_name}]"
        
        try:
            cursor = self.connection.cursor()
            cursor.execute(query)
            result = cursor.fetchone()
            return result[0] if result else 0
        except Exception as e:
            print(f"❌ Lỗi khi đếm dòng trong bảng {table_name}: {e}")
            return 0
    
    def get_sample_data(self, table_name, limit=5):
        """
        Lấy dữ liệu mẫu từ bảng
        """
        query = f"SELECT TOP {limit} * FROM [{table_name}]"
        
        try:
            df = pd.read_sql(query, self.connection)
            return df
        except Exception as e:
            print(f"❌ Lỗi khi lấy dữ liệu mẫu từ bảng {table_name}: {e}")
            return pd.DataFrame()
    
    def analyze_database(self):
        """
        Phân tích toàn bộ database và tạo báo cáo
        """
        print("🔍 Bắt đầu phân tích database...")
        print("=" * 60)
        
        # 1. Lấy danh sách bảng
        print("📋 Danh sách các bảng trong database:")
        tables_df = self.get_all_tables()
        if not tables_df.empty:
            print(tabulate(tables_df, headers='keys', tablefmt='grid'))
            print()
        
        # 2. Phân tích từng bảng
        analysis_results = {}
        
        for _, row in tables_df.iterrows():
            table_name = row['TABLE_NAME']
            print(f"🔍 Đang phân tích bảng: {table_name}")
            
            # Lấy cấu trúc bảng
            structure = self.get_table_structure(table_name)
            
            # Lấy số lượng dòng
            row_count = self.get_table_row_count(table_name)
            
            # Lấy dữ liệu mẫu
            sample_data = self.get_sample_data(table_name, 3)
            
            analysis_results[table_name] = {
                'structure': structure,
                'row_count': row_count,
                'sample_data': sample_data
            }
            
            print(f"   📊 Số dòng: {row_count}")
            print(f"   📋 Số cột: {len(structure)}")
            print()
        
        # 3. Lấy thông tin mối quan hệ
        print("🔗 Phân tích mối quan hệ giữa các bảng:")
        relationships = self.get_table_relationships()
        if not relationships.empty:
            print(tabulate(relationships, headers='keys', tablefmt='grid'))
        else:
            print("   Không tìm thấy mối quan hệ foreign key")
        print()
        
        return analysis_results
    
    def generate_report(self, analysis_results):
        """
        Tạo báo cáo chi tiết
        """
        report = {
            'timestamp': datetime.now().isoformat(),
            'database_name': 'Shopping_Demo',
            'total_tables': len(analysis_results),
            'tables': {}
        }
        
        for table_name, data in analysis_results.items():
            report['tables'][table_name] = {
                'row_count': data['row_count'],
                'column_count': len(data['structure']),
                'columns': data['structure'].to_dict('records') if not data['structure'].empty else [],
                'sample_data': data['sample_data'].to_dict('records') if not data['sample_data'].empty else []
            }
        
        # Lưu báo cáo ra file JSON
        with open('database_report.json', 'w', encoding='utf-8') as f:
            json.dump(report, f, ensure_ascii=False, indent=2)
        
        print("📄 Báo cáo đã được lưu vào file: database_report.json")
        
        # Tạo báo cáo HTML
        self.generate_html_report(report)
    
    def generate_html_report(self, report):
        """
        Tạo báo cáo HTML
        """
        html_content = f"""
        <!DOCTYPE html>
        <html lang="vi">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Báo cáo Database Shopping_Demo</title>
            <style>
                body {{ font-family: Arial, sans-serif; margin: 20px; }}
                .header {{ background-color: #f0f0f0; padding: 20px; border-radius: 5px; }}
                .table-section {{ margin: 20px 0; }}
                .table-name {{ background-color: #e0e0e0; padding: 10px; font-weight: bold; }}
                .structure-table {{ width: 100%; border-collapse: collapse; margin: 10px 0; }}
                .structure-table th, .structure-table td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                .structure-table th {{ background-color: #f2f2f2; }}
                .sample-data {{ background-color: #f9f9f9; padding: 10px; margin: 10px 0; }}
                .stats {{ background-color: #e8f5e8; padding: 10px; margin: 10px 0; border-radius: 5px; }}
            </style>
        </head>
        <body>
            <div class="header">
                <h1>📊 Báo cáo Database Shopping_Demo</h1>
                <p><strong>Thời gian tạo:</strong> {report['timestamp']}</p>
                <p><strong>Tổng số bảng:</strong> {report['total_tables']}</p>
            </div>
        """
        
        for table_name, table_data in report['tables'].items():
            html_content += f"""
            <div class="table-section">
                <div class="table-name">📋 Bảng: {table_name}</div>
                <div class="stats">
                    <strong>Số dòng:</strong> {table_data['row_count']} | 
                    <strong>Số cột:</strong> {table_data['column_count']}
                </div>
                
                <h3>Cấu trúc bảng:</h3>
                <table class="structure-table">
                    <tr>
                        <th>Tên cột</th>
                        <th>Kiểu dữ liệu</th>
                        <th>Cho phép NULL</th>
                        <th>Giá trị mặc định</th>
                        <th>Độ dài tối đa</th>
                    </tr>
            """
            
            for column in table_data['columns']:
                html_content += f"""
                    <tr>
                        <td>{column.get('COLUMN_NAME', '')}</td>
                        <td>{column.get('DATA_TYPE', '')}</td>
                        <td>{column.get('IS_NULLABLE', '')}</td>
                        <td>{column.get('COLUMN_DEFAULT', '')}</td>
                        <td>{column.get('CHARACTER_MAXIMUM_LENGTH', '')}</td>
                    </tr>
                """
            
            html_content += "</table>"
            
            if table_data['sample_data']:
                html_content += f"""
                <h3>Dữ liệu mẫu:</h3>
                <div class="sample-data">
                    <pre>{json.dumps(table_data['sample_data'], ensure_ascii=False, indent=2)}</pre>
                </div>
                """
            
            html_content += "</div>"
        
        html_content += """
        </body>
        </html>
        """
        
        with open('database_report.html', 'w', encoding='utf-8') as f:
            f.write(html_content)
        
        print("📄 Báo cáo HTML đã được lưu vào file: database_report.html")

def main():
    """
    Hàm chính để chạy phân tích database
    """
    # Connection string từ appsettings.json
    connection_string = (
        "Driver={ODBC Driver 17 for SQL Server};"
        "Server=localhost;"
        "Database=Shopping_Demo;"
        "Trusted_Connection=yes;"
    )
    
    # Tạo instance của DatabaseAnalyzer
    analyzer = DatabaseAnalyzer(connection_string)
    
    # Kết nối database
    if analyzer.connect():
        try:
            # Phân tích database
            analysis_results = analyzer.analyze_database()
            
            # Tạo báo cáo
            analyzer.generate_report(analysis_results)
            
            print("\n✅ Hoàn thành phân tích database!")
            print("📁 Các file báo cáo đã được tạo:")
            print("   - database_report.json (Báo cáo chi tiết)")
            print("   - database_report.html (Báo cáo HTML)")
            
        finally:
            # Đóng kết nối
            analyzer.disconnect()

if __name__ == "__main__":
    main()
