// Where focus lands after a route change.
//
// Blazor ships FocusOnNavigate, which parks focus on the new page's heading so a screen reader
// announces where you have landed. It does that with a plain .focus(), and focusing an element
// also scrolls it into view — so a moment after the dashboard settles, the browser drags the
// heading up under the top edge and the whole page appears to lurch. On a kitchen tablet that
// reads as the app twitching every time you open it.
//
// Taking the new page to the top first and then focusing without a scroll keeps the announcement
// and loses the lurch.
window.homeNavigation = {
    focusHeading(selector) {
        // The layout scrolls <main>, not the document, and a route change swaps the page inside a
        // container that keeps whatever offset the last page was left at.
        document.querySelector("main")?.scrollTo({ top: 0, behavior: "instant" });

        const heading = document.querySelector(selector);
        if (heading == null)
            return;

        // A heading is not focusable on its own. -1 makes it a focus target without putting it in
        // the tab order, which is what Blazor's own component does.
        if (!heading.hasAttribute("tabindex"))
            heading.setAttribute("tabindex", "-1");

        heading.focus({ preventScroll: true });
    }
};
