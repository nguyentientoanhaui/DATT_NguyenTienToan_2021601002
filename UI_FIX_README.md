# 🎨 Sửa lỗi Giao diện - Trang Index

## 🚨 Vấn đề đã gặp

Giao diện trang Index có vấn đề:
- Vùng trắng lớn che phủ nội dung chính
- Video background không hiển thị đúng
- Layout bị xung đột với CSS cũ

## 🔍 Nguyên nhân

1. **Xung đột CSS**: Các styles cũ trong layout chính xung đột với styles mới
2. **Inline styles**: Quá nhiều inline styles gây khó quản lý
3. **Z-index conflicts**: Các phần tử có z-index không đúng thứ tự
4. **Bootstrap conflicts**: Bootstrap CSS xung đột với custom styles

## ✅ Giải pháp đã áp dụng

### 1. **Tách CSS ra file riêng**
- Loại bỏ tất cả inline styles khỏi HTML
- Tạo file `index-custom.css` cho styles chính
- Tạo file `index-override.css` để override conflicts

### 2. **Cấu trúc CSS mới**
```
wwwroot/css/
├── index-custom.css      # Styles chính
├── index-override.css    # Override conflicts
└── modern-header.css     # Header styles
```

### 3. **Sửa layout structure**
```html
<!-- Trước -->
<section style="position: relative; height: 100vh; ...">

<!-- Sau -->
<section class="hero-section">
```

### 4. **Z-index hierarchy**
```
.modern-header: z-index: 9999
.hero-overlay: z-index: 2
.hero-content: z-index: 10
.hero-section: z-index: 1
```

## 📁 Files đã cập nhật

### Files chính:
1. **`Views/Home/Index.cshtml`** - Loại bỏ inline styles
2. **`wwwroot/css/index-custom.css`** - Styles chính
3. **`wwwroot/css/index-override.css`** - Override conflicts
4. **`wwwroot/css/modern-header.css`** - Header styles

### Cấu trúc HTML mới:
```html
<!-- Modern Header -->
<partial name="_ModernHeader" />

<!-- Certified Banner -->
<partial name="_CertifiedBanner" />

<!-- Hero Section -->
<section class="hero-section">
    <video autoplay muted loop>
        <source src="~/media/sliders/demo.webm" type="video/webm">
    </video>
    <div class="hero-overlay"></div>
    <div class="container">
        <div class="hero-content">
            <h1>Pre-Owned Luxury Watches</h1>
            <div class="hero-buttons">
                <!-- Buttons -->
            </div>
        </div>
    </div>
</section>
```

## 🎯 Kết quả

✅ **Video background hiển thị đúng**
✅ **Header fixed với scroll effect**
✅ **Brand carousel hoạt động**
✅ **Products section hiển thị đúng**
✅ **Responsive design**
✅ **Không còn vùng trắng che phủ**

## 🔧 CSS Classes chính

### Hero Section:
```css
.hero-section {
    position: relative;
    height: 100vh;
    overflow: hidden;
    background: #000;
}

.hero-overlay {
    position: absolute;
    background: rgba(0,0,0,0.4);
    z-index: 2;
}

.hero-content {
    position: relative;
    z-index: 10;
    color: white;
}
```

### Brand Carousel:
```css
.brand-carousel {
    background: #f8f9fa;
    padding: 3rem 0;
}

.brand-items {
    display: flex;
    gap: 2rem;
    overflow-x: auto;
}
```

### Products:
```css
.products-section {
    padding: 4rem 0;
    background: white;
}

.product-image-wrapper {
    border-radius: 10px;
    box-shadow: 0 4px 15px rgba(0,0,0,0.1);
}
```

## 🚀 Kiểm tra

Sau khi sửa lỗi:
1. **Build project**: `dotnet build`
2. **Chạy ứng dụng**: `dotnet run`
3. **Kiểm tra trang Index**: 
   - Video background hiển thị
   - Header scroll effect
   - Brand carousel
   - Products grid
4. **Test responsive**: Thay đổi kích thước màn hình

## 📞 Troubleshooting

Nếu vẫn gặp vấn đề:
1. Clear browser cache (Ctrl+F5)
2. Kiểm tra console errors
3. Verify file paths
4. Check CSS loading order
5. Inspect element để debug

---

**Lưu ý**: Luôn sử dụng CSS classes thay vì inline styles để dễ maintain! 🎉
