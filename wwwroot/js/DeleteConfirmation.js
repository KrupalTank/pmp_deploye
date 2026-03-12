// This script handles global delete confirmations
document.addEventListener("DOMContentLoaded", function () {
    // Select all elements with the 'btn-delete-confirm' class
    const deleteButtons = document.querySelectorAll('.btn-delete-confirm');

    deleteButtons.forEach(button => {
        button.addEventListener('click', function (event) {
            // Irreversibility Warning Message
            const message = "Are you sure you want to delete this record?\n\nThis process is irreversible and the data cannot be recovered.";

            // If the user clicks 'Cancel', prevent the redirect
            if (!confirm(message)) {
                event.preventDefault();
            }
        });
    });
});