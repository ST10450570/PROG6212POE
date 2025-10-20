// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

$(document).ready(function () {
    // --- Dynamic Total Calculation on Create Claim Form ---
    function calculateTotal() {
        const hours = parseFloat($('#hours').val()) || 0;
        const rate = parseFloat($('#rate').val()) || 0;
        const total = hours * rate;

        // Update the total display, formatted as South African Rand
        const formattedTotal = new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(total);
        $('#total-display').text(formattedTotal);
    }

    // Attach event listeners to input fields marked with 'data-calculate'
    $('[data-calculate="total"]').on('keyup input', calculateTotal);

    // Initial calculation on page load
    if ($('#hours').length > 0) {
        calculateTotal();
    }


    // --- File Upload Feedback ---
    $('#fileUpload').on('change', function () {
        const fileName = $(this).val().split('\\').pop();
        if (fileName) {
            $('#file-upload-feedback').text('File selected: ' + fileName);
        } else {
            $('#file-upload-feedback').text('');
        }
    });

});

// --- Generic Modal Handler for Reviewer Actions ---
function setupActionModal(event) {
    const button = event.relatedTarget;
    const actionType = button.getAttribute('data-action-type');
    const claimId = button.getAttribute('data-claim-id');
    const claimNumber = button.getAttribute('data-claim-number');

    const modal = document.getElementById('actionModal');
    const form = modal.querySelector('form');
    const modalTitle = modal.querySelector('.modal-title');
    const actionName = modal.querySelector('#actionName');
    const claimNumberSpan = modal.querySelector('#actionClaimNumber');
    const claimIdInput = modal.querySelector('#actionClaimId');
    const submitButton = modal.querySelector('#actionSubmitButton');

    const commentsSection = modal.querySelector('#comments-section');
    const reasonSection = modal.querySelector('#reason-section');
    const commentsInput = modal.querySelector('textarea[name="comments"]');
    const reasonInput = modal.querySelector('input[name="reason"]');
    const commentsLabel = modal.querySelector('#comments-label');


    // Reset form fields
    commentsInput.value = '';
    reasonInput.value = '';
    commentsInput.required = false;
    reasonInput.required = false;

    // Set common values
    claimIdInput.value = claimId;
    claimNumberSpan.textContent = claimNumber;

    // Clear existing button classes
    submitButton.className = 'btn';

    // Configure modal based on action type
    switch (actionType) {
        case 'Verify':
            form.action = `/Claims/Verify`;
            modalTitle.textContent = 'Verify Claim';
            actionName.textContent = 'verify';
            submitButton.textContent = 'Verify Claim';
            submitButton.classList.add('btn-success');
            commentsSection.style.display = 'block';
            reasonSection.style.display = 'none';
            commentsLabel.textContent = "Comments (Optional)";
            break;
        case 'Return':
            form.action = `/Claims/Return`;
            modalTitle.textContent = 'Return Claim for Correction';
            actionName.textContent = 'return';
            submitButton.textContent = 'Return for Correction';
            submitButton.classList.add('btn-warning');
            commentsSection.style.display = 'block';
            reasonSection.style.display = 'none';
            commentsLabel.textContent = "Correction Instructions *";
            commentsInput.required = true;
            break;
        case 'Approve':
            form.action = `/Claims/Approve`;
            modalTitle.textContent = 'Approve Claim';
            actionName.textContent = 'approve';
            submitButton.textContent = 'Approve Claim';
            submitButton.classList.add('btn-success');
            commentsSection.style.display = 'block';
            reasonSection.style.display = 'none';
            commentsLabel.textContent = "Comments (Optional)";
            break;
        case 'Reject':
            form.action = `/Claims/Reject`;
            modalTitle.textContent = 'Reject Claim';
            actionName.textContent = 'reject';
            submitButton.textContent = 'Reject Claim';
            submitButton.classList.add('btn-danger');
            commentsSection.style.display = 'none'; // Rejection uses the reason field
            reasonSection.style.display = 'block';
            reasonInput.required = true;
            break;
    }
}