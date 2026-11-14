/**
 * sweetalert.js
 * ---------------------------------------------------------------------------
 * Tiny dependency-free popup API for the SweetAlert-style popups in css/sweetalert.css.
 *
 *   Swal.fire({ icon: 'success', title: 'Good job!', text: 'You clicked the button!' })
 *   Swal.fire('Good job!', 'You clicked the button!', 'success')           // shorthand: title, text, icon
 *   Swal.fire({ icon: 'question', title: 'Delete it?', showCancelButton: true })
 *       .then(function (result) { if (result.isConfirmed) { ... } })
 *   Swal.success('Saved')   Swal.error('Oops')   Swal.warning('Careful')   Swal.info('FYI')   Swal.question('Sure?')
 *   Swal.close()
 *
 * Options: icon ('success' | 'error' | 'warning' | 'info' | 'question'), title, text, html,
 *          confirmButtonText ('OK'), showCancelButton (false), cancelButtonText ('Cancel'),
 *          timer (ms, auto-close), allowOutsideClick (true), allowEscapeKey (true)
 * The promise resolves to { isConfirmed, isDismissed, dismiss } where dismiss is
 * 'cancel' | 'backdrop' | 'esc' | 'timer' | 'close'.
 *
 * Note: `title` and `text` are inserted as plain text. `html` is inserted as HTML: never pass untrusted input to it.
 */
(function (root) {
  'use strict';

  var ICON_NAMES = ['success', 'error', 'warning', 'info', 'question'];
  var CHARS = { warning: '!', info: 'i', question: '?' };
  var current = null; // the popup that is currently open
  var uid = 0;

  function el(tag, className, text) {
    var node = document.createElement(tag);
    if (className) node.className = className;
    if (text != null) node.textContent = text;
    return node;
  }

  function iconNode(type) {
    var icon = el('div', 'swal-icon swal-icon-' + type + ' swal-icon-show');
    icon.setAttribute('aria-hidden', 'true');
    if (type === 'success') {
      icon.innerHTML =
        '<div class="swal-success-circular-line-left"></div>' +
        '<span class="swal-success-line-tip"></span>' +
        '<span class="swal-success-line-long"></span>' +
        '<div class="swal-success-ring"></div>' +
        '<div class="swal-success-fix"></div>' +
        '<div class="swal-success-circular-line-right"></div>';
    } else if (type === 'error') {
      icon.innerHTML = '<span class="swal-x-mark"><span class="swal-x-mark-line-left"></span><span class="swal-x-mark-line-right"></span></span>';
    } else {
      icon.appendChild(el('div', 'swal-icon-content', CHARS[type]));
    }
    return icon;
  }

  function close(reason) {
    var instance = current;
    if (!instance) return;
    current = null;
    instance.finish(reason || 'close');
  }

  function fire(options, text, icon) {
    // shorthand: fire(title, text, icon)
    if (typeof options === 'string') options = { title: options, text: text, icon: icon };
    options = options || {};
    if (current) close('close');

    var id = ++uid;
    var type = ICON_NAMES.indexOf(options.icon) >= 0 ? options.icon : null;
    var previouslyFocused = document.activeElement;

    var container = el('div', 'swal-container');
    var popup = el('div', 'swal-popup');
    popup.setAttribute('role', 'dialog');
    popup.setAttribute('aria-modal', 'true');
    popup.tabIndex = -1;

    if (type) popup.appendChild(iconNode(type));

    if (options.title) {
      var title = el('h2', 'swal-title', options.title);
      title.id = 'swal-title-' + id;
      popup.setAttribute('aria-labelledby', title.id);
      popup.appendChild(title);
    }
    if (options.html != null || options.text) {
      var body = el('div', 'swal-text');
      body.id = 'swal-text-' + id;
      if (options.html != null) body.innerHTML = options.html;
      else body.textContent = options.text;
      popup.setAttribute('aria-describedby', body.id);
      popup.appendChild(body);
    }

    var actions = el('div', 'swal-actions');
    var confirmBtn = el('button', 'swal-btn swal-btn-confirm', options.confirmButtonText || 'OK');
    confirmBtn.type = 'button';
    var cancelBtn = null;
    if (options.showCancelButton) {
      cancelBtn = el('button', 'swal-btn swal-btn-cancel', options.cancelButtonText || 'Cancel');
      cancelBtn.type = 'button';
      actions.appendChild(confirmBtn);
      actions.appendChild(cancelBtn);
    } else {
      actions.appendChild(confirmBtn);
    }
    popup.appendChild(actions);
    container.appendChild(popup);

    var resolveFn;
    var promise = new Promise(function (resolve) { resolveFn = resolve; });
    var timer = null;
    var done = false;

    function onKey(e) {
      if (e.key === 'Escape' && options.allowEscapeKey !== false) {
        e.preventDefault();
        close('esc');
      } else if (e.key === 'Tab') {
        // keep focus inside the popup
        var focusable = popup.querySelectorAll('button');
        var first = focusable[0], last = focusable[focusable.length - 1];
        if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus(); }
        else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus(); }
      }
    }

    function finish(reason) {
      if (done) return;
      done = true;
      clearTimeout(timer);
      document.removeEventListener('keydown', onKey, true);
      var confirmed = reason === 'confirm';
      var result = { isConfirmed: confirmed, isDismissed: !confirmed, dismiss: confirmed ? undefined : reason };

      var removed = false;
      function remove() {
        if (removed) return;
        removed = true;
        if (container.parentNode) container.parentNode.removeChild(container);
        if (!document.querySelector('.swal-container')) document.documentElement.classList.remove('swal-open');
        if (previouslyFocused && previouslyFocused.focus) previouslyFocused.focus();
        resolveFn(result);
      }
      container.classList.add('swal-closing');
      popup.addEventListener('animationend', remove);
      setTimeout(remove, 250); // fallback if animations are disabled
    }

    confirmBtn.addEventListener('click', function () { close('confirm'); });
    if (cancelBtn) cancelBtn.addEventListener('click', function () { close('cancel'); });
    container.addEventListener('mousedown', function (e) {
      if (e.target === container && options.allowOutsideClick !== false) close('backdrop');
    });
    document.addEventListener('keydown', onKey, true);

    document.body.appendChild(container);
    document.documentElement.classList.add('swal-open');
    (confirmBtn).focus();

    if (options.timer) timer = setTimeout(function () { close('timer'); }, options.timer);

    current = { finish: finish };
    return promise;
  }

  var Swal = { fire: fire, close: close };
  ICON_NAMES.forEach(function (name) {
    Swal[name] = function (title, text, opts) {
      var o = Object.assign({}, opts || {}, { icon: name, title: title, text: text });
      return fire(o);
    };
  });

  root.Swal = Swal;
})(window);
