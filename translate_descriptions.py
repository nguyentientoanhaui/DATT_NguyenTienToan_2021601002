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

def translate_descriptions_file():
    """Dịch file mô tả sản phẩm"""
    print("🚀 BẮT ĐẦU DỊCH FILE MÔ TẢ SẢN PHẨM")
    print("=" * 60)
    
    # Kiểm tra file
    input_file = "Chuyển sang tiếng việt.txt"
    if not os.path.exists(input_file):
        print(f"❌ Không tìm thấy file {input_file}")
        return False
    
    # Khởi tạo translator
    translator = GoogleTranslator(source='auto', target='vi')
    translation_cache = {}
    
    # Load cache cũ nếu có
    cache_file = "translation_cache_descriptions.json"
    if os.path.exists(cache_file):
        try:
            with open(cache_file, 'r', encoding='utf-8') as f:
                translation_cache = json.load(f)
            print(f"✅ Đã load cache cũ với {len(translation_cache)} bản dịch")
        except:
            print("⚠️ Không thể load cache cũ, bắt đầu mới")
    
    try:
        # Đọc file
        print(f"📖 Đang đọc file {input_file}...")
        with open(input_file, 'r', encoding='utf-8') as file:
            lines = file.readlines()
        
        print(f"✅ Đã đọc file! Tổng số dòng: {len(lines)}")
        
        # Dịch từng dòng
        translated_lines = []
        total_translated = 0
        
        print(f"\n🔄 BẮT ĐẦU DỊCH {len(lines)} DÒNG...")
        print("=" * 60)
        
        for i, line in enumerate(lines):
            line = line.strip()
            
            if not line:
                translated_lines.append("")
                continue
            
            # Bỏ qua dòng tiêu đề
            if line == "Chuyển sang tiếng việt" or line == "Description":
                translated_lines.append(line)
                continue
            
            # Kiểm tra xem có phải là text tiếng Anh không
            if is_english_text(line):
                print(f"  [{i+1}/{len(lines)}] Đang dịch: {line[:100]}...")
                translated_text = translate_text_with_api(line, translator, translation_cache)
                translated_lines.append(translated_text)
                total_translated += 1
            else:
                # Giữ nguyên nếu không phải tiếng Anh
                translated_lines.append(line)
            
            # Hiển thị tiến độ
            if (i + 1) % 10 == 0:
                print(f"📊 Tiến độ: {i+1}/{len(lines)} ({((i+1)/len(lines)*100):.1f}%)")
            
            # Lưu cache định kỳ
            if (i + 1) % 50 == 0:
                with open(cache_file, 'w', encoding='utf-8') as f:
                    json.dump(translation_cache, f, ensure_ascii=False, indent=2)
                print(f"💾 Đã lưu cache tạm thời ({len(translation_cache)} bản dịch)")
        
        # Ghi file kết quả
        output_file = "Mô_tả_sản_phẩm_Tiếng_Việt.txt"
        print(f"\n💾 GHI FILE KẾT QUẢ: {output_file}")
        print("=" * 50)
        
        with open(output_file, 'w', encoding='utf-8') as file:
            file.write('\n'.join(translated_lines))
        
        print(f"✅ Đã ghi file thành công!")
        print(f"📁 File kết quả: {output_file}")
        
        # Lưu cache cuối cùng
        with open(cache_file, 'w', encoding='utf-8') as f:
            json.dump(translation_cache, f, ensure_ascii=False, indent=2)
        
        print(f"💾 Đã lưu cache dịch thuật: {cache_file}")
        
        # Thống kê kết quả
        print(f"\n📊 THỐNG KÊ KẾT QUẢ:")
        print("=" * 60)
        print(f"✅ Tổng số dòng: {len(lines)}")
        print(f"✅ Số dòng đã dịch: {total_translated}")
        print(f"✅ Cache dịch thuật: {len(translation_cache)} bản dịch")
        
        # Hiển thị mẫu kết quả
        print(f"\n📋 MẪU KẾT QUẢ DỊCH:")
        print("=" * 60)
        
        for i, (original, translated) in enumerate(zip(lines[:5], translated_lines[:5])):
            if original.strip() and original.strip() != "Chuyển sang tiếng việt" and original.strip() != "Description":
                print(f"\n🔸 MẪU {i+1}:")
                print(f"   Gốc: {original[:100]}...")
                print(f"   Dịch: {translated[:100]}...")
                print("   " + "-" * 40)
        
        return True
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")
        return False

def main():
    print("🎯 SCRIPT DỊCH THUẬT MÔ TẢ SẢN PHẨM")
    print("=" * 60)
    
    # Thực hiện dịch
    success = translate_descriptions_file()
    
    if success:
        print("\n🎉 HOÀN THÀNH DỊCH THUẬT!")
        print("=" * 60)
        print("✅ Đã dịch tất cả mô tả sản phẩm sang tiếng Việt")
        print("✅ Đã sử dụng API dịch thuật")
        print("✅ Đã lưu cache để tái sử dụng")
        
    else:
        print("\n💥 DỊCH THUẬT THẤT BẠI!")

if __name__ == "__main__":
    main()
