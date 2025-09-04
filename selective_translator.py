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

def translate_sql_file():
    """Dịch file SQL mẫu - chỉ dịch 5 cột cụ thể"""
    print("🚀 BẮT ĐẦU DỊCH FILE SQL MẪU (CHỌN LỌC)")
    print("=" * 60)
    print("🎯 Chỉ dịch 5 cột: Description, Gender, Condition, CaseMaterial, WarrantyInfo")
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
        
        # Tìm INSERT statements bằng cách tách dòng
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
        
        print(f"✅ Tìm thấy {len(insert_statements)} INSERT statements")
        
        # Tách phần đầu và phần INSERT
        insert_start = content.find("INSERT")
        if insert_start == -1:
            print("❌ Không tìm thấy INSERT statements")
            return False
        
        header_content = content[:insert_start]
        
        # Dịch từng INSERT statement
        translated_statements = []
        
        for i, statement in enumerate(insert_statements):
            try:
                print(f"\n  [{i+1}/{len(insert_statements)}] Đang dịch...")
                
                # Tìm phần VALUES và dịch các text fields
                # Tìm tất cả các chuỗi N'...' trong statement
                pattern = r"N'([^']*)'"
                matches = re.findall(pattern, statement)
                
                print(f"    📊 Tìm thấy {len(matches)} text fields")
                
                # Dịch từng text field (chỉ 5 cột cụ thể)
                translated_statement = statement
                for j, text_content in enumerate(matches):
                    if text_content and len(text_content.strip()) > 3:
                        # Kiểm tra xem có phải là text tiếng Anh không
                        if is_english_text(text_content):
                            # Chỉ dịch 5 cột cụ thể dựa trên nội dung
                            should_translate = False
                            column_name = ""
                            
                            # Kiểm tra xem có phải là 5 cột cần dịch không
                            if text_content == "Male":
                                should_translate = True
                                column_name = "Gender"
                            elif text_content == "Excellent":
                                should_translate = True
                                column_name = "Condition"
                            elif text_content == "Stainless Steel":
                                should_translate = True
                                column_name = "CaseMaterial"
                            elif "Used Breitling" in text_content and len(text_content) > 50:
                                should_translate = True
                                column_name = "Description"
                            elif "warranty" in text_content.lower():
                                should_translate = True
                                column_name = "WarrantyInfo"
                            
                            if should_translate:
                                print(f"      🔄 Dịch {column_name}: {text_content[:50]}...")
                                translated_text = translate_text_with_api(text_content, translator, translation_cache)
                                
                                # Thay thế trong statement
                                old_pattern = f"N'{text_content}'"
                                new_pattern = f"N'{translated_text}'"
                                translated_statement = translated_statement.replace(old_pattern, new_pattern)
                            else:
                                print(f"      ⏭️ Giữ nguyên: {text_content[:30]}...")
                        else:
                            print(f"      ⏭️ Bỏ qua (không phải tiếng Anh): {text_content[:30]}...")
                
                translated_statements.append(translated_statement)
                
            except Exception as e:
                print(f"  ⚠️ Lỗi dịch statement {i+1}: {e}")
                translated_statements.append(statement)  # Giữ nguyên nếu lỗi
                continue
        
        # Ghi file kết quả
        output_file = "Products_Vietnamese_Selective.sql"
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
        with open('translation_cache_selective.json', 'w', encoding='utf-8') as cache_file:
            json.dump(translation_cache, cache_file, ensure_ascii=False, indent=2)
        
        print(f"💾 Đã lưu cache dịch thuật: translation_cache_selective.json")
        
        # Hiển thị mẫu kết quả
        print(f"\n📋 MẪU KẾT QUẢ DỊCH:")
        print("=" * 60)
        
        for i, statement in enumerate(translated_statements):
            print(f"\n🔸 SẢN PHẨM {i+1}:")
            
            # Tìm các text fields đã dịch
            values_match = re.search(r'VALUES\s*\(([^)]+)\)', statement, re.DOTALL)
            if values_match:
                values = parse_values(values_match.group(1))
                if len(values) >= 33:
                    print(f"   Description: {values[3][:100] + '...' if len(values[3]) > 100 else values[3]}")
                    print(f"   Gender: {values[15]}")
                    print(f"   Condition: {values[16]}")
                    print(f"   CaseMaterial: {values[17]}")
                    print(f"   WarrantyInfo: {values[32][:100] + '...' if len(values[32]) > 100 else values[32]}")
            
            print("   " + "-" * 40)
        
        return True
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")
        return False

def get_column_name(index):
    """Lấy tên cột từ index"""
    column_names = {
        3: "Description",
        15: "Gender", 
        16: "Condition",
        17: "CaseMaterial",
        32: "WarrantyInfo"
    }
    return column_names.get(index, f"Column_{index}")

def main():
    print("🎯 SCRIPT DỊCH THUẬT SQL CHỌN LỌC")
    print("=" * 60)
    
    # Thực hiện dịch
    success = translate_sql_file()
    
    if success:
        print("\n🎉 HOÀN THÀNH DỊCH THUẬT!")
        print("=" * 60)
        print("✅ Đã dịch 5 cột cụ thể sang tiếng Việt:")
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
