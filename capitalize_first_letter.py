import re
import os

def capitalize_first_letter():
    """Viết hoa chữ cái đầu tiên của mỗi đoạn văn"""
    print("🔤 BẮT ĐẦU VIẾT HOA CHỮ CÁI ĐẦU TIÊN")
    print("=" * 60)
    
    # Kiểm tra file
    input_file = "Mô_tả_sản_phẩm_Tiếng_Việt_Đã_Làm_Sạch.txt"
    if not os.path.exists(input_file):
        print(f"❌ Không tìm thấy file {input_file}")
        return False
    
    try:
        # Đọc file
        print(f"📖 Đang đọc file {input_file}...")
        with open(input_file, 'r', encoding='utf-8') as file:
            lines = file.readlines()
        
        print(f"✅ Đã đọc file! Tổng số dòng: {len(lines)}")
        
        # Xử lý từng dòng
        processed_lines = []
        total_processed = 0
        
        print(f"\n🔤 BẮT ĐẦU XỬ LÝ {len(lines)} DÒNG...")
        print("=" * 60)
        
        for i, line in enumerate(lines):
            line = line.strip()
            
            if not line:
                processed_lines.append("")
                continue
            
            # Bỏ qua dòng tiêu đề
            if line == "Chuyển sang tiếng việt" or line == "Description":
                processed_lines.append(line)
                continue
            
            # Viết hoa chữ cái đầu tiên của đoạn văn
            processed_line = line
            
            # Tìm chữ cái đầu tiên và viết hoa
            if processed_line and processed_line[0].islower():
                processed_line = processed_line[0].upper() + processed_line[1:]
                print(f"  [{i+1}/{len(lines)}] Đã viết hoa: {line[:50]}... → {processed_line[:50]}...")
                total_processed += 1
            
            processed_lines.append(processed_line)
            
            # Hiển thị tiến độ
            if (i + 1) % 100 == 0:
                print(f"📊 Tiến độ: {i+1}/{len(lines)} ({((i+1)/len(lines)*100):.1f}%)")
        
        # Ghi file kết quả
        output_file = "Mô_tả_sản_phẩm_Tiếng_Việt_Hoàn_Chỉnh.txt"
        print(f"\n💾 GHI FILE KẾT QUẢ: {output_file}")
        print("=" * 50)
        
        with open(output_file, 'w', encoding='utf-8') as file:
            file.write('\n'.join(processed_lines))
        
        print(f"✅ Đã ghi file thành công!")
        print(f"📁 File kết quả: {output_file}")
        
        # Thống kê kết quả
        print(f"\n📊 THỐNG KÊ KẾT QUẢ:")
        print("=" * 60)
        print(f"✅ Tổng số dòng: {len(lines)}")
        print(f"✅ Số dòng đã viết hoa: {total_processed}")
        
        # Hiển thị mẫu kết quả
        print(f"\n📋 MẪU KẾT QUẢ:")
        print("=" * 60)
        
        for i, (original, processed) in enumerate(zip(lines[:5], processed_lines[:5])):
            if original.strip() and original.strip() != "Chuyển sang tiếng việt" and original.strip() != "Description":
                print(f"\n🔸 MẪU {i+1}:")
                print(f"   Trước: {original[:100]}...")
                print(f"   Sau: {processed[:100]}...")
                print("   " + "-" * 40)
        
        return True
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")
        return False

def main():
    print("🎯 SCRIPT VIẾT HOA CHỮ CÁI ĐẦU TIÊN")
    print("=" * 60)
    
    # Thực hiện xử lý
    success = capitalize_first_letter()
    
    if success:
        print("\n🎉 HOÀN THÀNH!")
        print("=" * 60)
        print("✅ Đã viết hoa chữ cái đầu tiên của mỗi đoạn văn")
        print("✅ File đã sẵn sàng sử dụng")
        
    else:
        print("\n💥 XỬ LÝ THẤT BẠI!")

if __name__ == "__main__":
    main()
