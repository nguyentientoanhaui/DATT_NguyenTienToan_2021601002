// Center Notification System - Minimalist Black & White Design
class CenterNotification {
    constructor() {
        this.notifications = [];
        this.maxNotifications = 3;
        this.defaultDuration = 3000;
        this.init();
    }

    init() {
        // Create notification container
        this.container = document.createElement('div');
        this.container.id = 'center-notification-container';
        this.container.style.cssText = `
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            z-index: 10000;
            pointer-events: none;
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 10px;
        `;
        
        document.body.appendChild(this.container);
        this.addStyles();
    }

    addStyles() {
        const style = document.createElement('style');
        style.textContent = `
            .center-notification {
                background: white;
                border: 1px solid #e5e5e5;
                border-radius: 8px;
                box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
                padding: 20px 24px;
                min-width: 300px;
                max-width: 500px;
                text-align: center;
                pointer-events: auto;
                opacity: 0;
                transform: scale(0.8) translateY(-20px);
                transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
                position: relative;
                overflow: hidden;
            }

            .center-notification.show {
                opacity: 1;
                transform: scale(1) translateY(0);
            }

            .center-notification.hide {
                opacity: 0;
                transform: scale(0.8) translateY(-20px);
            }

            .center-notification.success {
                border-left: 4px solid #27ae60;
            }

            .center-notification.error {
                border-left: 4px solid #e74c3c;
            }

            .center-notification.warning {
                border-left: 4px solid #f39c12;
            }

            .center-notification.info {
                border-left: 4px solid #3498db;
            }

            .notification-icon {
                font-size: 24px;
                margin-bottom: 12px;
                display: block;
            }

            .notification-title {
                font-size: 16px;
                font-weight: 600;
                color: #333;
                margin: 0 0 8px 0;
                line-height: 1.3;
            }

            .notification-message {
                font-size: 14px;
                color: #666;
                margin: 0;
                line-height: 1.4;
            }

            .notification-close {
                position: absolute;
                top: 8px;
                right: 8px;
                background: none;
                border: none;
                font-size: 18px;
                color: #999;
                cursor: pointer;
                width: 24px;
                height: 24px;
                display: flex;
                align-items: center;
                justify-content: center;
                border-radius: 50%;
                transition: all 0.2s ease;
            }

            .notification-close:hover {
                background: #f5f5f5;
                color: #333;
            }

            .notification-progress {
                position: absolute;
                bottom: 0;
                left: 0;
                height: 3px;
                background: #333;
                transition: width linear;
                border-radius: 0 0 8px 8px;
            }

            .notification-progress.success {
                background: #27ae60;
            }

            .notification-progress.error {
                background: #e74c3c;
            }

            .notification-progress.warning {
                background: #f39c12;
            }

            .notification-progress.info {
                background: #3498db;
            }

            /* Animation for multiple notifications */
            .center-notification:nth-child(1) {
                z-index: 10003;
            }

            .center-notification:nth-child(2) {
                z-index: 10002;
                transform: translate(-50%, -50%) scale(0.95);
            }

            .center-notification:nth-child(3) {
                z-index: 10001;
                transform: translate(-50%, -50%) scale(0.9);
            }

            /* Responsive */
            @@media (max-width: 480px) {
                .center-notification {
                    min-width: 280px;
                    max-width: calc(100vw - 40px);
                    padding: 16px 20px;
                }

                .notification-title {
                    font-size: 15px;
                }

                .notification-message {
                    font-size: 13px;
                }
            }
        `;
        document.head.appendChild(style);
    }

    show(message, type = 'info', title = null, duration = null) {
        // Remove oldest notification if max reached
        if (this.notifications.length >= this.maxNotifications) {
            this.remove(this.notifications[0]);
        }

        const notification = this.createNotification(message, type, title, duration);
        this.container.appendChild(notification);
        this.notifications.push(notification);

        // Show animation
        setTimeout(() => {
            notification.classList.add('show');
        }, 10);

        // Auto remove
        const autoDuration = duration || this.defaultDuration;
        if (autoDuration > 0) {
            setTimeout(() => {
                this.remove(notification);
            }, autoDuration);
        }

        return notification;
    }

    createNotification(message, type, title, duration) {
        const notification = document.createElement('div');
        notification.className = `center-notification ${type}`;

        const icons = {
            success: '✓',
            error: '✕',
            warning: '⚠',
            info: 'ℹ'
        };

        const titles = {
            success: 'Thành công',
            error: 'Lỗi',
            warning: 'Cảnh báo',
            info: 'Thông báo'
        };

        const displayTitle = title || titles[type] || titles.info;
        const icon = icons[type] || icons.info;

        notification.innerHTML = `
            <button class="notification-close" onclick="centerNotification.remove(this.parentElement)">×</button>
            <span class="notification-icon">${icon}</span>
            <h4 class="notification-title">${displayTitle}</h4>
            <p class="notification-message">${message}</p>
            <div class="notification-progress ${type}"></div>
        `;

        // Add progress bar animation
        if (duration || this.defaultDuration > 0) {
            const progressBar = notification.querySelector('.notification-progress');
            const autoDuration = duration || this.defaultDuration;
            progressBar.style.transition = `width ${autoDuration}ms linear`;
            setTimeout(() => {
                progressBar.style.width = '0%';
            }, 10);
        }

        return notification;
    }

    remove(notification) {
        if (!notification || !notification.parentElement) return;

        notification.classList.remove('show');
        notification.classList.add('hide');

        setTimeout(() => {
            if (notification.parentElement) {
                notification.remove();
                const index = this.notifications.indexOf(notification);
                if (index > -1) {
                    this.notifications.splice(index, 1);
                }
            }
        }, 300);
    }

    clear() {
        this.notifications.forEach(notification => {
            this.remove(notification);
        });
    }

    // Convenience methods
    success(message, title = null, duration = null) {
        return this.show(message, 'success', title, duration);
    }

    error(message, title = null, duration = null) {
        return this.show(message, 'error', title, duration);
    }

    warning(message, title = null, duration = null) {
        return this.show(message, 'warning', title, duration);
    }

    info(message, title = null, duration = null) {
        return this.show(message, 'info', title, duration);
    }
}

// Initialize global instance
let centerNotification;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    centerNotification = new CenterNotification();
});

// Global functions for easy access
function showCenterNotification(message, type = 'info', title = null, duration = null) {
    if (centerNotification) {
        return centerNotification.show(message, type, title, duration);
    }
}

function showSuccess(message, title = null, duration = null) {
    if (centerNotification) {
        return centerNotification.success(message, title, duration);
    }
}

function showError(message, title = null, duration = null) {
    if (centerNotification) {
        return centerNotification.error(message, title, duration);
    }
}

function showWarning(message, title = null, duration = null) {
    if (centerNotification) {
        return centerNotification.warning(message, title, duration);
    }
}

function showInfo(message, title = null, duration = null) {
    if (centerNotification) {
        return centerNotification.info(message, title, duration);
    }
}

// Export for module usage
if (typeof module !== 'undefined' && module.exports) {
    module.exports = CenterNotification;
}
