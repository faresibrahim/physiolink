// Off-canvas sidebar below 1200px (see .layout-menu in physiolink.css).
// Every [data-menu-toggle] element — the hamburger in .mobile-bar, the chevron
// in the sidebar header and the dark overlay — opens/closes it by toggling
// .layout-menu-expanded on <html>. Escape closes it; growing past the
// breakpoint resets it so the desktop column never carries the open state.
(function () {
    'use strict';

    var root = document.documentElement;
    var menu = document.getElementById('layout-menu');
    if (!menu) return;

    var openButton = document.querySelector('[data-menu-open]');
    var OPEN_CLASS = 'layout-menu-expanded';

    function isOpen() {
        return root.classList.contains(OPEN_CLASS);
    }

    function setOpen(open) {
        if (open === isOpen()) return;
        root.classList.toggle(OPEN_CLASS, open);
        if (openButton) openButton.setAttribute('aria-expanded', String(open));

        if (open) {
            // Land on the current page's link, else the first one
            var target = menu.querySelector('.menu-item.active > .menu-link') || menu.querySelector('.menu-link');
            if (target) target.focus({ preventScroll: true });
        } else if (openButton && menu.contains(document.activeElement)) {
            openButton.focus();
        }
    }

    document.querySelectorAll('[data-menu-toggle]').forEach(function (el) {
        el.addEventListener('click', function () {
            setOpen(!isOpen());
        });
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape' && isOpen()) setOpen(false);
    });

    var desktop = window.matchMedia('(min-width: 1200px)');
    var onBreakpoint = function (event) {
        if (event.matches) setOpen(false);
    };
    if (desktop.addEventListener) desktop.addEventListener('change', onBreakpoint);
    else desktop.addListener(onBreakpoint);
})();
