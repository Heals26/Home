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

    // Below this, the tab was away for a glance at a notification and the socket almost certainly
    // survived. Above it, on a phone, it almost certainly did not.
    var HIDDEN_GRACE_MS = 8000;

    // How long the circuit gets to answer before it is treated as gone. Generous, because a phone
    // waking up is also reconnecting its radio.
    var PING_TIMEOUT_MS = 4000;

    var element = null;
    var reloading = false;
    var pending = null;
    var hiddenAt = 0;

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

    // A locked phone freezes the tab. The socket dies, but nothing on the page runs to notice, so
    // it comes back looking alive and does nothing when tapped. Blazor works it out from its own
    // keepalive, which was frozen too, so the page can sit there dead for a long time.
    //
    // This asks the circuit directly. The call can only complete if there is a circuit alive to
    // run it, so the answer is in whether it comes back at all, not in what it returns. Anything
    // else, a rejection or silence, means the page is a corpse and reloading is the fix.
    function verifyCircuit() {
        if (reloading || typeof DotNet === "undefined")
            return;

        var settled = false;

        var timer = window.setTimeout(function () {
            if (settled)
                return;

            settled = true;
            reloadWhenReachable();
        }, PING_TIMEOUT_MS);

        var done = function (alive) {
            if (settled)
                return;

            settled = true;
            window.clearTimeout(timer);

            if (!alive)
                reloadWhenReachable();
        };

        try {
            DotNet.invokeMethodAsync("Home.WebUI", "HomeCircuitPing")
                .then(function () { done(true); })
                .catch(function () { done(false); });
        } catch (e) {
            done(false);
        }
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
            if (document.visibilityState === "hidden") {
                hiddenAt = Date.now();
                return;
            }

            if (element.classList.contains("components-reconnect-rejected")) {
                reload();
                return;
            }

            var away = hiddenAt === 0 ? 0 : Date.now() - hiddenAt;

            hiddenAt = 0;

            if (away >= HIDDEN_GRACE_MS)
                verifyCircuit();
        });
    }

    if (document.readyState === "loading")
        document.addEventListener("DOMContentLoaded", start);
    else
        start();
})();
