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

    // "{0} is required." in the page's language (window.plI18n, set by _Layout).
    function requiredMessage(label) {
        var template = (window.plI18n && window.plI18n.required) || '{0} is required.';
        return template.replace('{0}', label);
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
                setFieldError(field, requiredMessage(pair[1]));
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
                window.setTimeout(function () { success.hidden = true; }, 200);
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

    // Table row "⋮" menus ([data-dt-menu]). The list is position: fixed and
    // placed under its button so the table's scroll container can't clip it.
    // One open at a time; outside click, Escape, Tab, scroll or resize close it.
    function initRowMenus() {
        var open = null;

        function close(returnFocus) {
            if (!open) return;
            open.list.hidden = true;
            open.toggle.setAttribute('aria-expanded', 'false');
            if (returnFocus) open.toggle.focus();
            open = null;
        }

        function place(toggle, list) {
            var r = toggle.getBoundingClientRect();
            var rtl = document.documentElement.dir === 'rtl';
            var width = list.offsetWidth;
            var height = list.offsetHeight;
            var left = rtl ? r.left : r.right - width;
            left = Math.max(8, Math.min(left, window.innerWidth - width - 8));
            var top = r.bottom + 4;
            if (top + height > window.innerHeight - 8) top = Math.max(8, r.top - height - 4);
            list.style.left = left + 'px';
            list.style.top = top + 'px';
        }

        // Capture phase: runs before an item's own onclick, so the menu closes and
        // focus is back on its button before a dialog opens (and later returns there).
        document.addEventListener('click', function (e) {
            var toggle = e.target.closest('[data-dt-menu-toggle]');
            if (toggle) {
                var list = toggle.closest('[data-dt-menu]').querySelector('.dt-menu-list');
                var wasOpen = open && open.toggle === toggle;
                close(false);
                if (wasOpen) return;
                list.hidden = false;
                toggle.setAttribute('aria-expanded', 'true');
                open = { toggle: toggle, list: list };
                place(toggle, list);
                // Keyboard activation (detail 0) moves focus into the menu
                if (e.detail === 0) {
                    var first = list.querySelector('[role="menuitem"]');
                    if (first) first.focus();
                }
                return;
            }
            if (!open) return;
            if (e.target.closest('[role="menuitem"]') && open.list.contains(e.target)) close(true);
            else if (!open.list.contains(e.target)) close(false);
        }, true);

        document.addEventListener('keydown', function (e) {
            if (!open) return;
            if (e.key === 'Escape') {
                close(true);
            } else if (e.key === 'Tab') {
                close(false);
            } else if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
                e.preventDefault();
                var items = Array.prototype.slice.call(open.list.querySelectorAll('[role="menuitem"]'));
                var i = items.indexOf(document.activeElement);
                i = e.key === 'ArrowDown' ? (i + 1) % items.length : (i - 1 + items.length) % items.length;
                items[i].focus();
            }
        });

        window.addEventListener('resize', function () { close(false); });
        window.addEventListener('scroll', function () { close(false); }, true);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initToasts);
    } else {
        initToasts();
    }
    initRowMenus();

    // ---- Loading state for buttons -------------------------------------------
    // start(el) disables a button, hides its icon and prepends a small spinner;
    // the returned function undoes it. A trigger that sits inside a "⋮" row menu
    // (already closed by the time it runs) passes the busy state to the menu's
    // own button, which is where focus went back to.
    function busyTarget(el) {
        if (!el) return null;
        var menu = el.closest && el.closest('[data-dt-menu]');
        if (menu && el.getAttribute('role') === 'menuitem') {
            return menu.querySelector('[data-dt-menu-toggle]') || el;
        }
        return el;
    }

    function startLoading(el) {
        el = busyTarget(el);
        if (!el || el.classList.contains('is-loading')) return function () { };
        el.classList.add('is-loading');
        el.setAttribute('aria-busy', 'true');
        var spinner = document.createElement('span');
        spinner.className = 'spinner spinner-sm';
        spinner.setAttribute('aria-hidden', 'true');
        el.insertBefore(spinner, el.firstChild);
        var wasDisabled = el.disabled;
        if ('disabled' in el) el.disabled = true;
        return function stop() {
            el.classList.remove('is-loading');
            el.removeAttribute('aria-busy');
            if (spinner.parentNode) spinner.parentNode.removeChild(spinner);
            if ('disabled' in el) el.disabled = wasDisabled;
        };
    }

    // Every POST form shows the spinner on the button that submitted it, once
    // its own validation and confirm() have let the submit through. Disabling
    // waits a tick so the button is still part of the posted form data, and it
    // stops a second click from sending the form twice. GET forms (filters,
    // search) are left alone — the browser already shows the page loading.
    document.addEventListener('submit', function (e) {
        if (e.defaultPrevented) return;
        var form = e.target;
        if ((form.getAttribute('method') || 'get').toLowerCase() !== 'post') return;
        if (form.hasAttribute('data-no-loading')) return;
        var button = e.submitter || form.querySelector('[type="submit"]');
        if (!button) return;
        window.setTimeout(function () { startLoading(button); }, 0);
    });

    // Coming back with the browser's Back button restores the page from cache
    // with its buttons still busy — reset them.
    window.addEventListener('pageshow', function (e) {
        if (!e.persisted) return;
        Array.prototype.forEach.call(document.querySelectorAll('.is-loading'), function (el) {
            el.classList.remove('is-loading');
            el.removeAttribute('aria-busy');
            var spinner = el.querySelector(':scope > .spinner');
            if (spinner) el.removeChild(spinner);
            if ('disabled' in el) el.disabled = false;
        });
    });

    window.plModal = {
        opened: onModalOpen,
        closed: onModalClose
    };
    window.plLoading = { start: startLoading };
    window.plForm = {
        setFieldError: setFieldError,
        clearErrors: clearErrors,
        showFormError: showFormError,
        requireFields: requireFields
    };
})();
