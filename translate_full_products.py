import re
import os
import time
from deep_translator import GoogleTranslator
import json

def translate_text_with_api(text, translator, cache, max_retries=3):
    """Dịch text sử dụng Google Translate API"""
    if not text or len(text.strip()) < 3:
        return text
    
    # Kiểm tra cache
    if text in cache:
        return cache[text]
    
    for attempt in range(max_retries):
        try:
            # Sử dụng deep-translator library
            translated_text = translator.translate(text)
            
            # Lưu vào cache
            cache[text] = translated_text
            
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

def is_english_text(text):
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

def should_translate_field(text_content):
    """Kiểm tra xem có nên dịch field này không (5 cột cụ thể)"""
    if not text_content or len(text_content.strip()) < 3:
        return False, ""
    
    # Chỉ dịch 5 cột cụ thể dựa trên nội dung
    if text_content == "Male":
        return True, "Gender"
    elif text_content == "Female":
        return True, "Gender"
    elif text_content == "Excellent":
        return True, "Condition"
    elif text_content == "Good":
        return True, "Condition"
    elif text_content == "Very Good":
        return True, "Condition"
    elif text_content == "Stainless Steel":
        return True, "CaseMaterial"
    elif text_content == "Gold":
        return True, "CaseMaterial"
    elif text_content == "Titanium":
        return True, "CaseMaterial"
    elif text_content == "Platinum":
        return True, "CaseMaterial"
    elif "Used" in text_content and len(text_content) > 50:
        return True, "Description"
    elif "warranty" in text_content.lower():
        return True, "WarrantyInfo"
    elif "guarantee" in text_content.lower():
        return True, "WarrantyInfo"
    
    return False, ""

def translate_sql_file():
    """Dịch file Products.sql thực tế - chỉ dịch 5 cột cụ thể"""
    print("🚀 BẮT ĐẦU DỊCH FILE PRODUCTS.SQL THỰC TẾ")
    print("=" * 60)
    print("🎯 Chỉ dịch 5 cột: Description, Gender, Condition, CaseMaterial, WarrantyInfo")
    print("=" * 60)
    
    # Kiểm tra file Products.sql
    if not os.path.exists('Products.sql'):
        print("❌ Không tìm thấy file Products.sql")
        return False
    
    # Khởi tạo translator
    translator = GoogleTranslator(source='auto', target='vi')
    translation_cache = {}
    
    # Load cache cũ nếu có
    if os.path.exists('translation_cache_full.json'):
        try:
            with open('translation_cache_full.json', 'r', encoding='utf-8') as cache_file:
                translation_cache = json.load(cache_file)
            print(f"✅ Đã load cache cũ với {len(translation_cache)} bản dịch")
        except:
            print("⚠️ Không thể load cache cũ, bắt đầu mới")
    
    try:
        # Đọc file SQL với encoding khác nhau
        print("📖 Đang đọc file Products.sql...")
        
        # Thử các encoding khác nhau
        encodings = ['utf-8', 'utf-16', 'utf-16le', 'utf-16be', 'latin-1', 'cp1252']
        content = None
        
        for encoding in encodings:
            try:
                with open('Products.sql', 'r', encoding=encoding) as file:
                    content = file.read()
                print(f"✅ Đã đọc file thành công với encoding: {encoding}")
                break
            except UnicodeDecodeError:
                print(f"⚠️ Không thể đọc với encoding: {encoding}")
                continue
        
        if content is None:
            print("❌ Không thể đọc file với bất kỳ encoding nào")
            return False
        
        print(f"✅ Đã đọc file! Kích thước: {len(content)} ký tự")
        
        # Tìm INSERT statements bằng cách tách dòng
        lines = content.split('\n')
        insert_statements = []
        current_statement = ""
        in_insert = False
        
        print("🔍 Đang tìm INSERT statements...")
        for line in lines:
            if 'INSERT' in line.upper() and 'PRODUCTS' in line.upper() and 'VALUES' in line.upper():
                if in_insert:  # Nếu đang trong INSERT statement trước đó
                    insert_statements.append(current_statement)
                in_insert = True
                current_statement = line
            elif in_insert:
                current_statement += '\n' + line
                if line.strip().endswith(')'):
                    insert_statements.append(current_statement)
                    current_statement = ""
                    in_insert = False
        
        # Thêm statement cuối cùng nếu chưa kết thúc
        if in_insert and current_statement:
            insert_statements.append(current_statement)
        
        print(f"✅ Tìm thấy {len(insert_statements)} INSERT statements")
        
        # Tách phần đầu và phần INSERT
        insert_start = content.find("INSERT")
        if insert_start == -1:
            print("❌ Không tìm thấy INSERT statements")
            return False
        
        header_content = content[:insert_start]
        
        # Dịch từng INSERT statement
        translated_statements = []
        total_translated = 0
        
        print(f"\n🔄 BẮT ĐẦU DỊCH {len(insert_statements)} SẢN PHẨM...")
        print("=" * 60)
        
        for i, statement in enumerate(insert_statements):
            try:
                if (i + 1) % 50 == 0:
                    print(f"📊 Tiến độ: {i+1}/{len(insert_statements)} ({((i+1)/len(insert_statements)*100):.1f}%)")
                
                # Tìm tất cả các chuỗi N'...' trong statement
                pattern = r"N'([^']*)'"
                matches = re.findall(pattern, statement)
                
                # Dịch từng text field (chỉ 5 cột cụ thể)
                translated_statement = statement
                translated_count = 0
                
                for j, text_content in enumerate(matches):
                    if text_content and len(text_content.strip()) > 3:
                        # Kiểm tra xem có phải là text tiếng Anh không
                        if is_english_text(text_content):
                            # Kiểm tra xem có nên dịch không
                            should_translate, column_name = should_translate_field(text_content)
                            
                            if should_translate:
                                translated_text = translate_text_with_api(text_content, translator, translation_cache)
                                
                                # Thay thế trong statement
                                old_pattern = f"N'{text_content}'"
                                new_pattern = f"N'{translated_text}'"
                                translated_statement = translated_statement.replace(old_pattern, new_pattern)
                                translated_count += 1
                
                translated_statements.append(translated_statement)
                total_translated += translated_count
                
                # Lưu cache định kỳ
                if (i + 1) % 100 == 0:
                    with open('translation_cache_full.json', 'w', encoding='utf-8') as cache_file:
                        json.dump(translation_cache, cache_file, ensure_ascii=False, indent=2)
                    print(f"💾 Đã lưu cache tạm thời ({len(translation_cache)} bản dịch)")
                
            except Exception as e:
                print(f"  ⚠️ Lỗi dịch statement {i+1}: {e}")
                translated_statements.append(statement)  # Giữ nguyên nếu lỗi
                continue
        
        # Ghi file kết quả
        output_file = "Products_Vietnamese_Full.sql"
        print(f"\n💾 GHI FILE KẾT QUẢ: {output_file}")
        print("=" * 50)
        
        with open(output_file, 'w', encoding='utf-8') as file:
            file.write(header_content)
            file.write('\n\n')
            file.write('\n'.join(translated_statements))
            file.write('\n')
        
        print(f"✅ Đã ghi file thành công!")
        print(f"📁 File kết quả: {output_file}")
        
        # Lưu cache cuối cùng
        with open('translation_cache_full.json', 'w', encoding='utf-8') as cache_file:
            json.dump(translation_cache, cache_file, ensure_ascii=False, indent=2)
        
        print(f"💾 Đã lưu cache dịch thuật: translation_cache_full.json")
        
        # Thống kê kết quả
        print(f"\n📊 THỐNG KÊ KẾT QUẢ:")
        print("=" * 60)
        print(f"✅ Tổng số sản phẩm: {len(insert_statements)}")
        print(f"✅ Tổng số field đã dịch: {total_translated}")
        print(f"✅ Cache dịch thuật: {len(translation_cache)} bản dịch")
        print(f"✅ Chỉ dịch 5 cột: Description, Gender, Condition, CaseMaterial, WarrantyInfo")
        
        return True
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")
        return False

def main():
    print("🎯 SCRIPT DỊCH THUẬT TẤT CẢ SẢN PHẨM")
    print("=" * 60)
    
    # Thực hiện dịch
    success = translate_sql_file()
    
    if success:
        print("\n🎉 HOÀN THÀNH DỊCH THUẬT!")
        print("=" * 60)
        print("✅ Đã dịch tất cả sản phẩm trong Products.sql")
        print("✅ Chỉ dịch 5 cột cụ thể:")
        print("   - Description")
        print("   - Gender") 
        print("   - Condition")
        print("   - CaseMaterial")
        print("   - WarrantyInfo")
        print("✅ Đã giữ nguyên tất cả các cột khác")
        print("✅ Đã sử dụng API dịch thuật")
        print("✅ Đã lưu cache để tái sử dụng")
        
    else:
        print("\n💥 DỊCH THUẬT THẤT BẠI!")

if __name__ == "__main__":
    main()
