// The browser's own time zone, so the calendar can be shown as the person looking at it sees
// time rather than as the server does. Read once per circuit by ViewerClock.
window.homeClock = {
    timeZone() {
        try {
            return Intl.DateTimeFormat().resolvedOptions().timeZone || "";
        } catch {
            return "";
        }
    }
};
