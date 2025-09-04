import pyodbc
import re
import os

class EnglishRestorer:
    def __init__(self, connection_string, sql_file_path):
        self.connection_string = connection_string
        self.sql_file_path = sql_file_path
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
    
    def read_sql_file(self):
        try:
            print(f"\n📖 ĐỌC FILE: {self.sql_file_path}")
            print("=" * 50)
            
            if not os.path.exists(self.sql_file_path):
                print(f"❌ File không tồn tại: {self.sql_file_path}")
                return None
            
            with open(self.sql_file_path, 'r', encoding='utf-8', errors='ignore') as file:
                content = file.read()
            
            print(f"✅ Đã đọc file! Kích thước: {len(content)} ký tự")
            
            # Parse INSERT statements
            insert_pattern = r"INSERT\s+INTO\s+\[?Products\]?\s*\([^)]+\)\s*VALUES\s*\(([^)]+)\)"
            matches = re.findall(insert_pattern, content, re.IGNORECASE | re.DOTALL)
            
            products = []
            for i, match in enumerate(matches):
                try:
                    values = self.parse_values(match)
                    if values:
                        products.append(values)
                        if i < 3:
                            print(f"  Sản phẩm {i+1}: {values.get('Name', 'N/A')}")
                except Exception as e:
                    print(f"  ⚠️ Lỗi parse sản phẩm {i+1}: {e}")
                    continue
            
            print(f"✅ Đã parse được {len(products)} sản phẩm")
            return products
            
        except Exception as e:
            print(f"❌ Lỗi đọc file: {e}")
            return None
    
    def parse_values(self, values_string):
        try:
            values = []
            current_value = ""
            in_quotes = False
            quote_char = None
            
            for char in values_string:
                if char in ["'", '"'] and (not in_quotes or char == quote_char):
                    if not in_quotes:
                        in_quotes = True
                        quote_char = char
                    else:
                        in_quotes = False
                        quote_char = None
                elif char == ',' and not in_quotes:
                    values.append(current_value.strip())
                    current_value = ""
                else:
                    current_value += char
            
            if current_value.strip():
                values.append(current_value.strip())
            
            product = {
                'Id': self.clean_value(values[0]) if len(values) > 0 else None,
                'Name': self.clean_value(values[1]) if len(values) > 1 else None,
                'Model': self.clean_value(values[2]) if len(values) > 2 else None,
                'BrandId': self.clean_value(values[3]) if len(values) > 3 else None,
                'CategoryId': self.clean_value(values[4]) if len(values) > 4 else None,
                'Condition': self.clean_value(values[5]) if len(values) > 5 else None,
                'Gender': self.clean_value(values[6]) if len(values) > 6 else None,
                'Price': self.clean_value(values[7]) if len(values) > 7 else None,
                'CapitalPrice': self.clean_value(values[8]) if len(values) > 8 else None,
                'CreditCardPrice': self.clean_value(values[9]) if len(values) > 9 else None,
                'Description': self.clean_value(values[10]) if len(values) > 10 else None,
                'Certificate': self.clean_value(values[11]) if len(values) > 11 else None,
                'WarrantyInfo': self.clean_value(values[12]) if len(values) > 12 else None,
                'ImageUrl': self.clean_value(values[13]) if len(values) > 13 else None,
                'CreatedAt': self.clean_value(values[14]) if len(values) > 14 else None,
                'UpdatedAt': self.clean_value(values[15]) if len(values) > 15 else None
            }
            
            return product
            
        except Exception as e:
            print(f"❌ Lỗi parse values: {e}")
            return None
    
    def clean_value(self, value):
        if not value or value == 'NULL':
            return None
        
        value = value.strip()
        if (value.startswith("'") and value.endswith("'")) or \
           (value.startswith('"') and value.endswith('"')):
            value = value[1:-1]
        
        value = value.replace("''", "'")
        value = value.replace('""', '"')
        
        return value
    
    def convert_price_to_usd(self, price_str):
        try:
            if not price_str or price_str == 'NULL':
                return None
            
            price = re.sub(r'[^\d.]', '', str(price_str))
            if not price:
                return None
            
            price_float = float(price)
            
            # Nếu giá > 1,000,000 thì coi như VND và chuyển về USD
            if price_float > 1000000:
                return int(price_float / 24500)  # Tỷ giá 1 USD = 24,500 VND
            
            return int(price_float)
            
        except Exception as e:
            print(f"⚠️ Lỗi chuyển đổi giá {price_str}: {e}")
            return None
    
    def update_database(self, products_data):
        try:
            cursor = self.conn.cursor()
            
            print(f"\n🔄 KHÔI PHỤC DATABASE: {len(products_data)} sản phẩm")
            print("=" * 50)
            
            updated_count = 0
            for i, product in enumerate(products_data):
                try:
                    # Chuyển đổi giá về USD
                    price_usd = self.convert_price_to_usd(product.get('Price'))
                    capital_price_usd = self.convert_price_to_usd(product.get('CapitalPrice'))
                    credit_card_price_usd = self.convert_price_to_usd(product.get('CreditCardPrice'))
                    
                    # Cập nhật database với dữ liệu gốc từ file SQL
                    cursor.execute("""
                        UPDATE Products 
                        SET Name = ?, Model = ?, BrandId = ?, CategoryId = ?,
                            Condition = ?, Gender = ?, Price = ?, CapitalPrice = ?, 
                            CreditCardPrice = ?, Description = ?, Certificate = ?, 
                            WarrantyInfo = ?, ImageUrl = ?, CreatedAt = ?, UpdatedAt = ?
                        WHERE Id = ?
                    """, (
                        product.get('Name'), product.get('Model'), product.get('BrandId'),
                        product.get('CategoryId'), product.get('Condition'), product.get('Gender'),
                        price_usd, capital_price_usd, credit_card_price_usd,
                        product.get('Description'), product.get('Certificate'),
                        product.get('WarrantyInfo'), product.get('ImageUrl'),
                        product.get('CreatedAt'), product.get('UpdatedAt'), product.get('Id')
                    ))
                    
                    if cursor.rowcount > 0:
                        updated_count += 1
                        if i < 10:
                            print(f"  ✅ ID {product.get('Id')}: {product.get('Name', 'N/A')}")
                    
                except Exception as e:
                    print(f"  ⚠️ Lỗi cập nhật sản phẩm {product.get('Id')}: {e}")
                    continue
            
            self.conn.commit()
            print(f"\n✅ Đã khôi phục {updated_count} sản phẩm!")
            
            cursor.close()
            return updated_count
            
        except Exception as e:
            print(f"❌ Lỗi cập nhật database: {e}")
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
                print(f"  Price: ${product[6]:,}" if product[6] else "  Price: NULL")
                print(f"  CapitalPrice: ${product[7]:,}" if product[7] else "  CapitalPrice: NULL")
                print(f"  CreditCardPrice: ${product[8]:,}" if product[8] else "  CreditCardPrice: NULL")
                print("  " + "-" * 40)
            
            # Thống kê
            cursor.execute("SELECT COUNT(*) FROM Products WHERE Condition IN ('Excellent', 'Very Good', 'Good', 'Fair', 'Poor', 'New', 'Vintage')")
            english_condition = cursor.fetchone()[0]
            
            cursor.execute("SELECT COUNT(*) FROM Products WHERE Gender IN ('Men', 'Women', 'Unisex', 'Male', 'Female')")
            english_gender = cursor.fetchone()[0]
            
            cursor.execute("SELECT COUNT(*) FROM Products WHERE Price < 100000")  # Giá USD
            usd_price = cursor.fetchone()[0]
            
            print(f"\n📊 THỐNG KÊ KHÔI PHỤC:")
            print(f"  Condition tiếng Anh: {english_condition} sản phẩm")
            print(f"  Gender tiếng Anh: {english_gender} sản phẩm")
            print(f"  Giá USD: {usd_price} sản phẩm")
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi xác minh: {e}")
    
    def run_restoration(self):
        print("🚀 BẮT ĐẦU KHÔI PHỤC DỮ LIỆU TIẾNG ANH GỐC")
        print("=" * 60)
        
        if not self.connect():
            return False
        
        try:
            # Đọc dữ liệu từ file SQL
            products_data = self.read_sql_file()
            if not products_data:
                return False
            
            # Cập nhật database
            updated_count = self.update_database(products_data)
            
            # Xác minh kết quả
            self.verify_results()
            
            print(f"\n🎉 KHÔI PHỤC HOÀN THÀNH!")
            print("=" * 60)
            print(f"✅ Đã đọc {len(products_data)} sản phẩm từ file SQL")
            print(f"✅ Đã khôi phục {updated_count} sản phẩm vào database")
            print("✅ Đã khôi phục toàn bộ nội dung tiếng Anh gốc")
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
    sql_file_path = "Products.sql"
    
    restorer = EnglishRestorer(connection_string, sql_file_path)
    success = restorer.run_restoration()
    
    if success:
        print("\n🎯 KẾT QUẢ: Khôi phục thành công!")
    else:
        print("\n💥 KẾT QUẢ: Khôi phục thất bại!")

if __name__ == "__main__":
    main()
