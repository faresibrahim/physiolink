// Page motion for the admin panel. Lenis smooths the scroll and is stepped by GSAP's ticker, so
// the scroll position, ScrollTrigger and every tween advance on the same frame. Pages render at
// rest — navigating between them is too frequent to animate — and the components in js/bits/
// register here. Everything is built inside gsap.matchMedia(): switching to reduced motion
// reverts it — native scroll, resting states — and switching back rebuilds it.
(function () {
    'use strict';

    var MOTION_OK = '(prefers-reduced-motion: no-preference)';
    var REDUCED = '(prefers-reduced-motion: reduce)';

    var gsap = window.gsap;
    var ScrollTrigger = window.ScrollTrigger;
    var components = [];
    var started = false;

    // Without GSAP nothing can move; show the page at rest.
    if (!gsap || !ScrollTrigger) {
        window.plMotion = {
            component: function () {},
            whenMotion: function (setup, reduced) { if (reduced) reduced(); return function () {}; }
        };
        return;
    }

    gsap.registerPlugin(ScrollTrigger);

    // Runs `setup` only while motion is allowed. Tweens and ScrollTriggers it creates are
    // reverted when the admin switches to reduced motion, and `reduced` runs instead.
    function whenMotion(setup, reduced) {
        var mm = gsap.matchMedia();
        mm.add(MOTION_OK, function () { return setup(); });
        if (reduced) mm.add(REDUCED, function () { reduced(); });
        return function () { mm.revert(); };
    }

    function smoothScroll() {
        if (!window.Lenis) return;

        whenMotion(function () {
            var lenis = new window.Lenis({
                lerp: 0.1,
                // Wide tables, the exercise picker and Tom Select keep their own scroll.
                allowNestedScroll: true,
                // An open dialog owns the wheel; the page behind it stays where it was.
                prevent: function (node) { return node.classList.contains('modal-overlay'); }
            });
            lenis.on('scroll', ScrollTrigger.update);

            function raf(time) { lenis.raf(time * 1000); }
            gsap.ticker.add(raf);
            gsap.ticker.lagSmoothing(0);

            return function () {
                gsap.ticker.remove(raf);
                gsap.ticker.lagSmoothing(500, 33);
                lenis.destroy();
            };
        });
    }

    function mount(c) {
        Array.prototype.forEach.call(document.querySelectorAll(c.selector), c.init);
    }

    // Components in js/bits/ register a selector and an init; each init runs once per matching
    // element, after every deferred script has loaded. One registered later mounts at once.
    function component(selector, init) {
        var c = { selector: selector, init: init };
        components.push(c);
        if (started) mount(c);
    }

    function start() {
        started = true;
        smoothScroll();
        components.forEach(mount);

        // DM Sans swaps in after first layout; re-measure trigger positions once it has.
        if (document.fonts) document.fonts.ready.then(function () { ScrollTrigger.refresh(); });
    }

    window.plMotion = {
        component: component,
        whenMotion: whenMotion
    };

    // Deferred scripts run before DOMContentLoaded, so the components after this file have
    // all registered by the time it fires.
    if (document.readyState === 'complete') {
        start();
    } else {
        document.addEventListener('DOMContentLoaded', start);
    }
})();
