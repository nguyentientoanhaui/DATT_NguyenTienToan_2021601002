/**
 * Simple Loading Manager
 * Quản lý các trạng thái loading đơn giản và hiệu quả
 */

class SimpleLoading {
    constructor() {
        this.activeLoadings = new Set();
        this.defaultOptions = {
            type: 'dots', // dots, circle, pulse, bar
            text: 'Đang tải...',
            overlay: true,
            container: null,
            duration: 0 // 0 = không tự động ẩn
        };
    }

    /**
     * Hiển thị loading
     * @param {string} id - ID duy nhất cho loading
     * @param {object} options - Tùy chọn loading
     */
    show(id = 'default', options = {}) {
        const config = { ...this.defaultOptions, ...options };
        
        // Nếu đã có loading với ID này, ẩn nó trước
        this.hide(id);
        
        const loadingElement = this.createLoadingElement(id, config);
        
        if (config.container) {
            // Loading trong container cụ thể
            const container = typeof config.container === 'string' 
                ? document.querySelector(config.container) 
                : config.container;
            
            if (container) {
                container.style.position = 'relative';
                container.appendChild(loadingElement);
            }
        } else if (config.overlay) {
            // Loading overlay toàn màn hình
            document.body.appendChild(loadingElement);
        }
        
        this.activeLoadings.add(id);
        
        // Tự động ẩn sau thời gian nhất định
        if (config.duration > 0) {
            setTimeout(() => this.hide(id), config.duration);
        }
        
        return loadingElement;
    }

    /**
     * Ẩn loading
     * @param {string} id - ID của loading cần ẩn
     */
    hide(id = 'default') {
        const loadingElement = document.getElementById(`simple-loading-${id}`);
        
        if (loadingElement) {
            loadingElement.style.opacity = '0';
            loadingElement.style.transition = 'opacity 0.3s ease';
            
            setTimeout(() => {
                if (loadingElement.parentNode) {
                    loadingElement.parentNode.removeChild(loadingElement);
                }
            }, 300);
        }
        
        this.activeLoadings.delete(id);
    }

    /**
     * Ẩn tất cả loading
     */
    hideAll() {
        this.activeLoadings.forEach(id => this.hide(id));
    }

    /**
     * Tạo element loading
     * @param {string} id - ID của loading
     * @param {object} config - Cấu hình loading
     */
    createLoadingElement(id, config) {
        const loadingDiv = document.createElement('div');
        loadingDiv.id = `simple-loading-${id}`;
        loadingDiv.className = config.overlay ? 'simple-loading-overlay' : 'simple-loading';
        
        const container = document.createElement('div');
        container.className = 'simple-loading-container';
        
        let spinnerHTML = '';
        
        switch (config.type) {
            case 'dots':
                spinnerHTML = `
                    <div class="simple-loading-dots">
                        <div class="simple-loading-dot"></div>
                        <div class="simple-loading-dot"></div>
                        <div class="simple-loading-dot"></div>
                    </div>
                `;
                break;
            case 'circle':
                spinnerHTML = '<div class="simple-circle-loading"></div>';
                break;
            case 'pulse':
                spinnerHTML = '<div class="simple-pulse-loading"></div>';
                break;
            case 'bar':
                spinnerHTML = '<div class="simple-bar-loading"></div>';
                break;
            default:
                spinnerHTML = `
                    <div class="simple-loading-dots">
                        <div class="simple-loading-dot"></div>
                        <div class="simple-loading-dot"></div>
                        <div class="simple-loading-dot"></div>
                    </div>
                `;
        }
        
        container.innerHTML = `
            ${spinnerHTML}
            ${config.text ? `<div class="simple-loading-text">${config.text}</div>` : ''}
        `;
        
        loadingDiv.appendChild(container);
        return loadingDiv;
    }

    /**
     * Tạo loading cho button
     * @param {HTMLElement} button - Button element
     * @param {string} text - Text hiển thị khi loading
     */
    buttonLoading(button, text = 'Đang xử lý...') {
        if (!button) return;
        
        const originalText = button.textContent;
        const originalDisabled = button.disabled;
        
        button.classList.add('btn-loading');
        button.disabled = true;
        button.textContent = text;
        
        return {
            hide: () => {
                button.classList.remove('btn-loading');
                button.disabled = originalDisabled;
                button.textContent = originalText;
            }
        };
    }

    /**
     * Tạo loading cho card
     * @param {HTMLElement} card - Card element
     */
    cardLoading(card) {
        if (!card) return;
        
        card.classList.add('card-loading');
        
        return {
            hide: () => {
                card.classList.remove('card-loading');
            }
        };
    }
}

// Tạo instance global
window.simpleLoading = new SimpleLoading();

// Utility functions
window.showLoading = (id, options) => window.simpleLoading.show(id, options);
window.hideLoading = (id) => window.simpleLoading.hide(id);
window.hideAllLoading = () => window.simpleLoading.hideAll();

// Auto-hide loading khi page load
document.addEventListener('DOMContentLoaded', () => {
    // Ẩn tất cả loading sau 1 giây
    setTimeout(() => {
        window.simpleLoading.hideAll();
    }, 1000);
});

// Export cho module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = SimpleLoading;
}
