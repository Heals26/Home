// Keeps the page usable while its connection to the server comes and goes.
//
// A phone loses its connection nearly every time it is locked, and Blazor gets it back on its own,
// usually inside a second, to a circuit the server is still holding with everything on it. So:
//
// - A short reconnect shows nothing. A small notice appears at the top only once reconnecting has
//   taken long enough to notice, or straight away when someone tries to use the page meanwhile.
//   The page stays on screen to be read and scrolled.
// - Nothing done while the connection is down is thrown away. Taps and Enter are held and played
//   back in order once it returns, each control once, and whatever was typed meanwhile is sent
//   again so the server sees what the box really holds.
// - Only a circuit the server has let go of reloads the page, and only once the server answers,
//   because a reload while the network is down trades this for the browser's own error page.
//
// Blazor marks its connection state with classes on #components-reconnect-modal, the notice in
// App.razor, and announces each change with a components-reconnect-state-changed event.
(function () {
    "use strict";

    // Unlocking a phone usually costs well under a second, which is not worth a notice.
    var NOTICE_DELAY_MS = 2000;

    // Lets the render Blazor held back while it was disconnected land first, so held taps are
    // played against the page as it now is.
    var REPLAY_DELAY_MS = 150;

    var RETRY_DELAY_MS = 3000;

    // Below this, the tab was away for a glance at a notification and the socket almost certainly
    // survived. Above it, on a phone, it often did not.
    var HIDDEN_GRACE_MS = 8000;

    // How long the circuit gets to answer before it is treated as gone. Generous, because a phone
    // waking up is also reconnecting its radio.
    var PING_TIMEOUT_MS = 4000;

    var DOWN_CLASSES = [
        "components-reconnect-show",
        "components-reconnect-retrying",
        "components-reconnect-failed",
        "components-reconnect-rejected",
        "components-reconnect-paused",
        "components-reconnect-resume-failed"
    ];

    // Everything Blazor listens for in this app, plus the clicks and submits it acts on.
    var INTERCEPTED = [
        "blur", "change", "click", "dragend", "dragenter", "dragover", "dragstart", "drop", "focus",
        "input", "keydown", "mousedown", "pointercancel", "pointerdown", "pointerleave",
        "pointermove", "pointerup", "submit"
    ];

    var CONTROLS = "a, button, input, label, select, textarea, [role='button']";

    var element = null;
    var reloading = false;
    var probe = null;
    var hiddenAt = 0;
    var noticeTimer = null;
    var replayTimer = null;
    var held = [];
    var typed = new Map();
    var refocused = new Set();

    function isDown() {
        if (!element)
            return false;

        for (var i = 0; i < DOWN_CLASSES.length; i++) {
            if (element.classList.contains(DOWN_CLASSES[i]))
                return true;
        }

        return false;
    }

    function reload() {
        if (reloading)
            return;

        reloading = true;
        window.location.reload();
    }

    // Waits for the server to answer before doing anything, retrying every few seconds.
    function whenReachable(then) {
        if (reloading || probe)
            return;

        probe = window.setTimeout(function () {
            probe = null;

            if (navigator.onLine === false) {
                whenReachable(then);
                return;
            }

            fetch(window.location.href, { method: "HEAD", cache: "no-store" })
                .then(then)
                .catch(function () { whenReachable(then); });
        }, RETRY_DELAY_MS);
    }

    // Blazor has stopped retrying, but the circuit may well still be on the server, which keeps
    // everything on the page and everything held. Only a circuit that is gone costs a reload.
    function resume() {
        if (!isDown())
            return;

        if (typeof Blazor === "undefined" || typeof Blazor.reconnect !== "function") {
            reload();
            return;
        }

        Blazor.reconnect()
            .then(function (connected) {
                if (!connected)
                    reload();
            })
            .catch(function () { whenReachable(resume); });
    }

    function showNotice() {
        window.clearTimeout(noticeTimer);
        noticeTimer = null;

        element.classList.add("home-reconnect-open");

        // In the top layer, so an open sheet cannot hide it.
        if (typeof element.showPopover === "function" && !element.matches(":popover-open"))
            element.showPopover();
    }

    function hideNotice() {
        window.clearTimeout(noticeTimer);
        noticeTimer = null;

        element.classList.remove("home-reconnect-open");

        if (typeof element.hidePopover === "function" && element.matches(":popover-open"))
            element.hidePopover();
    }

    function hold(kind, control, run) {
        // Tapped again because nothing seemed to happen, which is not a second tap.
        for (var i = 0; i < held.length; i++) {
            if (held[i].kind === kind && held[i].control === control)
                return;
        }

        control.classList.add("home-held");
        held.push({ kind: kind, control: control, run: run });
        showNotice();
    }

    // Enter in a text box, held the way the box itself would have handled it.
    function holdEnter(box) {
        hold("enter", box, function () {
            if (box.dataset.homeSubmitOnEnter === "true" || !box.form)
                box.dispatchEvent(new KeyboardEvent("keydown", { key: "Enter", bubbles: true, cancelable: true }));
            else
                box.form.requestSubmit();
        });
    }

    function isTextBox(target) {
        return target instanceof HTMLInputElement
            && ["button", "checkbox", "color", "file", "radio", "range", "reset", "submit"].indexOf(target.type) < 0;
    }

    // A tap into somewhere to type does its whole job in the browser, so there is nothing to hold.
    function isTypingIn(control) {
        if (control instanceof HTMLLabelElement)
            control = control.control;

        return isTextBox(control) || control instanceof HTMLTextAreaElement || control instanceof HTMLSelectElement;
    }

    // While the connection is down Blazor cannot be told anything, and an event it tries to send
    // is lost. So nothing reaches it: what matters is held for later, and the rest waits.
    function intercept(e) {
        if (reloading || !isDown() || !(e.target instanceof Element) || element.contains(e.target))
            return;

        e.stopPropagation();

        // Only a control acts on a tap; one on the page around them has nothing to wait for.
        var control = e.target.closest(CONTROLS);

        // Played back on the control rather than whatever was under the finger, which can be an
        // icon, and an SVG element has no click() to play it back with.
        if (e.type === "click" && control && !isTypingIn(control)) {
            e.preventDefault();
            hold("click", control, function () { control.click(); });
        }
        else if (e.type === "keydown" && e.key === "Enter" && !e.isComposing && isTextBox(e.target)) {
            e.preventDefault();
            holdEnter(e.target);
        }
        else if (e.type === "input" || e.type === "change") {
            var kinds = typed.get(e.target) || {};

            kinds[e.type] = true;
            typed.set(e.target, kinds);
        }
        else if (e.type === "focus" || e.type === "blur") {
            refocused.add(e.target);
        }
        else if (e.type === "submit") {
            e.preventDefault();
        }
    }

    // The server is told what changed while it could not hear: what was typed first, so a held
    // Save sends what the form now says, then where the cursor went, so a box that lost it can
    // put its suggestions away.
    function replay() {
        replayTimer = null;

        if (isDown())
            return;

        typed.forEach(function (kinds, field) {
            if (!field.isConnected)
                return;

            if (kinds.input)
                field.dispatchEvent(new Event("input", { bubbles: true }));

            if (kinds.change)
                field.dispatchEvent(new Event("change", { bubbles: true }));
        });

        typed.clear();

        refocused.forEach(function (field) {
            if (field.isConnected)
                field.dispatchEvent(new FocusEvent(field === document.activeElement ? "focus" : "blur"));
        });

        refocused.clear();

        var actions = held;

        held = [];

        actions.forEach(function (action) {
            action.control.classList.remove("home-held");

            if (action.control.isConnected)
                action.run();
        });
    }

    function onStateChanged(e) {
        var state = e.detail ? e.detail.state : null;

        if (state === "show") {
            window.clearTimeout(noticeTimer);
            noticeTimer = window.setTimeout(showNotice, NOTICE_DELAY_MS);
        }
        else if (state === "hide") {
            hideNotice();
            window.clearTimeout(probe);
            probe = null;
            window.clearTimeout(replayTimer);
            replayTimer = window.setTimeout(replay, REPLAY_DELAY_MS);
        }
        else if (state === "failed") {
            showNotice();
            whenReachable(resume);
        }
        else if (state === "rejected" || state === "resume-failed") {
            // The server is up and has let this circuit go, which a reload fixes at once.
            showNotice();
            reload();
        }
        else if (state === "paused") {
            showNotice();
        }
    }

    // A socket can die while the phone is locked without the page hearing about it, and then the
    // page looks alive and every tap goes nowhere. Asking the circuit something settles it: a dead
    // socket fails the asking, which is what starts Blazor reconnecting. Only a call that is never
    // answered while Blazor still believes it is connected leaves nothing but a reload.
    function verifyCircuit() {
        if (reloading || typeof DotNet === "undefined")
            return;

        var settled = false;

        var timer = window.setTimeout(function () {
            if (settled)
                return;

            settled = true;

            if (!isDown())
                whenReachable(reload);
        }, PING_TIMEOUT_MS);

        var done = function () {
            if (settled)
                return;

            settled = true;
            window.clearTimeout(timer);
        };

        try {
            DotNet.invokeMethodAsync("Home.WebUI", "HomeCircuitPing").then(done, done);
        } catch (e) {
            done();
        }
    }

    function onVisibilityChange() {
        if (document.visibilityState === "hidden") {
            hiddenAt = Date.now();
            return;
        }

        var away = hiddenAt === 0 ? 0 : Date.now() - hiddenAt;

        hiddenAt = 0;

        if (element.classList.contains("components-reconnect-rejected")) {
            reload();
            return;
        }

        // Blazor itself retries the moment the page is visible again, unless it has given up.
        if (element.classList.contains("components-reconnect-failed")) {
            whenReachable(resume);
            return;
        }

        if (!isDown() && away >= HIDDEN_GRACE_MS)
            verifyCircuit();
    }

    function start() {
        element = document.getElementById("components-reconnect-modal");

        if (!element)
            return;

        element.addEventListener("components-reconnect-state-changed", onStateChanged);

        var retry = document.getElementById("home-reconnect-retry");

        if (retry)
            retry.addEventListener("click", reload);

        INTERCEPTED.forEach(function (type) {
            window.addEventListener(type, intercept, true);
        });

        document.addEventListener("visibilitychange", onVisibilityChange);
    }

    if (document.readyState === "loading")
        document.addEventListener("DOMContentLoaded", start);
    else
        start();
})();
