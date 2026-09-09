// Enkel fokusfälla för modaler: håller kvar Tab/Shift+Tab-navigering
// inom dialogens element så tangentbordsanvändare inte kan tabba ut
// till innehåll bakom den mörka overlayn.
window.eduSenseFocusTrap = (() => {
    let handler = null;

    function getFocusable(container) {
        return Array.from(container.querySelectorAll(
            'a[href], button:not([disabled]), textarea, input, select, [tabindex]:not([tabindex="-1"])'
        )).filter(el => el.offsetParent !== null);
    }

    function trap(elementId) {
        release();

        const container = document.getElementById(elementId);
        if (!container) {
            return;
        }

        handler = (e) => {
            if (e.key !== 'Tab') {
                return;
            }

            const focusable = getFocusable(container);
            if (focusable.length === 0) {
                return;
            }

            const first = focusable[0];
            const last = focusable[focusable.length - 1];

            if (e.shiftKey && document.activeElement === first) {
                e.preventDefault();
                last.focus();
            } else if (!e.shiftKey && document.activeElement === last) {
                e.preventDefault();
                first.focus();
            }
        };

        document.addEventListener('keydown', handler, true);
    }

    function release() {
        if (handler) {
            document.removeEventListener('keydown', handler, true);
            handler = null;
        }
    }

    return { trap, release };
})();
