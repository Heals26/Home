using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Home.WebUI.Components.Shared.Navigation;

public partial class HomeFocusOnNavigate
{

    #region Fields

    private bool m_ShouldSetFocus;

    #endregion Fields

    #region Properties

    /// <summary>
    /// The route that has just matched. The router hands over a fresh one on every navigation,
    /// which is the signal that a new page is on screen.
    /// </summary>
    [Parameter, EditorRequired] public Microsoft.AspNetCore.Components.RouteData RouteData { get; set; } = null!;

    /// <summary>
    /// What to announce on the new page.
    /// </summary>
    [Parameter] public string Selector { get; set; } = "h1";

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!this.m_ShouldSetFocus)
            return;

        this.m_ShouldSetFocus = false;

        // Swallowed the same way the modal does: during prerender there is no browser to call, and
        // a page that failed to render a heading has nothing to announce either way.
        try { await this.JS.InvokeVoidAsync("homeNavigation.focusHeading", this.Selector); } catch { }
    }

    protected override void OnParametersSet()
        => this.m_ShouldSetFocus = true;

    #endregion Lifecycle Methods

}
