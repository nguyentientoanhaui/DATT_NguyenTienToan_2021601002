// Brand Filter JavaScript
document.addEventListener('DOMContentLoaded', function() {
	// Add event listeners for filter sections
	const filterTitles = document.querySelectorAll('.filter-title');
	filterTitles.forEach(title => {
		title.addEventListener('click', function() {
			const filterSection = this.closest('.filter-section');
			const filterOptions = filterSection.querySelector('.filter-options');
			const icon = this.querySelector('i');
			
			// Toggle filter options visibility
			if (filterOptions.style.display === 'none' || filterOptions.style.display === '') {
				filterOptions.style.display = 'block';
				icon.style.transform = 'rotate(180deg)';
			} else {
				filterOptions.style.display = 'none';
				icon.style.transform = 'rotate(0deg)';
			}
		});
	});
	
	// Add event listeners for checkboxes
	const checkboxes = document.querySelectorAll('input[type="checkbox"]');
	checkboxes.forEach(checkbox => {
		checkbox.addEventListener('change', function() {
			// Apply filters immediately for checkboxes
			applyFilters();
		});
	});
	
	// Add event listeners for price inputs
	const priceInputs = document.querySelectorAll('input[name="minPrice"], input[name="maxPrice"]');
	priceInputs.forEach(input => {
		input.addEventListener('blur', function() {
			// Only apply filters when user finishes typing (on blur)
			const minPrice = document.querySelector('input[name="minPrice"]').value;
			const maxPrice = document.querySelector('input[name="maxPrice"]').value;
			
			if (minPrice || maxPrice) {
				applyFilters();
			}
		});
	});
});

// Normalize query params to a key->sorted array map (ignores order/duplicates)
function normalizeParams(searchParams) {
	const map = {};
	searchParams.forEach((value, key) => {
		if (!map[key]) map[key] = [];
		map[key].push(value);
	});
	Object.keys(map).forEach(k => map[k].sort());
	return map;
}

function areQueriesEquivalent(currentSearch, newParams) {
	const current = new URLSearchParams(currentSearch);
	// Ignore transient page param when comparing
	current.delete('page');
	const currentMap = normalizeParams(current);
	const newParamsClone = new URLSearchParams(newParams.toString());
	newParamsClone.delete('page');
	const nextMap = normalizeParams(newParamsClone);
	const allKeys = new Set([...Object.keys(currentMap), ...Object.keys(nextMap)]);
	for (const key of allKeys) {
		const a = currentMap[key] || [];
		const b = nextMap[key] || [];
		if (a.length !== b.length) return false;
		for (let i = 0; i < a.length; i++) {
			if (a[i] !== b[i]) return false;
		}
	}
	return true;
}

// Apply filters function
function applyFilters() {
	const form = document.getElementById('filter-form');
	if (!form) return;
	
	const formData = new FormData(form);
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
	const brandId = currentUrl.searchParams.get('brandId');
	
	if (pageSize) params.set('pageSize', pageSize);
	if (sortBy) params.set('sortBy', sortBy);
	if (brandId) params.set('brandId', brandId);
	
	// Reset to page 1 when applying filters
	params.set('page', '1');
	
	// Only navigate if query actually changed (order-insensitive)
	if (!areQueriesEquivalent(window.location.search, params)) {
		// Update only the search part to avoid absolute vs relative mismatches
		window.location.search = '?' + params.toString();
	}
}

// Change per page function
function changePerPage(pageSize) {
	console.log('changePerPage called with:', pageSize);
	const url = new URL(window.location);
	url.searchParams.set('pageSize', pageSize);
	url.searchParams.set('page', '1');
	
	// Preserve search term if it exists
	const searchTerm = url.searchParams.get('searchTerm');
	if (searchTerm) {
		url.searchParams.set('searchTerm', searchTerm);
	}
	
	// Preserve all filter parameters
	const form = document.getElementById('filter-form');
	if (form) {
		const formData = new FormData(form);
		for (let [key, value] of formData.entries()) {
			if (value) {
				url.searchParams.set(key, value);
			}
		}
	}
	
	console.log('Redirecting to:', url.toString());
	window.location.href = url.toString();
}

// Change sort function
function changeSort(sortBy) {
	console.log('changeSort called with:', sortBy);
	const url = new URL(window.location);
	url.searchParams.set('sortBy', sortBy);
	url.searchParams.set('page', '1');
	
	// Preserve search term if it exists
	const searchTerm = url.searchParams.get('searchTerm');
	if (searchTerm) {
		url.searchParams.set('searchTerm', searchTerm);
	}
	
	// Preserve all filter parameters
	const form = document.getElementById('filter-form');
	if (form) {
		const formData = new FormData(form);
		for (let [key, value] of formData.entries()) {
			if (value) {
				url.searchParams.set(key, value);
			}
		}
	}
	
	console.log('Redirecting to:', url.toString());
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

// Clear all filters function
function clearAllFilters() {
	const form = document.getElementById('filter-form');
	if (!form) return;
	
	// Clear all checkboxes
	const checkboxes = form.querySelectorAll('input[type="checkbox"]');
	checkboxes.forEach(checkbox => {
		checkbox.checked = false;
	});
	
	// Clear price inputs
	const priceInputs = form.querySelectorAll('input[name="minPrice"], input[name="maxPrice"]');
	priceInputs.forEach(input => {
		input.value = '';
	});
	
	// Apply the cleared filters
	applyFilters();
}

// Show notification function
function showNotification(title, message, type) {
    // Sử dụng SweetAlert nếu có, nếu không thì dùng alert đơn giản
    if (typeof Swal !== 'undefined') {
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
