# AURUM Design - Luxury Watch Website

## Tổng quan
Thiết kế mới cho website bán đồng hồ xa xỉ với giao diện hiện đại, tối giản và chuyên nghiệp.

## Tính năng chính
- **Hero Section**: Video background với overlay và call-to-action buttons
- **Brand Carousel**: Hiển thị các thương hiệu đồng hồ nổi tiếng
- **Product Grid**: Layout responsive với hover effects
- **Wishlist**: Chức năng yêu thích sản phẩm
- **Responsive Design**: Tối ưu cho mọi thiết bị

## Cấu trúc file
```
Views/Home/Index.cshtml          # Trang chính với thiết kế mới
wwwroot/css/aurum-style.css       # CSS chính cho thiết kế AURUM
```

## Troubleshooting
1. **Video không load**: 
   - Check file path: `wwwroot/media/sliders/demo.webm`
   - Ensure video format is supported

2. **CSS conflicts**:
   - Đảm bảo `aurum-style.css` được load
   - Check console errors

3. **Responsive issues**:
   - Test on different screen sizes
   - Check media queries

### Files đã thay đổi:
- `Views/Home/Index.cshtml` - Hoàn toàn mới
- `wwwroot/css/aurum-style.css` - CSS mới
- `wwwroot/css/index-custom.css` - Legacy (không dùng)

### Dependencies:
- Bootstrap 5
- Font Awesome
- jQuery
- SweetAlert2 (optional)

## Notes
- Design inspired by luxury watch retailers
- Black & white color scheme for elegance
- Smooth animations and transitions
- Mobile-first responsive approach
