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

    // --- Password Visibility Toggle ---
    $(document).on('click', '.toggle-password', function () {
        // Find the input field relative to the clicked button
        const input = $(this).closest('.input-group').find('input');
        const icon = $(this).find('i');

        if (input.attr('type') === 'password') {
            input.attr('type', 'text');
            icon.removeClass('fa-eye').addClass('fa-eye-slash');
        } else {
            input.attr('type', 'password');
            icon.removeClass('fa-eye-slash').addClass('fa-eye');
        }
    });

    // --- Modal Event Handler Setup ---
    const actionModal = document.getElementById('actionModal');
    if (actionModal) {
        actionModal.addEventListener('show.bs.modal', setupActionModal);
    }
});

// --- IMPROVED Modal Handler for Reviewer Actions ---
function setupActionModal(event) {
    console.log('setupActionModal called');

    const button = event.relatedTarget;
    if (!button) {
        console.error('No button found in event');
        return;
    }

    const actionType = button.getAttribute('data-action-type');
    const claimId = button.getAttribute('data-claim-id');
    const claimNumber = button.getAttribute('data-claim-number');

    console.log('Action:', actionType, 'Claim:', claimNumber);

    const modal = document.getElementById('actionModal');
    if (!modal) {
        console.error('Modal element not found');
        return;
    }

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

    // Verify all elements exist
    if (!form || !modalTitle || !actionName || !claimNumberSpan || !claimIdInput ||
        !submitButton || !commentsSection || !reasonSection || !commentsInput ||
        !reasonInput || !commentsLabel) {
        console.error('One or more modal elements not found');
        return;
    }

    // Reset form fields and visibility
    commentsInput.value = '';
    reasonInput.value = '';
    commentsInput.required = false;
    reasonInput.required = false;

    // Reset sections
    commentsSection.style.display = 'block';
    reasonSection.style.display = 'none';

    // Set common values
    claimIdInput.value = claimId;
    claimNumberSpan.textContent = claimNumber;

    // Clear existing button classes
    submitButton.className = 'btn';
    submitButton.disabled = false;

    // Configure modal based on action type
    switch (actionType) {
        case 'Verify':
            form.action = `/Claims/Verify`;
            modalTitle.textContent = 'Verify Claim';
            actionName.textContent = 'verify';
            submitButton.textContent = 'Verify Claim';
            submitButton.classList.add('btn-success');
            commentsLabel.textContent = "Comments (Optional)";
            commentsInput.required = false;
            console.log('Modal configured for Verify');
            break;

        case 'Return':
            form.action = `/Claims/Return`;
            modalTitle.textContent = 'Return Claim for Correction';
            actionName.textContent = 'return';
            submitButton.textContent = 'Return for Correction';
            submitButton.classList.add('btn-warning');
            commentsLabel.textContent = "Correction Instructions *";
            commentsInput.required = true;
            commentsInput.placeholder = "Explain what needs to be corrected...";
            console.log('Modal configured for Return');
            break;

        case 'Approve':
            form.action = `/Claims/Approve`;
            modalTitle.textContent = 'Approve Claim';
            actionName.textContent = 'approve';
            submitButton.textContent = 'Approve Claim';
            submitButton.classList.add('btn-success');
            commentsLabel.textContent = "Comments (Optional)";
            commentsInput.required = false;
            console.log('Modal configured for Approve');
            break;

        case 'Reject':
            form.action = `/Claims/Reject`;
            modalTitle.textContent = 'Reject Claim';
            actionName.textContent = 'reject';
            submitButton.textContent = 'Reject Claim';
            submitButton.classList.add('btn-danger');
            commentsSection.style.display = 'none';
            reasonSection.style.display = 'block';
            reasonInput.required = true;
            reasonInput.placeholder = "Provide a clear reason for rejection...";
            console.log('Modal configured for Reject');
            break;

        default:
            console.error('Unknown action type:', actionType);
            return;
    }

    // Add form validation
    form.onsubmit = function (e) {
        if (actionType === 'Return' && !commentsInput.value.trim()) {
            e.preventDefault();
            alert('Correction instructions are required when returning a claim.');
            commentsInput.focus();
            return false;
        }
        if (actionType === 'Reject' && !reasonInput.value.trim()) {
            e.preventDefault();
            alert('A reason is required when rejecting a claim.');
            reasonInput.focus();
            return false;
        }
        return true;
    };

    console.log('Modal setup complete');
}