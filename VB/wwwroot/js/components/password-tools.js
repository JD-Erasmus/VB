(() => {
    const ready = (callback) => {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', callback, { once: true });
        } else {
            callback();
        }
    };

    const toast = (icon, title) => {
        if (!title) {
            return;
        }

        if (window.Swal?.fire) {
            window.Swal.fire({
                toast: true,
                position: 'top-end',
                icon: icon || 'info',
                title,
                showConfirmButton: false,
                timer: 1600,
                timerProgressBar: true
            });
            return;
        }

        console.log(`[${icon || 'info'}] ${title}`);
    };

    const requestGenerateConfirmation = async (container, messageSource) => {
        if (!messageSource) {
            return true;
        }

        const title = container.dataset.confirmGenerateTitle || 'Replace password?';
        const text = container.dataset.confirmGenerateText || 'Generating a new password will overwrite the current one.';
        const confirmButtonText = container.dataset.confirmGenerateConfirm || 'Replace';

        if (window.Swal?.fire) {
            const result = await window.Swal.fire({
                icon: 'warning',
                title,
                text,
                showCancelButton: true,
                confirmButtonText
            });

            return !!result.isConfirmed;
        }

        return window.confirm(`${title}\n\n${text}`);
    };

    const bindPasswordTools = (container) => {
        if (!container || container.dataset.passwordToolsBound === 'true') {
            return;
        }

        const passwordInput = container.querySelector('[data-password-input]');
        if (!(passwordInput instanceof HTMLInputElement)) {
            return;
        }

        const generateBtn = container.querySelector('[data-password-action="generate"]');
        const toggleBtn = container.querySelector('[data-password-action="toggle"]');
        const copyBtn = container.querySelector('[data-password-action="copy"]');
        const generateUrl = container.dataset.generateUrl;
        const requiresConfirm = container.dataset.confirmGenerate === 'true';

        generateBtn?.addEventListener('click', async () => {
            if (!generateBtn || !generateUrl) {
                return;
            }

            if (requiresConfirm && passwordInput.value) {
                const confirmed = await requestGenerateConfirmation(container, true);
                if (!confirmed) {
                    return;
                }
            }

            const previousText = generateBtn.textContent;

            try {
                generateBtn.disabled = true;
                generateBtn.textContent = generateBtn.dataset.labelBusy || 'Generating...';

                const response = await fetch(generateUrl, {
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!response.ok) {
                    throw new Error(`Request failed: ${response.status}`);
                }

                const payload = await response.json();
                passwordInput.value = payload?.password || '';
                toast('success', 'Strong password generated');
            } catch (error) {
                console.error('Generate password failed', error);
                toast('error', 'Could not generate password');
            } finally {
                generateBtn.disabled = false;
                generateBtn.textContent = previousText;
            }
        });

        toggleBtn?.addEventListener('click', () => {
            const showing = passwordInput.type === 'text';
            passwordInput.type = showing ? 'password' : 'text';
            toggleBtn.textContent = showing
                ? (toggleBtn.dataset.labelShow || 'Show')
                : (toggleBtn.dataset.labelHide || 'Hide');
        });

        copyBtn?.addEventListener('click', async () => {
            if (!passwordInput.value) {
                toast('info', 'No password to copy');
                return;
            }

            const previousText = copyBtn.textContent;

            try {
                if (navigator.clipboard?.writeText) {
                    await navigator.clipboard.writeText(passwordInput.value);
                } else {
                    const originalType = passwordInput.type;
                    passwordInput.type = 'text';
                    passwordInput.select();
                    passwordInput.setSelectionRange(0, passwordInput.value.length);
                    document.execCommand('copy');
                    passwordInput.type = originalType;
                }

                copyBtn.textContent = copyBtn.dataset.labelDone || 'Copied!';
                toast('success', 'Password copied');
            } catch (error) {
                console.error('Copy failed', error);
                toast('error', 'Unable to copy password');
            } finally {
                setTimeout(() => {
                    copyBtn.textContent = previousText;
                }, 1200);
            }
        });

        container.dataset.passwordToolsBound = 'true';
    };

    ready(() => {
        const containers = document.querySelectorAll('[data-password-tools]');
        containers.forEach(bindPasswordTools);
    });
})();
