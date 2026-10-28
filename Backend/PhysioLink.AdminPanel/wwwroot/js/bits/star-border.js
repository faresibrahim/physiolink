// A light that travels along a control's edge — a port of React Bits' StarBorder
// (reactbits.dev/animations/star-border): the same two radial "stars" sweeping in opposite
// directions. The original wraps its own pill; here it decorates an existing .btn, so the stars
// are clipped to a ring over the button's border and the button keeps its fill and label. The
// motion itself is CSS (.star-border in physiolink.css), which drops the ring under reduced
// motion.
//
//   <button class="btn btn-primary" data-star-border>…</button>
(function () {
    'use strict';

    window.plMotion.component('[data-star-border]', function (el) {
        if (el.querySelector('.star-border')) return;

        var ring = document.createElement('span');
        ring.className = 'star-border';
        ring.setAttribute('aria-hidden', 'true');
        ring.innerHTML = '<i class="star-border-star star-border-bottom"></i>' +
                         '<i class="star-border-star star-border-top"></i>';
        el.insertBefore(ring, el.firstChild);
    });
})();
