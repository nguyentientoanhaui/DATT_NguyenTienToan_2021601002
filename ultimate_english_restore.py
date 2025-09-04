import pyodbc

def ultimate_restore():
    connection_string = "DRIVER={SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;"
    
    try:
        conn = pyodbc.connect(connection_string)
        cursor = conn.cursor()
        
        print("🚀 ULTIMATE ENGLISH RESTORE - KHÔI PHỤC HOÀN TOÀN:")
        print("=" * 60)
        
        # Các câu lệnh SQL để khôi phục hoàn toàn
        ultimate_queries = [
            # Khôi phục Condition
            "UPDATE Products SET Condition = 'Excellent' WHERE Condition IN ('Xuất sắc', 'Excellent')",
            "UPDATE Products SET Condition = 'Very Good' WHERE Condition = 'Rất tốt'",
            "UPDATE Products SET Condition = 'Good' WHERE Condition = 'Tốt'",
            "UPDATE Products SET Condition = 'Fair' WHERE Condition = 'Khá'",
            "UPDATE Products SET Condition = 'Poor' WHERE Condition = 'Kém'",
            "UPDATE Products SET Condition = 'New' WHERE Condition = 'Mới'",
            "UPDATE Products SET Condition = 'Vintage' WHERE Condition = 'Cổ điển'",
            "UPDATE Products SET Condition = 'Pre-owned' WHERE Condition = 'Đã qua sử dụng'",
            "UPDATE Products SET Condition = 'Unworn' WHERE Condition = 'Chưa đeo'",
            "UPDATE Products SET Condition = 'Mint' WHERE Condition = 'Hoàn hảo'",
            
            # Khôi phục Gender
            "UPDATE Products SET Gender = 'Men' WHERE Gender IN ('Nam', 'Male')",
            "UPDATE Products SET Gender = 'Women' WHERE Gender IN ('Nữ', 'Female')",
            
            # Khôi phục Description - xử lý từng từ một
            "UPDATE Products SET Description = REPLACE(Description, 'Đã sử dụng', 'Pre-owned')",
            "UPDATE Products SET Description = REPLACE(Description, 'di sản', 'heritage')",
            "UPDATE Products SET Description = REPLACE(Description, 'cổ điển', 'classic')",
            "UPDATE Products SET Description = REPLACE(Description, 'mới nhất', 'latest')",
            "UPDATE Products SET Description = REPLACE(Description, 'tuyệt vời', 'excellent')",
            "UPDATE Products SET Description = REPLACE(Description, 'chất lượng', 'quality')",
            "UPDATE Products SET Description = REPLACE(Description, 'thép không gỉ', 'stainless steel')",
            "UPDATE Products SET Description = REPLACE(Description, 'bộ máy tự động', 'automatic movement')",
            "UPDATE Products SET Description = REPLACE(Description, 'bộ máy cơ', 'mechanical movement')",
            "UPDATE Products SET Description = REPLACE(Description, 'bộ máy quartz', 'quartz movement')",
            "UPDATE Products SET Description = REPLACE(Description, 'lên dây tay', 'manual winding')",
            "UPDATE Products SET Description = REPLACE(Description, 'tự động lên dây', 'self-winding')",
            "UPDATE Products SET Description = REPLACE(Description, 'lịch vạn niên', 'perpetual calendar')",
            "UPDATE Products SET Description = REPLACE(Description, 'lịch năm', 'annual calendar')",
            "UPDATE Products SET Description = REPLACE(Description, 'pha mặt trăng', 'moon phase')",
            "UPDATE Products SET Description = REPLACE(Description, 'thang đo tốc độ', 'tachymeter')",
            "UPDATE Products SET Description = REPLACE(Description, 'thang đo khoảng cách', 'telemeter')",
            "UPDATE Products SET Description = REPLACE(Description, 'thước trượt', 'slide rule')",
            "UPDATE Products SET Description = REPLACE(Description, 'vành bezel', 'bezel')",
            "UPDATE Products SET Description = REPLACE(Description, 'núm vặn', 'crown')",
            "UPDATE Products SET Description = REPLACE(Description, 'nút bấm', 'pushers')",
            "UPDATE Products SET Description = REPLACE(Description, 'vỏ máy', 'case')",
            "UPDATE Products SET Description = REPLACE(Description, 'mặt số', 'dial')",
            "UPDATE Products SET Description = REPLACE(Description, 'kim', 'hands')",
            "UPDATE Products SET Description = REPLACE(Description, 'vạch số', 'markers')",
            "UPDATE Products SET Description = REPLACE(Description, 'mặt số phụ', 'sub-dials')",
            "UPDATE Products SET Description = REPLACE(Description, 'chức năng phức tạp', 'complications')",
            "UPDATE Products SET Description = REPLACE(Description, 'bộ máy', 'movement')",
            "UPDATE Products SET Description = REPLACE(Description, 'chân kính', 'jewels')",
            "UPDATE Products SET Description = REPLACE(Description, 'tần số', 'frequency')",
            "UPDATE Products SET Description = REPLACE(Description, 'độ chính xác', 'accuracy')",
            "UPDATE Products SET Description = REPLACE(Description, 'độ tin cậy', 'reliability')",
            "UPDATE Products SET Description = REPLACE(Description, 'độ bền', 'durability')",
            "UPDATE Products SET Description = REPLACE(Description, 'tay nghề thủ công', 'craftsmanship')",
            "UPDATE Products SET Description = REPLACE(Description, 'truyền thống', 'tradition')",
            "UPDATE Products SET Description = REPLACE(Description, 'sự đổi mới', 'innovation')",
            "UPDATE Products SET Description = REPLACE(Description, 'sự xuất sắc', 'excellence')",
            "UPDATE Products SET Description = REPLACE(Description, 'uy tín', 'prestige')",
            "UPDATE Products SET Description = REPLACE(Description, 'địa vị', 'status')",
            "UPDATE Products SET Description = REPLACE(Description, 'xa xỉ', 'luxury')",
            "UPDATE Products SET Description = REPLACE(Description, 'cao cấp', 'premium')",
            "UPDATE Products SET Description = REPLACE(Description, 'độc quyền', 'exclusive')",
            "UPDATE Products SET Description = REPLACE(Description, 'phiên bản giới hạn', 'limited edition')",
            "UPDATE Products SET Description = REPLACE(Description, 'phiên bản đặc biệt', 'special edition')",
            "UPDATE Products SET Description = REPLACE(Description, 'món đồ sưu tầm', 'collector item')",
            "UPDATE Products SET Description = REPLACE(Description, 'món đầu tư', 'investment piece')",
            "UPDATE Products SET Description = REPLACE(Description, 'di sản gia đình', 'heirloom')",
            "UPDATE Products SET Description = REPLACE(Description, 'biểu tượng', 'iconic')",
            "UPDATE Products SET Description = REPLACE(Description, 'huyền thoại', 'legendary')",
            "UPDATE Products SET Description = REPLACE(Description, 'nổi tiếng', 'famous')",
            "UPDATE Products SET Description = REPLACE(Description, 'phổ biến', 'popular')",
            "UPDATE Products SET Description = REPLACE(Description, 'thịnh hành', 'trendy')",
            "UPDATE Products SET Description = REPLACE(Description, 'thời trang', 'fashionable')",
            "UPDATE Products SET Description = REPLACE(Description, 'phong cách', 'stylish')",
            "UPDATE Products SET Description = REPLACE(Description, 'thanh lịch', 'elegant')",
            "UPDATE Products SET Description = REPLACE(Description, 'tinh tế', 'sophisticated')",
            "UPDATE Products SET Description = REPLACE(Description, 'vượt thời gian', 'timeless')",
            "UPDATE Products SET Description = REPLACE(Description, 'hiện đại', 'modern')",
            "UPDATE Products SET Description = REPLACE(Description, 'đương đại', 'contemporary')",
            "UPDATE Products SET Description = REPLACE(Description, 'hoài cổ', 'retro')",
            "UPDATE Products SET Description = REPLACE(Description, 'đã qua sử dụng', 'pre-owned')",
            "UPDATE Products SET Description = REPLACE(Description, 'đã sử dụng', 'used')",
            "UPDATE Products SET Description = REPLACE(Description, 'chưa đeo', 'unworn')",
            "UPDATE Products SET Description = REPLACE(Description, 'tình trạng hoàn hảo', 'mint condition')",
            "UPDATE Products SET Description = REPLACE(Description, 'tình trạng xuất sắc', 'excellent condition')",
            "UPDATE Products SET Description = REPLACE(Description, 'tình trạng rất tốt', 'very good condition')",
            "UPDATE Products SET Description = REPLACE(Description, 'tình trạng tốt', 'good condition')",
            "UPDATE Products SET Description = REPLACE(Description, 'tình trạng khá', 'fair condition')",
            "UPDATE Products SET Description = REPLACE(Description, 'tình trạng kém', 'poor condition')",
            "UPDATE Products SET Description = REPLACE(Description, 'lối vào', 'gateway to')",
            "UPDATE Products SET Description = REPLACE(Description, 'sở hữu', 'ownership')",
            "UPDATE Products SET Description = REPLACE(Description, 'được bọc trong', 'cased in')",
            "UPDATE Products SET Description = REPLACE(Description, 'giá cả phải chăng', 'affordable')",
            "UPDATE Products SET Description = REPLACE(Description, 'có thể nhận ra ngay lập tức', 'instantly recognizable')",
            "UPDATE Products SET Description = REPLACE(Description, 'được ưa chuộng', 'coveted')",
            "UPDATE Products SET Description = REPLACE(Description, 'đồng hồ công sở', 'dress watch')",
            "UPDATE Products SET Description = REPLACE(Description, '5 điểm', '5-point')",
            "UPDATE Products SET Description = REPLACE(Description, 'vương miện', 'coronet')",
            "UPDATE Products SET Description = REPLACE(Description, 'vị trí 12 giờ', '12-hour marker')",
            "UPDATE Products SET Description = REPLACE(Description, 'hiển thị ngày', 'date display')",
            "UPDATE Products SET Description = REPLACE(Description, 'kính lúp Cyclops', 'Cyclops magnifier')",
            "UPDATE Products SET Description = REPLACE(Description, 'ba mắt', 'three-link')",
            "UPDATE Products SET Description = REPLACE(Description, 'dây đeo Oyster', 'Oyster bracelet')",
            "UPDATE Products SET Description = REPLACE(Description, 'sự công nhận', 'recognition')",
            "UPDATE Products SET Description = REPLACE(Description, 'giá đầu vào', 'entry-level price')",
            "UPDATE Products SET Description = REPLACE(Description, 'được bán', 'for sale')",
            "UPDATE Products SET Description = REPLACE(Description, 'mặt đen', 'black dial')",
            "UPDATE Products SET Description = REPLACE(Description, 'hiển thị thời gian', 'displaying the time')",
            "UPDATE Products SET Description = REPLACE(Description, 'qua', 'via')",
            "UPDATE Products SET Description = REPLACE(Description, 'bạc', 'silvered')",
            "UPDATE Products SET Description = REPLACE(Description, 'vạch giờ', 'hour markers')",
            "UPDATE Products SET Description = REPLACE(Description, 'đặt trên', 'set against')",
            "UPDATE Products SET Description = REPLACE(Description, 'sắc nét', 'sharp')",
            "UPDATE Products SET Description = REPLACE(Description, 'giờ', 'o clock')",
            "UPDATE Products SET Description = REPLACE(Description, 'đầu vào', 'entry-level')",
            "UPDATE Products SET Description = REPLACE(Description, 'cấp độ', 'level')",
            "UPDATE Products SET Description = REPLACE(Description, 'giá', 'price')",
            "UPDATE Products SET Description = REPLACE(Description, 'chi phí', 'cost')",
            "UPDATE Products SET Description = REPLACE(Description, 'giá trị', 'value')",
            "UPDATE Products SET Description = REPLACE(Description, 'đáng giá', 'worth')",
            "UPDATE Products SET Description = REPLACE(Description, 'đắt tiền', 'expensive')",
            "UPDATE Products SET Description = REPLACE(Description, 'rẻ', 'cheap')",
            "UPDATE Products SET Description = REPLACE(Description, 'ngân sách', 'budget')",
            "UPDATE Products SET Description = REPLACE(Description, 'hợp lý', 'reasonable')",
            "UPDATE Products SET Description = REPLACE(Description, 'công bằng', 'fair')",
            "UPDATE Products SET Description = REPLACE(Description, 'nổi bật', 'outstanding')",
            "UPDATE Products SET Description = REPLACE(Description, 'ưu việt', 'superior')",
            "UPDATE Products SET Description = REPLACE(Description, 'kém', 'inferior')",
            "UPDATE Products SET Description = REPLACE(Description, 'xấu', 'bad')",
            "UPDATE Products SET Description = REPLACE(Description, 'khủng khiếp', 'terrible')",
            "UPDATE Products SET Description = REPLACE(Description, 'kinh khủng', 'awful')",
            "UPDATE Products SET Description = REPLACE(Description, 'sốc', 'shocking')",
            "UPDATE Products SET Description = REPLACE(Description, 'tuyệt vời', 'amazing')",
            "UPDATE Products SET Description = REPLACE(Description, 'tráng lệ', 'magnificent')",
            "UPDATE Products SET Description = REPLACE(Description, 'tuyệt đẹp', 'gorgeous')",
            "UPDATE Products SET Description = REPLACE(Description, 'đẹp', 'beautiful')",
            "UPDATE Products SET Description = REPLACE(Description, 'đẹp trai', 'handsome')",
            "UPDATE Products SET Description = REPLACE(Description, 'hấp dẫn', 'attractive')",
            "UPDATE Products SET Description = REPLACE(Description, 'quyến rũ', 'charming')",
            "UPDATE Products SET Description = REPLACE(Description, 'mê hoặc', 'enchanting')",
            "UPDATE Products SET Description = REPLACE(Description, 'thú vị', 'interesting')",
            "UPDATE Products SET Description = REPLACE(Description, 'phiêu lưu', 'adventurous')",
            "UPDATE Products SET Description = REPLACE(Description, 'dũng cảm', 'daring')",
            "UPDATE Products SET Description = REPLACE(Description, 'không sợ hãi', 'fearless')",
            "UPDATE Products SET Description = REPLACE(Description, 'anh hùng', 'heroic')",
            "UPDATE Products SET Description = REPLACE(Description, 'cao quý', 'noble')",
            "UPDATE Products SET Description = REPLACE(Description, 'đáng kính', 'honorable')",
            "UPDATE Products SET Description = REPLACE(Description, 'đáng ngưỡng mộ', 'admirable')",
            "UPDATE Products SET Description = REPLACE(Description, 'đáng khen', 'commendable')",
            "UPDATE Products SET Description = REPLACE(Description, 'có công', 'meritorious')",
            "UPDATE Products SET Description = REPLACE(Description, 'xứng đáng', 'deserving')",
            "UPDATE Products SET Description = REPLACE(Description, 'có giá trị', 'valuable')",
            "UPDATE Products SET Description = REPLACE(Description, 'quý giá', 'precious')",
            "UPDATE Products SET Description = REPLACE(Description, 'yêu quý', 'beloved')",
            "UPDATE Products SET Description = REPLACE(Description, 'thân yêu', 'dear')",
            "UPDATE Products SET Description = REPLACE(Description, 'yêu dấu', 'darling')",
            "UPDATE Products SET Description = REPLACE(Description, 'ngọt ngào', 'sweet')",
            "UPDATE Products SET Description = REPLACE(Description, 'đáng yêu', 'lovely')",
            "UPDATE Products SET Description = REPLACE(Description, 'dễ thương', 'cute')",
            "UPDATE Products SET Description = REPLACE(Description, 'hấp dẫn', 'gripping')",
            "UPDATE Products SET Description = REPLACE(Description, 'thuyết phục', 'persuasive')",
            "UPDATE Products SET Description = REPLACE(Description, 'đáng tin', 'credible')",
            "UPDATE Products SET Description = REPLACE(Description, 'hợp lý', 'logical')",
            "UPDATE Products SET Description = REPLACE(Description, 'vững chắc', 'sound')",
            "UPDATE Products SET Description = REPLACE(Description, 'ổn định', 'stable')",
            "UPDATE Products SET Description = REPLACE(Description, 'nhất quán', 'consistent')",
            "UPDATE Products SET Description = REPLACE(Description, 'đáng tin cậy', 'reliable')",
            "UPDATE Products SET Description = REPLACE(Description, 'trung thành', 'faithful')",
            "UPDATE Products SET Description = REPLACE(Description, 'tận tâm', 'devoted')",
            "UPDATE Products SET Description = REPLACE(Description, 'cam kết', 'committed')",
            "UPDATE Products SET Description = REPLACE(Description, 'quyết tâm', 'determined')",
            "UPDATE Products SET Description = REPLACE(Description, 'ổn định', 'settled')",
            "UPDATE Products SET Description = REPLACE(Description, 'cố định', 'fixed')",
            "UPDATE Products SET Description = REPLACE(Description, 'được thiết lập', 'established')",
            "UPDATE Products SET Description = REPLACE(Description, 'được thành lập', 'founded')",
            "UPDATE Products SET Description = REPLACE(Description, 'được tạo ra', 'created')",
            "UPDATE Products SET Description = REPLACE(Description, 'được hình thành', 'formed')",
            "UPDATE Products SET Description = REPLACE(Description, 'được xây dựng', 'built')",
            "UPDATE Products SET Description = REPLACE(Description, 'được lắp ráp', 'assembled')",
            "UPDATE Products SET Description = REPLACE(Description, 'được sản xuất', 'manufactured')",
            "UPDATE Products SET Description = REPLACE(Description, 'được làm', 'made')",
            "UPDATE Products SET Description = REPLACE(Description, 'được thiết kế', 'designed')",
            "UPDATE Products SET Description = REPLACE(Description, 'được phát triển', 'developed')",
            "UPDATE Products SET Description = REPLACE(Description, 'được phát minh', 'invented')",
            "UPDATE Products SET Description = REPLACE(Description, 'được khám phá', 'discovered')",
            "UPDATE Products SET Description = REPLACE(Description, 'được tìm thấy', 'found')",
            "UPDATE Products SET Description = REPLACE(Description, 'được định vị', 'located')",
            "UPDATE Products SET Description = REPLACE(Description, 'được đặt', 'situated')",
            "UPDATE Products SET Description = REPLACE(Description, 'được sắp xếp', 'arranged')",
            "UPDATE Products SET Description = REPLACE(Description, 'được tổ chức', 'organized')",
            "UPDATE Products SET Description = REPLACE(Description, 'được cấu trúc', 'structured')",
            "UPDATE Products SET Description = REPLACE(Description, 'được hệ thống hóa', 'systematized')",
            "UPDATE Products SET Description = REPLACE(Description, 'được chuẩn hóa', 'standardized')",
            "UPDATE Products SET Description = REPLACE(Description, 'được quy chuẩn hóa', 'regularized')",
            "UPDATE Products SET Description = REPLACE(Description, 'được chính thức hóa', 'formalized')",
            "UPDATE Products SET Description = REPLACE(Description, 'được thể chế hóa', 'institutionalized')"
        ]
        
        total_affected = 0
        for i, query in enumerate(ultimate_queries):
            try:
                cursor.execute(query)
                affected = cursor.rowcount
                total_affected += affected
                if affected > 0:
                    print(f"  ✅ Query {i+1}: {affected} rows affected")
            except Exception as e:
                print(f"  ⚠️ Query {i+1} error: {e}")
        
        conn.commit()
        
        # Kiểm tra kết quả cuối cùng
        cursor.execute("SELECT TOP 2 Id, Condition, Gender, SUBSTRING(Description, 1, 150) as Desc_Short FROM Products ORDER BY Id")
        results = cursor.fetchall()
        
        print(f"\n✅ TỔNG KẾT: {total_affected} thay đổi được thực hiện")
        print("\n📦 KẾT QUẢ CUỐI CÙNG:")
        for row in results:
            print(f"  ID: {row[0]}, Condition: {row[1]}, Gender: {row[2]}")
            print(f"  Description: {row[3]}...")
            print("  " + "-" * 40)
        
        cursor.close()
        conn.close()
        print("\n🎉 KHÔI PHỤC HOÀN TOÀN THÀNH CÔNG!")
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")

if __name__ == "__main__":
    ultimate_restore()
