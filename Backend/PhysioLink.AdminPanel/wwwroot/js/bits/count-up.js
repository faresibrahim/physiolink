// A number that counts to its value the first time it scrolls into view — a port of React Bits'
// CountUp (reactbits.dev/text-animations/count-up). The original's spring becomes a GSAP
// expo-out tween: quick at first, then a long settle, no overshoot. The server renders the final
// value, so reduced motion shows it as-is; tabular figures keep the width still while it counts.
//
//   <div class="stat-value" data-count-up>@Model.PatientCount</div>
(function () {
    'use strict';

    var gsap = window.gsap;
    var ScrollTrigger = window.ScrollTrigger;

    window.plMotion.component('[data-count-up]', function (el) {
        var text = el.textContent.trim();
        var plain = text.replace(/,/g, '');

        // Only a bare number counts; anything else ("—", "12 min") is left as written.
        if (!/^-?\d+(\.\d+)?$/.test(plain)) return;

        var to = parseFloat(plain);
        var decimals = (plain.split('.')[1] || '').length;
        var format = new Intl.NumberFormat('en-US', {
            useGrouping: text.indexOf(',') !== -1,
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals
        }).format;

        window.plMotion.whenMotion(function () {
            var counter = { value: 0 };
            var write = function () { el.textContent = format(counter.value); };
            write();

            var tween = gsap.to(counter, {
                value: to,
                duration: 1.6,
                ease: 'expo.out',
                paused: true,
                onUpdate: write
            });
            ScrollTrigger.create({
                trigger: el,
                start: 'top bottom-=16',
                once: true,
                onEnter: function () { tween.play(); }
            });

            return function () { el.textContent = text; };
        });
    });
})();
