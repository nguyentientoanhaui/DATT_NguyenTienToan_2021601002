// Simple Wishlist Functionality
function initializeWishlist() {
    console.log('Initializing wishlist...');
    
    // Add click event listeners to all wishlist buttons
    const wishlistButtons = document.querySelectorAll('.wishlist-btn');
    console.log('Found wishlist buttons:', wishlistButtons.length);
    
    wishlistButtons.forEach(button => {
        // Set initial color to black and use filled heart
        const icon = button.querySelector('i');
        icon.style.color = '#000000';
        icon.classList.remove('fa-heart-o');
        icon.classList.add('fa-heart');
        
        button.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            const productId = this.getAttribute('data-product-id');
            addToWishlist(productId, this);
        });
    });
    
    // Load user's existing wishlist and mark hearts as red
    loadUserWishlist();
    
    // Force refresh after a short delay to ensure all elements are loaded
    setTimeout(() => {
        console.log('Force refreshing wishlist display...');
        loadUserWishlist();
    }, 500);
}

// Initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        // Small delay to ensure all elements are rendered
        setTimeout(initializeWishlist, 100);
    });
} else {
    // DOM is already loaded
    setTimeout(initializeWishlist, 100);
}

// Load user's existing wishlist
async function loadUserWishlist() {
    try {
        console.log('Loading user wishlist...');
        const response = await fetch('/Home/GetUserWishlist', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        
        console.log('Wishlist API response status:', response.status);
        const data = await response.json();
        console.log('Wishlist API response data:', data);
        
        if (data.success && data.wishlistedProductIds) {
            console.log('Found wishlisted products:', data.wishlistedProductIds);
            // Mark all wishlisted products with red hearts
            data.wishlistedProductIds.forEach(productId => {
                // Try multiple selectors to find the button
                let button = document.querySelector(`[data-product-id="${productId}"]`);
                if (!button) {
                    button = document.querySelector(`button[data-product-id="${productId}"]`);
                }
                if (!button) {
                    button = document.querySelector(`.wishlist-btn[data-product-id="${productId}"]`);
                }
                
                if (button) {
                    const icon = button.querySelector('i');
                    if (icon) {
                        icon.style.setProperty('color', '#e74c3c', 'important');
                        button.setAttribute('data-wishlisted', 'true');
                        console.log(`Marked product ${productId} as wishlisted`);
                    } else {
                        console.log(`Icon not found for product ${productId}`);
                    }
                } else {
                    console.log(`Button not found for product ${productId}`);
                    // Log all wishlist buttons to debug
                    const allButtons = document.querySelectorAll('.wishlist-btn');
                    console.log('All wishlist buttons:', Array.from(allButtons).map(b => b.getAttribute('data-product-id')));
                }
            });
        } else {
            console.log('No wishlisted products found or API error');
        }
    } catch (error) {
        console.error('Error loading user wishlist:', error);
    }
}

function addToWishlist(productId, button) {
    // Check if already wishlisted
    if (button.hasAttribute('data-wishlisted')) {
        showMessage('Thông báo', 'Sản phẩm đã có trong danh sách yêu thích', 'info');
        return;
    }
    
    // Show loading state
    const icon = button.querySelector('i');
    button.disabled = true;
    icon.style.color = '#999'; // Loading color
    
    // Get anti-forgery token
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    
    // Create form data
    const formData = new FormData();
    formData.append('Id', productId);
    if (token) {
        formData.append('__RequestVerificationToken', token);
    }
    
    console.log('Adding to wishlist:', productId);
    
    // Send request
    fetch('/Home/AddWishList', {
        method: 'POST',
        body: formData
    })
    .then(response => {
        console.log('Response status:', response.status);
        return response.json();
    })
    .then(data => {
        console.log('Response data:', data);
        
        // Reset button state
        button.disabled = false;
        
        if (data.success) {
            // Success - change heart to red and keep it red permanently
            icon.style.setProperty('color', '#e74c3c', 'important');
            button.setAttribute('data-wishlisted', 'true');
            
            console.log(`Successfully added product ${productId} to wishlist`);
            
            // Show success message
            showMessage('Thành công!', data.message, 'success');
        } else {
            // Error - only show error message, don't change heart color if it was already red
            if (!button.hasAttribute('data-wishlisted')) {
                icon.style.color = '#000000';
            }
            showMessage('Lỗi!', data.message || 'Có lỗi xảy ra', 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        
        // Reset button state
        button.disabled = false;
        
        // Don't change heart color if it was already red (wishlisted)
        if (!button.hasAttribute('data-wishlisted')) {
            icon.style.color = '#000000';
        }
        
        showMessage('Lỗi!', 'Không thể kết nối đến máy chủ', 'error');
    });
}

function showMessage(title, message, type) {
    // Try to use SweetAlert if available
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: title,
            text: message,
            icon: type,
            timer: type === 'success' ? 2000 : 4000,
            showConfirmButton: type !== 'success'
        });
    } else {
        // Fallback to alert
        alert(`${title}\n${message}`);
    }
}

// Function to refresh wishlist display (useful for dynamic content)
function refreshWishlistDisplay() {
    console.log('Refreshing wishlist display...');
    loadUserWishlist();
}

// Function to test wishlist API manually
function testWishlistAPI() {
    console.log('Testing wishlist API...');
    fetch('/Home/GetUserWishlist', {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => response.json())
    .then(data => {
        console.log('Test API response:', data);
        if (data.success && data.wishlistedProductIds) {
            console.log('Wishlisted products:', data.wishlistedProductIds);
        }
    })
    .catch(error => {
        console.error('Test API error:', error);
    });
}

// Expose test function globally
window.testWishlistAPI = testWishlistAPI;
