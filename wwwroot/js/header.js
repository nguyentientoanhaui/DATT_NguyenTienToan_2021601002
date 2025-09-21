// Header JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Initialize header functionality
    initializeHeaderIcons();
    initializeSearch();
    initializeCartCount();
    initializeCompareCount();
    initializeSearchSuggestions();
    initializeScrollEffect();
});

function initializeScrollEffect() {
    const header = document.querySelector('.integrated-header');
    
    window.addEventListener('scroll', function() {
        if (window.scrollY > 50) {
            header.classList.add('scrolled');
        } else {
            header.classList.remove('scrolled');
        }
    });
}

function initializeHeaderIcons() {
    // Cart icon click (top bar)
    const cartIcon = document.getElementById('cart-icon');
    if (cartIcon) {
        cartIcon.addEventListener('click', function() {
            window.location.href = '/Cart';
        });
    }

    // Cart icon click (main header)
    const cartIconMain = document.getElementById('cart-icon-main');
    if (cartIconMain) {
        cartIconMain.addEventListener('click', function() {
            window.location.href = '/Cart';
        });
    }

    // User dropdown toggle (top bar)
    const userIcon = document.getElementById('user-icon');
    const userDropdown = document.getElementById('user-dropdown-menu');
    
    if (userIcon && userDropdown) {
        console.log('User dropdown initialized (top bar)');
        userIcon.addEventListener('click', function(e) {
            e.stopPropagation();
            console.log('User icon clicked (top bar)');
            userDropdown.classList.toggle('show');
            console.log('Dropdown show class:', userDropdown.classList.contains('show'));
        });
    } else {
        console.log('User dropdown elements not found (top bar)', { userIcon, userDropdown });
    }

    // User dropdown toggle (main header)
    const userIconMain = document.getElementById('user-icon-main');
    const userDropdownMain = document.getElementById('user-dropdown-menu-main');
    
    if (userIconMain && userDropdownMain) {
        console.log('User dropdown initialized (main header)');
        userIconMain.addEventListener('click', function(e) {
            e.stopPropagation();
            console.log('User icon clicked (main header)');
            userDropdownMain.classList.toggle('show');
            console.log('Dropdown main show class:', userDropdownMain.classList.contains('show'));
        });
    } else {
        console.log('User dropdown elements not found (main header)', { userIconMain, userDropdownMain });
    }

    // Close dropdown when clicking outside
    document.addEventListener('click', function() {
        if (userDropdown) {
            userDropdown.classList.remove('show');
        }
        if (userDropdownMain) {
            userDropdownMain.classList.remove('show');
        }
    });

    // Mobile menu toggle (top bar)
    const mobileMenuIcon = document.getElementById('menu-icon');
    const mobileMenuOverlay = document.getElementById('mobile-menu-overlay');
    const closeMenuBtn = document.getElementById('close-menu');

    // Mobile menu toggle (main header)
    const mobileMenuIconMain = document.getElementById('menu-icon-main');

    if (mobileMenuIcon) {
        mobileMenuIcon.addEventListener('click', function() {
            toggleMobileMenu();
        });
    }

    if (mobileMenuIconMain) {
        mobileMenuIconMain.addEventListener('click', function() {
            toggleMobileMenu();
        });
    }

    if (closeMenuBtn) {
        closeMenuBtn.addEventListener('click', function() {
            closeMobileMenu();
        });
    }

    if (mobileMenuOverlay) {
        mobileMenuOverlay.addEventListener('click', function(e) {
            if (e.target === mobileMenuOverlay) {
                closeMobileMenu();
            }
        });
    }

    // Certified badge info
    const certifiedBadge = document.querySelector('.certified-badge');
    if (certifiedBadge) {
        certifiedBadge.addEventListener('click', function() {
            alert('Chúng tôi cam kết 100% sản phẩm chính hãng, có giấy tờ bảo hành đầy đủ.');
        });
    }
}

function initializeSearch() {
    // Top bar search
    const searchInput = document.getElementById('search-input');
    const searchIcon = document.getElementById('search-icon');
    const clearSearch = document.getElementById('clear-search');
    
    // Main header search
    const searchInputMain = document.getElementById('search-input-main');
    const searchIconMain = document.getElementById('search-icon-main');
    
    if (searchInput && searchIcon) {
        searchIcon.addEventListener('click', function() {
            performSearch(searchInput.value);
        });

        searchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                performSearch(this.value);
            }
        });

        // Search suggestions
        searchInput.addEventListener('input', function() {
            console.log('Search input changed:', this.value);
            if (this.value.length >= 2) {
                console.log('Loading search suggestions for:', this.value);
                loadSearchSuggestions(this.value, 'search-suggestions');
                if (clearSearch) clearSearch.style.display = 'block';
            } else {
                console.log('Hiding search suggestions');
                hideSearchSuggestions('search-suggestions');
                if (clearSearch) clearSearch.style.display = 'none';
            }
        });

        searchInput.addEventListener('focus', function() {
            if (this.value.length >= 2) {
                loadSearchSuggestions(this.value, 'search-suggestions');
                if (clearSearch) clearSearch.style.display = 'block';
            }
        });

        searchInput.addEventListener('blur', function() {
            setTimeout(() => {
                hideSearchSuggestions('search-suggestions');
            }, 200);
        });

        // Clear search
        if (clearSearch) {
            clearSearch.addEventListener('click', function() {
                searchInput.value = '';
                hideSearchSuggestions('search-suggestions');
                clearSearch.style.display = 'none';
                searchInput.focus();
            });
        }
    }
    
    // Main header search functionality
    if (searchInputMain && searchIconMain) {
        searchIconMain.addEventListener('click', function() {
            performSearch(searchInputMain.value);
        });

        searchInputMain.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                performSearch(this.value);
            }
        });
    }
}

function performSearch(searchTerm) {
    if (searchTerm.trim()) {
        window.location.href = `/Product/Search?searchTerm=${encodeURIComponent(searchTerm.trim())}`;
    }
}

function initializeCartCount() {
    loadCartCount();
}

function initializeCompareCount() {
    loadCompareCount();
}

function loadCartCount() {
    fetch('/Cart/GetCartCount')
        .then(response => response.json())
        .then(data => {
            const cartCountElements = document.querySelectorAll('#cart-count, #cart-count-main');
            cartCountElements.forEach(element => {
                element.textContent = data.count;
            });
        })
        .catch(error => {
            console.error('Error loading cart count:', error);
        });
}

function updateCartCount() {
    loadCartCount();
}

function loadCompareCount() {
    fetch('/Home/GetCompareCount')
        .then(response => response.json())
        .then(data => {
            const compareCountElement = document.getElementById('compareCount');
            if (compareCountElement) {
                compareCountElement.textContent = data.count;
                compareCountElement.style.display = data.count > 0 ? 'inline' : 'none';
            }
        })
        .catch(error => {
            console.error('Error loading compare count:', error);
        });
}

function updateCompareCount() {
    loadCompareCount();
}

function initializeSearchSuggestions() {
    // Search suggestions are now handled in initializeSearch()
}

function loadSearchSuggestions(searchTerm, containerId) {
    console.log('Fetching search suggestions for:', searchTerm);
    fetch(`/Product/GetSearchSuggestions?searchTerm=${encodeURIComponent(searchTerm)}`)
        .then(response => {
            console.log('Search suggestions response status:', response.status);
            return response.json();
        })
        .then(data => {
            console.log('Search suggestions data:', data);
            updateSearchSuggestions(data);
        })
        .catch(error => {
            console.error('Error loading search suggestions:', error);
            hideSearchSuggestions(containerId);
        });
}

function updateSearchSuggestions(data) {
    // Update Recently Searched Section
    const recentlySection = document.getElementById('recently-searched-section');
    const recentlyItems = document.getElementById('recently-searched-items');
    
    if (data.recentlySearched && data.recentlySearched.length > 0) {
        recentlyItems.innerHTML = '';
        data.recentlySearched.forEach(item => {
            const itemDiv = document.createElement('div');
            itemDiv.className = 'suggestion-item';
            itemDiv.innerHTML = `
                <i class="fas fa-clock"></i>
                <span>${item.term}</span>
                <i class="fas fa-times remove-history-item" data-term="${item.term}"></i>
            `;
            itemDiv.addEventListener('click', function(e) {
                if (!e.target.classList.contains('remove-history-item')) {
                    performSearch(item.term);
                }
            });
            recentlyItems.appendChild(itemDiv);
        });
        recentlySection.style.display = 'block';
    } else {
        recentlySection.style.display = 'none';
    }
    
    // Update Top Suggestions Section
    const topSection = document.getElementById('top-suggestions-section');
    const topItems = document.getElementById('top-suggestions-items');
    
    if (data.topSuggestions && data.topSuggestions.length > 0) {
        topItems.innerHTML = '';
        data.topSuggestions.forEach(item => {
            const itemDiv = document.createElement('div');
            itemDiv.className = 'suggestion-item';
            itemDiv.innerHTML = `
                <span>${item.term}</span>
                <i class="fas fa-arrow-right"></i>
            `;
            itemDiv.addEventListener('click', function() {
                performSearch(item.term);
            });
            topItems.appendChild(itemDiv);
        });
        topSection.style.display = 'block';
    } else {
        topSection.style.display = 'none';
    }
    
    // Update Product Suggestions Section
    const productSection = document.getElementById('product-suggestions-section');
    const productItems = document.getElementById('product-suggestions-items');
    
    if (data.productSuggestions && data.productSuggestions.length > 0) {
        productItems.innerHTML = '';
        data.productSuggestions.forEach(product => {
            console.log('Processing product:', product);
            console.log('Product price:', product.price, typeof product.price);
            
            // Ensure price is a number
            let displayPrice = product.price;
            if (typeof product.price === 'string') {
                displayPrice = parseFloat(product.price.replace(/[^\d.-]/g, ''));
            }
            
            const itemDiv = document.createElement('div');
            itemDiv.className = 'product-suggestion-item';
            itemDiv.innerHTML = `
                <a href="/Product/Details/${product.id}" class="product-suggestion-link">
                    <img src="${product.imageUrl}" alt="${product.name}" 
                         class="product-suggestion-image" 
                         onerror="this.src='/images/placeholder-product.jpg'; this.onerror=null;">
                    <div class="product-suggestion-info">
                        <div class="product-suggestion-name">${product.name}</div>
                        <div class="product-suggestion-details">${product.category} • ${product.brand}</div>
                    </div>
                    <div class="product-suggestion-price">${displayPrice.toLocaleString('vi-VN')} VNĐ</div>
                </a>
            `;
            itemDiv.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                console.log('Product suggestion clicked:', product);
                console.log('Navigating to:', `/Product/Details/${product.id}`);
                
                // Try multiple ways to navigate
                try {
                    window.location.href = `/Product/Details/${product.id}`;
                } catch (error) {
                    console.error('Error navigating:', error);
                    // Fallback method
                    window.open(`/Product/Details/${product.id}`, '_self');
                }
            });
            
            // Also add a direct link as backup
            itemDiv.style.cursor = 'pointer';
            itemDiv.title = `Click to view ${product.name}`;
            
            // Add click event to the link as well
            const link = itemDiv.querySelector('.product-suggestion-link');
            if (link) {
                link.addEventListener('click', function(e) {
                    console.log('Link clicked for product:', product.id);
                    // Let the default link behavior handle navigation
                });
            }
            
            productItems.appendChild(itemDiv);
        });
        productSection.style.display = 'block';
    } else {
        productSection.style.display = 'none';
    }
    
    // Show/hide the main suggestions container
    const suggestionsContainer = document.getElementById('search-suggestions');
    if (recentlySection.style.display !== 'none' || 
        topSection.style.display !== 'none' || 
        productSection.style.display !== 'none') {
        suggestionsContainer.style.display = 'block';
    } else {
        suggestionsContainer.style.display = 'none';
    }
    
    // Add event listeners for remove buttons
    document.querySelectorAll('.remove-history-item').forEach(btn => {
        btn.addEventListener('click', function(e) {
            e.stopPropagation();
            const term = this.getAttribute('data-term');
            removeSearchHistoryItem(term);
        });
    });
}

function hideSearchSuggestions(containerId) {
    const container = document.getElementById(containerId);
    if (container) {
        container.style.display = 'none';
    }
}

function clearSearchHistory() {
    fetch('/Product/ClearSearchHistory', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Reload search suggestions to update the display
            const searchInput = document.getElementById('search-input');
            if (searchInput && searchInput.value.length >= 2) {
                loadSearchSuggestions(searchInput.value, 'search-suggestions');
            }
        }
    })
    .catch(error => {
        console.error('Error clearing search history:', error);
    });
}

function removeSearchHistoryItem(searchTerm) {
    fetch('/Product/RemoveSearchHistoryItem', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ searchTerm: searchTerm })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Reload search suggestions to update the display
            const searchInput = document.getElementById('search-input');
            if (searchInput && searchInput.value.length >= 2) {
                loadSearchSuggestions(searchInput.value, 'search-suggestions');
            }
        }
    })
    .catch(error => {
        console.error('Error removing search history item:', error);
    });
}

function selectSuggestion(productId) {
    window.location.href = `/Product/Details/${productId}`;
}

function toggleMobileMenu() {
    const overlay = document.getElementById('mobile-menu-overlay');
    if (overlay) {
        overlay.style.display = 'block';
        document.body.style.overflow = 'hidden';
    }
}

function closeMobileMenu() {
    const overlay = document.getElementById('mobile-menu-overlay');
    if (overlay) {
        overlay.style.display = 'none';
        document.body.style.overflow = 'auto';
    }
}

function formatPriceSimple(price) {
    console.log('formatPriceSimple called with:', price, typeof price);
    
    try {
        // Convert to number if it's a string
        let numPrice;
        if (typeof price === 'string') {
            // Remove any non-numeric characters except decimal point
            numPrice = parseFloat(price.replace(/[^\d.-]/g, ''));
        } else {
            numPrice = price;
        }
        
        console.log('parsed price:', numPrice);
        
        // Simple formatting with commas
        const formatted = numPrice.toLocaleString('vi-VN') + ' VNĐ';
        console.log('formatted price:', formatted);
        return formatted;
    } catch (error) {
        console.error('Error formatting price:', error);
        return price + ' VNĐ';
    }
}

// Global functions
window.updateCartCount = updateCartCount;
window.clearSearchHistory = clearSearchHistory;
window.removeSearchHistoryItem = removeSearchHistoryItem;
