/**
 * OMS UI Library — Modal, Toast, Theme Toggle
 * Pure vanilla JS — no dependencies.
 */
(function (global) {
    'use strict';

    /* ══════════════════════════════════════════════════════
       THEME TOGGLE
       ══════════════════════════════════════════════════════ */
    const THEME_KEY = 'oms-theme';

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem(THEME_KEY, theme);

        // Update all toggle button labels/icons if present
        document.querySelectorAll('[data-theme-label]').forEach(function (el) {
            el.textContent = theme === 'light' ? '🌙 Chế độ tối' : '☀️ Chế độ sáng';
        });
    }

    function toggleTheme() {
        const current = document.documentElement.getAttribute('data-theme') || 'dark';
        applyTheme(current === 'dark' ? 'light' : 'dark');
    }

    function initTheme() {
        const saved = localStorage.getItem(THEME_KEY) || 'dark';
        applyTheme(saved);
    }

    /* ══════════════════════════════════════════════════════
       MODAL SYSTEM
       ══════════════════════════════════════════════════════ */

    /**
     * OmsModal — create a modal dialog programmatically.
     *
     * Usage:
     *   OmsModal.show({
     *     title: 'Xác nhận xóa',
     *     icon: 'danger',              // 'default' | 'danger' | 'warning' | 'success'
     *     body: 'Bạn có chắc không?', // HTML or text string
     *     confirmText: 'Xóa',
     *     cancelText: 'Hủy',
     *     onConfirm: function() { ... },
     *     size: ''                     // '' | 'sm' | 'lg' | 'xl'
     *   });
     *
     *   OmsModal.close();
     */
    var OmsModal = (function () {
        var backdrop = null;
        var activeOnConfirm = null;

        var ICONS = {
            default: '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" style="width:20px;height:20px"><path stroke-linecap="round" stroke-linejoin="round" d="M11.25 11.25l.041-.02a.75.75 0 011.063.852l-.708 2.836a.75.75 0 001.063.853l.041-.021M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-9-3.75h.008v.008H12V8.25z" /></svg>',
            danger:  '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" style="width:20px;height:20px"><path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.008v.008H12v-.008z" /></svg>',
            warning: '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" style="width:20px;height:20px"><path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m9-.75a9 9 0 11-18 0 9 9 0 0118 0zm-9 3.75h.008v.008H12v-.008z" /></svg>',
            success: '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" style="width:20px;height:20px"><path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>',
        };

        function createBackdrop() {
            var el = document.createElement('div');
            el.className = 'oms-modal-backdrop';
            el.setAttribute('role', 'dialog');
            el.setAttribute('aria-modal', 'true');
            // Close on backdrop click
            el.addEventListener('click', function (e) {
                if (e.target === el) close();
            });
            document.body.appendChild(el);
            return el;
        }

        function show(opts) {
            opts = opts || {};
            var title       = opts.title       || 'Thông báo';
            var body        = opts.body        || '';
            var icon        = opts.icon        || 'default';
            var confirmText = opts.confirmText || 'Xác nhận';
            var cancelText  = opts.cancelText  || 'Hủy';
            var size        = opts.size        || '';
            var hideCancelButton = opts.hideCancel || false;
            activeOnConfirm = opts.onConfirm   || null;

            // Build or reuse backdrop
            if (!backdrop) backdrop = createBackdrop();

            var sizeClass = size ? ' modal-' + size : '';

            backdrop.innerHTML =
                '<div class="oms-modal' + sizeClass + '" role="document">' +
                    '<div class="oms-modal-header">' +
                        '<div class="oms-modal-header-icon ' + icon + '">' + (ICONS[icon] || ICONS.default) + '</div>' +
                        '<h2 class="oms-modal-title">' + escHtml(title) + '</h2>' +
                        '<button class="oms-modal-close" aria-label="Đóng" id="oms-modal-close-btn">' +
                            '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" style="width:16px;height:16px"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" /></svg>' +
                        '</button>' +
                    '</div>' +
                    '<div class="oms-modal-body">' + body + '</div>' +
                    '<div class="oms-modal-footer">' +
                        (hideCancelButton ? '' : '<button class="btn-secondary-premium" style="padding:10px 20px;font-size:0.9rem;" id="oms-modal-cancel-btn">' + escHtml(cancelText) + '</button>') +
                        '<button class="btn-' + (icon === 'danger' ? 'danger' : 'primary') + '-premium" style="padding:10px 20px;font-size:0.9rem;" id="oms-modal-confirm-btn">' + escHtml(confirmText) + '</button>' +
                    '</div>' +
                '</div>';

            // Wire up buttons
            var closeBtn   = backdrop.querySelector('#oms-modal-close-btn');
            var cancelBtn  = backdrop.querySelector('#oms-modal-cancel-btn');
            var confirmBtn = backdrop.querySelector('#oms-modal-confirm-btn');

            if (closeBtn)   closeBtn.addEventListener('click', close);
            if (cancelBtn)  cancelBtn.addEventListener('click', close);
            if (confirmBtn) confirmBtn.addEventListener('click', function () {
                if (typeof activeOnConfirm === 'function') activeOnConfirm();
                close();
            });

            // Animate open
            requestAnimationFrame(function () {
                backdrop.classList.add('is-open');
                document.body.style.overflow = 'hidden';
                // Trap focus on confirm button
                if (confirmBtn) setTimeout(function () { confirmBtn.focus(); }, 120);
            });

            // Keyboard: Escape to close
            document._omsModalKeyHandler = function (e) {
                if (e.key === 'Escape') close();
            };
            document.addEventListener('keydown', document._omsModalKeyHandler);
        }

        function close() {
            if (!backdrop) return;
            backdrop.classList.remove('is-open');
            document.body.style.overflow = '';
            document.removeEventListener('keydown', document._omsModalKeyHandler);
            // Remove after animation
            setTimeout(function () {
                if (backdrop && backdrop.parentNode) {
                    backdrop.parentNode.removeChild(backdrop);
                    backdrop = null;
                }
            }, 250);
        }

        function escHtml(str) {
            return String(str)
                .replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;')
                .replace(/"/g, '&quot;');
        }

        return { show: show, close: close };
    })();

    /* ══════════════════════════════════════════════════════
       CONFIRM MODAL (convenience wrapper for delete / destructive)
       ══════════════════════════════════════════════════════ */
    function confirmDelete(message, onConfirm) {
        OmsModal.show({
            title: 'Xác nhận xóa',
            icon: 'danger',
            body: '<p style="margin:0;">' + (message || 'Bạn có chắc chắn muốn xóa không? Hành động này không thể hoàn tác.') + '</p>',
            confirmText: '🗑️ Xóa',
            cancelText: 'Hủy',
            onConfirm: onConfirm,
        });
    }

    /* ══════════════════════════════════════════════════════
       TOAST NOTIFICATION SYSTEM
       ══════════════════════════════════════════════════════ */
    var OmsToast = (function () {
        var container = null;
        var DURATION = 4000;

        var TOAST_ICONS = {
            success: '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="#10b981" style="width:20px;height:20px"><path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>',
            error:   '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="#ef4444" style="width:20px;height:20px"><path stroke-linecap="round" stroke-linejoin="round" d="M9.75 9.75l4.5 4.5m0-4.5l-4.5 4.5M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>',
            info:    '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="#4facfe" style="width:20px;height:20px"><path stroke-linecap="round" stroke-linejoin="round" d="M11.25 11.25l.041-.02a.75.75 0 011.063.852l-.708 2.836a.75.75 0 001.063.853l.041-.021M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-9-3.75h.008v.008H12V8.25z" /></svg>',
            warning: '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="#f59e0b" style="width:20px;height:20px"><path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m9-.75a9 9 0 11-18 0 9 9 0 0118 0zm-9 3.75h.008v.008H12v-.008z" /></svg>',
        };

        function getContainer() {
            if (!container) {
                container = document.createElement('div');
                container.className = 'oms-toast-container';
                container.setAttribute('aria-live', 'polite');
                document.body.appendChild(container);
            }
            return container;
        }

        function show(type, title, message, duration) {
            var c = getContainer();
            var toast = document.createElement('div');
            toast.className = 'oms-toast toast-' + type;
            toast.innerHTML =
                '<div class="oms-toast-icon">' + (TOAST_ICONS[type] || TOAST_ICONS.info) + '</div>' +
                '<div class="oms-toast-content">' +
                    '<p class="oms-toast-title">' + title + '</p>' +
                    (message ? '<p class="oms-toast-msg">' + message + '</p>' : '') +
                '</div>';

            // Click to dismiss
            toast.addEventListener('click', function () { dismiss(toast); });

            c.appendChild(toast);

            // Auto-dismiss
            var timer = setTimeout(function () { dismiss(toast); }, duration || DURATION);
            toast._omsTimer = timer;
        }

        function dismiss(toast) {
            clearTimeout(toast._omsTimer);
            toast.classList.add('is-leaving');
            setTimeout(function () {
                if (toast.parentNode) toast.parentNode.removeChild(toast);
            }, 300);
        }

        return {
            success: function (title, msg, dur) { show('success', title, msg, dur); },
            error:   function (title, msg, dur) { show('error',   title, msg, dur); },
            info:    function (title, msg, dur) { show('info',    title, msg, dur); },
            warning: function (title, msg, dur) { show('warning', title, msg, dur); },
        };
    })();

    /* ══════════════════════════════════════════════════════
       INIT — runs on DOMContentLoaded
       ══════════════════════════════════════════════════════ */
    document.addEventListener('DOMContentLoaded', function () {
        // 1. Apply saved theme
        initTheme();

        // 2. Wire up all [data-theme-toggle] elements
        document.querySelectorAll('[data-theme-toggle]').forEach(function (el) {
            el.addEventListener('click', toggleTheme);
        });

        // 3. Wire up delete confirmation links/buttons that use data-confirm
        //    e.g.: <a href="/Orders/Delete?id=X" data-confirm="Xóa đơn này?">Xóa</a>
        document.addEventListener('click', function (e) {
            var el = e.target.closest('[data-confirm]');
            if (!el) return;
            var message = el.getAttribute('data-confirm');
            var href    = el.getAttribute('href');
            var form    = el.closest('form');
            var isSubmitBtn = el.tagName === 'BUTTON' && el.type === 'submit';

            e.preventDefault();
            e.stopPropagation();

            confirmDelete(message, function () {
                if (href) {
                    window.location.href = href;
                } else if (isSubmitBtn && form) {
                    // Remove data-confirm to prevent re-trigger, then submit
                    el.removeAttribute('data-confirm');
                    form.submit();
                }
            });
        });
    });

    /* ══════════════════════════════════════════════════════
       EXPOSE GLOBALS
       ══════════════════════════════════════════════════════ */
    global.OmsModal    = OmsModal;
    global.OmsToast    = OmsToast;
    global.confirmDelete = confirmDelete;
    global.toggleTheme = toggleTheme;

})(window);
