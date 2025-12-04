// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    // Live Search with Debounce
    let debounceTimer;

    $('#searchString, #categoryId').on('input change', function () {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(function () {
            performSearch();
        }, 300); // 300ms delay
    });

    $('#resetBtn').click(function () {
        $('#searchString').val('');
        $('#categoryId').val('');
        $('#showPast').val('false');
        $('#togglePastBtn').text('Show Past Events');

        // Clear URL query parameters
        var newUrl = window.location.protocol + "//" + window.location.host + window.location.pathname;
        window.history.pushState({ path: newUrl }, '', newUrl);

        performSearch();
    });

    // Toggle Past Events
    $('#togglePastBtn').click(function () {
        var isPast = $('#showPast').val() === 'true';
        var newState = !isPast;
        $('#showPast').val(newState);
        $(this).text(newState ? "Show Upcoming Events" : "Show Past Events");
        performSearch();
    });

    function performSearch(pageNumber = 1) {
        var searchString = $('#searchString').val();
        var categoryId = $('#categoryId').val();
        var showPast = $('#showPast').val();

        $.ajax({
            url: '/Event/SearchEvents',
            type: 'POST',
            data: { searchString: searchString, categoryId: categoryId, pageNumber: pageNumber, showPast: showPast },
            success: function (result) {
                $('#eventsContainer').html(result);
            },
            error: function (xhr, status, error) {
                console.error("Search failed: " + error);
            }
        });
    }

    // AJAX Pagination
    $(document).on('click', '.pagination-link', function (e) {
        e.preventDefault();
        var page = $(this).data('page');
        if (page) {
            performSearch(page);
        }
    });

    // Quantity Increment/Decrement (Event Details)
    $('#btn-minus').click(function () {
        var input = $('#ticketQuantity');
        var val = parseInt(input.val()) || 1;
        if (val > 1) {
            input.val(val - 1).trigger('change');
        }
    });

    $('#btn-plus').click(function () {
        var input = $('#ticketQuantity');
        var val = parseInt(input.val()) || 1;
        var max = parseInt(input.attr('max'));
        if (val < max) {
            input.val(val + 1).trigger('change');
        }
    });

    // Quick Select Badges
    $('.quick-qty').click(function () {
        var qty = $(this).data('qty');
        $('#ticketQuantity').val(qty).trigger('change');
    });

    // Dynamic Price & Stock Warning
    $('#ticketQuantity').on('input change', function () {
        var quantity = parseInt($(this).val()) || 1;
        var price = parseFloat($(this).data('price'));
        var stock = parseInt($(this).data('stock'));

        // Update Total Price
        var total = quantity * price;
        $('#totalPrice').text('$' + total.toFixed(2));

        // Low Stock Warning
        if (stock < 10) {
            $('#stockWarning').text('Only ' + stock + ' tickets left!').show();
        } else if (stock - quantity < 5) {
            $('#stockWarning').text('Hurry! Low stock remaining.').show();
        } else {
            $('#stockWarning').hide();
        }
    });

    // Add to Cart AJAX
    $(document).on('click', '.btn-add-to-cart', function () {
        var form = $('#addToCartForm');
        var eventId = form.find('input[name="eventId"]').val();
        var quantity = form.find('input[name="quantity"]').val();

        $.ajax({
            url: '/Cart/AddToCart',
            type: 'POST',
            data: { eventId: eventId, quantity: quantity },
            success: function (response) {
                if (response.requiresLogin) {
                    var returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
                    window.location.href = '/Account/Login?ReturnUrl=' + returnUrl;
                    return;
                }

                if (response.success) {
                    // Show Modal
                    var purchaseModal = new bootstrap.Modal(document.getElementById('purchaseModal'));
                    purchaseModal.show();

                    // Update Cart Badge
                    var badge = $('#cart-badge');
                    if (badge.length) {
                        badge.text(response.cartCount);
                    } else {
                        // If badge doesn't exist yet (count was 0), you might need to add it or reload
                        // For now, assuming it exists or we just update the text
                        $('.nav-link.text-dark[href="/Cart"]').append(' <span id="cart-badge" class="badge bg-primary rounded-pill">' + response.cartCount + '</span>');
                    }
                }
            },
            error: function (xhr) {
                alert(xhr.responseText);
            }
        });
    });

    // Cart Page Quantity Update
    $(document).on('click', '.btn-update-cart', function () {
        var btn = $(this);
        var eventId = btn.data('id');
        var change = parseInt(btn.data('change'));
        var input = btn.siblings('input');
        var currentQty = parseInt(input.val());
        var newQty = currentQty + change;

        if (newQty <= 0) {
            if (confirm("Remove this item from cart?")) {
                updateCartQuantity(eventId, 0);
            }
        } else {
            updateCartQuantity(eventId, newQty, input);
        }
    });

    function updateCartQuantity(eventId, quantity, inputElement) {
        $.ajax({
            url: '/Cart/UpdateQuantity',
            type: 'POST',
            data: { eventId: eventId, quantity: quantity },
            success: function (response) {
                if (response.success) {
                    if (quantity === 0) {
                        location.reload();
                    } else {
                        if (inputElement) inputElement.val(quantity);
                        $('#total-' + eventId).text('$' + response.itemTotal.toFixed(2));
                        $('#cart-grand-total').text('$' + response.cartTotal.toFixed(2));
                        var badge = $('#cart-badge');
                        if (badge.length) badge.text(response.cartCount);
                    }
                } else {
                    alert(response.message);
                }
            },
            error: function () {
                alert("Error updating cart.");
            }
        });
    }
});
