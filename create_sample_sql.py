def create_sample_sql():
    print("📝 TẠO FILE SQL MẪU ĐỂ TEST")
    print("=" * 50)
    
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

INSERT [dbo].[Products] ([Id], [Name], [Slug], [Description], [Price], [BrandId], [CategoryId], [Image], [Quantity], [Sold], [CapitalPrice], [IsActive], [Model], [ModelNumber], [Year], [Gender], [Condition], [CaseMaterial], [CaseSize], [Crystal], [BezelMaterial], [SerialNumber], [DialColor], [HourMarkers], [Calibre], [MovementType], [Complication], [BraceletMaterial], [BraceletType], [ClaspType], [BoxAndPapers], [Certificate], [WarrantyInfo], [ItemNumber], [CreditCardPrice], [CreatedDate], [UpdatedDate], [ScrapedAt], [SourceUrl]) VALUES (3, N'Pre-owned Breitling Chronomat B01 42 Grey Dial', N'pre-owned-breitling-chronomat-b01-42-grey-dial', N'Used Breitling Chronomat B01 42 ref EB0134101M1S1 (2024) hails from the brand newest collection of Chronomats, released in 2020. The Chronomat B01 is an homage of sorts to the model released in the 1980s, featuring a similar rouleaux-style bracelet, an onion-shaped crown, the B01 in-house movement, and a rider tab bezel.', CAST(171377500.00 AS Decimal(18, 2)), 17, 287, N'https://images.bobswatches.com/breitling/images/Used-Breitling-Chronomat-EB0134101M1S1-Grey-Index-Dial-SKU180401.jpg', 1, 0, CAST(137102000.00 AS Decimal(18, 2)), 1, N'Chronomat', N'EB0134101M1S1', 2024, N'Male', N'Excellent', N'Titanium', N'42MM', N'Sapphire', N'Titanium Timing', N'5354XXX', N'Slate w/ Index hour markers, Luminous hands & Lumi', N'Index hour markers', N'Breitling Caliber 01', N'Automatic', N'Chronograph', N'Rubber', N'Rubber', N'Deployant', 1, N'Certified by CSA Watches - Leading independent watch authentication provider', N'This used Breitling comes with valid warranty from Breitling until 04/30/2030, plus 1 year warranty from Bob Watches', N'180401', CAST(176347570.00 AS Decimal(18, 2)), CAST(N'2025-08-13T01:02:32.1123060' AS DateTime2), CAST(N'2025-08-21T20:52:51.2700000' AS DateTime2), CAST(N'2025-08-13T01:02:32.1123060' AS DateTime2), N'https://www.bobswatches.com/breitling/pre-owned-breitling-chronomat-b01-42-grey-dial.html')

SET IDENTITY_INSERT [dbo].[Products] OFF
"""
    
    try:
        with open('Products_Sample.sql', 'w', encoding='utf-8') as file:
            file.write(sample_sql)
        
        print("✅ Đã tạo file Products_Sample.sql thành công!")
        print("📁 File chứa 3 sản phẩm mẫu để test dịch thuật")
        
    except Exception as e:
        print(f"❌ Lỗi tạo file: {e}")

if __name__ == "__main__":
    create_sample_sql()
