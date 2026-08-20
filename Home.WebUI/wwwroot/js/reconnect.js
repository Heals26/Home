// Blazor drives the reconnect overlay by toggling classes on #home-reconnect. The markup and the
// look live with the rest of the app; this file only decides what happens when reconnecting stops
// being something the user should wait for.
//
// Blazor's own overlay leaves a dead page sitting there once it gives up. A dead page is worse
// than a reload: nothing on it can talk to the browser any more, so a tap does nothing and the
// session looks broken. Reloading restores the stored session and lands the family back where
// they were.
(function () {
    "use strict";

    var RETRY_DELAY_MS = 3000;

    var element = null;
    var reloading = false;
    var pending = null;

    function reload() {
        if (reloading)
            return;

        reloading = true;
        window.location.reload();
    }

    // Only reload once the server is actually answering. Reloading while the network is down
    // trades our overlay for the browser's error page, which nobody can recover from.
    function reloadWhenReachable() {
        if (reloading || pending)
            return;

        pending = window.setTimeout(function () {
            pending = null;

            if (navigator.onLine === false) {
                reloadWhenReachable();
                return;
            }

            fetch(window.location.href, { method: "HEAD", cache: "no-store" })
                .then(reload)
                .catch(reloadWhenReachable);
        }, RETRY_DELAY_MS);
    }

    function onClassChange() {
        // Rejected means the server is up and has thrown this circuit away — exactly the case a
        // reload fixes instantly and silently.
        if (element.classList.contains("components-reconnect-rejected")) {
            reload();
            return;
        }

        if (element.classList.contains("components-reconnect-failed"))
            reloadWhenReachable();
    }

    function start() {
        element = document.getElementById("components-reconnect-modal");

        if (!element)
            return;

        new MutationObserver(onClassChange).observe(element, { attributeFilter: ["class"] });

        var retry = document.getElementById("home-reconnect-retry");

        if (retry)
            retry.addEventListener("click", reload);

        // A tab that has been in the background often comes back to a circuit the server has
        // already dropped. Asking Blazor to prove the connection now means the overlay appears
        // and resolves before anyone taps something that silently does nothing.
        document.addEventListener("visibilitychange", function () {
            if (document.visibilityState === "visible" && element.classList.contains("components-reconnect-rejected"))
                reload();
        });
    }

    if (document.readyState === "loading")
        document.addEventListener("DOMContentLoaded", start);
    else
        start();
})();
