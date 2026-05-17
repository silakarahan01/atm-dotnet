window.atmJs = {
    _keyRef: null,
    _onKey: null,

    initKeyboard(dotNetRef) {
        this._keyRef = dotNetRef;
        this._onKey = (e) => {
            if (!this._keyRef) return;
            if (e.key >= '0' && e.key <= '9') {
                this._keyRef.invokeMethodAsync('HandleKey', e.key);
            } else if (e.key === 'Enter') {
                e.preventDefault();
                this._keyRef.invokeMethodAsync('HandleKey', 'Enter');
            } else if (e.key === 'Backspace') {
                e.preventDefault();
                this._keyRef.invokeMethodAsync('HandleKey', 'Backspace');
            } else if (e.key === 'Escape') {
                this._keyRef.invokeMethodAsync('HandleKey', 'Escape');
            }
        };
        document.addEventListener('keydown', this._onKey);
    },

    disposeKeyboard() {
        if (this._onKey) document.removeEventListener('keydown', this._onKey);
        this._keyRef = null;
        this._onKey = null;
    },

    animateBalance(elementId, targetValue, durationMs = 1400) {
        const el = document.getElementById(elementId);
        if (!el) return;
        const start = performance.now();
        function step(now) {
            const t = Math.min((now - start) / durationMs, 1);
            const eased = 1 - Math.pow(1 - t, 3);
            const current = targetValue * eased;
            el.textContent = current.toLocaleString('tr-TR', {
                minimumFractionDigits: 2, maximumFractionDigits: 2
            }) + ' ₺';
            if (t < 1) requestAnimationFrame(step);
        }
        requestAnimationFrame(step);
    },

    shake(elementId) {
        const el = document.getElementById(elementId);
        if (!el) return;
        el.style.animation = 'none';
        el.offsetHeight;
        el.style.animation = 'shake 0.5s ease';
        setTimeout(() => { el.style.animation = ''; }, 600);
    },

    flashError(elementId) {
        const el = document.getElementById(elementId);
        if (!el) return;
        el.style.transition = 'background 0.15s';
        el.style.background = '#1a0808';
        setTimeout(() => { el.style.background = '#0d1117'; }, 400);
    }
};
