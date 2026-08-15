// Per-device theme preference. The stored value is one of "dark", "light" or "device";
// anything else (including nothing stored, or storage being unavailable) means dark, so a
// kitchen tablet never changes appearance on its own.
//
// The *first* application happens in an inline script in App.razor, before the body renders —
// this file only handles changes made after the circuit is up.
window.homeTheme = {
    storageKey: "home-theme",
    mediaQuery: null,

    resolve(preference) {
        if (preference === "light")
            return "light";

        if (preference === "device")
            return window.matchMedia("(prefers-color-scheme: light)").matches ? "light" : "dark";

        return "dark";
    },

    get() {
        try {
            const stored = localStorage.getItem(window.homeTheme.storageKey);
            return stored === "light" || stored === "device" ? stored : "dark";
        } catch {
            return "dark";
        }
    },

    set(preference) {
        try {
            localStorage.setItem(window.homeTheme.storageKey, preference);
        } catch {
            // Private browsing or storage disabled — the choice just won't survive a reload.
        }

        document.documentElement.setAttribute("data-theme", window.homeTheme.resolve(preference));
        window.homeTheme.watchDevice(preference === "device");
    },

    // "Match device" has to keep matching when the OS flips at dusk, not only at page load.
    watchDevice(listen) {
        window.homeTheme.mediaQuery ??= window.matchMedia("(prefers-color-scheme: light)");
        window.homeTheme.mediaQuery.removeEventListener("change", window.homeTheme.onDeviceChanged);

        if (listen)
            window.homeTheme.mediaQuery.addEventListener("change", window.homeTheme.onDeviceChanged);
    },

    onDeviceChanged(event) {
        document.documentElement.setAttribute("data-theme", event.matches ? "light" : "dark");
    }
};

window.homeTheme.watchDevice(window.homeTheme.get() === "device");
