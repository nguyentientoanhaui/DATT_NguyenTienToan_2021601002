// Product Listing Page JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Initialize product listing functionality
    initializeProductListing();
    
    // Initialize lazy loading for images
    initializeLazyLoading();
    
    // Initialize product card interactions
    initializeProductCards();
});

// Product Listing Functions
function initializeProductListing() {
    // Add smooth scrolling for pagination
    const paginationLinks = document.querySelectorAll('.pagination a');
    paginationLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const href = this.getAttribute('href');
            if (href) {
                // Show loading state
                showPageLoading();
                window.location.href = href;
            }
        });
    });
    
    // Add keyboard navigation for product cards
    const productCards = document.querySelectorAll('.product-card');
    productCards.forEach(card => {
        card.addEventListener('keydown', function(e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                const link = this.querySelector('.product-name a');
                if (link) {
                    link.click();
                }
            }
        });
        
        // Make cards focusable
        card.setAttribute('tabindex', '0');
    });
}

// Lazy Loading for Images
function initializeLazyLoading() {
    const images = document.querySelectorAll('.product-image');
    
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src || img.src;
                    img.classList.remove('lazy');
                    imageObserver.unobserve(img);
                }
            });
        });
        
        images.forEach(img => {
            if (img.dataset.src) {
                imageObserver.observe(img);
            }
        });
    }
}

// Product Card Interactions
function initializeProductCards() {
    const productCards = document.querySelectorAll('.product-card');
    
    productCards.forEach(card => {
        // Add hover effects
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-5px)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
        
        // Add click outside to close any open modals
        card.addEventListener('click', function(e) {
            if (e.target.closest('.wishlist-btn') || 
                e.target.closest('.buy-btn') || 
                e.target.closest('.sell-btn') || 
                e.target.closest('.quote-btn')) {
                return; // Don't prevent default for buttons
            }
            
            // Allow normal navigation for product links
            if (e.target.closest('.product-name a')) {
                return;
            }
        });
    });
}

// Loading States
function showPageLoading() {
    const loadingOverlay = document.createElement('div');
    loadingOverlay.id = 'page-loading-overlay';
    loadingOverlay.innerHTML = `
        <div class="loading-spinner">
            <div class="spinner"></div>
            <p>Loading products...</p>
        </div>
    `;
    
    loadingOverlay.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(255, 255, 255, 0.9);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 9999;
    `;
    
    document.body.appendChild(loadingOverlay);
}

// Product Quick View Modal
function showQuickView(productId) {
    // Fetch product details via AJAX
    fetch(`/Product/QuickView/${productId}`)
        .then(response => response.text())
        .then(html => {
            const modal = document.createElement('div');
            modal.className = 'quick-view-modal';
            modal.innerHTML = `
                <div class="modal-overlay">
                    <div class="modal-content">
                        <button class="modal-close" onclick="closeQuickView()">×</button>
                        ${html}
                    </div>
                </div>
            `;
            
            document.body.appendChild(modal);
            
            // Add styles
            const modalStyles = `
                .quick-view-modal {
                    position: fixed;
                    top: 0;
                    left: 0;
                    width: 100%;
                    height: 100%;
                    z-index: 10000;
                    animation: fadeIn 0.3s ease;
                }
                
                .modal-overlay {
                    background: rgba(0, 0, 0, 0.5);
                    width: 100%;
                    height: 100%;
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    padding: 20px;
                }
                
                .modal-content {
                    background: white;
                    border-radius: 8px;
                    max-width: 800px;
                    width: 100%;
                    max-height: 90vh;
                    overflow-y: auto;
                    position: relative;
                    animation: slideIn 0.3s ease;
                }
                
                .modal-close {
                    position: absolute;
                    top: 15px;
                    right: 15px;
                    background: none;
                    border: none;
                    font-size: 24px;
                    cursor: pointer;
                    z-index: 1;
                    color: #666;
                }
                
                .modal-close:hover {
                    color: #333;
                }
                
                @keyframes fadeIn {
                    from { opacity: 0; }
                    to { opacity: 1; }
                }
                
                @keyframes slideIn {
                    from { transform: translateY(-50px); opacity: 0; }
                    to { transform: translateY(0); opacity: 1; }
                }
            `;
            
            const styleSheet = document.createElement('style');
            styleSheet.textContent = modalStyles;
            document.head.appendChild(styleSheet);
            
            // Close modal when clicking outside
            modal.querySelector('.modal-overlay').addEventListener('click', function(e) {
                if (e.target === this) {
                    closeQuickView();
                }
            });
            
            // Close modal with Escape key
            document.addEventListener('keydown', function(e) {
                if (e.key === 'Escape') {
                    closeQuickView();
                }
            });
        })
        .catch(error => {
            console.error('Error loading quick view:', error);
            showNotification('Failed to load product details.', 'error');
        });
}

function closeQuickView() {
    const modal = document.querySelector('.quick-view-modal');
    if (modal) {
        modal.remove();
    }
}

// Product Comparison
let comparisonList = [];

function addToComparison(productId) {
    if (comparisonList.length >= 4) {
        showNotification('You can compare up to 4 products at a time.', 'warning');
        return;
    }
    
    if (comparisonList.includes(productId)) {
        showNotification('Product is already in comparison list.', 'info');
        return;
    }
    
    comparisonList.push(productId);
    updateComparisonUI();
    showNotification('Product added to comparison list.', 'success');
}

function removeFromComparison(productId) {
    comparisonList = comparisonList.filter(id => id !== productId);
    updateComparisonUI();
}

function updateComparisonUI() {
    const comparisonBar = document.getElementById('comparison-bar');
    
    if (comparisonList.length > 0) {
        if (!comparisonBar) {
            createComparisonBar();
        }
        updateComparisonBar();
    } else {
        if (comparisonBar) {
            comparisonBar.remove();
        }
    }
}

function createComparisonBar() {
    const bar = document.createElement('div');
    bar.id = 'comparison-bar';
    bar.innerHTML = `
        <div class="comparison-content">
            <span class="comparison-title">Compare Products (${comparisonList.length}/4)</span>
            <div class="comparison-items"></div>
            <button class="compare-btn" onclick="compareProducts()">Compare Now</button>
            <button class="clear-comparison-btn" onclick="clearComparison()">Clear All</button>
        </div>
    `;
    
    bar.style.cssText = `
        position: fixed;
        bottom: 0;
        left: 0;
        width: 100%;
        background: var(--primary-color);
        color: white;
        padding: 15px 0;
        z-index: 1000;
        box-shadow: 0 -2px 10px rgba(0,0,0,0.1);
        animation: slideUp 0.3s ease;
    `;
    
    const barStyles = `
        .comparison-content {
            max-width: 1400px;
            margin: 0 auto;
            padding: 0 20px;
            display: flex;
            align-items: center;
            gap: 20px;
        }
        
        .comparison-title {
            font-weight: 600;
            white-space: nowrap;
        }
        
        .comparison-items {
            display: flex;
            gap: 10px;
            flex: 1;
            overflow-x: auto;
        }
        
        .comparison-item {
            background: rgba(255,255,255,0.1);
            padding: 8px 12px;
            border-radius: 4px;
            display: flex;
            align-items: center;
            gap: 8px;
            white-space: nowrap;
        }
        
        .comparison-item img {
            width: 30px;
            height: 30px;
            object-fit: cover;
            border-radius: 2px;
        }
        
        .comparison-item .remove-btn {
            background: none;
            border: none;
            color: white;
            cursor: pointer;
            font-size: 12px;
        }
        
        .compare-btn, .clear-comparison-btn {
            padding: 8px 16px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-weight: 600;
            white-space: nowrap;
        }
        
        .compare-btn {
            background: var(--accent-color);
            color: white;
        }
        
        .clear-comparison-btn {
            background: rgba(255,255,255,0.2);
            color: white;
        }
        
        @keyframes slideUp {
            from { transform: translateY(100%); }
            to { transform: translateY(0); }
        }
    `;
    
    const styleSheet = document.createElement('style');
    styleSheet.textContent = barStyles;
    document.head.appendChild(styleSheet);
    
    document.body.appendChild(bar);
}

function updateComparisonBar() {
    const itemsContainer = document.querySelector('.comparison-items');
    const title = document.querySelector('.comparison-title');
    
    if (itemsContainer && title) {
        title.textContent = `Compare Products (${comparisonList.length}/4)`;
        
        itemsContainer.innerHTML = comparisonList.map(productId => {
            const productCard = document.querySelector(`[data-product-id="${productId}"]`);
            const productName = productCard ? productCard.querySelector('.product-name').textContent : 'Product';
            const productImage = productCard ? productCard.querySelector('.product-image').src : '/images/placeholder-product.svg';
            
            return `
                <div class="comparison-item">
                    <img src="${productImage}" alt="${productName}">
                    <span>${productName}</span>
                    <button class="remove-btn" onclick="removeFromComparison(${productId})">×</button>
                </div>
            `;
        }).join('');
    }
}

function compareProducts() {
    if (comparisonList.length < 2) {
        showNotification('Please select at least 2 products to compare.', 'warning');
        return;
    }
    
    // Add products to compare table via API
    addProductsToCompare(comparisonList);
}

async function addProductsToCompare(productIds) {
    try {
        // Get anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        
        // Add each product to compare
        for (const productId of productIds) {
            const formData = new FormData();
            formData.append('Id', productId);
            if (token) {
                formData.append('__RequestVerificationToken', token);
            }
            
            const response = await fetch('/Home/AddCompare', {
                method: 'POST',
                body: formData
            });
            
            if (!response.ok) {
                throw new Error(`Failed to add product ${productId} to compare`);
            }
        }
        
        // Clear comparison list
        comparisonList = [];
        updateComparisonUI();
        
        // Redirect to compare page
        window.location.href = '/Home/Compare';
        
    } catch (error) {
        console.error('Error adding products to compare:', error);
        showNotification('Error adding products to compare', 'error');
    }
}

function clearComparison() {
    comparisonList = [];
    updateComparisonUI();
    showNotification('Comparison list cleared.', 'info');
}

// Enhanced Product Search
function initializeProductSearch() {
    const searchInput = document.querySelector('.product-search input');
    if (searchInput) {
        let searchTimeout;
        
        searchInput.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                performSearch(this.value);
            }, 500);
        });
    }
}

function performSearch(query) {
    if (query.length < 2) return;
    
    fetch(`/Product/Search?q=${encodeURIComponent(query)}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                updateSearchResults(data.results);
            }
        })
        .catch(error => {
            console.error('Search error:', error);
        });
}

function updateSearchResults(results) {
    // Update product grid with search results
    const productGrid = document.querySelector('.product-grid');
    if (productGrid && results.length > 0) {
        // Update grid with new results
        // This would require server-side rendering or client-side templating
    }
}

// Export functions for global access
window.productListing = {
    showQuickView,
    closeQuickView,
    addToComparison,
    removeFromComparison,
    compareProducts,
    clearComparison
};
