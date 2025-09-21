// Product Details Slider JavaScript
let currentSlide = 0;
let totalSlides = 0;

document.addEventListener('DOMContentLoaded', function() {
    // Debug: Check if jQuery is available
    console.log('jQuery available:', typeof $ !== 'undefined');
    if (typeof $ !== 'undefined') {
        console.log('jQuery version:', $.fn.jquery);
    }
    
        // Initialize slider with a small delay to ensure DOM is fully loaded
    setTimeout(function() {
        initializeSlider();
        
        // Re-initialize slider after images are loaded
        const images = document.querySelectorAll('.slider-container img');
        let imagesLoaded = 0;
        
        images.forEach(img => {
            if (img.complete) {
                imagesLoaded++;
            } else {
                img.addEventListener('load', function() {
                    imagesLoaded++;
                    if (imagesLoaded >= images.length) {
                        initializeSlider();
                    }
                });
            }
        });
        
        if (imagesLoaded === images.length) {
            initializeSlider();
        }
    }, 100);
    
    // Initialize existing functionality
    initializeExistingFeatures();
});

function initializeSlider() {
    try {
        const sliderContainer = document.querySelector('.slider-container');
        if (!sliderContainer) {
            console.warn('Slider container not found');
            return;
        }

        const slides = document.querySelectorAll('.slide');
        const indicators = document.querySelectorAll('.indicator');
        const thumbnails = document.querySelectorAll('.thumbnail');
        
        if (!slides || slides.length === 0) {
            console.warn('No slides found');
            return;
        }
        
        totalSlides = slides.length;
        currentSlide = 0;
        
        // Initialize slides
        slides.forEach((slide, index) => {
            if (!slide) return;
            
            if (index === 0) {
                slide.style.display = 'block';
                slide.classList.add('active');
            } else {
                slide.style.display = 'none';
                slide.classList.remove('active');
            }
        });
        
        // Initialize navigation controls
        const prevButton = document.querySelector('.slider-nav.prev');
        const nextButton = document.querySelector('.slider-nav.next');
        
        // Add click handlers for navigation
        if (prevButton) {
            prevButton.addEventListener('click', () => changeSlide(-1));
        }
        
        if (nextButton) {
            nextButton.addEventListener('click', () => changeSlide(1));
        }
        
        // Add click handlers for indicators
        indicators.forEach(indicator => {
            if (!indicator) return;
            
            indicator.addEventListener('click', (e) => {
                const slideTo = parseInt(e.currentTarget.getAttribute('data-slide-to') || '0', 10);
                if (!isNaN(slideTo)) {
                    goToSlide(slideTo);
                }
            });
        });
        
        // Add click handlers for thumbnails
        thumbnails.forEach(thumbnail => {
            if (!thumbnail) return;
            
            thumbnail.addEventListener('click', (e) => {
                const slideTo = parseInt(e.currentTarget.getAttribute('data-slide-to') || '0', 10);
                if (!isNaN(slideTo)) {
                    goToSlide(slideTo);
                }
            });
        });
        
        // Initialize keyboard navigation
        document.addEventListener('keydown', function(e) {
            if (e.key === 'ArrowLeft') {
                changeSlide(-1);
            } else if (e.key === 'ArrowRight') {
                changeSlide(1);
            }
        });
        
        // Initial update of the slider
        updateSlider();
    } catch (error) {
        console.error('Error in initializeSlider:', error);
    }
}

function changeSlide(direction) {
    try {
        if (totalSlides <= 1) return;
        
        currentSlide += direction;
        
        if (currentSlide >= totalSlides) {
            currentSlide = 0;
        } else if (currentSlide < 0) {
            currentSlide = totalSlides - 1;
        }
        
        updateSlider();
    } catch (error) {
        console.error('Error in changeSlide:', error);
    }
}

function goToSlide(index) {
    try {
        if (totalSlides <= 0) return;
        
        // Ensure index is within bounds
        if (index < 0) index = totalSlides - 1;
        if (index >= totalSlides) index = 0;
        
        currentSlide = index;
        updateSlider();
    } catch (error) {
        console.error('Error in goToSlide:', error);
    }
}

function updateSlider() {
    try {
        const slides = document.querySelectorAll('.slide');
        const indicators = document.querySelectorAll('.indicator');
        const thumbnails = document.querySelectorAll('.thumbnail');
        
        if (!slides || slides.length === 0) return;
        
        // Update slides - only show the current slide
        slides.forEach((slide, index) => {
            if (!slide) return;
            
            if (index === currentSlide) {
                slide.style.display = 'block';
                slide.classList.add('active');
            } else {
                slide.style.display = 'none';
                slide.classList.remove('active');
            }
        });
        
        // Update indicators if they exist
        if (indicators && indicators.length > 0) {
            indicators.forEach((indicator, index) => {
                if (!indicator) return;
                
                if (index === currentSlide) {
                    indicator.classList.add('active');
                } else {
                    indicator.classList.remove('active');
                }
            });
        }
        
        // Update thumbnails if they exist
        if (thumbnails && thumbnails.length > 0) {
            thumbnails.forEach((thumbnail, index) => {
                if (!thumbnail) return;
                
                if (index === currentSlide) {
                    thumbnail.classList.add('active');
                    try {
                        // Scroll thumbnails container to show active thumbnail
                        thumbnail.scrollIntoView({
                            behavior: 'smooth',
                            block: 'nearest',
                            inline: 'center'
                        });
                    } catch (e) {
                        console.warn('Error scrolling to thumbnail:', e);
                    }
                } else {
                    thumbnail.classList.remove('active');
                }
            });
        }
    } catch (error) {
        console.error('Error in updateSlider:', error);
    }
}

function initializeExistingFeatures() {
    // Star rating interaction
    const starLabels = document.querySelectorAll('.star-rating-selector label');
    const starInputs = document.querySelectorAll('.star-rating-selector input');
    
    starLabels.forEach(label => {
        label.addEventListener('mouseenter', function() {
            this.classList.add('hover');
            const prevLabels = Array.from(this.parentNode.children).slice(0, Array.from(this.parentNode.children).indexOf(this));
            prevLabels.forEach(prevLabel => {
                if (prevLabel.tagName === 'LABEL') {
                    prevLabel.classList.add('hover');
                }
            });
        });
        
        label.addEventListener('mouseleave', function() {
            document.querySelectorAll('.star-rating-selector label').forEach(l => l.classList.remove('hover'));
        });
    });
    
    // Update hidden input field with selected rating
    starInputs.forEach(input => {
        input.addEventListener('change', function() {
            const selectedRatingInput = document.getElementById('selectedRating');
            if (selectedRatingInput) {
                selectedRatingInput.value = this.value;
            }
        });
    });
    
    // Form validation
    const reviewForm = document.querySelector('.review-form');
    if (reviewForm) {
        reviewForm.addEventListener('submit', function(e) {
            let isValid = true;
            
            // Check if name is provided
            const nameInput = document.getElementById('Name');
            if (nameInput && nameInput.value.trim() === '') {
                showFieldError(nameInput, 'Vui lòng nhập tên của bạn');
                isValid = false;
            } else {
                clearFieldError(nameInput);
            }
            
            // Check if email is provided and valid
            const emailInput = document.getElementById('Email');
            if (emailInput) {
                const email = emailInput.value.trim();
                const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (email === '') {
                    showFieldError(emailInput, 'Vui lòng nhập email của bạn');
                    isValid = false;
                } else if (!emailRegex.test(email)) {
                    showFieldError(emailInput, 'Email không hợp lệ');
                    isValid = false;
                } else {
                    clearFieldError(emailInput);
                }
            }
            
            // Check if comment is provided
            const commentInput = document.getElementById('Comment');
            if (commentInput && commentInput.value.trim() === '') {
                showFieldError(commentInput, 'Vui lòng nhập nhận xét của bạn');
                isValid = false;
            } else {
                clearFieldError(commentInput);
            }
            
            if (!isValid) {
                e.preventDefault();
            }
        });
    }
}

function showFieldError(input, message) {
    if (!input) return;
    
    let errorElement = input.nextElementSibling;
    if (!errorElement || !errorElement.classList.contains('text-danger')) {
        errorElement = document.createElement('div');
        errorElement.className = 'text-danger';
        input.parentNode.insertBefore(errorElement, input.nextSibling);
    }
    errorElement.textContent = message;
}

function clearFieldError(input) {
    if (!input) return;
    
    const errorElement = input.nextElementSibling;
    if (errorElement && errorElement.classList.contains('text-danger')) {
        errorElement.remove();
    }
}

// Add to cart function
function addToCart(productId) {
    const formData = new FormData();
    formData.append('productId', productId);
    
    fetch('/Home/AddToCart', {
        method: 'POST',
        body: formData
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        if (data.success) {
            showNotification('Sản phẩm đã được thêm vào giỏ hàng!', 'success');
            // Update cart count
            if (typeof updateCartCount === 'function') {
                updateCartCount();
            }
        } else {
            showNotification(data.message || 'Có lỗi xảy ra!', 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showNotification('Có lỗi xảy ra!', 'error');
    });
}

// Test function to check server response
function testWishlistEndpoint() {
    console.log('Testing wishlist endpoint...');
    
    // Try jQuery first, fallback to fetch
    if (typeof $ !== 'undefined') {
        $.ajax({
            url: '/Home/TestWishlist',
            type: 'GET',
            dataType: 'json',
            success: function(data) {
                console.log('Test jQuery AJAX success, data:', data);
                showNotification('Test endpoint working!', 'success');
            },
            error: function(xhr, status, error) {
                console.error('Test jQuery AJAX error:', xhr.responseText, status, error);
                showNotification('Test endpoint failed!', 'error');
            }
        });
    } else {
        // Fallback to fetch
        fetch('/Home/TestWishlist', {
            method: 'GET'
        })
        .then(response => {
            console.log('Test fetch response:', response);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('Test fetch success, data:', data);
            showNotification('Test endpoint working!', 'success');
        })
        .catch(error => {
            console.error('Test fetch error:', error);
            showNotification('Test endpoint failed!', 'error');
        });
    }
}

// Add to wishlist function
function addToWishlist(productId) {
    console.log('Starting addToWishlist with productId:', productId);
    
    // Try jQuery first, fallback to fetch
    if (typeof $ !== 'undefined') {
        $.ajax({
            url: '/Home/AddWishList',
            type: 'POST',
            data: { Id: productId },
            dataType: 'json',
            success: function(data) {
                console.log('jQuery AJAX success, data:', data);
                if (data.success) {
                    showNotification('Sản phẩm đã được thêm vào wishlist!', 'success');
                    // Update heart icon color
                    const heartIcon = document.querySelector(`button[onclick="addToWishlist(${productId})"] i`);
                    if (heartIcon) {
                        heartIcon.style.color = '#ff4d4d';
                    }
                } else {
                    showNotification(data.message || 'Có lỗi xảy ra!', 'error');
                }
            },
            error: function(xhr, status, error) {
                console.error('jQuery AJAX error:', xhr.responseText, status, error);
                showNotification('Có lỗi xảy ra khi thêm vào wishlist!', 'error');
            }
        });
    } else {
        // Fallback to fetch
        const formData = new FormData();
        formData.append('Id', productId);
        
        fetch('/Home/AddWishList', {
            method: 'POST',
            body: formData
        })
        .then(response => {
            console.log('Fetch response:', response);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('Fetch success, data:', data);
            if (data.success) {
                showNotification('Sản phẩm đã được thêm vào wishlist!', 'success');
                // Update heart icon color
                const heartIcon = document.querySelector(`button[onclick="addToWishlist(${productId})"] i`);
                if (heartIcon) {
                    heartIcon.style.color = '#ff4d4d';
                }
            } else {
                showNotification(data.message || 'Có lỗi xảy ra!', 'error');
            }
        })
        .catch(error => {
            console.error('Fetch error:', error);
            showNotification('Có lỗi xảy ra khi thêm vào wishlist!', 'error');
        });
    }
}

// Get quote function
function getQuote(productId) {
    showNotification('Chức năng này sẽ được phát triển sau!', 'info');
}

// Show notification function
function showNotification(message, type = 'info') {
    if (typeof showCenterNotification !== 'undefined') {
        // Use new center notification system
        showCenterNotification(message, type);
    } else if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: type === 'success' ? 'Thành công!' : type === 'error' ? 'Lỗi!' : 'Thông báo',
            text: message,
            icon: type,
            timer: type === 'success' ? 2000 : undefined,
            timerProgressBar: type === 'success'
        });
    } else {
        alert(message);
    }
}
