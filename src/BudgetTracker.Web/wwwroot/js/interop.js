window.budgetInterop = {
    setTheme: function (theme) {
        document.documentElement.setAttribute('data-theme', theme);
        try { localStorage.setItem('budget-tracker.theme', theme); } catch (e) { }
    },

    scrollToTop: function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
};
