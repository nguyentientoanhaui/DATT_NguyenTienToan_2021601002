-- Script đơn giản để thêm cột Age vào bảng AspNetUsers
ALTER TABLE AspNetUsers ADD Age INT NULL;
PRINT 'Đã thêm cột Age vào bảng AspNetUsers thành công!';
