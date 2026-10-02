// The size of the browser window in CSS pixels, sent with feedback so a layout problem on a phone
// can be told apart from one on a tablet.
window.homeScreen = {
    size() {
        return { width: window.innerWidth, height: window.innerHeight };
    }
};
