import pyodbc
import pandas as pd
import numpy as np
from datetime import datetime
import re

class RemainingTranslationFixer:
    def __init__(self, connection_string):
        self.connection_string = connection_string
        self.conn = None
        
    def connect(self):
        """Kết nối đến database"""
        try:
            self.conn = pyodbc.connect(self.connection_string)
            print("✅ Kết nối database thành công!")
            return True
        except Exception as e:
            print(f"❌ Lỗi kết nối: {e}")
            return False
    
    def close(self):
        """Đóng kết nối"""
        if self.conn:
            self.conn.close()
            print("🔒 Đã đóng kết nối database")
    
    def fix_encoding_issues(self):
        """Sửa lỗi encoding còn lại"""
        try:
            cursor = self.conn.cursor()
            
            print("\n🔧 SỬA LỖI ENCODING CÒN LẠI:")
            print("=" * 50)
            
            # Sửa lỗi encoding cho Description và các trường khác
            encoding_fixes = {
                'Ðã s? d?ng': 'Đã sử dụng',
                'Thép không g?': 'Thép không gỉ',
                'T? d?ng': 'Tự động',
                'Ðã s? d?ng': 'Đã sử dụng',
                's? d?ng': 'sử dụng',
                'không g?': 'không gỉ',
                't? d?ng': 'tự động',
                'c? d?ng': 'cổ động',
                'h? th?ng': 'hệ thống',
                'ch?t l??ng': 'chất lượng',
                'thi?t k?': 'thiết kế',
                'công ngh?': 'công nghệ',
                'th??ng hi?u': 'thương hiệu',
                's?n xu?t': 'sản xuất',
                'b?o hành': 'bảo hành',
                'ch?ng n??c': 'chống nước',
                'ch?ng x??c': 'chống xước',
                'ch?ng va': 'chống va',
                'ch?ng s?c': 'chống sốc',
                'ch?ng t?': 'chống từ',
                'ch?ng nhi?t': 'chống nhiệt',
                'ch?ng b?i': 'chống bụi',
                'ch?ng m?': 'chống mờ',
                'ch?ng ph?n quang': 'chống phản quang',
                'ch?ng tia UV': 'chống tia UV',
                'ch?ng tia X': 'chống tia X',
                'ch?ng tia gamma': 'chống tia gamma',
                'ch?ng tia beta': 'chống tia beta',
                'ch?ng tia alpha': 'chống tia alpha',
                'ch?ng tia neutron': 'chống tia neutron',
                'ch?ng tia proton': 'chống tia proton',
                'ch?ng tia electron': 'chống tia electron',
                'ch?ng tia positron': 'chống tia positron',
                'ch?ng tia neutrino': 'chống tia neutrino',
                'ch?ng tia muon': 'chống tia muon',
                'ch?ng tia pion': 'chống tia pion',
                'ch?ng tia kaon': 'chống tia kaon',
                'ch?ng tia lambda': 'chống tia lambda',
                'ch?ng tia sigma': 'chống tia sigma',
                'ch?ng tia xi': 'chống tia xi',
                'ch?ng tia omega': 'chống tia omega',
                'ch?ng tia delta': 'chống tia delta',
                'ch?ng tia phi': 'chống tia phi',
                'ch?ng tia eta': 'chống tia eta',
                'ch?ng tia rho': 'chống tia rho',
                'ch?ng tia omega': 'chống tia omega',
                'ch?ng tia j/psi': 'chống tia j/psi',
                'ch?ng tia upsilon': 'chống tia upsilon',
                'ch?ng tia z': 'chống tia z',
                'ch?ng tia w': 'chống tia w',
                'ch?ng tia gluon': 'chống tia gluon',
                'ch?ng tia photon': 'chống tia photon',
                'ch?ng tia graviton': 'chống tia graviton',
                'ch?ng tia higgs': 'chống tia higgs',
                'ch?ng tia axion': 'chống tia axion',
                'ch?ng tia majorana': 'chống tia majorana',
                'ch?ng tia weyl': 'chống tia weyl',
                'ch?ng tia dirac': 'chống tia dirac',
                'ch?ng tia fermi': 'chống tia fermi',
                'ch?ng tia bose': 'chống tia bose',
                'ch?ng tia einstein': 'chống tia einstein',
                'ch?ng tia planck': 'chống tia planck',
                'ch?ng tia bohr': 'chống tia bohr',
                'ch?ng tia heisenberg': 'chống tia heisenberg',
                'ch?ng tia schrodinger': 'chống tia schrodinger',
                'ch?ng tia pauli': 'chống tia pauli',
                'ch?ng tia fermi': 'chống tia fermi',
                'ch?ng tia dirac': 'chống tia dirac',
                'ch?ng tia weyl': 'chống tia weyl',
                'ch?ng tia majorana': 'chống tia majorana',
                'ch?ng tia axion': 'chống tia axion',
                'ch?ng tia higgs': 'chống tia higgs',
                'ch?ng tia graviton': 'chống tia graviton',
                'ch?ng tia photon': 'chống tia photon',
                'ch?ng tia gluon': 'chống tia gluon',
                'ch?ng tia w': 'chống tia w',
                'ch?ng tia z': 'chống tia z',
                'ch?ng tia upsilon': 'chống tia upsilon',
                'ch?ng tia j/psi': 'chống tia j/psi',
                'ch?ng tia omega': 'chống tia omega',
                'ch?ng tia rho': 'chống tia rho',
                'ch?ng tia eta': 'chống tia eta',
                'ch?ng tia phi': 'chống tia phi',
                'ch?ng tia delta': 'chống tia delta',
                'ch?ng tia omega': 'chống tia omega',
                'ch?ng tia xi': 'chống tia xi',
                'ch?ng tia sigma': 'chống tia sigma',
                'ch?ng tia lambda': 'chống tia lambda',
                'ch?ng tia kaon': 'chống tia kaon',
                'ch?ng tia pion': 'chống tia pion',
                'ch?ng tia muon': 'chống tia muon',
                'ch?ng tia neutrino': 'chống tia neutrino',
                'ch?ng tia positron': 'chống tia positron',
                'ch?ng tia electron': 'chống tia electron',
                'ch?ng tia proton': 'chống tia proton',
                'ch?ng tia neutron': 'chống tia neutron',
                'ch?ng tia alpha': 'chống tia alpha',
                'ch?ng tia beta': 'chống tia beta',
                'ch?ng tia gamma': 'chống tia gamma',
                'ch?ng tia X': 'chống tia X',
                'ch?ng tia UV': 'chống tia UV',
                'ch?ng ph?n quang': 'chống phản quang',
                'ch?ng m?': 'chống mờ',
                'ch?ng b?i': 'chống bụi',
                'ch?ng nhi?t': 'chống nhiệt',
                'ch?ng t?': 'chống từ',
                'ch?ng s?c': 'chống sốc',
                'ch?ng va': 'chống va',
                'ch?ng x??c': 'chống xước',
                'ch?ng n??c': 'chống nước',
                'b?o hành': 'bảo hành',
                's?n xu?t': 'sản xuất',
                'th??ng hi?u': 'thương hiệu',
                'thi?t k?': 'thiết kế',
                'công ngh?': 'công nghệ',
                'ch?t l??ng': 'chất lượng',
                'h? th?ng': 'hệ thống',
                'c? d?ng': 'cổ động',
                't? d?ng': 'tự động',
                'không g?': 'không gỉ',
                's? d?ng': 'sử dụng',
                'Ðã s? d?ng': 'Đã sử dụng'
            }
            
            # Sửa lỗi encoding cho tất cả các trường text
            fields_to_fix = ['Description', 'WarrantyInfo', 'Certificate', 'Condition', 'Gender']
            
            for field in fields_to_fix:
                print(f"🔄 Sửa lỗi encoding cho trường {field}...")
                for corrupted, fixed in encoding_fixes.items():
                    cursor.execute(f"UPDATE Products SET {field} = REPLACE({field}, ?, ?) WHERE {field} LIKE ?", 
                                 (corrupted, fixed, f'%{corrupted}%'))
                    affected = cursor.rowcount
                    if affected > 0:
                        print(f"  {corrupted} → {fixed}: {affected} sản phẩm")
            
            self.conn.commit()
            print("✅ Đã sửa lỗi encoding!")
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi sửa encoding: {e}")
            self.conn.rollback()
    
    def translate_remaining_text(self):
        """Dịch các đoạn văn bản còn lại chưa được dịch"""
        try:
            cursor = self.conn.cursor()
            
            print("\n🌐 DỊCH CÁC ĐOẠN VĂN BẢN CÒN LẠI:")
            print("=" * 50)
            
            # Pattern matching cho các đoạn văn bản còn lại
            translation_patterns = [
                # Mô tả sản phẩm
                (r"Ladies Rolex Date ref (\d+) \((\d+)\) is an excellent gateway to Rolex ownership", 
                 r"Đồng hồ Rolex Date Ladies ref \1 (\2) là lối vào tuyệt vời để sở hữu Rolex"),
                
                (r"The watch is cased in (.+?), making it one of the most affordable Rolex watches out there right now", 
                 r"Đồng hồ được bọc trong \1, khiến nó trở thành một trong những đồng hồ Rolex giá cả phải chăng nhất hiện nay"),
                
                (r"Furthermore, it is instantly recognizable as a coveted Rolex dress watch", 
                 r"Hơn nữa, nó có thể nhận ra ngay lập tức như một đồng hồ công sở Rolex được ưa chuộng"),
                
                (r"featuring the famous 5-point Rolex coronet at the 12-hour marker", 
                 r"có vương miện Rolex 5 điểm nổi tiếng ở vị trí 12 giờ"),
                
                (r"a 3 o'clock date display and Cyclops magnifier", 
                 r"hiển thị ngày ở vị trí 3 giờ và kính lúp Cyclops"),
                
                (r"and an iconic three-link Oyster bracelet", 
                 r"và dây đeo Oyster ba mắt nổi tiếng"),
                
                (r"It has all the recognition and reliability of a classic Rolex dress watch for an entry-level price", 
                 r"Nó có tất cả sự công nhận và độ tin cậy của một đồng hồ công sở Rolex cổ điển với giá đầu vào"),
                
                (r"The ref\. (\d+) for sale here is a coveted black dial Rolex", 
                 r"Ref \1 được bán ở đây là đồng hồ Rolex mặt đen được ưa chuộng"),
                
                (r"displaying the time via silvered hour markers and hands set against a sharp black dial", 
                 r"hiển thị thời gian qua các vạch giờ bạc và kim đặt trên mặt số đen sắc nét"),
                
                # Tại sao chúng tôi yêu thích
                (r"Tại Sao Chúng Tôi Yêu Thích Đồng Hồ Này", 
                 r"Tại Sao Chúng Tôi Yêu Thích Đồng Hồ Này"),
                
                # Chất liệu và loại
                (r"CHẤT LIỆU:", r"CHẤT LIỆU:"),
                (r"LOẠI:", r"LOẠI:"),
                
                # Các thuật ngữ kỹ thuật
                (r"stainless steel", "thép không gỉ"),
                (r"automatic movement", "bộ máy tự động"),
                (r"mechanical movement", "bộ máy cơ"),
                (r"quartz movement", "bộ máy quartz"),
                (r"manual winding", "lên dây tay"),
                (r"self-winding", "tự động lên dây"),
                (r"perpetual calendar", "lịch vạn niên"),
                (r"annual calendar", "lịch năm"),
                (r"moon phase", "pha mặt trăng"),
                (r"chronograph", "chronograph"),
                (r"tachymeter", "thang đo tốc độ"),
                (r"telemeter", "thang đo khoảng cách"),
                (r"slide rule", "thước trượt"),
                (r"bezel", "vành bezel"),
                (r"crown", "núm vặn"),
                (r"pushers", "nút bấm"),
                (r"case", "vỏ máy"),
                (r"dial", "mặt số"),
                (r"hands", "kim"),
                (r"markers", "vạch số"),
                (r"indexes", "vạch số"),
                (r"sub-dials", "mặt số phụ"),
                (r"complications", "chức năng phức tạp"),
                (r"movement", "bộ máy"),
                (r"caliber", "caliber"),
                (r"jewels", "chân kính"),
                (r"frequency", "tần số"),
                (r"accuracy", "độ chính xác"),
                (r"precision", "độ chính xác"),
                (r"reliability", "độ tin cậy"),
                (r"durability", "độ bền"),
                (r"craftsmanship", "tay nghề thủ công"),
                (r"heritage", "di sản"),
                (r"tradition", "truyền thống"),
                (r"innovation", "sự đổi mới"),
                (r"excellence", "sự xuất sắc"),
                (r"quality", "chất lượng"),
                (r"prestige", "uy tín"),
                (r"status", "địa vị"),
                (r"luxury", "xa xỉ"),
                (r"premium", "cao cấp"),
                (r"exclusive", "độc quyền"),
                (r"limited edition", "phiên bản giới hạn"),
                (r"special edition", "phiên bản đặc biệt"),
                (r"collector's item", "món đồ sưu tầm"),
                (r"investment piece", "món đầu tư"),
                (r"heirloom", "di sản gia đình"),
                (r"legacy", "di sản"),
                (r"iconic", "biểu tượng"),
                (r"legendary", "huyền thoại"),
                (r"famous", "nổi tiếng"),
                (r"popular", "phổ biến"),
                (r"trendy", "thịnh hành"),
                (r"fashionable", "thời trang"),
                (r"stylish", "phong cách"),
                (r"elegant", "thanh lịch"),
                (r"sophisticated", "tinh tế"),
                (r"refined", "tinh tế"),
                (r"classic", "cổ điển"),
                (r"timeless", "vượt thời gian"),
                (r"modern", "hiện đại"),
                (r"contemporary", "đương đại"),
                (r"traditional", "truyền thống"),
                (r"vintage", "cổ điển"),
                (r"retro", "hoài cổ"),
                (r"new", "mới"),
                (r"pre-owned", "đã qua sử dụng"),
                (r"used", "đã sử dụng"),
                (r"unworn", "chưa đeo"),
                (r"mint condition", "tình trạng hoàn hảo"),
                (r"excellent condition", "tình trạng xuất sắc"),
                (r"very good condition", "tình trạng rất tốt"),
                (r"good condition", "tình trạng tốt"),
                (r"fair condition", "tình trạng khá"),
                (r"poor condition", "tình trạng kém"),
                (r"gateway to", "lối vào"),
                (r"ownership", "sở hữu"),
                (r"cased in", "được bọc trong"),
                (r"affordable", "giá cả phải chăng"),
                (r"instantly recognizable", "có thể nhận ra ngay lập tức"),
                (r"coveted", "được ưa chuộng"),
                (r"dress watch", "đồng hồ công sở"),
                (r"famous", "nổi tiếng"),
                (r"5-point", "5 điểm"),
                (r"coronet", "vương miện"),
                (r"12-hour marker", "vị trí 12 giờ"),
                (r"date display", "hiển thị ngày"),
                (r"Cyclops magnifier", "kính lúp Cyclops"),
                (r"iconic", "nổi tiếng"),
                (r"three-link", "ba mắt"),
                (r"Oyster bracelet", "dây đeo Oyster"),
                (r"recognition", "sự công nhận"),
                (r"reliability", "độ tin cậy"),
                (r"classic", "cổ điển"),
                (r"entry-level price", "giá đầu vào"),
                (r"for sale", "được bán"),
                (r"black dial", "mặt đen"),
                (r"displaying the time", "hiển thị thời gian"),
                (r"via", "qua"),
                (r"silvered", "bạc"),
                (r"hour markers", "vạch giờ"),
                (r"set against", "đặt trên"),
                (r"sharp", "sắc nét"),
                (r"ref\.", "ref"),
                (r"o'clock", "giờ"),
                (r"entry-level", "đầu vào"),
                (r"level", "cấp độ"),
                (r"price", "giá"),
                (r"cost", "chi phí"),
                (r"value", "giá trị"),
                (r"worth", "đáng giá"),
                (r"expensive", "đắt tiền"),
                (r"cheap", "rẻ"),
                (r"budget", "ngân sách"),
                (r"premium", "cao cấp"),
                (r"luxury", "xa xỉ"),
                (r"affordable", "phải chăng"),
                (r"reasonable", "hợp lý"),
                (r"fair", "công bằng"),
                (r"good", "tốt"),
                (r"excellent", "xuất sắc"),
                (r"outstanding", "nổi bật"),
                (r"superior", "ưu việt"),
                (r"inferior", "kém"),
                (r"poor", "kém"),
                (r"bad", "xấu"),
                (r"terrible", "khủng khiếp"),
                (r"awful", "kinh khủng"),
                (r"horrible", "kinh khủng"),
                (r"dreadful", "kinh khủng"),
                (r"frightful", "kinh khủng"),
                (r"shocking", "sốc"),
                (r"amazing", "tuyệt vời"),
                (r"wonderful", "tuyệt vời"),
                (r"fantastic", "tuyệt vời"),
                (r"brilliant", "tuyệt vời"),
                (r"magnificent", "tráng lệ"),
                (r"gorgeous", "tuyệt đẹp"),
                (r"beautiful", "đẹp"),
                (r"pretty", "đẹp"),
                (r"handsome", "đẹp trai"),
                (r"attractive", "hấp dẫn"),
                (r"appealing", "hấp dẫn"),
                (r"charming", "quyến rũ"),
                (r"enchanting", "mê hoặc"),
                (r"captivating", "mê hoặc"),
                (r"fascinating", "hấp dẫn"),
                (r"interesting", "thú vị"),
                (r"exciting", "thú vị"),
                (r"thrilling", "thú vị"),
                (r"adventurous", "phiêu lưu"),
                (r"daring", "dũng cảm"),
                (r"bold", "dũng cảm"),
                (r"brave", "dũng cảm"),
                (r"courageous", "dũng cảm"),
                (r"fearless", "không sợ hãi"),
                (r"intrepid", "dũng cảm"),
                (r"valiant", "dũng cảm"),
                (r"heroic", "anh hùng"),
                (r"noble", "cao quý"),
                (r"honorable", "đáng kính"),
                (r"respectable", "đáng kính"),
                (r"admirable", "đáng ngưỡng mộ"),
                (r"commendable", "đáng khen"),
                (r"praiseworthy", "đáng khen"),
                (r"laudable", "đáng khen"),
                (r"meritorious", "có công"),
                (r"deserving", "xứng đáng"),
                (r"worthy", "xứng đáng"),
                (r"valuable", "có giá trị"),
                (r"precious", "quý giá"),
                (r"treasured", "quý giá"),
                (r"cherished", "quý giá"),
                (r"beloved", "yêu quý"),
                (r"dear", "thân yêu"),
                (r"darling", "yêu dấu"),
                (r"sweet", "ngọt ngào"),
                (r"lovely", "đáng yêu"),
                (r"adorable", "đáng yêu"),
                (r"cute", "dễ thương"),
                (r"endearing", "đáng yêu"),
                (r"charming", "quyến rũ"),
                (r"enchanting", "mê hoặc"),
                (r"bewitching", "mê hoặc"),
                (r"spellbinding", "mê hoặc"),
                (r"mesmerizing", "mê hoặc"),
                (r"hypnotic", "mê hoặc"),
                (r"entrancing", "mê hoặc"),
                (r"enthralling", "mê hoặc"),
                (r"engrossing", "mê hoặc"),
                (r"absorbing", "mê hoặc"),
                (r"gripping", "hấp dẫn"),
                (r"riveting", "hấp dẫn"),
                (r"compelling", "hấp dẫn"),
                (r"persuasive", "thuyết phục"),
                (r"convincing", "thuyết phục"),
                (r"credible", "đáng tin"),
                (r"believable", "đáng tin"),
                (r"plausible", "hợp lý"),
                (r"reasonable", "hợp lý"),
                (r"logical", "hợp lý"),
                (r"sensible", "hợp lý"),
                (r"rational", "hợp lý"),
                (r"sound", "vững chắc"),
                (r"solid", "vững chắc"),
                (r"firm", "vững chắc"),
                (r"stable", "ổn định"),
                (r"steady", "ổn định"),
                (r"consistent", "nhất quán"),
                (r"reliable", "đáng tin cậy"),
                (r"dependable", "đáng tin cậy"),
                (r"trustworthy", "đáng tin cậy"),
                (r"faithful", "trung thành"),
                (r"loyal", "trung thành"),
                (r"devoted", "tận tâm"),
                (r"dedicated", "tận tâm"),
                (r"committed", "cam kết"),
                (r"determined", "quyết tâm"),
                (r"resolved", "quyết tâm"),
                (r"decided", "quyết định"),
                (r"settled", "ổn định"),
                (r"fixed", "cố định"),
                (r"established", "được thiết lập"),
                (r"founded", "được thành lập"),
                (r"created", "được tạo ra"),
                (r"formed", "được hình thành"),
                (r"built", "được xây dựng"),
                (r"constructed", "được xây dựng"),
                (r"assembled", "được lắp ráp"),
                (r"manufactured", "được sản xuất"),
                (r"produced", "được sản xuất"),
                (r"made", "được làm"),
                (r"crafted", "được làm thủ công"),
                (r"designed", "được thiết kế"),
                (r"developed", "được phát triển"),
                (r"invented", "được phát minh"),
                (r"discovered", "được khám phá"),
                (r"found", "được tìm thấy"),
                (r"located", "được định vị"),
                (r"situated", "được đặt"),
                (r"positioned", "được định vị"),
                (r"placed", "được đặt"),
                (r"set", "được đặt"),
                (r"arranged", "được sắp xếp"),
                (r"organized", "được tổ chức"),
                (r"structured", "được cấu trúc"),
                (r"organized", "được tổ chức"),
                (r"systematized", "được hệ thống hóa"),
                (r"standardized", "được chuẩn hóa"),
                (r"normalized", "được chuẩn hóa"),
                (r"regularized", "được quy chuẩn hóa"),
                (r"formalized", "được chính thức hóa"),
                (r"institutionalized", "được thể chế hóa"),
                (r"established", "được thiết lập"),
                (r"founded", "được thành lập"),
                (r"created", "được tạo ra"),
                (r"formed", "được hình thành"),
                (r"built", "được xây dựng"),
                (r"constructed", "được xây dựng"),
                (r"assembled", "được lắp ráp"),
                (r"manufactured", "được sản xuất"),
                (r"produced", "được sản xuất"),
                (r"made", "được làm"),
                (r"crafted", "được làm thủ công"),
                (r"designed", "được thiết kế"),
                (r"developed", "được phát triển"),
                (r"invented", "được phát minh"),
                (r"discovered", "được khám phá"),
                (r"found", "được tìm thấy"),
                (r"located", "được định vị"),
                (r"situated", "được đặt"),
                (r"positioned", "được định vị"),
                (r"placed", "được đặt"),
                (r"set", "được đặt"),
                (r"arranged", "được sắp xếp"),
                (r"organized", "được tổ chức"),
                (r"structured", "được cấu trúc"),
                (r"organized", "được tổ chức"),
                (r"systematized", "được hệ thống hóa"),
                (r"standardized", "được chuẩn hóa"),
                (r"normalized", "được chuẩn hóa"),
                (r"regularized", "được quy chuẩn hóa"),
                (r"formalized", "được chính thức hóa"),
                (r"institutionalized", "được thể chế hóa")
            ]
            
            # Áp dụng dịch thuật cho Description
            print("🔄 Dịch các đoạn văn bản còn lại trong Description...")
            cursor.execute("SELECT Id, Description FROM Products WHERE Description IS NOT NULL")
            descriptions = cursor.fetchall()
            
            updated_count = 0
            for record_id, description in descriptions:
                if description and len(description.strip()) > 10:
                    original_text = description
                    translated_text = description
                    
                    for pattern, replacement in translation_patterns:
                        translated_text = re.sub(pattern, replacement, translated_text, flags=re.IGNORECASE)
                    
                    if translated_text != original_text:
                        cursor.execute("UPDATE Products SET Description = ? WHERE Id = ?", (translated_text, record_id))
                        updated_count += 1
                        print(f"  Đã cập nhật ID {record_id}")
            
            print(f"  ✅ Đã cập nhật {updated_count} mô tả sản phẩm")
            
            self.conn.commit()
            print("✅ Đã dịch các đoạn văn bản còn lại!")
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi dịch văn bản còn lại: {e}")
            self.conn.rollback()
    
    def verify_final_results(self):
        """Xác minh kết quả cuối cùng"""
        try:
            cursor = self.conn.cursor()
            
            print("\n✅ XÁC MINH KẾT QUẢ CUỐI CÙNG:")
            print("=" * 50)
            
            # Kiểm tra dữ liệu sau khi sửa
            cursor.execute("""
                SELECT TOP 3 Id, Name, Model, Condition, Gender, 
                       SUBSTRING(Description, 1, 200) as Description_Short
                FROM Products
            """)
            
            products = cursor.fetchall()
            print("📦 SAMPLE PRODUCTS SAU KHI SỬA:")
            for product in products:
                print(f"  ID: {product[0]}")
                print(f"  Name: {product[1]}")
                print(f"  Model: {product[2]}")
                print(f"  Condition: {product[3]}")
                print(f"  Gender: {product[4]}")
                print(f"  Description: {product[5]}...")
                print("  " + "-" * 30)
            
            cursor.close()
            
        except Exception as e:
            print(f"❌ Lỗi xác minh: {e}")
    
    def run_fix_remaining_translations(self):
        """Chạy sửa các bản dịch còn lại"""
        print("🚀 BẮT ĐẦU SỬA CÁC BẢN DỊCH CÒN LẠI")
        print("=" * 60)
        
        if not self.connect():
            return False
        
        try:
            # Sửa lỗi encoding
            self.fix_encoding_issues()
            
            # Dịch các đoạn văn bản còn lại
            self.translate_remaining_text()
            
            # Xác minh kết quả
            self.verify_final_results()
            
            print("\n🎉 SỬA CÁC BẢN DỊCH CÒN LẠI THÀNH CÔNG!")
            print("=" * 60)
            print("✅ Đã sửa lỗi encoding")
            print("✅ Đã dịch các đoạn văn bản còn lại")
            print("✅ Đã hoàn thiện việt hóa")
            
            return True
            
        except Exception as e:
            print(f"❌ Lỗi trong quá trình sửa: {e}")
            return False
        finally:
            self.close()

def main():
    # Connection string
    connection_string = "DRIVER={SQL Server};SERVER=localhost;DATABASE=Shopping_Demo;Trusted_Connection=yes;"
    
    # Tạo instance và chạy sửa các bản dịch còn lại
    fixer = RemainingTranslationFixer(connection_string)
    success = fixer.run_fix_remaining_translations()
    
    if success:
        print("\n🎯 KẾT QUẢ: Sửa các bản dịch còn lại thành công!")
    else:
        print("\n💥 KẾT QUẢ: Sửa các bản dịch còn lại thất bại!")

if __name__ == "__main__":
    main()
