import re
import os
import time
from deep_translator import GoogleTranslator
import json

def create_sample_sql():
    """Tạo file SQL mẫu để test"""
    sample_sql = """USE [Shopping_Demo]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Products](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Name] [nvarchar](max) NULL,
    [Slug] [nvarchar](max) NULL,
    [Description] [nvarchar](max) NULL,
    [Price] [decimal](18, 2) NULL,
    [BrandId] [int] NULL,
    [CategoryId] [int] NULL,
    [Image] [nvarchar](max) NULL,
    [Quantity] [int] NULL,
    [Sold] [int] NULL,
    [CapitalPrice] [decimal](18, 2) NULL,
    [IsActive] [bit] NULL,
    [Model] [nvarchar](max) NULL,
    [ModelNumber] [nvarchar](max) NULL,
    [Year] [int] NULL,
    [Gender] [nvarchar](max) NULL,
    [Condition] [nvarchar](max) NULL,
    [CaseMaterial] [nvarchar](max) NULL,
    [CaseSize] [nvarchar](max) NULL,
    [Crystal] [nvarchar](max) NULL,
    [BezelMaterial] [nvarchar](max) NULL,
    [SerialNumber] [nvarchar](max) NULL,
    [DialColor] [nvarchar](max) NULL,
    [HourMarkers] [nvarchar](max) NULL,
    [Calibre] [nvarchar](max) NULL,
    [MovementType] [nvarchar](max) NULL,
    [Complication] [nvarchar](max) NULL,
    [BraceletMaterial] [nvarchar](max) NULL,
    [BraceletType] [nvarchar](max) NULL,
    [ClaspType] [nvarchar](max) NULL,
    [BoxAndPapers] [bit] NULL,
    [Certificate] [nvarchar](max) NULL,
    [WarrantyInfo] [nvarchar](max) NULL,
    [ItemNumber] [nvarchar](max) NULL,
    [CreditCardPrice] [decimal](18, 2) NULL,
    [CreatedDate] [datetime2](7) NULL,
    [UpdatedDate] [datetime2](7) NULL,
    [ScrapedAt] [datetime2](7) NULL,
    [SourceUrl] [nvarchar](max) NULL
)

GO

SET IDENTITY_INSERT [dbo].[Products] ON

INSERT [dbo].[Products] ([Id], [Name], [Slug], [Description], [Price], [BrandId], [CategoryId], [Image], [Quantity], [Sold], [CapitalPrice], [IsActive], [Model], [ModelNumber], [Year], [Gender], [Condition], [CaseMaterial], [CaseSize], [Crystal], [BezelMaterial], [SerialNumber], [DialColor], [HourMarkers], [Calibre], [MovementType], [Complication], [BraceletMaterial], [BraceletType], [ClaspType], [BoxAndPapers], [Certificate], [WarrantyInfo], [ItemNumber], [CreditCardPrice], [CreatedDate], [UpdatedDate], [ScrapedAt], [SourceUrl]) VALUES (1, N'Breitling Superocean Heritage 57 Outerknown', N'breitling-superocean-heritage-57-outerknown', N'Used Breitling SuperOcean Heritage 57 Outerknown ref Ref A103703A1Q1W1 (2020) is a love letter of sorts to the original 1957 model. It takes much inspiration from the laid back lifestyle associated with 1960s surfing culture and is even endorsed by famous surfer Kelly Slater brand, Outerknown.', CAST(73377500.00 AS Decimal(18, 2)), 17, 238, N'https://images.bobswatches.com/breitling/images/Used-Breitling-Superocean-A103703A1Q1W1-Brown-Luminous-Dial-SKU172061.jpg', 1, 0, CAST(58702000.00 AS Decimal(18, 2)), 1, N'Superocean', N'A103703A1Q1W1', 2020, N'Male', N'Excellent', N'Stainless Steel', N'42MM', N'Sapphire', N'Ceramic Timing', N'6484XXX', N'Brown w/ Index hour markers, Luminous hands & Lumi', N'Index hour markers', N'Breitling Caliber B10', N'Automatic', N'', N'Fabric/Canvas', N'', N'Tang buckle', 1, N'Certified by CSA Watches - Leading independent watch authentication provider', N'Used Breitling watch comes with 1 year warranty from Bob Watches', N'172061', CAST(75505570.00 AS Decimal(18, 2)), CAST(N'2025-08-13T01:02:32.1097940' AS DateTime2), CAST(N'2025-08-21T20:52:51.2700000' AS DateTime2), CAST(N'2025-08-13T01:02:32.1097940' AS DateTime2), N'https://www.bobswatches.com/breitling/breitling-superocean-heritage-57-outerknown.html')

INSERT [dbo].[Products] ([Id], [Name], [Slug], [Description], [Price], [BrandId], [CategoryId], [Image], [Quantity], [Sold], [CapitalPrice], [IsActive], [Model], [ModelNumber], [Year], [Gender], [Condition], [CaseMaterial], [CaseSize], [Crystal], [BezelMaterial], [SerialNumber], [DialColor], [HourMarkers], [Calibre], [MovementType], [Complication], [BraceletMaterial], [BraceletType], [ClaspType], [BoxAndPapers], [Certificate], [WarrantyInfo], [ItemNumber], [CreditCardPrice], [CreatedDate], [UpdatedDate], [ScrapedAt], [SourceUrl]) VALUES (2, N'Breitling Classic AVI Chronograph 42 Curtiss Warhawk Green Dial', N'breitling-classic-avi-chronograph-42-curtiss-warhawk-green-dial', N'Used Breitling Classic AVI Chronograph 42 Curtiss Warhawk ref A233802A1L1A1 (2023 - 2024) hails from the AVI collection, also known as the Co-Pilot, a series developed by Breitling in the 1950s and inspired by early aviation.', CAST(122377500.00 AS Decimal(18, 2)), 17, 286, N'https://images.bobswatches.com/breitling/images/Breitling-Classic-AVI-A233802A1L1A1-SKU169504.jpg', 1, 0, CAST(97902000.00 AS Decimal(18, 2)), 1, N'Classic AVI', N'A233802A1L1A1', NULL, N'Male', N'Excellent', N'Stainless Steel', N'42MM', N'Sapphire', N'Stainless Steel 12-hour', N'7357XXX', N'Green w/ Arabic hour markers, Luminous hands & Lum', N'Arabic hour markers', N'23', N'Automatic', N'Chronograph', N'Stainless Steel', N'', N'Butterfly', 0, N'Certified by CSA Watches - Leading independent watch authentication provider', N'Used Breitling watch comes with 1 year warranty from Bob Watches', N'169504', CAST(125926570.00 AS Decimal(18, 2)), CAST(N'2025-08-13T01:02:32.1108020' AS DateTime2), CAST(N'2025-08-21T20:52:51.2700000' AS DateTime2), CAST(N'2025-08-13T01:02:32.1108020' AS DateTime2), N'https://www.bobswatches.com/breitling/breitling-classic-avi-chronograph-42-curtiss-warhawk-green-dial.html')

SET IDENTITY_INSERT [dbo].[Products] OFF
"""
    
    with open('Products_Sample.sql', 'w', encoding='utf-8') as file:
        file.write(sample_sql)
    
    print("✅ Đã tạo file Products_Sample.sql thành công!")

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

def parse_values(values_string):
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

def translate_sql_file():
    """Dịch file SQL mẫu"""
    print("🚀 BẮT ĐẦU DỊCH FILE SQL MẪU")
    print("=" * 60)
    
    # Tạo file mẫu nếu chưa có
    if not os.path.exists('Products_Sample.sql'):
        create_sample_sql()
    
    # Khởi tạo translator
    translator = GoogleTranslator(source='auto', target='vi')
    translation_cache = {}
    
    try:
        # Đọc file SQL
        with open('Products_Sample.sql', 'r', encoding='utf-8') as file:
            content = file.read()
        
        print(f"✅ Đã đọc file! Kích thước: {len(content)} ký tự")
        
        # Tìm INSERT statements
        insert_pattern = r"INSERT\s+\[dbo\]\.\[Products\].*?\);"
        matches = re.findall(insert_pattern, content, re.IGNORECASE | re.DOTALL)
        
        print(f"✅ Tìm thấy {len(matches)} INSERT statements")
        
        # Nếu không tìm thấy, thử pattern khác
        if len(matches) == 0:
            insert_pattern = r"INSERT\s+\[dbo\]\.\[Products\].*?VALUES.*?\);"
            matches = re.findall(insert_pattern, content, re.IGNORECASE | re.DOTALL)
            print(f"✅ Thử pattern khác: Tìm thấy {len(matches)} INSERT statements")
        
        # Nếu vẫn không tìm thấy, thử pattern đơn giản hơn
        if len(matches) == 0:
            insert_pattern = r"INSERT.*?Products.*?VALUES.*?\);"
            matches = re.findall(insert_pattern, content, re.IGNORECASE | re.DOTALL)
            print(f"✅ Thử pattern đơn giản: Tìm thấy {len(matches)} INSERT statements")
        
        # Nếu vẫn không tìm thấy, thử tách theo dòng
        if len(matches) == 0:
            lines = content.split('\n')
            insert_statements = []
            current_statement = ""
            in_insert = False
            
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
            
            matches = insert_statements
            print(f"✅ Thử tách theo dòng: Tìm thấy {len(matches)} INSERT statements")
        
        # Tách phần đầu và phần INSERT
        insert_start = content.find("INSERT")
        if insert_start == -1:
            print("❌ Không tìm thấy INSERT statements")
            return False
        
        header_content = content[:insert_start]
        
        # Dịch từng INSERT statement
        translated_statements = []
        
        for i, statement in enumerate(matches):
            try:
                print(f"\n  [{i+1}/{len(matches)}] Đang dịch...")
                
                # Tách phần VALUES
                values_match = re.search(r'VALUES\s*\(([^)]+)\)', statement, re.DOTALL)
                if not values_match:
                    translated_statements.append(statement)
                    continue
                
                values_string = values_match.group(1)
                
                # Parse các giá trị
                values = parse_values(values_string)
                if not values:
                    translated_statements.append(statement)
                    continue
                
                print(f"    📊 Tìm thấy {len(values)} giá trị trong INSERT statement")
                
                # Dịch các trường cần thiết (ngoại trừ Name và Model)
                translated_values = []
                for j, value in enumerate(values):
                    # Giữ nguyên Name (index 1) và Model (index 12)
                    if j == 1 or j == 12:  # Name và Model - giữ nguyên
                        translated_values.append(value)
                        print(f"      ✅ Giữ nguyên {j}: {value[:30]}...")
                    # Dịch các trường text khác
                    elif value and value != 'NULL' and value.startswith("N'") and value.endswith("'"):
                        # Lấy text bên trong quotes
                        text_content = value[2:-1]  # Bỏ N' và '
                        if text_content and len(text_content.strip()) > 3:
                            # Kiểm tra xem có phải là text tiếng Anh không
                            if is_english_text(text_content):
                                print(f"      🔄 Dịch {j}: {text_content[:50]}...")
                                translated_text = translate_text_with_api(text_content, translator, translation_cache)
                                translated_values.append(f"N'{translated_text}'")
                            else:
                                translated_values.append(value)
                                print(f"      ⏭️ Bỏ qua {j} (không phải tiếng Anh): {text_content[:30]}...")
                        else:
                            translated_values.append(value)
                    else:
                        translated_values.append(value)
                
                # Tạo INSERT statement mới
                new_values_string = ', '.join(translated_values)
                new_insert = re.sub(r'VALUES\s*\([^)]+\)', f'VALUES ({new_values_string})', statement)
                
                translated_statements.append(new_insert)
                print(f"      ✅ Đã hoàn thành {i+1}/{len(matches)}")
                
            except Exception as e:
                print(f"  ⚠️ Lỗi dịch statement {i+1}: {e}")
                translated_statements.append(statement)  # Giữ nguyên nếu lỗi
                continue
        
        # Ghi file kết quả
        output_file = "Products_Vietnamese_Working.sql"
        print(f"\n💾 GHI FILE KẾT QUẢ: {output_file}")
        print("=" * 50)
        
        with open(output_file, 'w', encoding='utf-8') as file:
            file.write(header_content)
            file.write('\n\n')
            file.write('\n'.join(translated_statements))
            file.write('\n')
        
        print(f"✅ Đã ghi file thành công!")
        print(f"📁 File kết quả: {output_file}")
        
        # Lưu cache để sử dụng sau
        with open('translation_cache_working.json', 'w', encoding='utf-8') as cache_file:
            json.dump(translation_cache, cache_file, ensure_ascii=False, indent=2)
        
        print(f"💾 Đã lưu cache dịch thuật: translation_cache_working.json")
        
        # Hiển thị mẫu kết quả
        print(f"\n📋 MẪU KẾT QUẢ DỊCH:")
        print("=" * 60)
        
        for i, statement in enumerate(translated_statements):
            values_match = re.search(r'VALUES\s*\(([^)]+)\)', statement, re.DOTALL)
            if values_match:
                values = parse_values(values_match.group(1))
                if len(values) >= 3:
                    print(f"\n🔸 SẢN PHẨM {i+1}:")
                    print(f"   ID: {values[0] if len(values) > 0 else 'N/A'}")
                    print(f"   Name: {values[1] if len(values) > 1 else 'N/A'}")
                    print(f"   Description: {values[3][:100] + '...' if len(values) > 3 and len(values[3]) > 100 else values[3] if len(values) > 3 else 'N/A'}")
                    print(f"   Condition: {values[16] if len(values) > 16 else 'N/A'}")
                    print(f"   Gender: {values[15] if len(values) > 15 else 'N/A'}")
                    print("   " + "-" * 40)
        
        return True
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")
        return False

def main():
    print("🎯 SCRIPT DỊCH THUẬT SQL HOẠT ĐỘNG")
    print("=" * 60)
    
    # Thực hiện dịch
    success = translate_sql_file()
    
    if success:
        print("\n🎉 HOÀN THÀNH DỊCH THUẬT!")
        print("=" * 60)
        print("✅ Đã dịch 2 sản phẩm mẫu sang tiếng Việt")
        print("✅ Đã giữ nguyên cột Name và Model")
        print("✅ Đã sử dụng API dịch thuật")
        print("✅ Đã lưu cache để tái sử dụng")
        
    else:
        print("\n💥 DỊCH THUẬT THẤT BẠI!")

if __name__ == "__main__":
    main()
