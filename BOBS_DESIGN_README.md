# 🎨 Thiết kế BOB'S - Luxury Watch Website

## 📋 Tổng quan

Trang web đã được thiết kế lại hoàn toàn để giống hệt trang BOB'S với header tích hợp vào hero section, tạo cảm giác sang trọng và hiện đại cho website bán đồng hồ luxury.

## 🎯 Tính năng chính

### 1. **Header Tích hợp hoàn toàn**
- **Top Bar**: Thanh đen mỏng với thông tin liên hệ, links và search
- **Logo Section**: Logo "VPe" ở trung tâm
- **Tích hợp**: Header nằm trong hero section, không tách biệt
- **Màu sắc**: Tất cả text và icon màu trắng trên nền video tối

### 2. **Hero Section với Video Background**
- **Video**: `demo.webm` làm background với overlay tối
- **Chiều cao**: 80% viewport height (80vh)
- **Text chính**: "Pre-Owned Luxury Watches"
- **Buttons**: "Buy a Watch" và "Sell a Watch" với style bầu dục
- **Animation**: Fade-in cho text và buttons

### 3. **Brand Carousel**
- **Vị trí**: 20% viewport height còn lại (20vh)
- **Auto-scroll**: Tự động cuộn mỗi 3 giây
- **Brands**: Hiển thị từ database với logo tròn nhỏ gọn
- **AI Icon**: Icon brain với animation pulse
- **Hover effects**: Scale và shadow khi hover
- **Layout**: Horizontal strip ngay dưới hero section

### 4. **Products Section**
- **Modern Cards**: Thiết kế card hiện đại với shadow
- **Hover Effects**: Transform và scale khi hover
- **Wishlist**: Button heart cho user đã đăng nhập
- **Add to Cart**: Button xanh với hover effects

## 📁 Cấu trúc Files

### Files chính:
```
Views/Home/Index.cshtml          # Trang chính với thiết kế mới
wwwroot/css/aurum-style.css       # CSS chính cho thiết kế AURUM
```

### Files đã xóa (không cần thiết):
```
Views/Shared/_ModernHeader.cshtml
Views/Shared/_CertifiedBanner.cshtml
wwwroot/css/modern-header.css
wwwroot/css/index-override.css
```

## 🎨 CSS Classes chính

### Header Integration:
```css
.integrated-header {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    z-index: 10;
    color: white;
}

.top-bar {
    background: rgba(0, 0, 0, 0.8);
    padding: 8px 0;
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}
```

### Hero Section:
```css
.hero-section {
    position: relative;
    height: 80vh;
    overflow: hidden;
    background: #000;
}

.hero-video {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    object-fit: cover;
    opacity: 0.8;
}
```

### Brand Carousel:
```css
.brand-section {
    background: #f8f9fa;
    padding: 2rem 0;
    height: 20vh;
    display: flex;
    align-items: center;
}

.brand-items {
    display: flex;
    gap: 2rem;
    overflow-x: auto;
    scroll-behavior: smooth;
    scrollbar-width: none;
    align-items: center;
}
```

### Product Cards:
```css
.product-card {
    background: white;
    border-radius: 15px;
    box-shadow: 0 5px 20px rgba(0, 0, 0, 0.1);
    transition: all 0.3s ease;
}

.product-card:hover {
    transform: translateY(-10px);
    box-shadow: 0 15px 40px rgba(0, 0, 0, 0.15);
}
```

## 🔧 JavaScript Features

### Header Icons Functionality:
```javascript
// Search functionality
$('#searchIcon').click(function() {
    performSearch();
});

// Cart icon functionality
$('#cartIcon').click(function() {
    window.location.href = '/Cart/Index';
});

// User icon functionality
$('#userIcon').click(function() {
    if (User.Identity.IsAuthenticated) {
        window.location.href = '/Account/UpdateAccount';
    } else {
        window.location.href = '/Account/Login';
    }
});

// Mobile menu functionality
$('#menuIcon').click(function() {
    toggleMobileMenu();
});
```

### Search Function:
```javascript
function performSearch() {
    const searchTerm = $('#searchInput').val().trim();
    if (searchTerm) {
        window.location.href = '/Product/Search?searchTerm=' + encodeURIComponent(searchTerm);
    } else {
        Swal.fire({
            title: 'Thông báo',
            text: 'Vui lòng nhập từ khóa tìm kiếm',
            icon: 'warning'
        });
    }
}
```

### Mobile Menu:
```javascript
function toggleMobileMenu() {
    // Creates slide-in mobile menu with navigation links
    // Includes: Home, Products, Cart, Account, Contact
    // Dynamic based on user authentication status
}
```

### Auto-scroll Brand Carousel:
```javascript
setInterval(function() {
    brandItems.animate({
        scrollLeft: '+=' + scrollAmount
    }, 1000);
    
    // Reset to beginning when reaching end
    if (brandItems.scrollLeft() >= brandItems[0].scrollWidth - brandItems.width()) {
        brandItems.animate({
            scrollLeft: 0
        }, 1000);
    }
}, 3000);
```

### Product Functions:
```javascript
function addToCart(productId) {
    $.post('/Cart/AddToCart', { productId: productId }, function(response) {
        if (response.success) {
            Swal.fire({
                title: 'Thành công!',
                text: 'Sản phẩm đã được thêm vào giỏ hàng',
                icon: 'success'
            });
        }
    });
}

function addToWishlist(productId) {
    $.post('/Home/AddToWishlist', { productId: productId }, function(response) {
        if (response.success) {
            Swal.fire({
                title: 'Thành công!',
                text: 'Sản phẩm đã được thêm vào danh sách yêu thích',
                icon: 'success'
            });
        }
    });
}
```

## 📱 Responsive Design

### Mobile (≤768px):
- Hero section: 70vh
- Brand section: 30vh
- Header stack vertically
- Hero title nhỏ hơn
- Brand carousel compact với logo nhỏ hơn
- Product grid 2 cột

### Tablet (768px - 1024px):
- Hero section: 80vh
- Brand section: 20vh
- Layout vừa phải
- Brand carousel medium
- Product grid 3 cột

### Desktop (>1024px):
- Hero section: 80vh
- Brand section: 20vh
- Layout đầy đủ
- Brand carousel rộng
- Product grid 4 cột

## 🎯 Đặc điểm thiết kế BOB'S

### 1. **Header Integration**
- Header hoàn toàn tích hợp vào hero section
- Không có header riêng biệt
- Tất cả elements màu trắng trên nền video

### 2. **Top Bar Layout**
- **Left**: Contact info + links
- **Center**: Certified badge với checkmark
- **Right**: Search box + icons

### 3. **Logo Placement**
- Logo "VPe" ở trung tâm
- Font size lớn, letter-spacing
- Text shadow để nổi bật

### 4. **Hero Content**
- Text chính ở trung tâm
- Buttons bầu dục với border trắng
- Animation fade-in

### 5. **Color Scheme**
- **Primary**: Đen, trắng, xanh dương
- **Accent**: Xanh lá (checkmark), đỏ (heart)
- **Background**: Video với overlay tối

## 🚀 Setup Instructions

1. **Build project**:
   ```bash
   dotnet build
   ```

2. **Run application**:
   ```bash
   dotnet run
   ```

3. **Check features**:
   - Header tích hợp vào hero
   - Video background
   - Brand carousel auto-scroll
   - Product cards hover effects

## 📞 Troubleshooting

### Common Issues:

1. **Video không hiển thị**:
   - Kiểm tra file path: `~/media/sliders/demo.webm`
   - Đảm bảo browser hỗ trợ WebM

2. **Header không hiển thị đúng**:
   - Clear browser cache (Ctrl+F5)
   - Kiểm tra CSS loading order

3. **Brands không hiển thị**:
   - Kiểm tra ViewBag.Brands trong HomeController
   - Verify database connection

4. **CSS conflicts**:
   - Đảm bảo `aurum-style.css` được load
   - Check console errors

## 🎉 Kết quả

✅ **Header tích hợp hoàn toàn vào hero section**
✅ **Video background mượt mà**
✅ **Brand carousel auto-scroll**
✅ **Product cards hiện đại**
✅ **Responsive design**
✅ **Performance optimized**

## 🔄 Migration Notes

### Files đã thay đổi:
- `Views/Home/Index.cshtml` - Hoàn toàn mới
- `wwwroot/css/aurum-style.css` - CSS mới
- `wwwroot/css/index-custom.css` - Legacy (không dùng)

### Files đã xóa:
- Tất cả partial views cũ
- CSS files cũ gây conflict
- README files cũ

---

**Lưu ý**: Thiết kế này giống hệt BOB'S với header tích hợp và video background! 🚀
