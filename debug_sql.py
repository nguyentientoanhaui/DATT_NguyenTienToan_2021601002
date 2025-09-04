import re

def debug_sql_file():
    print("🔍 DEBUG FILE PRODUCTS.SQL")
    print("=" * 50)
    
    try:
        # Đọc file với encoding UTF-8
        with open('Products.sql', 'r', encoding='utf-8', errors='ignore') as file:
            content = file.read()
        
        print(f"✅ Đã đọc file! Kích thước: {len(content)} ký tự")
        
        # Tìm INSERT statements với nhiều pattern khác nhau
        patterns = [
            r"INSERT\s+\[dbo\]\.\[Products\].*?\);",
            r"INSERT\s+\[dbo\]\.\[Products\].*?VALUES",
            r"INSERT.*?Products.*?VALUES",
            r"INSERT.*?\[Products\].*?VALUES"
        ]
        
        for i, pattern in enumerate(patterns):
            matches = re.findall(pattern, content, re.IGNORECASE | re.DOTALL)
            print(f"Pattern {i+1}: Tìm thấy {len(matches)} matches")
            if matches:
                print(f"  Ví dụ: {matches[0][:100]}...")
        
        # Tìm dòng đầu tiên có INSERT
        lines = content.split('\n')
        insert_lines = []
        for i, line in enumerate(lines):
            if 'INSERT' in line.upper() and 'PRODUCTS' in line.upper():
                insert_lines.append((i+1, line.strip()))
                if len(insert_lines) >= 3:
                    break
        
        print(f"\n📋 Tìm thấy {len(insert_lines)} dòng INSERT Products:")
        for line_num, line in insert_lines:
            print(f"  Dòng {line_num}: {line[:80]}...")
        
        # Kiểm tra cấu trúc VALUES
        if insert_lines:
            first_insert_line = insert_lines[0][1]
            print(f"\n🔍 Phân tích dòng INSERT đầu tiên:")
            print(f"  {first_insert_line}")
            
            # Tìm VALUES
            values_match = re.search(r'VALUES\s*\(', first_insert_line, re.IGNORECASE)
            if values_match:
                print(f"  ✅ Tìm thấy VALUES tại vị trí: {values_match.start()}")
            else:
                print(f"  ❌ Không tìm thấy VALUES")
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")

if __name__ == "__main__":
    debug_sql_file()
