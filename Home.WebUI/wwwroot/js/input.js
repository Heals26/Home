// A text box someone is typing into belongs to the browser.
//
// Every keystroke is a round trip, and the answer to one can land after the next two have been
// typed, so rendering the value back into the box puts it back as it was and eats letters. The
// server only writes to a box when it means to set the text, and it writes here, into the same
// element. Replacing the element instead, which is what this used to do, took the cursor with it:
// a shopping list had to be tapped again after every item.
window.homeInput = {
    write(element, value) {
        if (element && element.value !== value)
            element.value = value;
    },

    // Enter hands over the line and empties the box straight away, in the browser, so the next
    // item can be typed while this one is being saved. Emptying it from the server instead lands a
    // round trip later and takes the first letters of the next item with it.
    submitOnEnter(element, caller) {
        if (!element || element.dataset.homeSubmitOnEnter === "true")
            return;

        element.dataset.homeSubmitOnEnter = "true";

        element.addEventListener("keydown", function (e) {
            if (e.key !== "Enter" || e.isComposing || e.shiftKey || e.altKey || e.ctrlKey || e.metaKey)
                return;

            // Neither Blazor's own key handler for the box nor a form around it sees this Enter.
            e.preventDefault();
            e.stopPropagation();

            const text = element.value;

            if (text.trim().length === 0)
                return;

            element.value = "";
            element.dispatchEvent(new Event("input", { bubbles: true }));

            caller.invokeMethodAsync("SubmitFromBrowserAsync", text).catch(function () {
                // The line never arrived, so it goes back where it was typed rather than vanishing,
                // unless something new has been typed there since.
                if (element.value.length > 0)
                    return;

                element.value = text;
                element.dispatchEvent(new Event("input", { bubbles: true }));
            });
        });
    }
};
