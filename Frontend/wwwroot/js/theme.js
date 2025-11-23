(function () {
    const KEY = "app-theme";
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

    // Função que aplica o tema
    function applyTheme(theme) {
        const root = document.documentElement;
        if (theme === "dark") {
            root.classList.add("dark");
        } else {
            root.classList.remove("dark");
        }
        localStorage.setItem(KEY, theme);
    }

    // Função para obter o tema guardado ou preferido
    function getTheme() {
        const saved = localStorage.getItem(KEY);
        if (saved) {
            return saved;
        }
        // Se não houver nada salvo, usa a preferência do sistema
        return mediaQuery.matches ? "dark" : "light";
    }

    // --- Esta é a parte importante ---
    // É executada IMEDIATAMENTE quando o script é lido.
    // Não espera pelo Blazor ou pelo DOMContentLoaded.
    const initialTheme = getTheme();
    applyTheme(initialTheme);
    // ---------------------------------

    // Expõe as funções para o Blazor poder chamá-las
    window.theme = {
        toggle: () => {
            const current = getTheme();
            const next = current === "light" ? "dark" : "light";
            applyTheme(next);
        },
        set: (theme) => {
            applyTheme(theme);
        },
        get: () => {
            return getTheme();
        }
    };
})();