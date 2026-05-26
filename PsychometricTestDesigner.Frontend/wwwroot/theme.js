window.ptdTheme = {
    get: () => {
        const saved = localStorage.getItem("ptd-theme") || "dark";
        document.documentElement.dataset.theme = saved;
        return saved;
    },
    set: (theme) => {
        const next = theme === "light" ? "light" : "dark";
        document.documentElement.classList.add("theme-changing");
        localStorage.setItem("ptd-theme", next);
        document.documentElement.dataset.theme = next;
        window.dispatchEvent(new CustomEvent("ptd-theme-changed", { detail: { theme: next } }));
        window.clearTimeout(window.__ptdThemeTimer);
        window.__ptdThemeTimer = window.setTimeout(() => {
            document.documentElement.classList.remove("theme-changing");
        }, 420);
        return next;
    },
    toggle: () => {
        const current = document.documentElement.dataset.theme || localStorage.getItem("ptd-theme") || "dark";
        return window.ptdTheme.set(current === "light" ? "dark" : "light");
    }
};

window.ptdTheme.get();
