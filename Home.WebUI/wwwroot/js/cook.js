// Cooking-mode helpers: keep the tablet awake while a recipe is open, and chime when a
// timer finishes. Everything degrades silently — cooking works without any of it.
window.homeCook = {
    wakeLock: null,

    async keepAwake() {
        try {
            if ("wakeLock" in navigator) {
                window.homeCook.wakeLock = await navigator.wakeLock.request("screen");
                document.addEventListener("visibilitychange", window.homeCook.reacquire);
            }
        } catch {
            // Not supported or denied — the screen just sleeps as normal.
        }
    },

    async reacquire() {
        if (document.visibilityState === "visible" && window.homeCook.wakeLock !== null) {
            try {
                window.homeCook.wakeLock = await navigator.wakeLock.request("screen");
            } catch {
                // Lost for good; nothing useful to do.
            }
        }
    },

    async release() {
        document.removeEventListener("visibilitychange", window.homeCook.reacquire);

        try {
            await window.homeCook.wakeLock?.release();
        } catch {
            // Already released.
        }

        window.homeCook.wakeLock = null;
    },

    chime() {
        try {
            const context = new (window.AudioContext || window.webkitAudioContext)();
            const oscillator = context.createOscillator();
            const gain = context.createGain();

            oscillator.type = "sine";
            oscillator.frequency.value = 880;
            gain.gain.setValueAtTime(0.001, context.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.4, context.currentTime + 0.02);
            gain.gain.exponentialRampToValueAtTime(0.001, context.currentTime + 1.2);

            oscillator.connect(gain).connect(context.destination);
            oscillator.start();
            oscillator.stop(context.currentTime + 1.25);
        } catch {
            // No audio context — the visual pulse still shows.
        }
    }
};
