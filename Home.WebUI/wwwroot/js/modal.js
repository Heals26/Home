// Puts the cursor in a modal's first field the moment it opens. Done here rather than with an
// ElementReference per modal because every modal wants it and none of them should have to ask:
// the alternative is twenty screens each remembering to focus, and one of them forgetting.
window.homeModal = {
    focusFirstField: function (panelID) {
        var panel = document.getElementById(panelID);

        if (!panel)
            return;

        // Skips anything the user can't type into, and anything deliberately taken out of the tab
        // order — the hidden submit button that gives the form its Enter key, chiefly.
        var field = panel.querySelector(
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
