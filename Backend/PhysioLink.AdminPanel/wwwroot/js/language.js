// First-visit language screen: marks the chosen card, plays a short exit, then
// submits. Browsers with cross-document view transitions (Chrome/Edge, Safari
// 18.2+) let the transition carry the logo into the login card; the rest get a
// fade-out here and an entrance animation on the login page.
(function () {
    'use strict';

    var form = document.getElementById('langForm');
    var stage = document.getElementById('langStage');
    if (!form || !stage) return;

    var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var hasViewTransitions = 'onpagereveal' in window;
    var submitting = false;

    form.addEventListener('submit', function (event) {
        if (submitting) return;
        var choice = event.submitter;
        if (!choice || !choice.classList.contains('lang-option')) return;

        event.preventDefault();
        submitting = true;

        choice.classList.add('is-chosen');
        form.classList.add('has-choice');
        form.querySelectorAll('.lang-option').forEach(function (btn) {
            btn.setAttribute('aria-disabled', 'true');
        });

        var delay = reduceMotion ? 0 : (hasViewTransitions ? 260 : 220);
        window.setTimeout(function () {
            if (!hasViewTransitions && !reduceMotion) {
                stage.classList.add('is-leaving');
                window.setTimeout(send, 380);
            } else {
                send();
            }
        }, delay);

        function send() {
            if (typeof form.requestSubmit === 'function') {
                form.requestSubmit(choice);
            } else {
                var hidden = document.createElement('input');
                hidden.type = 'hidden';
                hidden.name = choice.name;
                hidden.value = choice.value;
                form.appendChild(hidden);
                form.submit();
            }
        }
    });

    // Coming back with the browser's Back button restores a faded page from the
    // back/forward cache — reset it so the choices are usable again.
    window.addEventListener('pageshow', function (event) {
        if (!event.persisted) return;
        submitting = false;
        stage.classList.remove('is-leaving');
        form.classList.remove('has-choice');
        form.querySelectorAll('.lang-option').forEach(function (btn) {
            btn.classList.remove('is-chosen');
            btn.removeAttribute('aria-disabled');
        });
    });
})();
