// Global scripts (confirmation helper, etc).
(function (window, document) {
    'use strict';

    function buildConfirmOptions(trigger) {
        return {
            title: trigger.getAttribute('data-confirm-title') || 'Are you sure?',
            text: trigger.getAttribute('data-confirm-text') || '',
            icon: trigger.getAttribute('data-confirm-icon') || 'warning',
            showCancelButton: trigger.getAttribute('data-confirm-show-cancel') !== 'false',
            confirmButtonText: trigger.getAttribute('data-confirm-confirm-text') || 'Confirm',
            cancelButtonText: trigger.getAttribute('data-confirm-cancel-text') || 'Cancel',
            confirmButtonColor: trigger.getAttribute('data-confirm-confirm-color') || undefined,
            focusCancel: trigger.getAttribute('data-confirm-focus') === 'cancel',
            focusConfirm: trigger.getAttribute('data-confirm-focus') === 'confirm'
        };
    }

    function showConfirmation(trigger) {
        const swal = window.Swal;
        const options = buildConfirmOptions(trigger);

        if (swal && typeof swal.fire === 'function') {
            return swal.fire(options);
        }

        const message = options.text || options.title || 'Are you sure?';
        const result = window.confirm(message);
        return Promise.resolve({ isConfirmed: result });
    }

    function performConfirmedAction(trigger) {
        const submitSelector = trigger.getAttribute('data-confirm-submit');
        const url = trigger.getAttribute('data-confirm-url') || trigger.getAttribute('href');
        const method = trigger.getAttribute('data-confirm-method');
        const targetSelector = trigger.getAttribute('data-confirm-target');
        const openIn = trigger.getAttribute('data-confirm-open');

        if (submitSelector) {
            const form = document.querySelector(submitSelector);
            if (form && typeof form.submit === 'function') {
                if (url) {
                    form.setAttribute('action', url);
                }
                if (method) {
                    form.setAttribute('method', method);
                }
                form.submit();
                return;
            }
        }

        if (targetSelector) {
            const target = document.querySelector(targetSelector);
            if (target) {
                if (target.tagName === 'FORM' && typeof target.submit === 'function') {
                    if (url) {
                        target.setAttribute('action', url);
                    }
                    if (method) {
                        target.setAttribute('method', method);
                    }
                    target.submit();
                    return;
                }

                if (typeof target.click === 'function') {
                    target.click();
                    return;
                }
            }
        }

        if (url) {
            if (openIn && openIn !== '_self') {
                window.open(url, openIn);
            }
            else {
                window.location.href = url;
            }
        }
    }

    function onConfirmClick(event) {
        const trigger = event.target.closest('[data-confirm]');
        if (!trigger) {
            return;
        }

        if (trigger.getAttribute('data-confirm-disabled') === 'true'
            || trigger.hasAttribute('disabled')
            || trigger.getAttribute('aria-disabled') === 'true') {
            return;
        }

        event.preventDefault();

        showConfirmation(trigger).then(result => {
            if (result?.isConfirmed) {
                performConfirmedAction(trigger);
            }
        });
    }

    function initConfirmations() {
        document.addEventListener('click', onConfirmClick);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initConfirmations);
    }
    else {
        initConfirmations();
    }
}(window, document));
