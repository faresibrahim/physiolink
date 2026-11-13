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

    // Sign in: spinner on the button while the credentials are checked. The
    // browser's own required-field check runs first, so an empty form never
    // reaches here. Disabling waits a tick so the click still submits.
    var form = document.querySelector('.auth-form');
    if (form) {
        form.addEventListener('submit', function (event) {
            if (event.defaultPrevented) return;
            var button = form.querySelector('.auth-submit');
            if (!button) return;
            window.setTimeout(function () {
                button.classList.add('is-loading');
                button.setAttribute('aria-busy', 'true');
                var spinner = document.createElement('span');
                spinner.className = 'spinner spinner-sm';
                spinner.setAttribute('aria-hidden', 'true');
                button.insertBefore(spinner, button.firstChild);
                button.disabled = true;
            }, 0);
        });
    }

    // Back button restores a busy page from cache — reset the button.
    window.addEventListener('pageshow', function (event) {
        if (!event.persisted) return;
        var button = document.querySelector('.auth-submit.is-loading');
        if (!button) return;
        button.classList.remove('is-loading');
        button.removeAttribute('aria-busy');
        var spinner = button.querySelector('.spinner');
        if (spinner) button.removeChild(spinner);
        button.disabled = false;
    });

    // Close (×) on the sign-in error alert
    document.querySelectorAll('.alert-dismiss').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var alertEl = btn.closest('.alert');
            if (alertEl) alertEl.hidden = true;
        });
    });
})();
