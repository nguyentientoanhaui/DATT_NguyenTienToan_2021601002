// Order Notifications System
class OrderNotificationSystem {
    constructor() {
        this.checkInterval = 30000; // Check every 30 seconds
        this.lastOrderStatus = {};
        this.init();
    }

    init() {
        // Start checking for order status changes
        this.startStatusChecking();
        
        // Listen for order status updates
        this.listenForStatusUpdates();
    }

    startStatusChecking() {
        setInterval(() => {
            this.checkOrderStatus();
        }, this.checkInterval);
    }

    async checkOrderStatus() {
        try {
            const response = await fetch('/Account/GetOrderStatuses', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                }
            });

            if (response.ok) {
                const orderStatuses = await response.json();
                this.processOrderStatusChanges(orderStatuses);
            }
        } catch (error) {
            console.log('Error checking order status:', error);
        }
    }

    processOrderStatusChanges(orderStatuses) {
        orderStatuses.forEach(order => {
            const orderCode = order.orderCode;
            const currentStatus = order.status;
            const previousStatus = this.lastOrderStatus[orderCode];

            if (previousStatus !== undefined && previousStatus !== currentStatus) {
                this.showStatusChangeNotification(order);
            }

            this.lastOrderStatus[orderCode] = currentStatus;
        });
    }

    showStatusChangeNotification(order) {
        const statusMessages = {
            0: 'Đơn hàng đã được giao thành công!',
            1: 'Đơn hàng mới đã được tạo',
            2: 'Đơn hàng đang chờ xác nhận',
            3: 'Đơn hàng đã bị hủy',
            4: 'Đơn hàng đang được giao',
            5: 'Yêu cầu hoàn hàng',
            6: 'Hoàn hàng thành công'
        };

        const message = statusMessages[order.status] || 'Trạng thái đơn hàng đã thay đổi';
        
        // Show browser notification if supported
        if ('Notification' in window && Notification.permission === 'granted') {
            new Notification('Cập nhật đơn hàng', {
                body: `Mã đơn hàng ${order.orderCode}: ${message}`,
                icon: '/images/logo.png'
            });
        }

        // Show in-app notification
        this.showInAppNotification(order.orderCode, message);
    }

    showInAppNotification(orderCode, message) {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = 'order-notification';
        notification.innerHTML = `
            <div class="notification-content">
                <h4>Cập nhật đơn hàng</h4>
                <p><strong>Mã đơn hàng:</strong> ${orderCode}</p>
                <p>${message}</p>
                <button onclick="this.parentElement.parentElement.remove()" class="close-btn">Đóng</button>
            </div>
        `;

        // Add styles
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: white;
            border: 1px solid #ddd;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            z-index: 9999;
            max-width: 300px;
            animation: slideIn 0.3s ease-out;
        `;

        // Add to page
        document.body.appendChild(notification);

        // Auto remove after 5 seconds
        setTimeout(() => {
            if (notification.parentElement) {
                notification.remove();
            }
        }, 5000);
    }

    listenForStatusUpdates() {
        // Listen for real-time updates (if using SignalR or similar)
        // This is a placeholder for future implementation
    }

    requestNotificationPermission() {
        if ('Notification' in window && Notification.permission === 'default') {
            Notification.requestPermission().then(permission => {
                if (permission === 'granted') {
                    console.log('Notification permission granted');
                }
            });
        }
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Only initialize on pages where user is logged in
    if (document.querySelector('[data-user-authenticated="true"]')) {
        const orderNotifications = new OrderNotificationSystem();
        orderNotifications.requestNotificationPermission();
    }
});

// Add CSS for notifications
const notificationStyles = `
    @keyframes slideIn {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }

    .order-notification {
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    }

    .notification-content {
        padding: 15px;
    }

    .notification-content h4 {
        margin: 0 0 10px 0;
        color: #333;
        font-size: 16px;
    }

    .notification-content p {
        margin: 5px 0;
        color: #666;
        font-size: 14px;
    }

    .close-btn {
        background: #007bff;
        color: white;
        border: none;
        padding: 5px 10px;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
        margin-top: 10px;
    }

    .close-btn:hover {
        background: #0056b3;
    }
`;

// Add styles to head
const styleSheet = document.createElement('style');
styleSheet.textContent = notificationStyles;
document.head.appendChild(styleSheet);
