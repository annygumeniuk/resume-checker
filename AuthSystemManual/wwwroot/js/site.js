document.addEventListener("DOMContentLoaded", function () {
        const checkboxes = document.querySelectorAll('input[type="checkbox"][data-toggle="skill-dropdown"]');
    checkboxes.forEach(function (checkbox) {
        checkbox.addEventListener('change', function () {
            const dropdown = this.closest('tr').querySelector('.skill-level');
            dropdown.disabled = !this.checked;
        });
        });
    });