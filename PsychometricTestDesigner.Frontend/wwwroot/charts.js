(function () {
    const chartStore = new Map();
    const palette = ["#8b5cf6", "#22c55e", "#facc15", "#ef4444", "#38bdf8", "#f472b6"];

    function ctx(id) {
        const canvas = document.getElementById(id);
        if (!canvas) return null;
        return canvas.getContext("2d");
    }

    function destroy(id) {
        const chart = chartStore.get(id);
        if (chart) chart.destroy();
        chartStore.delete(id);
    }

    function themeColor(name, fallback) {
        const value = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
        return value || fallback;
    }

    function options(stacked) {
        const text = themeColor("--text-primary", "#f5f7fa");
        const muted = themeColor("--text-secondary", "#a9b1c3");
        const axis = themeColor("--chart-axis", muted);
        const grid = themeColor("--chart-grid", themeColor("--border", "rgba(255,255,255,.08)"));
        return {
            responsive: true,
            maintainAspectRatio: false,
            layout: { padding: 14 },
            plugins: {
                legend: {
                    labels: {
                        color: text,
                        font: { size: 14 },
                        boxWidth: 18,
                        boxHeight: 10
                    }
                },
                tooltip: {
                    titleFont: { size: 14 },
                    bodyFont: { size: 13 }
                }
            },
            scales: {
                x: {
                    stacked,
                    ticks: {
                        color: axis,
                        font: { size: 13 },
                        autoSkip: false,
                        maxRotation: 52,
                        minRotation: 25
                    },
                    grid: { color: grid }
                },
                y: {
                    stacked,
                    beginAtZero: true,
                    ticks: {
                        color: axis,
                        font: { size: 13 }
                    },
                    grid: { color: grid }
                }
            }
        };
    }

    function refreshTheme() {
        chartStore.forEach((chart) => {
            const stacked = Boolean(chart.options?.scales?.x?.stacked);
            chart.options = options(stacked);
            chart.update("none");
        });
    }

    function fallbackBar(id, labels, values, label) {
        const context = ctx(id);
        if (!context) return;
        const canvas = context.canvas;
        const width = canvas.width = canvas.clientWidth || 640;
        const height = canvas.height = canvas.clientHeight || 280;
        const text = themeColor("--text-primary", "#f5f7fa");
        const muted = themeColor("--text-secondary", "#a9b1c3");
        context.clearRect(0, 0, width, height);
        context.fillStyle = muted;
        context.font = "13px Segoe UI, sans-serif";
        context.fillText(label || "Значение", 16, 22);
        const max = Math.max(1, ...values.map(Number));
        const barWidth = Math.max(20, (width - 40) / Math.max(1, values.length) - 10);
        values.forEach((value, index) => {
            const barHeight = (Number(value) / max) * (height - 80);
            const x = 20 + index * (barWidth + 10);
            const y = height - 34 - barHeight;
            context.fillStyle = palette[index % palette.length];
            context.fillRect(x, y, barWidth, barHeight);
            context.fillStyle = text;
            context.fillText(String(Math.round(Number(value) * 10) / 10), x, y - 6);
            context.fillStyle = muted;
            context.fillText(String(labels[index] || "").slice(0, 12), x, height - 12);
        });
    }

    window.ptdCharts = {
        bar(id, labels, values, label, colors) {
            if (!window.Chart) {
                fallbackBar(id, labels || [], values || [], label);
                return;
            }

            const context = ctx(id);
            if (!context) return;
            destroy(id);
            chartStore.set(id, new Chart(context, {
                type: "bar",
                data: {
                    labels,
                    datasets: [{
                        label,
                        data: values,
                        backgroundColor: colors || palette[0],
                        borderColor: colors || palette[0],
                        borderWidth: 1,
                        borderRadius: 8
                    }]
                },
                options: options(false)
            }));
        },

        groupedBar(id, labels, datasets) {
            if (!window.Chart) {
                fallbackBar(id, labels || [], (datasets && datasets[0] && datasets[0].data) || [], "Количество");
                return;
            }

            const context = ctx(id);
            if (!context) return;
            destroy(id);
            chartStore.set(id, new Chart(context, {
                type: "bar",
                data: {
                    labels,
                    datasets: (datasets || []).map((set, index) => ({
                        label: set.label,
                        data: set.data,
                        backgroundColor: set.backgroundColor || set.color || palette[index % palette.length],
                        borderColor: set.borderColor || set.backgroundColor || set.color || palette[index % palette.length],
                        borderRadius: 8
                    }))
                },
                options: options(false)
            }));
        },

        line(id, labels, datasets) {
            if (!window.Chart) {
                fallbackBar(id, labels || [], (datasets && datasets[0] && datasets[0].data) || [], "Динамика");
                return;
            }

            const context = ctx(id);
            if (!context) return;
            destroy(id);
            chartStore.set(id, new Chart(context, {
                type: "line",
                data: {
                    labels,
                    datasets: (datasets || []).map((set, index) => ({
                        label: set.label,
                        data: set.data,
                        borderColor: set.borderColor || set.color || palette[index % palette.length],
                        backgroundColor: set.backgroundColor || `${set.borderColor || set.color || palette[index % palette.length]}33`,
                        tension: 0.35,
                        fill: false
                    }))
                },
                options: options(false)
            }));
        },

        refreshTheme
    };

    window.addEventListener("ptd-theme-changed", () => {
        window.requestAnimationFrame(refreshTheme);
    });
})();
