// Login page: show/hide password toggle (the eye button inside the password field).
(function () {
    'use strict';

    document.querySelectorAll('[data-password-toggle]').forEach(function (button) {
        var input = document.getElementById(button.getAttribute('aria-controls'));
        if (!input) return;

        button.addEventListener('click', function () {
            var show = input.type === 'password';
            input.type = show ? 'text' : 'password';
            button.setAttribute('aria-pressed', String(show));
            button.setAttribute('aria-label', show
                ? (button.dataset.labelHide || 'Hide password')
                : (button.dataset.labelShow || 'Show password'));
        });
    });
})();
