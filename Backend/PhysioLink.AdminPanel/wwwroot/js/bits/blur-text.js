// Text that resolves out of a blur, word by word — a port of React Bits' BlurText
// (reactbits.dev/text-animations/blur-text), driven by GSAP. Same choreography: each word steps
// from blurred and offset, through a half-resolved midpoint, to rest, once, when it scrolls into
// view. The offset is smaller than the original's 50px so it settles rather than drops. The
// server renders the text at rest, so reduced motion simply shows it.
//
//   <h1 data-blur-text>Good morning</h1>
//   data-blur-delay  seconds between one word starting and the next (0.1)
//   data-blur-start  seconds to wait before the first word (0)
(function () {
    'use strict';

    var gsap = window.gsap;
    var ScrollTrigger = window.ScrollTrigger;

    // Seconds per step; each word takes two.
    var STEP = 0.35;

    // Wraps each word in its own span. Only plain text is split, so markup inside a heading is
    // never rewritten.
    function split(el) {
        if (el.children.length) return [];
        var words = el.textContent.trim().split(/\s+/);
        el.textContent = '';
        return words.map(function (word, i) {
            var seg = document.createElement('span');
            seg.className = 'blur-seg';
            seg.textContent = word;
            el.appendChild(seg);
            if (i < words.length - 1) el.appendChild(document.createTextNode(' '));
            return seg;
        });
    }

    window.plMotion.component('[data-blur-text]', function (el) {
        var delay = parseFloat(el.getAttribute('data-blur-delay'));
        var startDelay = parseFloat(el.getAttribute('data-blur-start'));
        var segs = null;

        window.plMotion.whenMotion(function () {
            segs = segs || split(el);
            if (!segs.length) return;

            gsap.set(segs, { filter: 'blur(10px)', opacity: 0, y: -8, willChange: 'transform, filter, opacity' });
            var tl = gsap.timeline({
                paused: true,
                delay: isFinite(startDelay) ? startDelay : 0,
                onComplete: function () { gsap.set(segs, { clearProps: 'all' }); }
            });
            segs.forEach(function (seg, i) {
                var at = i * (isFinite(delay) ? delay : 0.1);
                tl.to(seg, { filter: 'blur(5px)', opacity: 0.5, y: 2, duration: STEP, ease: 'power1.out' }, at)
                  .to(seg, { filter: 'blur(0px)', opacity: 1, y: 0, duration: STEP, ease: 'power2.out' }, at + STEP);
            });

            ScrollTrigger.create({
                trigger: el,
                start: 'top bottom-=16',
                once: true,
                onEnter: function () { tl.play(); }
            });
        });
    });
})();
