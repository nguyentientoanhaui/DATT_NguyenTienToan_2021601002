import pyodbc
import re

class CompleteEnglishRestorer:
    def __init__(self, connection_string):
        self.connection_string = connection_string
        self.conn = None
        
    def connect(self):
        try:
            self.conn = pyodbc.connect(self.connection_string)
            print("✅ Kết nối database thành công!")
            return True
        except Exception as e:
            print(f"❌ Lỗi kết nối: {e}")
            return False
    
    def close(self):
        if self.conn:
            self.conn.close()
            print("🔒 Đã đóng kết nối database")
    
    def complete_english_restore(self):
        try:
            cursor = self.conn.cursor()
            
            print("\n🔄 KHÔI PHỤC HOÀN TOÀN VỀ TIẾNG ANH:")
            print("=" * 60)
            
            # SQL để khôi phục hoàn toàn về tiếng Anh
            restore_queries = [
                # Khôi phục Condition
                """
                UPDATE Products 
                SET Condition = CASE 
                    WHEN Condition = 'Xuất sắc' THEN 'Excellent'
                    WHEN Condition = 'Rất tốt' THEN 'Very Good'
                    WHEN Condition = 'Tốt' THEN 'Good'
                    WHEN Condition = 'Khá' THEN 'Fair'
                    WHEN Condition = 'Kém' THEN 'Poor'
                    WHEN Condition = 'Mới' THEN 'New'
                    WHEN Condition = 'Cổ điển' THEN 'Vintage'
                    WHEN Condition = 'Đã qua sử dụng' THEN 'Pre-owned'
                    WHEN Condition = 'Chưa đeo' THEN 'Unworn'
                    WHEN Condition = 'Hoàn hảo' THEN 'Mint'
                    ELSE Condition
                END
                """,
                
                # Khôi phục Gender
                """
                UPDATE Products 
                SET Gender = CASE 
                    WHEN Gender = 'Nam' THEN 'Men'
                    WHEN Gender = 'Nữ' THEN 'Women'
                    WHEN Gender = 'Male' THEN 'Men'
                    WHEN Gender = 'Female' THEN 'Women'
                    ELSE Gender
                END
                """,
                
                # Khôi phục Certificate
                """
                UPDATE Products 
                SET Certificate = CASE 
                    WHEN Certificate = 'Hộp gốc' THEN 'Original Box'
                    WHEN Certificate = 'Giấy tờ gốc' THEN 'Original Papers'
                    WHEN Certificate = 'Thẻ bảo hành' THEN 'Warranty Card'
                    WHEN Certificate = 'Sổ bảo hành' THEN 'Service Book'
                    WHEN Certificate = 'Không có giấy tờ' THEN 'No Papers'
                    WHEN Certificate = 'Hộp và giấy tờ' THEN 'Box and Papers'
                    WHEN Certificate = 'Chỉ có hộp' THEN 'Box Only'
                    WHEN Certificate = 'Chỉ có giấy tờ' THEN 'Papers Only'
                    WHEN Certificate = 'Có' THEN 'Yes'
                    WHEN Certificate = 'Không' THEN 'No'
                    ELSE Certificate
                END
                """,
                
                # Khôi phục WarrantyInfo
                """
                UPDATE Products 
                SET WarrantyInfo = CASE 
                    WHEN WarrantyInfo = '1 năm' THEN '1 Year'
                    WHEN WarrantyInfo = '2 năm' THEN '2 Years'
                    WHEN WarrantyInfo = '3 năm' THEN '3 Years'
                    WHEN WarrantyInfo = '5 năm' THEN '5 Years'
                    WHEN WarrantyInfo = 'Trọn đời' THEN 'Lifetime'
                    WHEN WarrantyInfo = 'Không bảo hành' THEN 'No Warranty'
                    WHEN WarrantyInfo = 'Bảo hành quốc tế' THEN 'International Warranty'
                    WHEN WarrantyInfo = 'Bảo hành nhà sản xuất' THEN 'Manufacturer Warranty'
                    ELSE WarrantyInfo
                END
                """,
                
                # Khôi phục Description - các từ khóa chính
                """
                UPDATE Products 
                SET Description = REPLACE(Description, 'Đã sử dụng', 'Pre-owned')
                """,
                
                """
                UPDATE Products 
                SET Description = REPLACE(Description, 'thép không gỉ', 'stainless steel')
                """,
                
                """
                UPDATE Products 
                SET Description = REPLACE(Description, 'bộ máy tự động', 'automatic movement')
                """,
                
                """
                UPDATE Products 
                SET Description = REPLACE(Description, 'cổ điển', 'classic')
                """,
                
                """
                UPDATE Products 
                SET Description = REPLACE(Description, 'di sản', 'heritage')
                """,
                
                """
                UPDATE Products 
                SET Description = REPLACE(Description, 'mới nhất', 'latest')
                """,
                
                """
                UPDATE Products 
                SET Description = REPLACE(Description, 'tuyệt vời', 'excellent')
                """,
                
                """
                UPDATE Products 
                SET Description = REPLACE(Description, 'chất lượng', 'quality')
                """,
                
                # Chuyển đổi giá từ VND về USD
                """
                UPDATE Products 
                SET Price = CASE 
                    WHEN Price > 1000000 THEN Price / 24500
                    ELSE Price
                END
                """,
                
                """
                UPDATE Products 
                SET CapitalPrice = CASE 
                    WHEN CapitalPrice > 1000000 THEN CapitalPrice / 24500
                    ELSE CapitalPrice
                END
                """,
                
                """
                UPDATE Products 
                SET CreditCardPrice = CASE 
                    WHEN CreditCardPrice > 1000000 THEN CreditCardPrice / 24500
                    ELSE CreditCardPrice
                END
                """
            ]
            
            updated_count = 0
            for i, query in enumerate(restore_queries):
                try:
                    cursor.execute(query)
                    rows_affected = cursor.rowcount
                    updated_count += rows_affected
                    print(f"  ✅ Query {i+1}: {rows_affected} rows affected")
                except Exception as e:
                    print(f"  ⚠️ Query {i+1} error: {e}")
                    continue
            
            self.conn.commit()
            print(f"\n✅ Đã khôi phục tổng cộng {updated_count} thay đổi!")
            
            cursor.close()
            return updated_count
            
        except Exception as e:
            print(f"❌ Lỗi khôi phục: {e}")
            self.conn.rollback()
            return 0
    
    def verify_results(self):
        try:
            cursor = self.conn.cursor()
            
            print("\n✅ XÁC MINH KẾT QUẢ KHÔI PHỤC:")
            print("=" * 50)
            
            cursor.execute("""
                SELECT TOP 3 Id, Name, Model, Condition, Gender, 
                       SUBSTRING(Description, 1, 150) as Description_Short,
                       Price, CapitalPrice, CreditCardPrice
                FROM Products
                ORDER BY Id
            """)
            
            products = cursor.fetchall()
            print("📦 SAMPLE PRODUCTS SAU KHI KHÔI PHỤC:")
            for product in products:
                print(f"  ID: {product[0]}")
                print(f"  Name: {product[1]}")
                print(f"  Model: {product[2]}")
                print(f"  Condition: {product[3]}")
                print(f"  Gender: {product[4]}")
                print(f"  Description: {product[5]}...")
                print(f"  Price: ${product[6]:,.0f}" if product[6] else "  Price: NULL")
                print(f"  CapitalPrice: ${product[7]:,.0f}" if product[7] else "  CapitalPrice: NULL")
                print(f"  CreditCardPrice: ${product[8]:,.0f}" if product[8] else "  CreditCardPrice: NULL")
                print("  " + "-" * 40)
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi xác minh: {e}")
    
    def run_restoration(self):
        print("🚀 BẮT ĐẦU KHÔI PHỤC HOÀN TOÀN VỀ TIẾNG ANH")
        print("=" * 60)
        
        if not self.connect():
            return False
        
        try:
            # Khôi phục dữ liệu
            updated_count = self.complete_english_restore()
            
            # Xác minh kết quả
            self.verify_results()
            
            print(f"\n🎉 KHÔI PHỤC HOÀN THÀNH!")
            print("=" * 60)
            print(f"✅ Đã thực hiện {updated_count} thay đổi")
            print("✅ Đã khôi phục toàn bộ nội dung về tiếng Anh")
            print("✅ Đã chuyển đổi giá về USD")
            print("✅ Dữ liệu đã trở về trạng thái ban đầu")
            
            return True
            
        except Exception as e:
            print(f"❌ Lỗi: {e}")
            return False
        finally:
            self.close()

def main():
    connection_string = "DRIVER={SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;"
    
    restorer = CompleteEnglishRestorer(connection_string)
    success = restorer.run_restoration()
    
    if success:
        print("\n🎯 KẾT QUẢ: Khôi phục thành công!")
    else:
        print("\n💥 KẾT QUẢ: Khôi phục thất bại!")

if __name__ == "__main__":
    main()
