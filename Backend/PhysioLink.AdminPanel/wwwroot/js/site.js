// Shared admin-panel behaviour: dialog focus handling, inline form errors and
// toast dismissal. Every page-level script builds on these rather than
// re-implementing them per modal.
(function () {
    'use strict';

    var FOCUSABLE = [
        'a[href]', 'button:not([disabled])', 'input:not([disabled])',
        'select:not([disabled])', 'textarea:not([disabled])', '[tabindex]:not([tabindex="-1"])'
    ].join(',');

    function visibleFocusable(root) {
        return Array.prototype.filter.call(
            root.querySelectorAll(FOCUSABLE),
            function (el) { return el.offsetParent !== null || el === document.activeElement; }
        );
    }

    // The element that opened the dialog, so focus can go back where it started.
    var returnFocusTo = null;

    // Keeps Tab inside the open dialog — without it, tabbing walks the page
    // behind the scrim and the keyboard user loses the dialog entirely.
    function trapTab(e) {
        if (e.key !== 'Tab') return;
        var overlay = document.querySelector('.modal-overlay.modal-visible');
        if (!overlay) return;

        var items = visibleFocusable(overlay);
        if (!items.length) return;

        var first = items[0];
        var last = items[items.length - 1];

        if (e.shiftKey && document.activeElement === first) {
            e.preventDefault();
            last.focus();
        } else if (!e.shiftKey && document.activeElement === last) {
            e.preventDefault();
            first.focus();
        }
    }

    document.addEventListener('keydown', trapTab);

    // Called by every open*Modal() helper once the overlay is visible.
    function onModalOpen(overlay) {
        if (!overlay) return;
        returnFocusTo = document.activeElement;
        document.body.style.overflow = 'hidden';

        var items = visibleFocusable(overlay);
        var target = overlay.querySelector('input:not([type=hidden]):not([disabled]), textarea, select') || items[0];
        if (target) {
            // Let the browser paint the dialog before moving the caret into it.
            window.requestAnimationFrame(function () { target.focus(); });
        }
    }

    function onModalClose(overlay) {
        document.body.style.overflow = '';
        clearErrors(overlay);
        if (returnFocusTo && document.contains(returnFocusTo)) {
            returnFocusTo.focus();
        }
        returnFocusTo = null;
    }

    var WARNING_ICON =
        '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" ' +
        'stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">' +
        '<circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/>' +
        '<line x1="12" y1="16" x2="12.01" y2="16"/></svg>';

    // Marks one field invalid and writes the reason under it, replacing the
    // browser alert() dialogs these forms used to throw.
    function setFieldError(field, message) {
        if (!field) return;
        field.setAttribute('aria-invalid', 'true');

        var group = field.closest('.form-group, .field');
        if (!group) return;

        var slot = group.querySelector('.field-error');
        if (!slot) {
            slot = document.createElement('span');
            slot.className = 'field-error';
            group.appendChild(slot);
        }
        slot.innerHTML = WARNING_ICON + '<span></span>';
        slot.lastChild.textContent = message;
        slot.hidden = false;
    }

    function clearErrors(scope) {
        if (!scope) return;
        Array.prototype.forEach.call(scope.querySelectorAll('[aria-invalid="true"]'), function (el) {
            el.removeAttribute('aria-invalid');
        });
        Array.prototype.forEach.call(scope.querySelectorAll('.field-error'), function (el) {
            el.hidden = true;
        });
        var banner = scope.querySelector('.form-error-banner');
        if (banner) banner.hidden = true;
    }

    // Shows a form-level message (a failed fetch, a rejected save) in the
    // dialog itself instead of an OS alert box.
    function showFormError(scope, message) {
        if (!scope) return;
        var banner = scope.querySelector('.form-error-banner');
        if (!banner) return;
        banner.textContent = message;
        banner.hidden = false;
    }

    // Validates a set of [field, label] pairs; returns true when all are filled.
    // Focuses the first offender so the keyboard lands where the fix is.
    function requireFields(scope, pairs) {
        clearErrors(scope);
        var firstBad = null;

        pairs.forEach(function (pair) {
            var field = typeof pair[0] === 'string' ? document.getElementById(pair[0]) : pair[0];
            if (!field) return;
            if (!field.value.trim()) {
                setFieldError(field, pair[1] + ' is required.');
                if (!firstBad) firstBad = field;
            }
        });

        if (firstBad) firstBad.focus();
        return !firstBad;
    }

    // Page toasts: success fades on its own, errors wait to be dismissed.
    function initToasts() {
        var success = document.getElementById('pl-toast-success');
        if (success) {
            window.setTimeout(function () {
                success.style.opacity = '0';
                window.setTimeout(function () { success.hidden = true; }, 400);
            }, 4000);
        }

        Array.prototype.forEach.call(document.querySelectorAll('.alert-dismiss'), function (btn) {
            btn.addEventListener('click', function () {
                var alertEl = btn.closest('.alert');
                if (alertEl) alertEl.hidden = true;
            });
        });

        // A toast restored from the back/forward cache is stale by definition.
        window.addEventListener('pageshow', function (e) {
            if (!e.persisted) return;
            Array.prototype.forEach.call(document.querySelectorAll('.alert'), function (el) {
                el.hidden = true;
            });
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initToasts);
    } else {
        initToasts();
    }

    window.plModal = {
        opened: onModalOpen,
        closed: onModalClose
    };
    window.plForm = {
        setFieldError: setFieldError,
        clearErrors: clearErrors,
        showFormError: showFormError,
        requireFields: requireFields
    };
})();
