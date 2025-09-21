// Product Filter JavaScript - Simple Version
document.addEventListener('DOMContentLoaded', function() {
    // Only add event listeners, don't call applyFilters on load
    addFilterEventListeners();
});

// Add event listeners for filter interactions
function addFilterEventListeners() {
    const filterForm = document.getElementById('filter-form');
    if (!filterForm) return;
    
    // Add event listeners for filter sections
    const filterTitles = filterForm.querySelectorAll('.filter-title');
    filterTitles.forEach(title => {
        title.addEventListener('click', function() {
            const filterSection = this.closest('.filter-section');
            const filterOptions = filterSection.querySelector('.filter-options');
            const icon = this.querySelector('i');
            
            // Toggle filter options visibility with smooth animation
            if (filterOptions.style.display === 'none' || filterOptions.style.display === '') {
                filterOptions.style.display = 'block';
                filterOptions.style.maxHeight = '200px';
                filterOptions.style.opacity = '1';
                icon.style.transform = 'rotate(180deg)';
                
                // Smooth scroll to show all options if needed
                setTimeout(() => {
                    if (filterOptions.scrollHeight > 200) {
                        filterOptions.scrollTop = 0;
                    }
                }, 100);
            } else {
                filterOptions.style.maxHeight = '0';
                filterOptions.style.opacity = '0';
                setTimeout(() => {
                    filterOptions.style.display = 'none';
                }, 300);
                icon.style.transform = 'rotate(0deg)';
            }
        });
    });
    
    // Add change listeners for checkboxes with debouncing
    const checkboxes = filterForm.querySelectorAll('input[type="checkbox"]');
    let filterTimeout;
    checkboxes.forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            // Debounce filter application to prevent too many requests
            clearTimeout(filterTimeout);
            filterTimeout = setTimeout(() => {
                applyFilters();
            }, 300);
        });
    });
    
    // Add blur listeners for price inputs with debouncing
    const priceInputs = filterForm.querySelectorAll('input[name="minPrice"], input[name="maxPrice"]');
    let priceTimeout;
    priceInputs.forEach(input => {
        input.addEventListener('blur', function() {
            const minPrice = filterForm.querySelector('input[name="minPrice"]').value;
            const maxPrice = filterForm.querySelector('input[name="maxPrice"]').value;
            
            if (minPrice || maxPrice) {
                clearTimeout(priceTimeout);
                priceTimeout = setTimeout(() => {
                    applyFilters();
                }, 300);
            }
        });
    });
}

// Clear all filters
function clearAllFilters() {
    const filterForm = document.getElementById('filter-form');
    if (!filterForm) return;
    
    // Uncheck all checkboxes
    const checkboxes = filterForm.querySelectorAll('input[type="checkbox"]');
    checkboxes.forEach(checkbox => {
        checkbox.checked = false;
    });
    
    // Clear price inputs
    const priceInputs = filterForm.querySelectorAll('input[name="minPrice"], input[name="maxPrice"]');
    priceInputs.forEach(input => {
        input.value = '';
    });
    
    // Apply filters to reset the page
    applyFilters();
}

// Apply filters function
function applyFilters() {
    const filterForm = document.getElementById('filter-form');
    if (!filterForm) return;
    
    try {
        const formData = new FormData(filterForm);
        const params = new URLSearchParams();
        
        // Add all form data to URL parameters
        for (let [key, value] of formData.entries()) {
            if (value) {
                params.append(key, value);
            }
        }
        
        // Add current page size and sort if they exist
        const currentUrl = new URL(window.location);
        const pageSize = currentUrl.searchParams.get('pageSize');
        const sortBy = currentUrl.searchParams.get('sortBy');
        
        if (pageSize) params.set('pageSize', pageSize);
        if (sortBy) params.set('sortBy', sortBy);
        
        // Reset to page 1 when applying filters
        params.set('page', '1');
        
        // Build new URL
        const newUrl = `${window.location.pathname}?${params.toString()}`;
        
        // Only navigate if URL is different to prevent infinite loops
        if (newUrl !== window.location.href) {
            window.location.href = newUrl;
        }
    } catch (error) {
        console.error('Error applying filters:', error);
    }
}

// Change per page function
function changePerPage(pageSize) {
    const url = new URL(window.location);
    url.searchParams.set('pageSize', pageSize);
    url.searchParams.set('page', '1');
    
    // Preserve all filter parameters
    const filterForm = document.getElementById('filter-form');
    if (filterForm) {
        const formData = new FormData(filterForm);
        for (let [key, value] of formData.entries()) {
            if (value) {
                url.searchParams.set(key, value);
            }
        }
    }
    
    window.location.href = url.toString();
}

// Change sort function
function changeSort(sortBy) {
    const url = new URL(window.location);
    url.searchParams.set('sortBy', sortBy);
    url.searchParams.set('page', '1');
    
    // Preserve all filter parameters
    const filterForm = document.getElementById('filter-form');
    if (filterForm) {
        const formData = new FormData(filterForm);
        for (let [key, value] of formData.entries()) {
            if (value) {
                url.searchParams.set(key, value);
            }
        }
    }
    
    window.location.href = url.toString();
}

// Add to cart function
function addToCart(productId) {
    fetch('/Home/AddToCart', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: `productId=${productId}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showNotification('Success', data.message, 'success');
            if (typeof updateCartCount === 'function') {
                updateCartCount();
            }
        } else {
            showNotification('Error', data.message, 'error');
        }
    })
    .catch(error => {
        showNotification('Error', 'Could not add to cart', 'error');
    });
}

// Add to wishlist function
function addToWishlist(productId) {
    fetch('/Home/AddWishList', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: `Id=${productId}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showNotification('Success', data.message, 'success');
        } else {
            showNotification('Error', data.message, 'error');
        }
    })
    .catch(error => {
        showNotification('Error', 'Could not add to wishlist', 'error');
    });
}

// Get quote function
function getQuote(productId) {
    showNotification('Info', 'Get Quote feature coming soon', 'info');
}

// Show notification function
function showNotification(title, message, type) {
    // Use new center notification system if available
    if (typeof showCenterNotification !== 'undefined') {
        showCenterNotification(message, type, title);
    } else if (typeof Swal !== 'undefined') {
        let icon = 'info';
        let swalTitle = title;
        
        if (type === 'success') {
            icon = 'success';
            swalTitle = 'Thành công';
        } else if (type === 'error') {
            icon = 'error';
            swalTitle = 'Lỗi';
        } else if (type === 'warning') {
            icon = 'warning';
            swalTitle = 'Cảnh báo';
        }
        
        Swal.fire({
            title: swalTitle,
            text: message,
            icon: icon,
            timer: type === 'success' ? 2000 : 3000,
            showConfirmButton: false,
            toast: true,
            position: 'top-end'
        });
    } else {
        // Fallback: sử dụng alert đơn giản
        alert(`${title}: ${message}`);
    }
}

// Update cart count function (if exists)
function updateCartCount() {
    // This function should be implemented in the main site.js or similar
    // For now, we'll just update the cart count without reloading
    // location.reload(); // Removed to prevent infinite loading
}

