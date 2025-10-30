// Synchronizes the vault board/table views and shared search filter.
(function (window, document) {
    'use strict';

    class VaultList {
        constructor(root) {
            if (!root) {
                throw new Error('VaultList requires a root element.');
            }

            this.root = root;
            this.searchInput = root.querySelector('[data-vault-search]');
            this.viewButtons = Array.from(root.querySelectorAll('[data-view-trigger]'));
            this.panels = Array.from(root.querySelectorAll('[data-view-panel]'));
            this.table = root.querySelector('[data-vault-table]');
            this.defaultView = root.getAttribute('data-default-view')
                || this.viewButtons.find(btn => btn.classList.contains('active'))?.getAttribute('data-view-trigger')
                || 'board';
            this.currentView = null;

            this.handleViewClick = this.handleViewClick.bind(this);
            this.handleSearchInput = this.handleSearchInput.bind(this);

            this.registerEvents();
            this.setActiveView(this.defaultView);
            this.applyFilter(this.searchInput?.value ?? '');
        }

        registerEvents() {
            this.viewButtons.forEach(btn => {
                btn.addEventListener('click', this.handleViewClick);
            });

            this.searchInput?.addEventListener('input', this.handleSearchInput);
        }

        handleViewClick(event) {
            const targetView = event.currentTarget?.getAttribute('data-view-trigger');
            if (!targetView) {
                return;
            }

            this.setActiveView(targetView);
        }

        handleSearchInput(event) {
            const value = event?.target?.value ?? '';
            this.applyFilter(value);
        }

        setActiveView(view) {
            if (!view || view === this.currentView) {
                return;
            }

            this.currentView = view;
            this.root.setAttribute('data-active-view', view);

            this.viewButtons.forEach(btn => {
                const matches = btn.getAttribute('data-view-trigger') === view;
                btn.classList.toggle('active', matches);
                btn.setAttribute('aria-pressed', matches ? 'true' : 'false');
            });

            this.panels.forEach(panel => {
                const matches = panel.getAttribute('data-view-panel') === view;
                panel.classList.toggle('is-active', matches);
                panel.classList.toggle('d-none', !matches);
            });
        }

        applyFilter(rawQuery) {
            const value = (rawQuery ?? '').toString().trim();
            const query = value.toLowerCase();
            const rows = this.getTableRows();
            const cards = this.getBoardCards();

            rows.forEach(row => {
                const haystack = (row.getAttribute('data-search') || row.textContent || '').toLowerCase();
                const matches = !query || haystack.includes(query);
                row.style.display = matches ? '' : 'none';
            });

            cards.forEach(card => {
                const haystack = (card.getAttribute('data-search') || card.textContent || '').toLowerCase();
                const matches = !query || haystack.includes(query);
                card.classList.toggle('d-none', !matches);
            });
        }

        getTableRows() {
            if (!this.table) {
                return [];
            }

            return Array.from(this.table.querySelectorAll('tbody tr'));
        }

        getBoardCards() {
            return Array.from(this.root.querySelectorAll('[data-board-card]'));
        }
    }

    function initialise() {
        const instances = [];
        document.querySelectorAll('[data-vault-list]').forEach(root => {
            if (root.__vaultListInstance) {
                instances.push(root.__vaultListInstance);
                return;
            }

            const instance = new VaultList(root);
            root.__vaultListInstance = instance;
            instances.push(instance);
        });

        return instances;
    }

    function autoInit() {
        initialise();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', autoInit);
    } else {
        autoInit();
    }

    if (typeof window !== 'undefined') {
        window.VaultList = VaultList;
        VaultList.init = initialise;
    }
}(window, document));
