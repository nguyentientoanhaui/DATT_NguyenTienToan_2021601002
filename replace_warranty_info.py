#!/usr/bin/env python3
"""
Script để thay thế "Bob's Watches" thành "Aurum Watches" trong database
Tạo bởi: Assistant
Ngày: 2025-01-27
"""

import pyodbc
import datetime
import logging

# Cấu hình logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('warranty_update.log'),
        logging.StreamHandler()
    ]
)

def get_connection():
    """Tạo kết nối đến SQL Server"""
    try:
        # Thay đổi connection string theo cấu hình của bạn
        connection_string = (
            "Driver={ODBC Driver 17 for SQL Server};"
            "Server=localhost;"  # Hoặc tên server của bạn
            "Database=Shopping_Demo;"
            "Trusted_Connection=yes;"
        )
        
        conn = pyodbc.connect(connection_string)
        logging.info("✅ Đã kết nối thành công đến database")
        return conn
        
    except Exception as e:
        logging.error(f"❌ Lỗi kết nối database: {str(e)}")
        return None

def backup_database(cursor):
    """Tạo backup database"""
    try:
        backup_file = f"C:\\Backup\\Shopping_Demo_Warranty_Backup_{datetime.datetime.now().strftime('%Y%m%d_%H%M%S')}.bak"
        
        backup_sql = f"""
        BACKUP DATABASE [Shopping_Demo] 
        TO DISK = '{backup_file}'
        WITH FORMAT, INIT;
        """
        
        logging.info("🔄 Đang tạo backup database...")
        cursor.execute(backup_sql)
        logging.info(f"✅ Backup thành công: {backup_file}")
        return True
        
    except Exception as e:
        logging.error(f"❌ Lỗi tạo backup: {str(e)}")
        return False

def check_records_to_update(cursor):
    """Kiểm tra số lượng bản ghi cần cập nhật"""
    try:
        query = """
        SELECT COUNT(*) as count
        FROM [dbo].[Products] 
        WHERE [WarrantyInfo] LIKE '%Bob''s Watches%'
        """
        
        cursor.execute(query)
        result = cursor.fetchone()
        count = result.count if result else 0
        
        logging.info(f"📊 Số bản ghi cần cập nhật: {count}")
        return count
        
    except Exception as e:
        logging.error(f"❌ Lỗi kiểm tra bản ghi: {str(e)}")
        return 0

def preview_records(cursor):
    """Preview một số bản ghi sẽ được cập nhật"""
    try:
        query = """
        SELECT TOP 5 
            [Id],
            [Name],
            LEFT([WarrantyInfo], 100) as [WarrantyInfo_Preview]
        FROM [dbo].[Products] 
        WHERE [WarrantyInfo] LIKE '%Bob''s Watches%'
        """
        
        cursor.execute(query)
        records = cursor.fetchall()
        
        logging.info("📋 Preview bản ghi sẽ được cập nhật:")
        for record in records:
            logging.info(f"  ID: {record.Id}, Name: {record.Name}")
            logging.info(f"    Warranty: {record.WarrantyInfo_Preview}...")
            
        return True
        
    except Exception as e:
        logging.error(f"❌ Lỗi preview bản ghi: {str(e)}")
        return False

def update_warranty_info(cursor):
    """Thực hiện cập nhật thông tin bảo hành"""
    try:
        # Bắt đầu transaction
        cursor.execute("BEGIN TRANSACTION")
        
        update_sql = """
        UPDATE [dbo].[Products] 
        SET [WarrantyInfo] = REPLACE([WarrantyInfo], 'Bob''s Watches', 'Aurum Watches'),
            [UpdatedDate] = GETDATE()
        WHERE [WarrantyInfo] LIKE '%Bob''s Watches%'
        """
        
        logging.info("🔄 Đang thực hiện cập nhật...")
        cursor.execute(update_sql)
        updated_count = cursor.rowcount
        
        logging.info(f"✅ Đã cập nhật {updated_count} bản ghi")
        
        # Verification
        verify_sql = """
        SELECT 
            COUNT(*) as remaining_bobs
        FROM [dbo].[Products] 
        WHERE [WarrantyInfo] LIKE '%Bob''s Watches%'
        """
        
        cursor.execute(verify_sql)
        remaining = cursor.fetchone().remaining_bobs
        
        if remaining > 0:
            logging.warning(f"⚠️  Vẫn còn {remaining} bản ghi chứa 'Bob's Watches'")
            cursor.execute("ROLLBACK TRANSACTION")
            return False
        else:
            logging.info("✅ Tất cả 'Bob's Watches' đã được thay thế")
            cursor.execute("COMMIT TRANSACTION")
            return True
            
    except Exception as e:
        logging.error(f"❌ Lỗi cập nhật: {str(e)}")
        cursor.execute("ROLLBACK TRANSACTION")
        return False

def final_verification(cursor):
    """Kiểm tra cuối cùng"""
    try:
        query = """
        SELECT 
            COUNT(*) as total_products_with_warranty,
            SUM(CASE WHEN [WarrantyInfo] LIKE '%Aurum Watches%' THEN 1 ELSE 0 END) as aurum_watches_count,
            SUM(CASE WHEN [WarrantyInfo] LIKE '%Bob''s Watches%' THEN 1 ELSE 0 END) as bobs_watches_remaining
        FROM [dbo].[Products] 
        WHERE [WarrantyInfo] IS NOT NULL AND [WarrantyInfo] != ''
        """
        
        cursor.execute(query)
        result = cursor.fetchone()
        
        logging.info("=== KIỂM TRA CUỐI CÙNG ===")
        logging.info(f"📊 Tổng sản phẩm có thông tin bảo hành: {result.total_products_with_warranty}")
        logging.info(f"✅ Sản phẩm có 'Aurum Watches': {result.aurum_watches_count}")
        logging.info(f"❌ Sản phẩm còn 'Bob's Watches': {result.bobs_watches_remaining}")
        
        return result.bobs_watches_remaining == 0
        
    except Exception as e:
        logging.error(f"❌ Lỗi kiểm tra cuối: {str(e)}")
        return False

def main():
    """Hàm chính"""
    logging.info("🚀 Bắt đầu quá trình cập nhật thông tin bảo hành")
    
    # Kết nối database
    conn = get_connection()
    if not conn:
        return False
    
    try:
        cursor = conn.cursor()
        
        # Bước 1: Tạo backup
        if not backup_database(cursor):
            logging.error("❌ Không thể tạo backup, dừng quá trình")
            return False
        
        # Bước 2: Kiểm tra bản ghi cần cập nhật
        count = check_records_to_update(cursor)
        if count == 0:
            logging.info("ℹ️  Không có bản ghi nào cần cập nhật")
            return True
        
        # Bước 3: Preview bản ghi
        preview_records(cursor)
        
        # Bước 4: Xác nhận từ user
        response = input(f"\n🤔 Bạn có muốn cập nhật {count} bản ghi? (y/N): ")
        if response.lower() != 'y':
            logging.info("❌ Người dùng hủy quá trình cập nhật")
            return False
        
        # Bước 5: Thực hiện cập nhật
        if not update_warranty_info(cursor):
            logging.error("❌ Cập nhật thất bại")
            return False
        
        # Bước 6: Kiểm tra cuối cùng
        if final_verification(cursor):
            logging.info("🎉 Quá trình cập nhật hoàn tất thành công!")
            return True
        else:
            logging.error("❌ Kiểm tra cuối cùng thất bại")
            return False
            
    except Exception as e:
        logging.error(f"❌ Lỗi không mong muốn: {str(e)}")
        return False
        
    finally:
        conn.close()
        logging.info("🔐 Đã đóng kết nối database")

if __name__ == "__main__":
    success = main()
    exit(0 if success else 1)









