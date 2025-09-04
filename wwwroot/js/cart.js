function updateCartCount() {
    $.ajax({
        url: '/Cart/GetCartCount',
        type: 'GET',
        success: function (result) {
            if (result > 0) {
                // If we already have a cart-count element, update its text
                if ($('.cart-count').length > 0) {
                    $('.cart-count').text(result);
                } else {
                    // Otherwise, create a new one
                    $('.cart-icon').append('<div class="cart-count">' + result + '</div>');
                }
            } else {
                // Remove the count if cart is empty
                $('.cart-count').remove();
            }
        }
    });
}

$(document).ready(function () {
    updateCartCount();

    $(document).on('click', '.add-to-cart', function () {
        // Wait a moment for the cart to update in the backend
        setTimeout(function () {
            updateCartCount();
        }, 500);
    });
});
