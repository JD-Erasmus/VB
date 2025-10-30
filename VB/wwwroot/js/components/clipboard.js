(() => {
    const ready = (callback) => {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', callback, { once: true });
        } else {
            callback();
        }
    };

    const getCopyValue = (trigger) => {
        const explicitText = trigger.dataset.copyText;
        if (typeof explicitText === 'string') {
            return explicitText;
        }

        const selector = trigger.dataset.copyTarget || trigger.getAttribute('data-copy');
        if (!selector) {
            return null;
        }

        const target = document.querySelector(selector);
        if (!target) {
            return null;
        }

        if (target instanceof HTMLInputElement || target instanceof HTMLTextAreaElement) {
            return target.value ?? '';
        }

        return target.textContent?.trim() ?? '';
    };

    const writeClipboard = async (text) => {
        if (!text) {
            return false;
        }

        if (navigator.clipboard?.writeText) {
            try {
                await navigator.clipboard.writeText(text);
                return true;
            } catch {
                // fall back to execCommand
            }
        }

        try {
            const textarea = document.createElement('textarea');
            textarea.value = text;
            textarea.setAttribute('readonly', '');
            textarea.style.position = 'fixed';
            textarea.style.top = '-1000px';
            document.body.appendChild(textarea);
            textarea.select();
            textarea.setSelectionRange(0, textarea.value.length);
            const success = document.execCommand('copy');
            document.body.removeChild(textarea);
            return success;
        } catch {
            return false;
        }
    };

    const notify = (result, trigger) => {
        const defaults = result
            ? { title: 'Copied', text: 'Value copied to clipboard.', icon: 'success' }
            : { title: 'Copy failed', text: 'Please copy the value manually.', icon: 'warning' };

        const title = result
            ? trigger.dataset.copyTitle || defaults.title
            : trigger.dataset.copyFailTitle || defaults.title;
        const text = result
            ? trigger.dataset.copySuccess || defaults.text
            : trigger.dataset.copyFailure || defaults.text;
        const icon = result
            ? trigger.dataset.copyIcon || defaults.icon
            : trigger.dataset.copyFailIcon || defaults.icon;

        if (window.Swal?.fire && (title || text)) {
            window.Swal.fire({
                title,
                text,
                icon
            });
            return;
        }

        if (title || text) {
            const label = title ? `${title}: ` : '';
            console.log(`${label}${text}`);
        }
    };

    ready(() => {
        document.addEventListener('click', async (event) => {
            const trigger = event.target.closest('[data-copy]');
            if (!trigger) {
                return;
            }

            event.preventDefault();

            const value = getCopyValue(trigger);
            if (value == null) {
                notify(false, trigger);
                return;
            }

            const copied = await writeClipboard(value);
            notify(copied, trigger);
        });
    });
})();
