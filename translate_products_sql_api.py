import re
import os
import time
import requests
from deep_translator import GoogleTranslator
import json

class SQLFileTranslator:
    def __init__(self, sql_file_path, output_file_path=None):
        self.sql_file_path = sql_file_path
        self.output_file_path = output_file_path or "Products_Vietnamese.sql"
        self.translator = GoogleTranslator(source='auto', target='vi')
        self.translation_cache = {}
        
    def read_sql_file(self):
        """Đọc file SQL và parse các INSERT statements"""
        try:
            print(f"📖 Đọc file: {self.sql_file_path}")
            
            if not os.path.exists(self.sql_file_path):
                print(f"❌ File không tồn tại: {self.sql_file_path}")
                return None
            
            with open(self.sql_file_path, 'r', encoding='utf-8', errors='ignore') as file:
                content = file.read()
            
            print(f"✅ Đã đọc file! Kích thước: {len(content)} ký tự")
            
            # Tìm tất cả INSERT statements
            insert_pattern = r"INSERT\s+\[dbo\]\.\[Products\]\s*\([^)]+\)\s*VALUES\s*\([^)]+\)"
            matches = re.findall(insert_pattern, content, re.IGNORECASE | re.DOTALL)
            
            print(f"✅ Tìm thấy {len(matches)} INSERT statements")
            return matches
            
        except Exception as e:
            print(f"❌ Lỗi đọc file: {e}")
            return None
    
    def translate_text_with_api(self, text, max_retries=3):
        """Dịch text sử dụng Google Translate API"""
        if not text or len(text.strip()) < 3:
            return text
        
        # Kiểm tra cache
        if text in self.translation_cache:
            return self.translation_cache[text]
        
        for attempt in range(max_retries):
            try:
                # Sử dụng deep-translator library
                translated_text = self.translator.translate(text)
                
                # Lưu vào cache
                self.translation_cache[text] = translated_text
                
                # Delay để tránh rate limiting
                time.sleep(0.5)
                
                return translated_text
                
            except Exception as e:
                print(f"⚠️ Lần thử {attempt + 1} thất bại: {e}")
                if attempt < max_retries - 1:
                    time.sleep(2 ** attempt)  # Exponential backoff
                else:
                    print(f"❌ Không thể dịch: {text}")
                    return text
    
    def parse_and_translate_insert(self, insert_statement):
        """Parse và dịch một INSERT statement"""
        try:
            # Tách phần VALUES
            values_match = re.search(r'VALUES\s*\(([^)]+)\)', insert_statement, re.DOTALL)
            if not values_match:
                return insert_statement
            
            values_string = values_match.group(1)
            
            # Parse các giá trị
            values = self.parse_values(values_string)
            if not values:
                return insert_statement
            
            # Dịch các trường cần thiết (ngoại trừ Name và Model)
            translated_values = []
            for i, value in enumerate(values):
                # Giữ nguyên Name (index 1) và Model (index 12)
                if i == 1 or i == 12:  # Name và Model - giữ nguyên
                    translated_values.append(value)
                # Dịch các trường text khác
                elif value and value != 'NULL' and value.startswith("N'") and value.endswith("'"):
                    # Lấy text bên trong quotes
                    text_content = value[2:-1]  # Bỏ N' và '
                    if text_content and len(text_content.strip()) > 3:
                        # Kiểm tra xem có phải là text tiếng Anh không
                        if self.is_english_text(text_content):
                            translated_text = self.translate_text_with_api(text_content)
                            translated_values.append(f"N'{translated_text}'")
                        else:
                            translated_values.append(value)
                    else:
                        translated_values.append(value)
                else:
                    translated_values.append(value)
            
            # Tạo INSERT statement mới
            new_values_string = ', '.join(translated_values)
            new_insert = re.sub(r'VALUES\s*\([^)]+\)', f'VALUES ({new_values_string})', insert_statement)
            
            return new_insert
            
        except Exception as e:
            print(f"❌ Lỗi dịch INSERT statement: {e}")
            return insert_statement
    
    def parse_values(self, values_string):
        """Parse các giá trị trong VALUES clause"""
        try:
            values = []
            current_value = ""
            in_quotes = False
            quote_char = None
            paren_count = 0
            
            for char in values_string:
                if char in ["'", '"'] and (not in_quotes or char == quote_char):
                    if not in_quotes:
                        in_quotes = True
                        quote_char = char
                    else:
                        in_quotes = False
                        quote_char = None
                elif char == '(' and not in_quotes:
                    paren_count += 1
                    current_value += char
                elif char == ')' and not in_quotes:
                    paren_count -= 1
                    current_value += char
                elif char == ',' and not in_quotes and paren_count == 0:
                    values.append(current_value.strip())
                    current_value = ""
                else:
                    current_value += char
            
            if current_value.strip():
                values.append(current_value.strip())
            
            return values
            
        except Exception as e:
            print(f"❌ Lỗi parse values: {e}")
            return []
    
    def is_english_text(self, text):
        """Kiểm tra xem text có phải là tiếng Anh không"""
        if not text:
            return False
        
        # Đếm số ký tự tiếng Anh
        english_chars = sum(1 for c in text if c.isalpha() and ord(c) < 128)
        total_chars = sum(1 for c in text if c.isalpha())
        
        if total_chars == 0:
            return False
        
        # Nếu ít nhất 70% là ký tự tiếng Anh thì coi như là tiếng Anh
        return english_chars / total_chars >= 0.7
    
    def translate_file(self):
        """Dịch toàn bộ file SQL"""
        print("🚀 BẮT ĐẦU DỊCH FILE PRODUCTS.SQL")
        print("=" * 60)
        
        # Đọc file
        insert_statements = self.read_sql_file()
        if not insert_statements:
            return False
        
        # Đọc phần đầu file (CREATE TABLE, etc.)
        with open(self.sql_file_path, 'r', encoding='utf-8', errors='ignore') as file:
            content = file.read()
        
        # Tách phần đầu và phần INSERT
        insert_start = content.find("INSERT")
        if insert_start == -1:
            print("❌ Không tìm thấy INSERT statements")
            return False
        
        header_content = content[:insert_start]
        
        # Dịch từng INSERT statement
        translated_statements = []
        total_statements = len(insert_statements)
        
        print(f"\n🔄 DỊCH {total_statements} INSERT STATEMENTS:")
        print("=" * 50)
        
        for i, statement in enumerate(insert_statements):
            try:
                print(f"  [{i+1}/{total_statements}] Đang dịch...")
                translated_statement = self.parse_and_translate_insert(statement)
                translated_statements.append(translated_statement)
                
                if (i + 1) % 10 == 0:
                    print(f"    ✅ Đã hoàn thành {i+1}/{total_statements}")
                
            except Exception as e:
                print(f"  ⚠️ Lỗi dịch statement {i+1}: {e}")
                translated_statements.append(statement)  # Giữ nguyên nếu lỗi
                continue
        
        # Ghi file kết quả
        try:
            print(f"\n💾 GHI FILE KẾT QUẢ: {self.output_file_path}")
            print("=" * 50)
            
            with open(self.output_file_path, 'w', encoding='utf-8') as file:
                file.write(header_content)
                file.write('\n\n')
                file.write('\n'.join(translated_statements))
                file.write('\n')
            
            print(f"✅ Đã ghi file thành công!")
            print(f"📁 File kết quả: {self.output_file_path}")
            
            # Lưu cache để sử dụng sau
            with open('translation_cache.json', 'w', encoding='utf-8') as cache_file:
                json.dump(self.translation_cache, cache_file, ensure_ascii=False, indent=2)
            
            print(f"💾 Đã lưu cache dịch thuật: translation_cache.json")
            
            return True
            
        except Exception as e:
            print(f"❌ Lỗi ghi file: {e}")
            return False
    
    def show_sample_results(self, num_samples=3):
        """Hiển thị mẫu kết quả dịch"""
        try:
            print(f"\n📋 MẪU KẾT QUẢ DỊCH ({num_samples} sản phẩm đầu tiên):")
            print("=" * 60)
            
            with open(self.output_file_path, 'r', encoding='utf-8') as file:
                content = file.read()
            
            # Tìm các INSERT statements
            insert_pattern = r"INSERT\s+\[dbo\]\.\[Products\]\s*\([^)]+\)\s*VALUES\s*\(([^)]+)\)"
            matches = re.findall(insert_pattern, content, re.IGNORECASE | re.DOTALL)
            
            for i, match in enumerate(matches[:num_samples]):
                values = self.parse_values(match)
                if len(values) >= 3:
                    print(f"\n🔸 SẢN PHẨM {i+1}:")
                    print(f"   ID: {values[0] if len(values) > 0 else 'N/A'}")
                    print(f"   Name: {values[1] if len(values) > 1 else 'N/A'}")
                    print(f"   Slug: {values[2] if len(values) > 2 else 'N/A'}")
                    print(f"   Description: {values[3][:100] + '...' if len(values) > 3 and len(values[3]) > 100 else values[3] if len(values) > 3 else 'N/A'}")
                    print(f"   Condition: {values[5] if len(values) > 5 else 'N/A'}")
                    print(f"   Gender: {values[6] if len(values) > 6 else 'N/A'}")
                    print("   " + "-" * 40)
            
        except Exception as e:
            print(f"❌ Lỗi hiển thị mẫu: {e}")

def main():
    sql_file_path = "Products.sql"
    output_file_path = "Products_Vietnamese.sql"
    
    translator = SQLFileTranslator(sql_file_path, output_file_path)
    
    # Kiểm tra xem có cache không
    if os.path.exists('translation_cache.json'):
        try:
            with open('translation_cache.json', 'r', encoding='utf-8') as cache_file:
                translator.translation_cache = json.load(cache_file)
            print("📚 Đã tải cache dịch thuật từ file trước đó")
        except Exception as e:
            print(f"⚠️ Không thể tải cache: {e}")
    
    # Thực hiện dịch
    success = translator.translate_file()
    
    if success:
        print("\n🎉 HOÀN THÀNH DỊCH THUẬT!")
        print("=" * 60)
        print("✅ Đã dịch toàn bộ nội dung sang tiếng Việt")
        print("✅ Đã giữ nguyên cột Name và Model")
        print("✅ Đã sử dụng API dịch thuật")
        print("✅ Đã lưu cache để tái sử dụng")
        
        # Hiển thị mẫu kết quả
        translator.show_sample_results()
        
    else:
        print("\n💥 DỊCH THUẬT THẤT BẠI!")

if __name__ == "__main__":
    main()
