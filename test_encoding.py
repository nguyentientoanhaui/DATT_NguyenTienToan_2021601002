import chardet

def test_file_encoding():
    print("🔍 KIỂM TRA ENCODING FILE PRODUCTS.SQL")
    print("=" * 50)
    
    try:
        # Đọc file dưới dạng bytes
        with open('Products.sql', 'rb') as file:
            raw_data = file.read()
        
        # Detect encoding
        result = chardet.detect(raw_data)
        print(f"✅ Detected encoding: {result}")
        
        # Thử các encoding khác nhau
        encodings = ['utf-8', 'utf-16', 'utf-16le', 'utf-16be', 'cp1252', 'iso-8859-1']
        
        for encoding in encodings:
            try:
                content = raw_data.decode(encoding)
                insert_count = content.upper().count('INSERT')
                products_count = content.upper().count('PRODUCTS')
                print(f"✅ {encoding}: INSERT={insert_count}, PRODUCTS={products_count}")
                
                if insert_count > 0 and products_count > 0:
                    # Tìm dòng đầu tiên có INSERT
                    lines = content.split('\n')
                    for i, line in enumerate(lines):
                        if 'INSERT' in line.upper() and 'PRODUCTS' in line.upper():
                            print(f"  📄 Dòng {i+1}: {line[:80]}...")
                            break
                    break
                    
            except Exception as e:
                print(f"❌ {encoding}: {e}")
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")

if __name__ == "__main__":
    test_file_encoding()
