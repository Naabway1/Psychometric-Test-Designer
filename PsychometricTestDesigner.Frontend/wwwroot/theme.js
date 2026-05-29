window.ptdTheme = {
    get: () => {
        const saved = localStorage.getItem("ptd-theme") || "dark";
        document.documentElement.dataset.theme = saved;
        return saved;
    },
    set: (theme) => {
        const next = theme === "light" ? "light" : "dark";

        const apply = () => {
            localStorage.setItem("ptd-theme", next);
            document.documentElement.dataset.theme = next;
            window.dispatchEvent(new CustomEvent("ptd-theme-changed", { detail: { theme: next } }));
        };

        if (document.startViewTransition) {
            const transition = document.startViewTransition(apply);
            transition.ready.then(() => {
                document.documentElement.animate(
                    [
                        { clipPath: "circle(0% at calc(100% - 92px) 30px)" },
                        { clipPath: "circle(145% at calc(100% - 92px) 30px)" }
                    ],
                    {
                        duration: 420,
                        easing: "cubic-bezier(.22,1,.36,1)",
                        pseudoElement: "::view-transition-new(root)"
                    });
            });
        } else {
            document.documentElement.classList.add("theme-changing");
            apply();
            window.clearTimeout(window.__ptdThemeTimer);
            window.__ptdThemeTimer = window.setTimeout(() => {
                document.documentElement.classList.remove("theme-changing");
            }, 260);
        }

        return next;
    },
    toggle: () => {
        const current = document.documentElement.dataset.theme || localStorage.getItem("ptd-theme") || "dark";
        return window.ptdTheme.set(current === "light" ? "dark" : "light");
    }
};

window.ptdTheme.get();
