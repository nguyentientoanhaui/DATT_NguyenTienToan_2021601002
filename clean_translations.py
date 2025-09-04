import re
import os

def clean_translations():
    """Làm sạch bản dịch, loại bỏ các lỗi dịch"""
    print("🧹 BẮT ĐẦU LÀM SẠCH BẢN DỊCH")
    print("=" * 60)
    
    # Kiểm tra file
    input_file = "Mô_tả_sản_phẩm_Tiếng_Việt.txt"
    if not os.path.exists(input_file):
        print(f"❌ Không tìm thấy file {input_file}")
        return False
    
    try:
        # Đọc file
        print(f"📖 Đang đọc file {input_file}...")
        with open(input_file, 'r', encoding='utf-8') as file:
            lines = file.readlines()
        
        print(f"✅ Đã đọc file! Tổng số dòng: {len(lines)}")
        
        # Làm sạch từng dòng
        cleaned_lines = []
        total_cleaned = 0
        
        print(f"\n🧹 BẮT ĐẦU LÀM SẠCH {len(lines)} DÒNG...")
        print("=" * 60)
        
        for i, line in enumerate(lines):
            line = line.strip()
            
            if not line:
                cleaned_lines.append("")
                continue
            
            # Bỏ qua dòng tiêu đề
            if line == "Chuyển sang tiếng việt" or line == "Description":
                cleaned_lines.append(line)
                continue
            
            # Làm sạch các lỗi dịch
            cleaned_line = line
            
            # Loại bỏ "Đạo Sử Dụng" và các biến thể
            cleaned_line = re.sub(r'Đạo\s+Sử\s+Dụng\s*', '', cleaned_line, flags=re.IGNORECASE)
            cleaned_line = re.sub(r'Đạo\s+Sử\s*', '', cleaned_line, flags=re.IGNORECASE)
            cleaned_line = re.sub(r'Đạo\s*', '', cleaned_line, flags=re.IGNORECASE)
            
            # Sửa các lỗi dịch khác
            cleaned_line = re.sub(r'Ệ\s+sử\s+dụng', 'Đã sử dụng', cleaned_line, flags=re.IGNORECASE)
            cleaned_line = re.sub(r'Đạo\s+S17366', 'Đã sử dụng Breitling Superocean ref A17366', cleaned_line, flags=re.IGNORECASE)
            
            # Loại bỏ các ký tự lạ
            cleaned_line = re.sub(r'[^\w\s\.,\-\(\)\[\]\{\}\'\"/\\@#$%^&*+=<>?|~`!@#$%^&*()_+\-=\[\]{};\':"\\|,.<>/?\u00C0-\u1EF9]', '', cleaned_line)
            
            # Loại bỏ khoảng trắng thừa
            cleaned_line = re.sub(r'\s+', ' ', cleaned_line).strip()
            
            # Nếu dòng bị thay đổi, đánh dấu
            if cleaned_line != line:
                print(f"  [{i+1}/{len(lines)}] Đã làm sạch: {line[:50]}... → {cleaned_line[:50]}...")
                total_cleaned += 1
            
            cleaned_lines.append(cleaned_line)
            
            # Hiển thị tiến độ
            if (i + 1) % 100 == 0:
                print(f"📊 Tiến độ: {i+1}/{len(lines)} ({((i+1)/len(lines)*100):.1f}%)")
        
        # Ghi file kết quả
        output_file = "Mô_tả_sản_phẩm_Tiếng_Việt_Đã_Làm_Sạch.txt"
        print(f"\n💾 GHI FILE KẾT QUẢ: {output_file}")
        print("=" * 50)
        
        with open(output_file, 'w', encoding='utf-8') as file:
            file.write('\n'.join(cleaned_lines))
        
        print(f"✅ Đã ghi file thành công!")
        print(f"📁 File kết quả: {output_file}")
        
        # Thống kê kết quả
        print(f"\n📊 THỐNG KÊ KẾT QUẢ:")
        print("=" * 60)
        print(f"✅ Tổng số dòng: {len(lines)}")
        print(f"✅ Số dòng đã làm sạch: {total_cleaned}")
        
        # Hiển thị mẫu kết quả
        print(f"\n📋 MẪU KẾT QUẢ LÀM SẠCH:")
        print("=" * 60)
        
        for i, (original, cleaned) in enumerate(zip(lines[:5], cleaned_lines[:5])):
            if original.strip() and original.strip() != "Chuyển sang tiếng việt" and original.strip() != "Description":
                print(f"\n🔸 MẪU {i+1}:")
                print(f"   Trước: {original[:100]}...")
                print(f"   Sau: {cleaned[:100]}...")
                print("   " + "-" * 40)
        
        return True
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")
        return False

def main():
    print("🎯 SCRIPT LÀM SẠCH BẢN DỊCH")
    print("=" * 60)
    
    # Thực hiện làm sạch
    success = clean_translations()
    
    if success:
        print("\n🎉 HOÀN THÀNH LÀM SẠCH!")
        print("=" * 60)
        print("✅ Đã loại bỏ 'Đạo Sử Dụng' và các lỗi dịch khác")
        print("✅ Đã làm sạch tất cả mô tả sản phẩm")
        
    else:
        print("\n💥 LÀM SẠCH THẤT BẠI!")

if __name__ == "__main__":
    main()
