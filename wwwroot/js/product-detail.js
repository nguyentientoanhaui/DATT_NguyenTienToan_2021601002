$(document).ready(function () {
    // Star rating interaction
    $('.star-rating-selector label').hover(
        function () {
            // Hover in
            $(this).addClass('hover');
            $(this).prevAll('label').addClass('hover');
        },
        function () {
            // Hover out
            $('.star-rating-selector label').removeClass('hover');
        }
    );

    // Update hidden input field with selected rating
    $('.star-rating-selector input').change(function () {
        $('#selectedRating').val($(this).val());
    });

    // Form validation
    $('.review-form').submit(function (e) {
        var isValid = true;

        // Check if name is provided
        if ($('#Name').val().trim() === '') {
            $('#Name').next('.text-danger').text('Vui lòng nhập tên của bạn');
            isValid = false;
        }

        // Check if email is provided and valid
        var email = $('#Email').val().trim();
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (email === '') {
            $('#Email').next('.text-danger').text('Vui lòng nhập email của bạn');
            isValid = false;
        } else if (!emailRegex.test(email)) {
            $('#Email').next('.text-danger').text('Email không hợp lệ');
            isValid = false;
        }

        // Check if comment is provided
        if ($('#Comment').val().trim() === '') {
            $('#Comment').next('.text-danger').text('Vui lòng nhập nhận xét của bạn');
            isValid = false;
        }

        if (!isValid) {
            e.preventDefault();
        }
    });
});