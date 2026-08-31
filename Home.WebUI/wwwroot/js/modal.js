// Opens a modal as a real <dialog>, which is where its keyboard behaviour comes from.
//
// showModal() is doing the work that would otherwise be ours to write and get wrong: Escape
// closes, Tab is trapped inside the panel instead of wandering onto the page behind it, the rest
// of the document goes inert to both pointers and screen readers, and the panel is painted in the
// top layer so no stacking context can bury it. None of that arrives on a plain div, which is what
// these were before.
//
// The one thing we take back from the browser is the closing itself: Escape's default would shut
// the element while Blazor still believed the modal was open, and the two would disagree until
// something forced a render. Cancelling the event and telling Blazor instead keeps Visible the
// single source of truth.
window.homeModal = {
    open: function (panelID, autoFocus, caller) {
        var dialog = document.getElementById(panelID);

        if (!dialog || dialog.dataset.homeModalOpened === "true")
            return;

        dialog.dataset.homeModalOpened = "true";

        dialog.addEventListener("cancel", function (e) {
            e.preventDefault();
            caller.invokeMethodAsync("CloseFromBrowserAsync");
        });

        if (!dialog.open)
            dialog.showModal();

        if (autoFocus) {
            window.homeModal.focusFirstField(dialog);
            return;
        }

        // showModal() focuses the first field on its own. Where the caller asked it not to — a
        // list whose first row is a rename box invites renaming it by accident — focus moves to
        // the panel itself, which still has to hold it for Escape and the tab trap to work.
        if (document.activeElement && dialog.contains(document.activeElement))
            document.activeElement.blur();

        dialog.focus();
    },

    focusFirstField: function (dialog) {
        // Skips anything the user can't type into, and anything deliberately taken out of the tab
        // order — the hidden submit button that gives the form its Enter key, chiefly.
        var field = dialog.querySelector(
            "input:not([type=hidden]):not([type=checkbox]):not([type=radio]):not([disabled]):not([tabindex='-1'])," +
            "textarea:not([disabled])," +
            "select:not([disabled])");

        if (!field)
            return;

        field.focus();

        // Puts the caret after existing text rather than selecting it, so opening an edit form
        // and typing appends instead of replacing what was there.
        if (typeof field.setSelectionRange === "function" && (field.type === "text" || field.type === "url" || field.type === "email")) {
            try { field.setSelectionRange(field.value.length, field.value.length); } catch { }
        }
    }
};
