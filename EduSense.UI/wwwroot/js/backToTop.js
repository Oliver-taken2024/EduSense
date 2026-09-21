// Lyssnar på sidans scroll-position och meddelar Blazor-komponenten när man
// passerat tröskelvärdet, istället för att polla från .NET-sidan.
window.eduSenseBackToTop = (() => {
    let handler = null;

    function init(dotNetRef, thresholdPx) {
        release();

        handler = () => {
            dotNetRef.invokeMethodAsync('OnScroll', window.scrollY > thresholdPx);
        };

        window.addEventListener('scroll', handler, { passive: true });
        handler(); // sätt rätt initialt läge direkt (t.ex. vid F5 mitt på en lång sida)
    }

    function release() {
        if (handler) {
            window.removeEventListener('scroll', handler);
            handler = null;
        }
    }

    function scrollToTop() {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    return { init, release, scrollToTop };
})();
