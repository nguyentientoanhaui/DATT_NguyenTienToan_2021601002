def debug_insert_statements():
    print("🔍 DEBUG INSERT STATEMENTS")
    print("=" * 50)
    
    try:
        # Đọc file SQL
        with open('Products_Sample.sql', 'r', encoding='utf-8') as file:
            content = file.read()
        
        print(f"✅ Đã đọc file! Kích thước: {len(content)} ký tự")
        
        # Tìm INSERT statements bằng cách tách dòng
        lines = content.split('\n')
        print(f"📄 Tổng số dòng: {len(lines)}")
        
        insert_lines = []
        for i, line in enumerate(lines):
            if 'INSERT' in line.upper():
                insert_lines.append((i+1, line.strip()))
        
        print(f"🔍 Tìm thấy {len(insert_lines)} dòng có INSERT:")
        for line_num, line in insert_lines:
            print(f"  Dòng {line_num}: {line[:100]}...")
        
        # Tìm INSERT statements hoàn chỉnh
        insert_statements = []
        current_statement = ""
        in_insert = False
        
        for i, line in enumerate(lines):
            if 'INSERT' in line.upper() and 'PRODUCTS' in line.upper():
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
        
        print(f"\n📋 Tìm thấy {len(insert_statements)} INSERT statements hoàn chỉnh:")
        for i, statement in enumerate(insert_statements):
            print(f"\n🔸 INSERT {i+1}:")
            print(f"   {statement[:200]}...")
            print(f"   Độ dài: {len(statement)} ký tự")
        
        # Kiểm tra VALUES trong statement đầu tiên
        if insert_statements:
            first_statement = insert_statements[0]
            print(f"\n🔍 Phân tích statement đầu tiên:")
            
            # Tìm VALUES
            values_start = first_statement.find('VALUES')
            if values_start != -1:
                print(f"   ✅ Tìm thấy VALUES tại vị trí: {values_start}")
                values_part = first_statement[values_start:]
                print(f"   📄 Phần VALUES: {values_part[:200]}...")
                
                # Tìm dấu ngoặc đóng cuối cùng
                last_paren = values_part.rfind(')')
                if last_paren != -1:
                    print(f"   ✅ Tìm thấy dấu ngoặc đóng tại vị trí: {last_paren}")
                    complete_values = values_part[:last_paren+1]
                    print(f"   📄 VALUES hoàn chỉnh: {complete_values[:200]}...")
                else:
                    print(f"   ❌ Không tìm thấy dấu ngoặc đóng")
            else:
                print(f"   ❌ Không tìm thấy VALUES")
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")

if __name__ == "__main__":
    debug_insert_statements()
