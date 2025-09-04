import requests
import json
import time

def test_translation_api():
    """Test translation API"""
    print("🧪 KIỂM TRA API DỊCH THUẬT")
    print("=" * 40)
    
    # Test text
    test_text = "This is a luxury watch with excellent condition"
    
    try:
        print(f"📝 Văn bản gốc: {test_text}")
        
        # Sử dụng LibreTranslate API
        url = "https://libretranslate.de/translate"
        
        payload = {
            "q": test_text,
            "source": "en",
            "target": "vi",
            "format": "text"
        }
        
        headers = {
            "Content-Type": "application/json"
        }
        
        print("🔄 Đang gửi yêu cầu dịch...")
        response = requests.post(url, json=payload, headers=headers, timeout=10)
        
        print(f"📊 Status code: {response.status_code}")
        
        if response.status_code == 200:
            result = response.json()
            translated_text = result.get("translatedText", test_text)
            print(f"✅ Dịch thành công: {translated_text}")
            return True
        else:
            print(f"❌ Lỗi API: {response.text}")
            return False
            
    except Exception as e:
        print(f"❌ Lỗi kết nối: {e}")
        return False

def test_database_connection():
    """Test database connection"""
    print("\n🗄️ KIỂM TRA KẾT NỐI DATABASE")
    print("=" * 40)
    
    try:
        import pyodbc
        
        connection_string = "DRIVER={SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;"
        conn = pyodbc.connect(connection_string)
        
        cursor = conn.cursor()
        cursor.execute("SELECT COUNT(*) FROM Products")
        count = cursor.fetchone()[0]
        
        print(f"✅ Kết nối database thành công!")
        print(f"📊 Số sản phẩm: {count}")
        
        # Lấy mẫu dữ liệu
        cursor.execute("SELECT TOP 1 Id, Name, Condition, Gender, Certificate, WarrantyInfo FROM Products")
        sample = cursor.fetchone()
        
        if sample:
            print("📦 Mẫu dữ liệu:")
            print(f"  ID: {sample[0]}")
            print(f"  Name: {sample[1]}")
            print(f"  Condition: {sample[2]}")
            print(f"  Gender: {sample[3]}")
            print(f"  Certificate: {sample[4]}")
            print(f"  WarrantyInfo: {sample[5][:100] if sample[5] else 'None'}...")
        
        cursor.close()
        conn.close()
        return True
        
    except Exception as e:
        print(f"❌ Lỗi database: {e}")
        return False

def main():
    print("🚀 BẮT ĐẦU KIỂM TRA")
    print("=" * 50)
    
    # Test translation API
    translation_ok = test_translation_api()
    
    # Test database connection
    database_ok = test_database_connection()
    
    print("\n📋 KẾT QUẢ KIỂM TRA:")
    print("=" * 50)
    print(f"🌐 API Dịch thuật: {'✅ OK' if translation_ok else '❌ LỖI'}")
    print(f"🗄️ Database: {'✅ OK' if database_ok else '❌ LỖI'}")
    
    if translation_ok and database_ok:
        print("\n🎯 TẤT CẢ OK! Có thể chạy script việt hóa.")
    else:
        print("\n⚠️ CÓ LỖI! Cần sửa trước khi chạy việt hóa.")

if __name__ == "__main__":
    main()
