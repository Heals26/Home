// Small per-device choices that are nobody's business but this screen's: which scope the board
// shows, and the like. Stored in localStorage so a kitchen tablet keeps its setting and a phone
// keeps a different one. Storage being unavailable simply means the choice does not survive a
// reload, which is the right failure.
window.homePreferences = {
    get(key) {
        try {
            return localStorage.getItem("home-pref:" + key) ?? "";
        } catch {
            return "";
        }
    },

    set(key, value) {
        try {
            localStorage.setItem("home-pref:" + key, value ?? "");
        } catch {
            // Private browsing or storage disabled.
        }
    }
};
