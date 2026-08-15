// The family board's desktop drag path. A drop only fires if something cancels dragover, and
// Blazor attaches @ondragover:preventDefault only for an event it already has a handler for — a C#
// dragover handler would round-trip to the server on every mouse move over a column and flood the
// circuit. So the cancelling happens natively here, delegated from the document so it keeps working
// across re-renders. Touch has no drag events at all; the chevron buttons on each card are the
// primary way to move an activity.
(function () {
    function allowDrop(event) {
        if (!(event.target instanceof Element)) {
            return;
        }

        if (event.target.closest("[data-board-drop-target]") === null) {
            return;
        }

        event.preventDefault();

        if (event.dataTransfer) {
            event.dataTransfer.dropEffect = "move";
        }
    }

    document.addEventListener("dragenter", allowDrop);
    document.addEventListener("dragover", allowDrop);
})();
