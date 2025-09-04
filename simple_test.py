import re

def test_sql_file():
    print("🔍 KIỂM TRA FILE PRODUCTS.SQL")
    print("=" * 50)
    
    try:
        # Đọc file với encoding UTF-8
        with open('Products.sql', 'r', encoding='utf-8', errors='ignore') as file:
            content = file.read()
        
        print(f"✅ Đã đọc file! Kích thước: {len(content)} ký tự")
        
        # Tìm INSERT statements với pattern đơn giản
        insert_pattern = r"INSERT\s+\[dbo\]\.\[Products\]"
        matches = re.findall(insert_pattern, content, re.IGNORECASE)
        
        print(f"✅ Tìm thấy {len(matches)} INSERT statements với pattern đơn giản")
        
        # Tìm INSERT statements với pattern phức tạp hơn
        insert_pattern2 = r"INSERT\s+\[dbo\]\.\[Products\].*?VALUES"
        matches2 = re.findall(insert_pattern2, content, re.IGNORECASE | re.DOTALL)
        
        print(f"✅ Tìm thấy {len(matches2)} INSERT statements với pattern phức tạp")
        
        # Tìm dòng đầu tiên có INSERT
        lines = content.split('\n')
        insert_lines = [i+1 for i, line in enumerate(lines) if 'INSERT' in line.upper()]
        
        if insert_lines:
            print(f"✅ INSERT statements bắt đầu từ dòng: {insert_lines[0]}")
            print(f"📄 Dòng đầu tiên: {lines[insert_lines[0]-1][:100]}...")
        else:
            print("❌ Không tìm thấy INSERT statements")
        
        # Hiển thị 10 dòng đầu tiên có INSERT
        print("\n📋 10 DÒNG ĐẦU TIÊN CÓ INSERT:")
        for i, line in enumerate(lines):
            if 'INSERT' in line.upper():
                print(f"  Dòng {i+1}: {line[:80]}...")
                if len([l for l in lines[:i+1] if 'INSERT' in l.upper()]) >= 10:
                    break
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")

if __name__ == "__main__":
    test_sql_file()
